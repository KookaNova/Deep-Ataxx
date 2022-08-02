using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cox.Infection.Management{
    public class GameManager : MonoBehaviour
    {
        public List<PieceComponent> redPieces = new List<PieceComponent>();
        public List<PieceComponent> greenPieces = new List<PieceComponent>();
        public List<PieceComponent> redPlayable = new List<PieceComponent>();
        public List<PieceComponent> greenPlayable = new List<PieceComponent>();
        
        public int turnNumber = 0;
        public int emptyTiles;
        public PlayerPersistentChoice data;
        public OpponentBehaviour opponent;

        GameUIManager gameUI;
        PlayerHelper player;
        public TileObject[] allTiles;
        public bool isGameOver = false;

        //Undo
        public List<BoardState> history = new List<BoardState>();
	    public int undoIndex = 0;

        public void StartGame(){
            var root = FindObjectOfType<UIDocument>().rootVisualElement;
            gameUI = root.Q<GameUIManager>();
            allTiles = FindObjectsOfType<TileObject>();
            player = FindObjectOfType<PlayerHelper>();
            opponent = FindObjectOfType<OpponentBehaviour>();



            if(data.enableAI){
                player.AssignSingleTurn(data.playerTurn);
            }
            CheckTeams();
        }

        public void EndTurn(){
            //clear lists
            redPieces.Clear();
            redPieces.TrimExcess();
            greenPieces.Clear();
            greenPieces.TrimExcess();
            //Change turn
            if(turnNumber == 0){
                turnNumber = 1;
            }
            else{
                turnNumber = 0;
            }
            Debug.Log("Current Turn = " + turnNumber);
            CheckTeams();
        }

        public void CheckTeams(){
            gameUI.ChangeTurn();
            //recreate lists (this is simply the easiest way to recount everything).
            var allPieces = FindObjectsOfType<PieceComponent>();
            for(int i = 0; i < allPieces.Length; i++){
                if(allPieces[i].moveTurn == 0){
                    redPieces.Add(allPieces[i]);
                }
                else if(allPieces[i].moveTurn == 1){
                    greenPieces.Add(allPieces[i]);
                }
            }

            if(redPieces.Count == 0 || greenPieces.Count == 0){
                GameOver("GameOver: Loser has no pieces left.");
            }
            gameUI.UpdateScore(); //Use piece counts to update scoreboard
            //Save new board after finding all pieces
            SaveBoardState();
            PlayabilityCheck(); //check the playability of each piece
        }

        public void PlayabilityCheck(){
            //first check if there are empty tiles;
            emptyTiles = 0;
            foreach(var tile in allTiles){
                if(tile.isDisabled)continue;
                if(tile.piece)continue;
                emptyTiles++;
            }

            if(emptyTiles == 0){
                GameOver("GameOver: No empty tiles left");
                return; //ends game if tiles are empty
            }
            redPlayable.Clear();
            redPlayable.TrimExcess();
            greenPlayable.Clear();
            greenPlayable.TrimExcess();

            //Make pieces playable
            switch(turnNumber) {
                case 0:
                    foreach(var piece in redPieces){
                        if(piece.CheckPlayability())redPlayable.Add(piece);
                    }
                    foreach(var piece in greenPieces){
                        piece.isPlayable = false;
                    }
                    if(redPlayable.Count < 1)EndTurn();
                break;
                case 1:
                    foreach(var piece in redPieces){
                        piece.isPlayable = false;
                    }
                    foreach(var piece in greenPieces){
                        if(piece.CheckPlayability())greenPlayable.Add(piece);
                    }
                    if(greenPlayable.Count < 1)EndTurn();
                break;
                default:
                    turnNumber = 0;
                    PlayabilityCheck();
                break;
            }

            if(data.enableAI && opponent.moveTurn == turnNumber)StartCoroutine(BeginAITurn());
        }

        void GameOver(string reason){
            isGameOver = true;
            Debug.Log(reason);
            string winner = null;
            if(redPieces.Count > greenPieces.Count){
                winner = "RED";
            }
            else{
                winner = "GREEN";
            }

            gameUI.GameOver(winner);

        }

        #region Board States and Undo
        public void SaveBoardState(){
		    //Clear redo possibilities
		    if(undoIndex > 0){
                int start = undoIndex;
                history.Insert(0,history[start]);
                history.RemoveAt(start+1);
                while(undoIndex > 0){
                    history.RemoveAt(undoIndex);
                    undoIndex--;
                }
                
            }

            BoardState newBoard = new BoardState(redPieces, greenPieces);
		    history.Insert(0, newBoard); //Add new board state at 0
		    undoIndex = 0; //Set index back to 0
	    }

        public void Undo(){
            if(history.Count == 0)return;
            undoIndex++;
            if(undoIndex >= history.Count){
                undoIndex--;
                return;
            }
            foreach(var piece in redPieces){
                Destroy(piece.gameObject);
            }
            foreach(var piece in greenPieces){
                Destroy(piece.gameObject);
            }
            redPieces.Clear();
            redPieces.TrimExcess();
            greenPieces.Clear();
            greenPieces.TrimExcess();
            foreach(var tile in allTiles){
                tile.piece = null;
            }

		    for(int i = 0; i < history[undoIndex].redPositions.Length; i++){
                TileObject tile = FindTileByName(history[undoIndex].redTileNames[i]);
                var p = Instantiate(data.selectedLevel.piece, tile.transform.position, Quaternion.identity);
                p.ChangeTeam(0);
                redPieces.Add(p);
                p.homeTile = tile;
                p.name = tile.name + ".piece";
                p.homeTile.piece = p;
            }
            for(int i = 0; i < history[undoIndex].greenPositions.Length; i++){
                TileObject tile = FindTileByName(history[undoIndex].greenTileNames[i]);
                var p = Instantiate(data.selectedLevel.piece, tile.transform.position, Quaternion.identity);
                p.ChangeTeam(1);
                greenPieces.Add(p);
                p.homeTile = tile;
                p.name = tile.name + ".piece";
                p.homeTile.piece = p;
            }
            if(turnNumber == 0){
                turnNumber = 1;
            }
            else{
                turnNumber = 0;
            }
            isGameOver = false;
            PlayabilityCheck();
        }
        /// <summary>
        /// Finds the first tile with the given string name.
        /// </summary>
        public TileObject FindTileByName(string tileName){
            TileObject foundTile = null;
            foreach(var tile in allTiles){
                if(tile.name == tileName){
                    foundTile = tile;
                    break;
                }
            }
            return foundTile;

        }

        #endregion

        public IEnumerator BeginAITurn(){
            yield return new WaitForSeconds(1);
            if(turnNumber == 0)opponent.FindMoves(redPlayable);
            if(turnNumber == 1)opponent.FindMoves(greenPlayable);
        }
    }
}

