using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    public class OpponentBehaviour : MonoBehaviour
    {
        GameManager gm;
        CharacterObject opponent;

        public int moveTurn = 0;

        List<AIMove> moves = new List<AIMove>();
        List<PieceComponent> playablePieces;


        AIMove finalMove;
        
        private void Awake() {
            gm = FindObjectOfType<GameManager>();
        }

        public void FindMoves(List<PieceComponent> pieces){
            if(gm.isGameOver)return; //Don't move if the game is over.

            //clear previous moves
            moves.Clear(); 
            moves.TrimExcess();

            //find a new move
            Debug.Log("Finding a move...");
            foreach(var piece in pieces){
                for(int i = 0; i < piece.playableTiles.Count; i++){
                    AIMove move = new AIMove(piece, piece.playableTiles[i]);
                    if(move.piece.homeTile == move.endTile){
                        move = null;
                        continue;
                    }
                    else{
                        move.AssignPoints();
                        if(gm.emptyTiles == 1){ //condition for when the final move is at hand. Probably should be cleaned up.
                            bool badMove = true;
                            foreach(var tile in move.piece.homeTile.adjacentTiles){
                                if(move.endTile == tile){
                                    badMove = false;
                                    break;
                                }
                            }
                            if(badMove)move.points -= 90;
                            moves.Add(move);
                        }
                        else{
                            moves.Add(move);
                        }
                        
                    }
                    
                }
            }
            FinalDecision();
        }

        public void FindFutureMoves(){
            
        }

        void FinalDecision(){
            finalMove = null; //erase previous final move

            Debug.Log("Making a move."); //pick a favorite potential move and select as a final move
            foreach(var move in moves){
                if(finalMove == null){ //first move is selected in case no later moves are better
                    finalMove = move;
                    continue;
                }
                if(move.points > finalMove.points){ //select move if it's better than the current final move
                    finalMove = move;
                    continue;
                }
                if(move.points == finalMove.points){ //randomly select between moves of equal value to determine final
                    int randomize = Random.Range(0,2);
                    if(randomize > 0){
                        finalMove = move;
                    }
                    continue;
                }
            }
            StartCoroutine(MakeMove());
            
        }
        private IEnumerator MakeMove(){
            if(gm.turnNumber != moveTurn || gm.isGameOver) yield break;
            finalMove.piece.homeTile.SelectTile(true); //select the piece from the final move. Used for visuals.
            yield return new WaitForSeconds(0.5f);
            finalMove.piece.homeTile.SelectTile(false);
            if(gm.turnNumber != moveTurn || gm.isGameOver) yield break;
            finalMove.PerformMove(); //perform move.

        }

    }

    public class AIMove{
        public PieceComponent piece;
        public TileObject endTile;
        public int points;

        public AIMove(PieceComponent _piece, TileObject _endTile){
            piece = _piece;
            endTile = _endTile;
        }

        public void AssignPoints(/*add values to help assign points*/){
            //Assess adjacent tiles for point gains and losses
            foreach(var tile in endTile.adjacentTiles){
                if(tile.piece == null)continue;
                if(tile.piece.moveTurn != piece.moveTurn){
                    //if the end tile can take pieces, add a point.
                    points += 1;
                }
            }
            foreach(var tile in piece.homeTile.adjacentTiles){
                if(tile.piece == null)continue;
                if(tile.piece.moveTurn == piece.moveTurn){
                    //for each adjacent piece that belongs to the ai, subtract a point if the move would leave it open to be taken.
                    for(int i = 0; i < tile.piece.homeTile.reachableTiles.Count; i++){
                        if(tile.piece.homeTile.reachableTiles[i].piece == null)continue;
                        if(tile.piece.homeTile.reachableTiles[i].piece.moveTurn != piece.moveTurn){
                            points -= 1;
                        }
                    }
                }
            }
            foreach(var tile in piece.homeTile.adjacentTiles){
                //add a point if a hop isn't performed
                if(endTile == tile)points += 1;
            }
        }

        public void PerformMove(){
            Debug.Log("Performing move " + piece.homeTile.name + " to " + endTile.name);
            piece.AIMovement(this);
        }
        /*
        public void PerformMove(BoardState board){
            Debug.Log("Performing theoretical move.");


        }*/
        
    }
}

