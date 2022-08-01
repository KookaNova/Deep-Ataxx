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
            
            moves.Clear();
            moves.TrimExcess();
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
                        moves.Add(move);
                    }
                    
                }
            }
            FinalDecision();
        }

        public void FindFutureMoves(){
            
        }

        void FinalDecision(){
            finalMove = null;
            Debug.Log("Making a move.");
            foreach(var move in moves){
                if(finalMove == null){
                    finalMove = move;
                    continue;
                }
                if(move.points > finalMove.points){
                    finalMove = move;
                    continue;
                }
                if(move.points == finalMove.points){
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
            finalMove.piece.homeTile.SelectTile(true);
            yield return new WaitForSeconds(0.5f);
            finalMove.piece.homeTile.SelectTile(false);
            if(gm.turnNumber != moveTurn || gm.isGameOver) yield break;
            finalMove.PerformMove();

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
            foreach(var tile in endTile.adjacentTiles){
                if(tile.piece == null)continue;
                if(tile.piece.moveTurn != piece.moveTurn){
                    points += 1; //plus aggression
                }
            }
            foreach(var tile in piece.homeTile.adjacentTiles){
                if(tile.piece == null)continue;
                if(tile.piece.moveTurn == piece.moveTurn){
                    for(int i = 0; i < tile.piece.homeTile.reachableTiles.Count; i++){
                        if(tile.piece.homeTile.reachableTiles[i].piece == null)continue;
                        if(tile.piece.homeTile.reachableTiles[i].piece.moveTurn != piece.moveTurn){
                            points -= 1;
                        }
                    }
                }
            }
            foreach(var tile in piece.homeTile.adjacentTiles){
                if(endTile == tile)points += 1;
            }
        }

        public void PerformMove(){
            Debug.Log("Performing move " + piece.homeTile.name + " to " + endTile.name);
            piece.AIMovement(this);
        }
        
    }
}

