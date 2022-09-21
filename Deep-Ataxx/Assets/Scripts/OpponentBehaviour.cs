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
        private BoardState boardState;

        Vector2Int[] friendlyPositions, enemyPositions, allTiles;
        List<AIMove> moves = new List<AIMove>();
        AIMove finalMove;
        
        private void Awake(){
            gm = FindObjectOfType<GameManager>();
            opponent = gm.data.opponent;
        }
        //create board states and picture the board.
        public void PlanBoard(BoardState board){
            depthLevel = 0;
            boardState = board;
            allTiles = boardState.allTiles;
            FindMove(boardState);
        }
        public void FindMove(BoardState board){
            //find all tiles with pieces that are capable of being played for the correct player.
            if(moveTurn == 0){
                friendlyPositions = boardState.p1_Positions; //P1 friendly
                enemyPositions = boardState.p2_Positions;
            }
            else{
                friendlyPositions = boardState.p2_Positions; //P2 friendly
                enemyPositions = boardState.p1_Positions;
            }
            //Determine if a tile has valid movements
            foreach(var selectedPosition in friendlyPositions){
                Tile selectedTile = new Tile(selectedPosition, allTiles); //select a positions to make a tile out of.
                List<Vector2Int> validEndPositions = new List<Vector2Int>(); //valid end tiles for potential moves

                foreach(var endTile in selectedTile.reachablePositions){
                    validEndPositions.Add(endTile);
                    //Tiles off the board are invalid
                    if(endTile.x < 0 || endTile.y < 0 || endTile.x >= gm.data.selectedLevel.columns || endTile.y >= gm.data.selectedLevel.rows){
                        validEndPositions.Remove(endTile);
                        continue;
                    }
                    //Tiles that are occupied are invalid
                    foreach(var invalid in boardState.invalidTiles){
                        if(endTile == invalid){
                            validEndPositions.Remove(endTile);
                        }
                    }
                }
                if(validEndPositions.Count <=0){
                    continue;
                }
                //Create a "move" for each valid end position with the given start tile.
                foreach(var end in validEndPositions){
                    moves.Add(new AIMove(selectedTile, new Tile(end, allTiles), friendlyPositions, enemyPositions));
                }
            }
            //Decide on which move is best
            PickMove();
        }

        void PickMove(){
            //pick a final move to perform based on points
            AIMove final = null;
            foreach(var move in moves){
                if(move.isHop && boardState.emptyTiles <= 1){
                    move.points -= 99;
                }
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
            PerformMove(final);
        }

        void PerformMove(AIMove move){
            moves.Clear();
            moves.TrimExcess();
            foreach(var tileObject in gm.allTiles){
                if(tileObject.gridPosition == move.tile.position){
                    foreach(var t in tileObject.reachableTiles){
                        if(t.gridPosition == move.endTile.position){
                            tileObject.piece.AIMovement(t);
                            return;
                        }
                    }
                }
            }
        }
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
                            points += 1; //times multiplier for aggression
                        }
                    }
                }
                //In the future (if desired), we can add points to favor the middle by comparing numbers to the max columns/2 and rows/2. Add a number to closer to that you are.
            }
            else{
                points++;
            }
        }
    }
}