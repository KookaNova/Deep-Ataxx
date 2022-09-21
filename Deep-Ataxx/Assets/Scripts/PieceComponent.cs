using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cox.Infection.Utilities;

namespace Cox.Infection.Management{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PieceComponent : MonoBehaviour
    {
        public bool isPlayable;
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
                sr.color = ColorManager.playerOne; //sets color
                lr.startColor = ColorManager.playerOne;
                lr.endColor = ColorManager.playerOne;
            }
            if(moveTurn == 1){
                sr.color = ColorManager.playerTwo; //sets color
                lr.startColor = ColorManager.playerTwo;
                lr.endColor = ColorManager.playerTwo;
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
            Debug.Log("Piece returned. Invalid move: " + homeTile.name + " to " + endTile.name + ". " + reason);
            endTile = null;
            transform.position = homeTile.transform.position;
        }
        void Hop(){
            Debug.Log("Hop performed.");
            homeTile.piece = null;
            homeTile = endTile;
            endTile = null;
            homeTile.piece = this;
            name = homeTile.name + ".piece";
            transform.position = homeTile.transform.position;
            Infect();
        }
        void Spread(){
            Debug.Log("Spread performed.");
            transform.position = homeTile.transform.position;
            var p = Instantiate(prefab, endTile.transform.position, Quaternion.identity);
            p.homeTile = endTile;
            p.name = p.homeTile.name + ".piece";
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
            player.hoveredTile = this.homeTile;
            if(!isPlayable)return;
            if(gm.data.enableAI){
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
            if(gm.data.enableAI){
                if(player.singleTurn != moveTurn)return;
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
                if(player.singleTurn != moveTurn)return;
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
                if(player.singleTurn != moveTurn)return;
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

            yield return new WaitForSecondsRealtime(.25f);

            lr.SetPosition(1, endTile.gameObject.transform.position);

            yield return new  WaitForSecondsRealtime(.75f);

            lr.enabled = false;
            animator.SetBool("isSelected", false);
            homeTile.SelectTile(false);
            PieceMoved();
        }
        
        #endregion

    }
}

