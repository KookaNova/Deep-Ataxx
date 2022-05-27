using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cox.Infection.Utilities;

namespace Cox.Infection.Management{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PieceComponent : MonoBehaviour
    {
        public bool isPlayable;
        public bool isAIPiece;
        public PieceComponent prefab;
        public TileObject homeTile;
        public List<TileObject> playableTiles = new List<TileObject>();
        public int moveTurn;
        TileObject endTile;

        GameManager gm;
        SpriteRenderer sr;
        LineRenderer lr;
        PlayerHelper player;
        Animator animator;

        private void Awake() {
            gm = FindObjectOfType<GameManager>();
            sr = GetComponent<SpriteRenderer>();
            lr = GetComponentInChildren<LineRenderer>();
            lr.SetPosition(0, transform.position);
            lr.enabled = false;
            animator = GetComponent<Animator>();
            player = FindObjectOfType<PlayerHelper>();
        }

        public void ChangeTeam(int newMoveTurn) {
            moveTurn = newMoveTurn;

            if(moveTurn == 0){
                sr.color = ColorManager.red; //sets color
                lr.startColor = ColorManager.red;
                lr.endColor = ColorManager.red;
            }
            if(moveTurn == 1){
                sr.color = ColorManager.green; //sets color
                lr.startColor = ColorManager.green;
                lr.endColor = ColorManager.green;
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
            if(homeTile.gridPosition == endTile.gridPosition){
                ReturnToHome("Attempted home position.");
            }
            else if(endTile.piece){
                ReturnToHome("Tile is occupied.");
            }
            else if(endTile.isDisabled){
                ReturnToHome("Tile is disabled.");
            }
            else if(Vector2Int.Distance(homeTile.gridPosition, endTile.gridPosition) >= 3){ 
                ReturnToHome("Tile is too far.");
            }
            else{
                if(Vector2Int.Distance(homeTile.gridPosition, endTile.gridPosition) >= 2){
                    Hop();
                }
                else{
                    Spread();
                }
            }

        }
        void ReturnToHome(string reason){
            Debug.Log("Piece returned. Invalid move. " + reason);
            endTile = null;
            transform.position = homeTile.transform.position;
        }
        void Hop(){
            Debug.Log("Hop performed.");
            homeTile.piece = null;
            homeTile = endTile;
            endTile = null;
            homeTile.piece = this;
            name = homeTile.name;
            transform.position = homeTile.transform.position;
            Infect();
        }
        void Spread(){
            Debug.Log("Spread performed.");
            transform.position = homeTile.transform.position;
            var p = Instantiate(prefab, endTile.transform.position, Quaternion.identity);
            p.homeTile = endTile;
            p.name = p.homeTile.name;
            endTile.piece = p;
            p.moveTurn = moveTurn;
            endTile = null;
            p.isPlayable = false;
            p.Infect();


        }
        void Infect(){
            foreach(var tile in homeTile.adjacentTiles){
                if(tile.piece == null)continue;
                tile.piece.ChangeTeam(moveTurn);
                tile.piece.isPlayable = false;
            }
            gm.EndTurn();
        }
        #endregion

        #region Inputs
        private void OnMouseEnter() {
            if(!isPlayable)return;
            if(player.isSinglePlayer){
                if(player.singleTurn != moveTurn)return;
            }
            animator.SetBool("isHovered", true);
        }

        private void OnMouseExit() {
            if(!isPlayable)return;
            animator.SetBool("isHovered", false);
        }

        private void OnMouseDown() {
            if(!isPlayable)return;
            if(player.isSinglePlayer){
                if(player.singleTurn != moveTurn)return;
            }
            player.selectedPiece = this;
            lr.SetPosition(0, transform.position);
            lr.enabled = true;
            animator.SetBool("isSelected", true);
        }

        private void OnMouseUp() {
            if(!isPlayable)return; 
            if(player.isSinglePlayer){
                if(player.singleTurn != moveTurn)return;
            }
            lr.enabled = false;
            animator.SetBool("isSelected", false);
            player.selectedPiece = null;
            endTile = player.hoveredTile;
            PieceMoved();

        }
        private void OnMouseDrag() {
            if(!isPlayable)return;
            if(player.isSinglePlayer){
                if(player.singleTurn != moveTurn)return;
            }
            lr.SetPosition(1, GetMousePosition());
            //transform.position = GetMousePosition();
        }

        Vector3 GetMousePosition(){
            var screenPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return new Vector3(screenPosition.x, screenPosition.y, 5);
        }

        public void AIMovement(AIMove move){
            endTile = move.endTile;
            Debug.Log("Ai move received.");
            PieceMoved();
        }
        
        #endregion

    }
}

