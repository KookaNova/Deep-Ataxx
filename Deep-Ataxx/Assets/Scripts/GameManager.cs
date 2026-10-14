using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Serialization;

namespace Cox.Infection.Management{
    public class GameManager : MonoBehaviour
    {
        public List<PieceComponent> p1_Pieces = new List<PieceComponent>();
        public List<PieceComponent> p2_Pieces = new List<PieceComponent>();
        public List<PieceComponent> p1_Playable = new List<PieceComponent>();
        public List<PieceComponent> p2_Playable = new List<PieceComponent>();
        public Vector2Int[] blockedSpaces;
        
        [FormerlySerializedAs("turnNumber")]
        public int currentPlayerId = 0;
        public int emptyTiles;
        public Vector2Int boardSize = Vector2Int.zero;
        public PlayerPersistentChoice data;
        public OpponentBehaviour computerOpponent;

        GameUIManager gameUI;
        PlayerHelper player;
        public TileObject[] allTiles;
        public Vector2Int[] allTilePositions;
        public bool isGameOver = false;

        //Undo
        [System.NonSerialized] public List<BoardState> history = new List<BoardState>();
        public int undoIndex = 0;
        bool isInitializingGame;
        bool isResolvingTurn;
        bool isRestoringState;
        bool isAIMoveInProgress;
        Coroutine gameOverRoutine;

        public bool CanUndo => history != null && undoIndex > 0 && undoIndex < history.Count && !isResolvingTurn && !isRestoringState && !isAIMoveInProgress;

        public void StartGame(){
            var root = FindAnyObjectByType<UIDocument>()?.rootVisualElement;
            gameUI = root.Q<GameUIManager>();
            allTiles = FindObjectsByType<TileObject>();
            player = FindAnyObjectByType<PlayerHelper>();
            computerOpponent = FindAnyObjectByType<OpponentBehaviour>();
            allTilePositions = new Vector2Int[allTiles.Length];
            
            for(int i = 0; i < allTiles.Length; i++){
                allTilePositions[i] = allTiles[i].gridPosition;
            }

            if(data.enableAI){
                player.AssignPlayerId(data.playerId);
            }
            if (data.selectedLevel != null)
            {
                currentPlayerId = data.isRedFirst
                    ? BoardRules.PlayerOneId
                    : BoardRules.PlayerTwoId;
            }

            isInitializingGame = true;
            try
            {
                CheckTeams();
            }
            finally
            {
                isInitializingGame = false;
            }

            history.Clear();
            undoIndex = -1;
            SaveBoardState();
            ScheduleAIMove();
        }

        public void EndTurn()
        {
            EndTurn(BoardRules.GetOtherPlayerId(currentPlayerId));
        }

        public void EndTurn(int nextPlayerId)
        {
            if (nextPlayerId != BoardRules.PlayerOneId && nextPlayerId != BoardRules.PlayerTwoId)
            {
                throw new System.ArgumentOutOfRangeException(nameof(nextPlayerId));
            }

            bool isRootTurn = !isResolvingTurn;
            if (isRootTurn)
            {
                isResolvingTurn = true;
            }

            try
            {
                p1_Pieces.Clear();
                p1_Pieces.TrimExcess();
                p2_Pieces.Clear();
                p2_Pieces.TrimExcess();

                currentPlayerId = nextPlayerId;
                Debug.Log("Current Turn = " + currentPlayerId);
                CheckTeams();
            }
            finally
            {
                if (isRootTurn)
                {
                    isResolvingTurn = false;
                    if (!isInitializingGame && !isRestoringState) {
                        isAIMoveInProgress = false;
                        SaveBoardState();
                        ScheduleAIMove();
                    }
                }
            }
        }

        /// <summary>Queues an AI move when the configured computer player owns the settled turn.</summary>
        void ScheduleAIMove()
        {
            if (data == null || !data.enableAI || computerOpponent == null || isGameOver || isResolvingTurn || isRestoringState)
            {
                return;
            }

            if (currentPlayerId == computerOpponent.PlayerId)
            {
                StartCoroutine(PerformAIMove());
            }
        }

        /// <summary>Chooses an AI move and routes it through the piece animation and completion flow.</summary>
        IEnumerator PerformAIMove()
        {
            isAIMoveInProgress = true;  //prevents undo mid-move
            yield return new WaitForSecondsRealtime(1f);
            //check if AI can make a move
            if (data == null || !data.enableAI || computerOpponent == null || isGameOver ||
                isResolvingTurn || isRestoringState || currentPlayerId != computerOpponent.PlayerId)
            {
                yield break;
            }

            var state = CaptureBoardState();
            computerOpponent.TryGetBestMove(state, out var move);
            TileObject sourceTile = null;
            TileObject destinationTile = null;
            foreach (var tile in allTiles)
            {
                if (tile.gridPosition.x == move.FromX && tile.gridPosition.y == move.FromY)
                {
                    sourceTile = tile;
                }
                if (tile.gridPosition.x == move.ToX && tile.gridPosition.y == move.ToY)
                {
                    destinationTile = tile;
                }
            }

            if (sourceTile == null || destinationTile == null || sourceTile.piece == null)
            {
                Debug.LogError("AI selected a move that cannot be mapped to live board tiles.");
                yield break;
            }

            sourceTile.piece.AIMovement(destinationTile);
        }

        void SaveBoardState()
        {
            if (undoIndex + 1 < history.Count)
            {
                history.RemoveRange(undoIndex + 1, history.Count - undoIndex - 1);
            }

            history.Add(CaptureBoardState());
            undoIndex = history.Count - 1;
            gameUI?.RefreshUndoButton();
        }

        public BoardState CaptureBoardState()
        {
            int width = 0;
            int height = 0;
            foreach (var tile in allTiles)
            {
                width = Mathf.Max(width, tile.gridPosition.x + 1);
                height = Mathf.Max(height, tile.gridPosition.y + 1);
            }

            var cells = new BoardCell[width * height];
            var blockedPositions = new HashSet<Vector2Int>();
            if (blockedSpaces != null)
            {
                foreach (var position in blockedSpaces)
                {
                    blockedPositions.Add(position);
                }
            }

            foreach (var tile in allTiles)
            {
                var position = tile.gridPosition;
                int index = position.y * width + position.x;
                if (blockedPositions.Contains(position))
                {
                    cells[index] = BoardCell.Blocked;
                }
                else if (tile.piece != null)
                {
                    cells[index] = BoardCell.ForPlayer(tile.piece.playerId);
                }
                else
                {
                    cells[index] = BoardCell.Empty;
                }
            }

            return new BoardState(width, height, cells, currentPlayerId);
        }

        public void CheckTeams(){
            gameUI.ChangeTurn();
            //recreate lists (this is simply the easiest way to recount everything).
            p1_Pieces.Clear();
            p2_Pieces.Clear();
            var allPieces = FindObjectsByType<PieceComponent>();
            for(int i = 0; i < allPieces.Length; i++){
                if(allPieces[i].playerId == 0){
                    p1_Pieces.Add(allPieces[i]);
                }
                else if(allPieces[i].playerId == 1){
                    p2_Pieces.Add(allPieces[i]);
                }
            }
            //Save new board after finding all pieces
            //SaveBoardState();
            gameUI.UpdateScore(); //Use piece counts to update scoreboard
            if(p1_Pieces.Count == 0 || p2_Pieces.Count == 0){
                if (!isGameOver) gameOverRoutine = StartCoroutine(GameOver("GameOver: Loser has no pieces left."));
                return;
            }

            if (BoardRules.isGameOver(CaptureBoardState()))
            {
                if (!isGameOver) gameOverRoutine = StartCoroutine(GameOver("GameOver: No legal moves remain."));
                return;
            }
            
            PlayabilityCheck(); //check the playability of each piece
        }

        public void PlayabilityCheck(){
            //first check if there are empty tiles;
            emptyTiles = 0;
            foreach(var tile in allTiles){
                if(tile.isDisabled)continue;
                if(tile.piece)continue;
                emptyTiles++;
            }

            if(emptyTiles == 0){
                if (!isGameOver) gameOverRoutine = StartCoroutine(GameOver("GameOver: No empty tiles left"));
                return; //ends game if tiles are empty
            }
            p1_Playable.Clear();
            p1_Playable.TrimExcess();
            p2_Playable.Clear();
            p2_Playable.TrimExcess();

            //Make pieces playable
            switch(currentPlayerId) {
                case 0:
                    foreach(var piece in p1_Pieces){
                        if(piece.CheckPlayability())p1_Playable.Add(piece);
                    }
                    foreach(var piece in p2_Pieces){
                        piece.isPlayable = false;
                    }
                    if(p1_Playable.Count < 1 && !isRestoringState)EndTurn();
                break;
                case 1:
                    foreach(var piece in p1_Pieces){
                        piece.isPlayable = false;
                    }
                    foreach(var piece in p2_Pieces){
                        if(piece.CheckPlayability())p2_Playable.Add(piece);
                    }
                    if(p2_Playable.Count < 1 && !isRestoringState)EndTurn();
                break;
                default:
                    currentPlayerId = 0;
                    PlayabilityCheck();
                break;
            }
        }

        public void Undo()
        {
            if (!CanUndo)
            {
                return;
            }

            int targetIndex = undoIndex - 1;
            if (data.enableAI)
            {
                int humanPlayerId = player.controlledPlayerId;
                while (targetIndex > 0 && history[targetIndex].CurrentPlayerId != humanPlayerId)
                {
                    targetIndex--;
                }
            }

            if (!CanRestoreBoardState(history[targetIndex]))
            {
                Debug.LogError("Undo could not restore the selected board state.");
                return;
            }

            if (gameOverRoutine != null)
            {
                StopCoroutine(gameOverRoutine);
                gameOverRoutine = null;
            }

            undoIndex = targetIndex;
            isGameOver = false;
            isRestoringState = true;
            try
            {
                RestoreBoardState(history[undoIndex]);
                CheckTeams();
            }
            finally
            {
                isRestoringState = false;
            }

            gameUI.ShowPlayScreen();
            ScheduleAIMove();
        }

        bool CanRestoreBoardState(BoardState state)
        {
            int requiredPieces = 0;
            for (int y = 0; y < state.Height; y++)
            {
                for (int x = 0; x < state.Width; x++)
                {
                    if (state.GetCell(x, y).Content == BoardCellContent.Piece)
                    {
                        requiredPieces++;
                    }
                }
            }

            return requiredPieces <= FindObjectsByType<PieceComponent>().Length;
        }

        void RestoreBoardState(BoardState state)
        {
            var tilesByPosition = new Dictionary<Vector2Int, TileObject>();
            foreach (var tile in allTiles)
            {
                tilesByPosition.Add(tile.gridPosition, tile);
                tile.piece = null;
            }

            foreach (var tile in allTiles)
            {
                var cell = state.GetCell(tile.gridPosition.x, tile.gridPosition.y);
                tile.BlockTile(cell.Content == BoardCellContent.Blocked);
                tile.SelectTile(false);
            }

            player.selectedPiece = null;
            var pieces = FindObjectsByType<PieceComponent>();
            int pieceIndex = 0;
            for (int y = 0; y < state.Height; y++)
            {
                for (int x = 0; x < state.Width; x++)
                {
                    var cell = state.GetCell(x, y);
                    if (cell.Content != BoardCellContent.Piece)
                    {
                        continue;
                    }

                    var tile = tilesByPosition[new Vector2Int(x, y)];
                    pieces[pieceIndex++].RestoreForUndo(tile, cell.PlayerId);
                }
            }

            for (; pieceIndex < pieces.Length; pieceIndex++)
            {
                pieces[pieceIndex].gameObject.SetActive(false);
                Destroy(pieces[pieceIndex].gameObject);
            }

            currentPlayerId = state.CurrentPlayerId;
        }

        IEnumerator GameOver(string reason) {
            isGameOver = true;
            Debug.Log(reason);
            string winner = null;
            if (p1_Pieces.Count > p2_Pieces.Count) {
                winner = "RED";
            }
            else {
                winner = "GREEN";
            }
            foreach (var tile in allTiles) {
                tile.isDisabled = true;
                if (tile.piece) tile.piece.isPlayable = false;
            }
            gameUI.HideScore();
            yield return new WaitForSecondsRealtime(1);
            if (isGameOver)
            {
                gameUI.GameOver(winner);
            }
            gameOverRoutine = null;
        }
    }
}

