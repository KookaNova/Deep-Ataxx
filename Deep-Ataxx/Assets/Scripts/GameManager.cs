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
        public PlayerPersistantChoice data;
        public OpponentBehaviour opponent;

        GameUIManager gameUI;
        PlayerHelper player;
        TileObject[] allTiles;

        public void StartGame(){
            var root = FindObjectOfType<UIDocument>().rootVisualElement;
            gameUI = root.Q<GameUIManager>();
            allTiles = FindObjectsOfType<TileObject>();
            player = FindObjectOfType<PlayerHelper>();
            opponent = FindObjectOfType<OpponentBehaviour>();

            if(data.invertFirstTurn)turnNumber = 1;
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
            PlayabilityCheck(); //check the playability of each piece
        }

        public void PlayabilityCheck(){
            //first check if there are empty tiles;
            int emptyTiles = 0;
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

        public IEnumerator BeginAITurn(){
            yield return new WaitForSeconds(1);
            if(turnNumber == 0)opponent.FindMoves(redPlayable);
            if(turnNumber == 1)opponent.FindMoves(greenPlayable);
        }
    }
}

