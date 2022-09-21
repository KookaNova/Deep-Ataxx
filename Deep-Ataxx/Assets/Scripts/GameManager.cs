using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cox.Infection.Management{
    public class GameManager : MonoBehaviour
    {
        public List<PieceComponent> p1_Pieces = new List<PieceComponent>();
        public List<PieceComponent> p2_Pieces = new List<PieceComponent>();
        public List<PieceComponent> p1_Playable = new List<PieceComponent>();
        public List<PieceComponent> p2_Playable = new List<PieceComponent>();
        public Vector2Int[] blockedSpaces;
        
        public int turnNumber = 0;
        public int emptyTiles;
        public Vector2Int boardSize = Vector2Int.zero;
        public PlayerPersistentChoice data;
        public OpponentBehaviour opponent;

        GameUIManager gameUI;
        PlayerHelper player;
        public TileObject[] allTiles;
        public Vector2Int[] allTilePositions;
        public bool isGameOver = false;

        //Undo
        public List<BoardState> history = new List<BoardState>();
	    public int undoIndex = 0;
        int turns = 0;

        public void StartGame(){
            var root = FindObjectOfType<UIDocument>().rootVisualElement;
            gameUI = root.Q<GameUIManager>();
            allTiles = FindObjectsOfType<TileObject>();
            player = FindObjectOfType<PlayerHelper>();
            opponent = FindObjectOfType<OpponentBehaviour>();
            allTilePositions = new Vector2Int[allTiles.Length];
            
            for(int i = 0; i < allTiles.Length; i++){
                allTilePositions[i] = allTiles[i].gridPosition;
            }

            if(data.enableAI){
                player.AssignSingleTurn(data.playerTurn);
            }
            SaveBoardState();
            CheckTeams();
        }

        public void EndTurn(){
            //clear lists
            p1_Pieces.Clear();
            p1_Pieces.TrimExcess();
            p2_Pieces.Clear();
            p2_Pieces.TrimExcess();
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
                    p1_Pieces.Add(allPieces[i]);
                }
                else if(allPieces[i].moveTurn == 1){
                    p2_Pieces.Add(allPieces[i]);
                }
            }
            //Save new board after finding all pieces
            SaveBoardState();
            gameUI.UpdateScore(); //Use piece counts to update scoreboard
            if(p1_Pieces.Count == 0 || p2_Pieces.Count == 0){
                GameOver("GameOver: Loser has no pieces left.");
                return;
            }
            
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
            p1_Playable.Clear();
            p1_Playable.TrimExcess();
            p2_Playable.Clear();
            p2_Playable.TrimExcess();

            //Make pieces playable
            switch(turnNumber) {
                case 0:
                    foreach(var piece in p1_Pieces){
                        if(piece.CheckPlayability())p1_Playable.Add(piece);
                    }
                    foreach(var piece in p2_Pieces){
                        piece.isPlayable = false;
                    }
                    if(p1_Playable.Count < 1)EndTurn();
                break;
                case 1:
                    foreach(var piece in p1_Pieces){
                        piece.isPlayable = false;
                    }
                    foreach(var piece in p2_Pieces){
                        if(piece.CheckPlayability())p2_Playable.Add(piece);
                    }
                    if(p2_Playable.Count < 1)EndTurn();
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
            if(p1_Pieces.Count > p2_Pieces.Count){
                winner = "RED";
            }
            else{
                winner = "GREEN";
            }
            foreach(var tile in allTiles){
                tile.isDisabled = true;
                if(tile.piece)tile.piece.isPlayable = false;
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

            BoardState newBoard = new BoardState(p1_Pieces, p2_Pieces, blockedSpaces, allTilePositions);
		    history.Insert(0, newBoard); //Add new board state at 0
		    undoIndex = 0; //Set index back to 0
            turns++;
	    }

        public void Undo(){
            if(history.Count == 0)return;
            undoIndex++;
            if(undoIndex >= history.Count){
                undoIndex--;
                return;
            }
            foreach(var piece in p1_Pieces){
                Destroy(piece.gameObject);
            }
            foreach(var piece in p2_Pieces){
                Destroy(piece.gameObject);
            }
            p1_Pieces.Clear();
            p1_Pieces.TrimExcess();
            p2_Pieces.Clear();
            p2_Pieces.TrimExcess();
            foreach(var tile in allTiles){
                tile.piece = null;
            }

		    for(int i = 0; i < history[undoIndex].p1_Positions.Length; i++){
                TileObject tile = FindTileByName(history[undoIndex].p1_TileNames[i]);
                var p = Instantiate(data.selectedLevel.piece, tile.transform.position, Quaternion.identity);
                p.ChangeTeam(0);
                p1_Pieces.Add(p);
                p.homeTile = tile;
                p.name = tile.name + ".piece";
                p.homeTile.piece = p;
            }
            for(int i = 0; i < history[undoIndex].p2_Positions.Length; i++){
                TileObject tile = FindTileByName(history[undoIndex].p2_TileNames[i]);
                var p = Instantiate(data.selectedLevel.piece, tile.transform.position, Quaternion.identity);
                p.ChangeTeam(1);
                p2_Pieces.Add(p);
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
            opponent.PlanBoard(history[undoIndex]);
        }
    }
}

