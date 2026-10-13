using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cox.Infection.Management{
    public class OpponentBehaviour : MonoBehaviour {
        const int WinScore = 1000000;
        const int PieceScore = 375;
        //1000 prioritizes taking pieces. This could be modified against aggression values to change what's important to the AI.
        GameManager gm;
        CharacterObject opponentAI;

        [FormerlySerializedAs("moveTurn")]
        public int playerId = 0;

        struct SearchNode
        {
            public BoardMove Move;
            public BoardState State;
            public int OrderingScore;

            public SearchNode(BoardMove move, BoardState state, int orderingScore)
            {
                Move = move;
                State = state;
                OrderingScore = orderingScore;
            }
        }

        public int PlayerId => playerId;

        /// <summary>Finds the game manager and initializes the selected opponent and AI player ID.</summary>
        private void Awake() {
            gm = FindAnyObjectByType<GameManager>();
            if (gm != null && gm.data != null)
            {
                opponentAI = gm.data.opponent;
                playerId = BoardRules.GetOtherPlayerId(gm.data.playerId);
            }
        }

        /// <summary>Searches the configured depth and returns the highest-scoring legal move.</summary>
        public bool TryGetBestMove(BoardState state, out BoardMove bestMove)
        {
            bestMove = default;
            if (state == null || state.CurrentPlayerId != playerId || BoardRules.isGameOver(state))
            {
                return false;
            }

            var moves = BoardRules.GetLegalMoves(state);
            if (moves.Count == 0)
            {
                return false;
            }
            //Depth of 0 means no move, so we avoid that here. Depth of 1 evaluates the AI's move, depth of 2 evaluates the player response
            int searchDepth = Mathf.Max(1, opponentAI != null ? opponentAI.thinkDepth : 1);
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            int bestScore = int.MinValue;
            var orderedNodes = new List<SearchNode>(moves.Count);

            foreach (var move in moves)
            {
                var nextState = BoardRules.ApplyMove(state, move);
                orderedNodes.Add(new SearchNode(move, nextState, EvaluatePositionScore(nextState)));
            }
            orderedNodes.Sort((left, right) => right.OrderingScore.CompareTo(left.OrderingScore));

            foreach (var node in orderedNodes)
            {
                int score = Search(node.State, searchDepth - 1, alpha, beta);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = node.Move;
                }

                alpha = Mathf.Max(alpha, bestScore);
                if (alpha >= beta)
                {
                    break;
                }
            }

            return true;
        }

        /// <summary>Scores future positions with minimax and prunes branches using alpha-beta bounds.</summary>
        int Search(BoardState state, int remainingDepth, int alpha, int beta)
        {
            //stop evaluating
            if (BoardRules.isGameOver(state))
            {
                return EvaluateFinalMove(state, remainingDepth);
            }
            if (remainingDepth <= 0)
            {
                return EvaluatePositionScore(state);
            }
            //pass
            var moves = BoardRules.GetLegalMoves(state);
            if (moves.Count == 0) {
                return Search(BoardRules.PassTurn(state), remainingDepth, alpha, beta);
            }
            //checks if AI is Active Player, then begins creating move nodes
            bool isActivePlayer = state.CurrentPlayerId == playerId;
            var orderedNodes = new List<SearchNode>(moves.Count);
            foreach (var move in moves) {
                var nextState = BoardRules.ApplyMove(state, move);
                orderedNodes.Add(new SearchNode(move, nextState, EvaluatePositionScore(nextState)));
            }
            Debug.Log("Ai is evaluating " + orderedNodes.Count.ToString() + " movement nodes.");
            orderedNodes.Sort((left, right) => isActivePlayer
                ? right.OrderingScore.CompareTo(left.OrderingScore)
                : left.OrderingScore.CompareTo(right.OrderingScore));
            //Begin Minimax with Alpha-Beta Pruning. 
            //If AI is active, start low and keep the (highest) idealScore, else start high and keep the (Lowest)idealScore
            if (isActivePlayer)
            {
                int idealScore = int.MinValue; 
                //search nodes for highest score
                foreach (var node in orderedNodes)
                {
                    idealScore = Mathf.Max(idealScore, Search(node.State, remainingDepth - 1, alpha, beta));
                    alpha = Mathf.Max(alpha, idealScore);
                    if (alpha >= beta)
                    {
                        break;//prune
                    }
                }
                return idealScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                //sesarch nodes for lowest score
                foreach (var node in orderedNodes)
                {
                    bestScore = Mathf.Min(bestScore, Search(node.State, remainingDepth - 1, alpha, beta));
                    beta = Mathf.Min(beta, bestScore);
                    if (alpha >= beta)
                    {
                        break;//prune
                    }
                }
                return bestScore;
            }
        }

        /// <summary>Assigns a decisive score to a finished game, preferring faster wins and slower losses.</summary>
        int EvaluateFinalMove(BoardState state, int remainingDepth)
        {
            int playerPieces = 0;
            int opponentPieces = 0;
            CountPieces(state, ref playerPieces, ref opponentPieces);
            if (playerPieces == opponentPieces)
            {
                return 0;
            }

            int materialScore = (playerPieces - opponentPieces) * PieceScore;
            return playerPieces > opponentPieces
                ? WinScore + remainingDepth + materialScore
                : -WinScore - remainingDepth + materialScore;
        }

        /// <summary>Scores a non-gameover board by piece advantage and available legal moves.</summary>
        int EvaluatePositionScore(BoardState state)
        {
            int playerPieces = 0;
            int opponentPieces = 0;
            CountPieces(state, ref playerPieces, ref opponentPieces);

            int playerMobility = BoardRules.GetLegalMoves(state, playerId).Count;
            int opponentMobility = BoardRules.GetLegalMoves(state, BoardRules.GetOtherPlayerId(playerId)).Count;
            int modifiedPieceScore = PieceScore * Mathf.Max(1, opponentAI != null ? opponentAI.aggression : 1); //we want to avoid this becoming zero
            return (playerPieces - opponentPieces) * modifiedPieceScore + playerMobility - opponentMobility;
        }

        /// <summary>Counts AI and opposing pieces on the supplied board.</summary>
        void CountPieces(BoardState state, ref int playerPieces, ref int opponentPieces)
        {
            for (int y = 0; y < state.Height; y++)
            {
                for (int x = 0; x < state.Width; x++)
                {
                    var cell = state.GetCell(x, y);
                    if (cell.Content != BoardCellContent.Piece)
                    {
                        continue;
                    }

                    if (cell.PlayerId == playerId)
                    {
                        playerPieces++;
                    }
                    else
                    {
                        opponentPieces++;
                    }
                }
            }
        }
    }
       
}