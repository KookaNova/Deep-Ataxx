using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cox.Infection.Management{
    public class GameManager : MonoBehaviour
    {
        public List<PieceComponent> redPieces = new List<PieceComponent>();
        public List<PieceComponent> greenPieces = new List<PieceComponent>();
        public bool redsTurn = true;
        GameUIManager gameUI;
        TileObject[] allTiles;

        public void StartGame(){
            var root = FindObjectOfType<UIDocument>().rootVisualElement;
            gameUI = root.Q<GameUIManager>();
            allTiles = FindObjectsOfType<TileObject>();

            
            redsTurn = true;
            gameUI.ChangeTurn();
            CheckTeams();
        }

        public void EndTurn(){
            redsTurn = !redsTurn; //must be first
            CheckTeams();
            gameUI.ChangeTurn();
        }

        public void CheckTeams(){
            //clear lists
            redPieces.Clear();
            redPieces.TrimExcess();
            greenPieces.Clear();
            greenPieces.TrimExcess();
            //recreate lists (this is simply the easiest way to recount everything).
            var allPieces = FindObjectsOfType<PieceComponent>();
            for(int i = 0; i < allPieces.Length; i++){
                if(allPieces[i].team == Team.RedTeam){
                    redPieces.Add(allPieces[i]);
                }
                else if(allPieces[i].team == Team.GreenTeam){
                    greenPieces.Add(allPieces[i]);
                }
            }
            PlayabilityCheck();
            gameUI.UpdateScore();

            if(redPieces.Count == 0 || greenPieces.Count == 0){
                GameOver("GameOver: Loser has no pieces left.");
            }
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

            //Make pieces playable
            int playableTiles = 0;
            switch(redsTurn) {
                case true:
                    foreach(var piece in redPieces){
                        if(piece.CheckPlayability())playableTiles++;
                    }
                    foreach(var piece in greenPieces){
                        piece.isPlayable = false;
                    }
                    if(playableTiles < 1)EndTurn();
                break;
                case false:
                    foreach(var piece in redPieces){
                        piece.isPlayable = false;
                    }
                    foreach(var piece in greenPieces){
                        if(piece.CheckPlayability())playableTiles++;
                    }
                    if(playableTiles < 1)EndTurn();
                break;
            }
            
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
    }
}

