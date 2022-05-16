using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cox.Infection.Management{
    public class GameManager : MonoBehaviour
    {
        public List<PieceComponent> redPieces = new List<PieceComponent>();
        public List<PieceComponent> greenPieces = new List<PieceComponent>();
        [HideInInspector] public bool redsTurn = true;
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
            redPieces.Clear();
            redPieces.TrimExcess();
            greenPieces.Clear();
            greenPieces.TrimExcess();
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

            if(redPieces.Count == 0 || greenPieces.Count == 0 || redPieces.Count + greenPieces.Count == allTiles.Length){
                GameOver();
            }
        }

       public void PlayabilityCheck(){
            switch(redsTurn) {
                case true:
                    foreach(var piece in redPieces){
                        piece.isPlayable = true;
                    }
                    foreach(var piece in greenPieces){
                        piece.isPlayable = false;
                    }
                    break;
                case false:
                    foreach(var piece in redPieces){
                        piece.isPlayable = false;
                    }
                    foreach(var piece in greenPieces){
                        piece.isPlayable = true;
                    }
                    break;
            }
            
        }

        void GameOver(){
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

