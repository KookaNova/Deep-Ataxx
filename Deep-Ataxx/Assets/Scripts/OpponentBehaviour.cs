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

        Vector2Int[] friendlyPositions, enemyPositions;

        List<AIMove> moves = new List<AIMove>();


        AIMove finalMove;
        
        private void Awake() {
            gm = FindObjectOfType<GameManager>();
            opponent = gm.data.opponent;
        }

        //create board states and picture the board.
        public void PlanBoard(BoardState board){
            depthLevel = 0;
            FindMove(board);
        }

        public void FindMove(BoardState board){
            //find all tiles with pieces that are capable of being played for the correct player.
            if(moveTurn == 0){
                friendlyPositions = board.p1_Positions; //P1 friendly
                enemyPositions = board.p2_Positions;
            }
            else{
                friendlyPositions = board.p2_Positions; //P2 friendly
                enemyPositions = board.p1_Positions;
            }
            //Determine if a tile has valid movements
            foreach(var tilePos in friendlyPositions){
                Tile tile = new Tile(tilePos);
                List<Tile> validEndTiles = new List<Tile>(); //valid end tiles for potential moves
                //Find valid end positions for movements
                foreach(var possibleEndTile in tile.reachablePositions){
                    bool isValid = true;
                    //Check if end position is on the board
                    if(possibleEndTile.x < 0 || possibleEndTile.y < 0 || possibleEndTile.x >= gm.boardSize.x || possibleEndTile.y >= gm.boardSize.y){
                        isValid = false;
                        continue;
                    }
                    //Check if end tile is valid
                    foreach(var invalidTile in board.invalidTiles){
                        if(possibleEndTile == invalidTile){
                            isValid = false;
                        }
                    }
                    if(isValid){
                        validEndTiles.Add(new Tile(possibleEndTile));
                    }
                }
                if(validEndTiles.Count > 0){
                    foreach(var et in validEndTiles){
                        AIMove move = new AIMove(tile, et, friendlyPositions, enemyPositions);
                        if(depthLevel == 0)move.isPrimeMove = true; //if the move is surface level, it is prime.
                        moves.Add(move);
                    }
                    
                }
            }

            PickMove();
        }

        void PickMove(){
            //pick a final move to perform based on points
            AIMove final = null;
            foreach(var move in moves){
                if(final == null){
                    final = move;
                    continue;
                    }
                if(move.points > final.points){
                    final = move;
                    continue;
                    }
                if(move.points == final.points){
                    int ran = Random.Range(0,2);
                    if(ran > 0)final = move;
                }
            }
            moves.Clear();
            moves.TrimExcess();
            PerformMove(final);
        }

        void PerformMove(AIMove move){
            foreach(var tile in gm.allTiles){
                if(tile.gridPosition == move.tile.position){
                    foreach(var t in tile.reachableTiles){
                        if(t.gridPosition == move.endTile.position){
                            tile.piece.AIMovement(t);
                            break;
                        }
                    }
                }
            }
        }
    }

    public class Tile{
        public Vector2Int position;
        public Vector2Int[] adjacentPositions {get;} = new Vector2Int[7];
        public Vector2Int[] reachablePositions {get;} = new Vector2Int[23];

        public Tile(Vector2Int _position){
            position = _position;
            adjacentPositions = FindAdjacentPositions(position);
            reachablePositions = FindReachablePositions(position);
        }

        #region FindNeighboringTiles
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
        #endregion
    }

    public class AIMove{
        public Tile tile;
        public Tile endTile;
        public int points = 0;
        public bool isPrimeMove = false;
        public bool isHop = false;

        private Vector2Int[] friends, enemies;

        /// <summary>
        /// Using a given tile and end tile, we create moves and calculate points based on the positions of friendly tiles
        /// and enemy tiles.
        /// </summary>
        public AIMove(Tile mainTile, Tile _endTile, Vector2Int[] _friendlyPositions, Vector2Int[] _enemyPositions){
            tile = mainTile;
            endTile = _endTile;
            friends = _friendlyPositions;
            enemies = _enemyPositions;
            CalculatePoints();
        }

        void CalculatePoints(){
            if(Vector2Int.Distance(tile.position, endTile.position) >= 2){
                isHop = true;
                //piece would hop, lose point for every exposed friendly tile
                foreach(var adjacent in tile.adjacentPositions){
                    foreach(var friendly in friends){
                        if(adjacent == friendly){
                            points -= 1; //written this way so we can add a defensive multiplier if desired.
                        }
                    }
                }

                //add points when the end tile has adjacent enemies
                foreach(var adjacent in endTile.adjacentPositions){
                    foreach(var enemy in enemies){
                        if(adjacent == enemy){
                            points += 1;//times multiplier for aggression
                        }
                    }
                }

                //

                //In the future (if desired), we can add points to favor the middle by comparing numbers to the max columns/2 and rows/2. Add a number to closer to that you are.

            }
            else{
                points++;
            }
        }
    }
}

