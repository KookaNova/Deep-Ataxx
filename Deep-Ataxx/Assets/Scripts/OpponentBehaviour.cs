using System.Collections.Generic;
using System.Linq;
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
        int stress = 0;
        int mistakeFactor = 0;
        int thinkDepth = 1;

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

        List<BoardMove> moveOptions = new List<BoardMove>();
        public int PlayerId => playerId;

        /// <summary>Finds the game manager and initializes the selected opponent and AI player ID.</summary>
        private void Awake() {
            gm = FindAnyObjectByType<GameManager>();
            if (gm != null && gm.data != null)
            {
                opponentAI = gm.data.opponent;
                mistakeFactor = opponentAI.mistakeLikeliness;
                thinkDepth = opponentAI.thinkDepth;
                playerId = BoardRules.GetOtherPlayerId(gm.data.playerId);
            }
        }

        /// <summary>Searches the configured depth and returns the highest-scoring legal move. 
        ///If true, the best move is output. If false, it failed to find a move.</summary>
        public bool ChooseMoveOptions(BoardState state, out BoardMove chosenMove) {
            moveOptions.Clear();
            EvaluateStressResponse(state);
            chosenMove = default;
            if (state == null || state.CurrentPlayerId != playerId || BoardRules.isGameOver(state)) {
                return false;
            }

            List<BoardMove> moves = BoardRules.GetLegalMoves(state);
            if (moves.Count == 0) {
                return false;
            }
            //Depth of 0 means no move, so we avoid that here. Depth of 1 evaluates the AI's move, depth of 2 evaluates the player response
            int searchDepth = Mathf.Max(1, thinkDepth);
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            int bestScore = int.MinValue;
            var orderedNodes = new List<SearchNode>(moves.Count);

           

            foreach (var move in moves) {
                var nextState = BoardRules.ApplyMove(state, move);
                orderedNodes.Add(new SearchNode(move, nextState, EvaluatePositionScore(nextState)));
            }
            orderedNodes.Sort((a, b) => b.OrderingScore.CompareTo(a.OrderingScore));

            //In search depth 1, we can go ahead and add moveOptions

            if (searchDepth == 1) {
                int optionCount = orderedNodes.Count;

                foreach(var node in orderedNodes) 
                {
                    moveOptions.Add(node.Move);
                }

                moveOptions.Reverse();
            }

            foreach (var node in orderedNodes) {
                int score = Search(node.State, searchDepth - 1, alpha, beta);
                if (score > bestScore) {
                    bestScore = score;
                    if (thinkDepth > 1) { moveOptions.Add(node.Move); }
                }
                alpha = Mathf.Max(alpha, bestScore);
                if (alpha >= beta) {
                    break;
                }
            }

            chosenMove = MakeMoveChoice();

            return true;
        }
        ///<summary>Makes a final decision based on a given mistake factor. </summary>
        BoardMove MakeMoveChoice() {
            BoardMove finalMove = default;
            int chosenMoveIndex = 0;
            int mistakeValue = Random.Range((mistakeFactor > 0 ? mistakeFactor : 1)/10, mistakeFactor);

            if (mistakeValue < 15 || moveOptions.Count <= 1) {
                chosenMoveIndex = moveOptions.Count - 1;
                Debug.Log("First Choice");
            }
            else if (mistakeValue < 75 || moveOptions.Count <= 2) {
                chosenMoveIndex = moveOptions.Count - 2;
                Debug.Log("Second Choice");
            }
            else {
                chosenMoveIndex = moveOptions.Count - 3;
                Debug.Log("Third Choice");
            }

            return finalMove = moveOptions[chosenMoveIndex];
        }
        ///<summary>Evaluate how the AI responds to stress factors </summary>
        public void EvaluateStressResponse(BoardState state) {
            if(!opponentAI.mistakesChangeWithStress && !opponentAI.thinkDepthChangesWithStress) {
                return; //doesn't respond to stress
            }
            int stressModifier = CalculateStress(state.GetScore(), state.GetEnemyScore(), state.GetMaxScore());
            stress = Mathf.Clamp(stress += stressModifier,0,100);

            if (opponentAI.mistakesChangeWithStress) {
                if (opponentAI.invertMistakeStressRelationship) {
                    mistakeFactor -= stress / 20;
                }
                else {
                    mistakeFactor += stress / 20;
                }
                Debug.Log("Mistake factor is now: " + mistakeFactor);
            }
            if (opponentAI.thinkDepthChangesWithStress && stress >= 50) {
                if (opponentAI.invertThinkDepthStressRelationship) {
                    thinkDepth = Mathf.Max(1, opponentAI.thinkDepth - 1);
                }
                else {
                    thinkDepth = opponentAI.thinkDepth + 1;
                }
                Debug.Log("Think depth is now: " + thinkDepth);
            }
        }
        int CalculateStress(int score, int enemyScore, int maxScore) {
            int difference = enemyScore - score;
            int remainingCells = maxScore - score - enemyScore;
            int stress = opponentAI.stressRate * ((difference*2) - (remainingCells/3));
            Debug.Log("Calculated stress modifier: " + stress);
            return stress;
        }

        /// <summary>Scores future positions with minimax and prunes branches using alpha-beta bounds.</summary>
        int Search(BoardState state, int remainingDepth, int alpha, int beta)
        {
            //stop evaluating if game over or think depth reaches 0
            if (BoardRules.isGameOver(state))
            {
                return EvaluateFinalMove(state, remainingDepth);
            }
            if (remainingDepth <= 0)
            {
                return EvaluatePositionScore(state);
            }
            //pass there are no moves

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
            
            orderedNodes.Sort((a, b) => isActivePlayer
                ? b.OrderingScore.CompareTo(a.OrderingScore)
                : a.OrderingScore.CompareTo(b.OrderingScore));
            //Begin Minimax with Alpha-Beta Pruning. 
            //If AI is active, start low and keep the (highest) idealScore, else start high and keep the (Lowest)idealScore
            if (isActivePlayer)
            {
                int idealScore = int.MinValue;
                //search nodes for highest score
                foreach (var node in orderedNodes) {
                    idealScore = Mathf.Max(idealScore, Search(node.State, remainingDepth - 1, alpha, beta));
                    if(idealScore > alpha) {
                        alpha = idealScore;
                    }
                    if (alpha >= beta) {
                        break;//prune
                    }
                }
                return idealScore;
            }
            else
            {
                int idealScore = int.MaxValue;
                //sesarch nodes for lowest score
                foreach (var node in orderedNodes)
                {
                    idealScore = Mathf.Min(idealScore, Search(node.State, remainingDepth - 1, alpha, beta));
                    if (idealScore < beta) {
                        beta = idealScore;
                    }
                    if (alpha >= beta)
                    {
                        break;//prune
                    }
                }
                return idealScore;
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