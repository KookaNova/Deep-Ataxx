using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    public class OpponentBehaviour : MonoBehaviour
    {
        GameManager gm;
        CharacterObject opponent;

        public int moveTurn = 0;

        private int depthLevel = 0;

        List<AIMove> moves = new List<AIMove>();
        List<PieceComponent> playablePieces;


        AIMove finalMove;
        
        private void Awake() {
            gm = FindObjectOfType<GameManager>();
        }

        //create board states and picture the board.
        public void ImaginingTheBoard(){
            depthLevel = opponent.thinkDepth;
            BoardState board = gm.history[gm.undoIndex]; //current board stored by gm
            FindMove(board);

            while(depthLevel > 0){
                //create board
                //find moves
                //subtract depth
                depthLevel--;

            }
        }

        public void FindMove(BoardState board){
            List<Tile> playableTiles = new List<Tile>();

            //find all tiles that are capable of being played.
            if(moveTurn == 0){
                foreach(var tilePos in board.p1_Positions){
                    Tile tile = new Tile(tilePos);
                    foreach(var neighbor in tile.reachablePositions){
                        if(neighbor.x < 0 || neighbor.y <0 || neighbor.x >= gm.boardSize.x || neighbor.y >= gm.boardSize.y){
                            continue;
                        }
                        playableTiles.Add(tile);
                        Debug.Log(tile + " is playable.");
                    }
                }
            }
            //select end action for playable tiles.


        }


    }

    public class Tile{
        public Vector2Int position;
        public Vector2Int[] adjacentPositions = new Vector2Int[7];
        public Vector2Int[] reachablePositions = new Vector2Int[23];

        public Tile(Vector2Int _position){
            position = _position;
            adjacentPositions = FindAdjacentPositions(position);
            reachablePositions = FindReachablePositions(position);

            
        }

        Vector2Int[] FindAdjacentPositions(Vector2Int _position){
            //Find adjacent positions clockwise from bottom left
            List<Vector2Int> adjacentList = new List<Vector2Int>();

            int x = _position.x - 1;
            //finds adjacent tiles programatically.
            for(int i = 0; i < 3; i++){
                int y = _position.y - 1;
                //begin positions check.
                for(int j = 0; j < 3; j++){
                    Vector2Int pos = new Vector2Int(x, y);
                    y++;
                    if(pos == _position){
                        continue;
                    }
                    adjacentList.Add(pos);
                    Debug.LogFormat("Tile {0}: adjacent {1}.", position, pos);
                    
                }
                x++;
            }
            return adjacentList.ToArray();
        }
        Vector2Int[] FindReachablePositions(Vector2Int _position){
            //Find reachable positions clockwise from bottom left, spiralling in to the next row once applicable.
            List<Vector2Int> reachableList = new List<Vector2Int>();

            int x = _position.x - 2;
            for(int i = 0; i < 5; i++){
                int y = _position.y - 2;
                //begin positions check.
                for(int j = 0; j < 5; j++){
                    Vector2Int pos = new Vector2Int(x, y);
                    y++;
                    if(pos == _position){
                        continue;
                    }
                    reachableList.Add(pos);
                    
                }
                x++;
            }
            return reachableList.ToArray();
        }
    }

    public class AIMove{
        public Vector2Int startTile;
        public Vector2Int endTile;

        public AIMove(Vector2Int startTile){

        }
    }
}

