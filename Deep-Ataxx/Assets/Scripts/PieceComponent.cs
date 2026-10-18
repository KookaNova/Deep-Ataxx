using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Cox.Infection.Utilities;

namespace Cox.Infection.Management{
    public class PieceComponent : MonoBehaviour
    {
        public bool isPlayable;
        [SerializeField] PieceComponent prefab;
        public TileObject homeTile;
        [SerializeField] UnityEngine.Rendering.Universal.Light2D glowLight; //Adds glow
        [SerializeField] UnityEngine.Rendering.Universal.Light2D normLight; //Light that uses normals

        List<TileObject> playableTiles = new List<TileObject>();
        [FormerlySerializedAs("moveTurn")]
        public int playerId;
        TileObject endTile;

        GameManager gm;
        SpriteRenderer sr;
        LineRenderer lr;
        PlayerHelper player;
        Animator animator;
        Vector2 scale;

        private void Awake() {
            gm = FindAnyObjectByType<GameManager>();
            sr = GetComponentInChildren<SpriteRenderer>();
            lr = GetComponentInChildren<LineRenderer>();
            lr.SetPosition(0, transform.position);
            lr.enabled = false;
            animator = GetComponentInChildren<Animator>();
            player = FindAnyObjectByType<PlayerHelper>();
            
        }

        public void SetPlayerId(int newPlayerId) {
            playerId = newPlayerId;

            if(playerId == 0) {
                glowLight.color = ColorManager.playerOne;
                normLight.color = ColorManager.playerOne;
                sr.color = ColorManager.playerOne; //sets color
                lr.startColor = ColorManager.playerOne;
                lr.endColor = ColorManager.playerOne;
            }
            if(playerId == 1) {
                glowLight.color = ColorManager.playerTwo;
                normLight.color = ColorManager.playerTwo;
                sr.color = ColorManager.playerTwo; //sets color
                lr.startColor = ColorManager.playerTwo;
                lr.endColor = ColorManager.playerTwo;
            }
        }

        public void RestoreForUndo(TileObject tile, int restoredPlayerId)
        {
            endTile = null;
            homeTile = tile;
            tile.piece = this;
            transform.position = tile.transform.position;
            name = tile.name + ".piece";
            SetPlayerId(restoredPlayerId);
            isPlayable = false;

            if (lr != null)
            {
                lr.enabled = false;
                lr.SetPosition(0, transform.position);
                lr.SetPosition(1, transform.position);
            }
            if (animator != null)
            {
                animator.SetBool("isSelected", false);
                animator.SetBool("isHovered", false);
            }
        }

        public bool CheckPlayability(){
            playableTiles.Clear();
            playableTiles.TrimExcess();
            isPlayable = false;
            foreach(var tile in homeTile.reachableTiles){
                if(tile.isDisabled || tile.piece || tile == homeTile)continue;
                playableTiles.Add(tile); //this list may become helpful for AI, that's why we're adding it in and hints. We could also highlight a piece's potential moves.
            }
            if(playableTiles.Count > 0)isPlayable = true;
            return isPlayable;
        }

        #region  Movement
        public void PieceMoved(){
            if (endTile == null)
            {
                transform.position = homeTile.transform.position;
                return;
            }

            var currentState = gm.CaptureBoardState();
            var legalMoves = BoardRules.GetLegalMoves(currentState);
            bool foundMove = false;
            BoardMove selectedMove = default;
            foreach (var move in legalMoves)
            {
                if (move.FromX == homeTile.gridPosition.x && move.FromY == homeTile.gridPosition.y &&
                    move.ToX == endTile.gridPosition.x && move.ToY == endTile.gridPosition.y)
                {
                    selectedMove = move;
                    foundMove = true;
                    break;
                }
            }

            if (!foundMove)
            {
                ReturnToHome("Move is not legal.");
                return;
            }

            var resultingState = BoardRules.ApplyMove(currentState, selectedMove);
            if (selectedMove.Type == BoardMoveType.Hop)
            {
                Hop();
            }
            else
            {
                Spread();
            }

            ApplyPlayerIds(resultingState);
            gm.EndTurn(resultingState.CurrentPlayerId);
        }
        void ReturnToHome(string reason){
            Debug.Log("Piece returned. Invalid move from " + homeTile.name + " to " + (endTile != null ? endTile.name : "no destination") + ". " + reason);
            endTile = null;
            transform.position = homeTile.transform.position;
        }
        void Hop(){
            homeTile.piece = null;
            homeTile = endTile;
            endTile = null;
            homeTile.piece = this;
            name = homeTile.name + ".piece";
            transform.position = homeTile.transform.position;
        }
        void Spread(){
            transform.position = homeTile.transform.position;
            var p = Instantiate(prefab, endTile.transform.position, Quaternion.identity);
            p.homeTile = endTile;
            p.name = p.homeTile.name + ".piece";
            endTile.piece = p;
            p.SetPlayerId(playerId);
            endTile = null;
            p.isPlayable = false;
        }

        void ApplyPlayerIds(BoardState state)
        {
            foreach (var tile in gm.allTiles)
            {
                if (tile.piece == null)
                {
                    continue;
                }

                var cell = state.GetCell(tile.gridPosition.x, tile.gridPosition.y);
                if (cell.Content == BoardCellContent.Piece && tile.piece.playerId != cell.PlayerId)
                {
                    tile.piece.SetPlayerId(cell.PlayerId);
                }
                tile.piece.isPlayable = false;
            }
        }
        #endregion

        #region Inputs
        private void OnMouseEnter() {
            player.hoveredTile = this.homeTile;
            if(!isPlayable)return;
            if(gm.data.enableAI){
                if(player.controlledPlayerId != playerId)return;
            }
            animator.SetBool("isHovered", true);
        }

        private void OnMouseExit() {
            if(!isPlayable)return;
            animator.SetBool("isHovered", false);
        }

        private void OnMouseDown() {
            if(!isPlayable)return;
            if(gm.data.enableAI){
                if(player.controlledPlayerId != playerId)return;
            }
            player.selectedPiece = this;
            lr.SetPosition(0, transform.position);
            lr.enabled = true;
            animator.SetBool("isSelected", true);
            homeTile.SelectTile(true);
        }

        private void OnMouseUp() {
            if(!isPlayable)return; 
            if(gm.data.enableAI){
                if(player.controlledPlayerId != playerId)return;
            }
            lr.enabled = false;
            animator.SetBool("isSelected", false);
            player.selectedPiece = null;
            endTile = player.hoveredTile;
            homeTile.SelectTile(false);
            PieceMoved();
            

        }
        private void OnMouseDrag() {
            if(!isPlayable)return;
            if(gm.data.enableAI){
                if(player.controlledPlayerId != playerId)return;
            }
            lr.SetPosition(1, GetMousePosition());
            //transform.position = GetMousePosition();
        }

        Vector3 GetMousePosition(){
            var screenPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return new Vector3(screenPosition.x, screenPosition.y, 5);
        }

        public void AIMovement(TileObject _endTile){
            Debug.Log("AI: " + this.name + " is attempting to move to " + _endTile);
            endTile = _endTile;
            StartCoroutine(AIPerform());
            
        }

        public void SetPositionByTileObject(TileObject newHomeTile){
            Debug.Log("Piece" + this.name + "is attempting to move to " + newHomeTile);
            homeTile.piece = null;
            homeTile = newHomeTile;
            transform.position = homeTile.transform.position;
        }

        IEnumerator AIPerform(){
            lr.SetPosition(0, transform.position);
            lr.enabled = true;
            animator.SetBool("isSelected", true);
            homeTile.SelectTile(true);
            lr.SetPosition(1, transform.position);
            yield return new WaitForSecondsRealtime(.15f);
            Vector2 linePos = lr.GetPosition(1);
            Vector2 endTilePos = endTile.gameObject.transform.position;
            while (linePos !=  endTilePos) {
                yield return new WaitForFixedUpdate();
                Vector2 movePos = Vector2.Lerp(lr.GetPosition(1), endTile.gameObject.transform.position, .25f);
                lr.SetPosition(1, movePos);
                linePos = lr.GetPosition(1);
            }

            yield return new WaitForSecondsRealtime(.25f);

            lr.enabled = false;
            animator.SetBool("isSelected", false);
            homeTile.SelectTile(false);
            PieceMoved();
        }
        
        #endregion

    }
}

