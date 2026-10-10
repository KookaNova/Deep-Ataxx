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
            var root = FindAnyObjectByType<UIDocument>()?.rootVisualElement;
            gameUI = root.Q<GameUIManager>();
            allTiles = FindObjectsByType<TileObject>();
            player = FindAnyObjectByType<PlayerHelper>();
            opponent = FindAnyObjectByType<OpponentBehaviour>();
            allTilePositions = new Vector2Int[allTiles.Length];
            
            for(int i = 0; i < allTiles.Length; i++){
                allTilePositions[i] = allTiles[i].gridPosition;
            }

            if(data.enableAI){
                player.AssignSingleTurn(data.playerTurn);
            }
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
            var allPieces = FindObjectsByType<PieceComponent>();
            for(int i = 0; i < allPieces.Length; i++){
                if(allPieces[i].moveTurn == 0){
                    p1_Pieces.Add(allPieces[i]);
                }
                else if(allPieces[i].moveTurn == 1){
                    p2_Pieces.Add(allPieces[i]);
                }
            }
            //Save new board after finding all pieces
            //SaveBoardState();
            gameUI.UpdateScore(); //Use piece counts to update scoreboard
            if(p1_Pieces.Count == 0 || p2_Pieces.Count == 0){
                StartCoroutine(GameOver("GameOver: Loser has no pieces left."));
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
                StartCoroutine(GameOver("GameOver: No empty tiles left"));
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
        }

        IEnumerator GameOver(string reason) {
            isGameOver = true;
            Debug.Log(reason);
            string winner = null;
            if (p1_Pieces.Count > p2_Pieces.Count) {
                winner = "RED";
            }
            else {
                winner = "GREEN";
            }
            foreach (var tile in allTiles) {
                tile.isDisabled = true;
                if (tile.piece) tile.piece.isPlayable = false;
            }
            gameUI.HideScore();
            yield return new WaitForSecondsRealtime(1);
            gameUI.GameOver(winner);
        }
    }
}

