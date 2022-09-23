using System.Collections;
using System.Collections.Generic;
using Cox.Infection.Utilities;
using UnityEngine;

namespace Cox.Infection.Management{
    public class TileObject : MonoBehaviour
    {
        PlayerHelper player;

        public bool isDisabled = false;
        public PieceComponent piece = null;
        public Vector2Int gridPosition;
        public List<TileObject> adjacentTiles;
        public List<TileObject> reachableTiles;

        
        public GameObject blockade;
        public SpriteRenderer sr;

        Vector2 scale;

        private void Awake() {
            scale = transform.localScale;
            player = FindObjectOfType<PlayerHelper>();
            sr = GetComponent<SpriteRenderer>();
            sr.color = ColorManager.tileColor;
            blockade.SetActive(false);
            transform.localScale = new Vector3(1,1,1);
        }
        void LateUpdate() {
            if(transform.localScale.x > scale.x){
                Vector2 newScale = Vector2.MoveTowards(transform.localScale, scale, 5 * Time.deltaTime);
                transform.localScale = newScale;
            }
        }

        private void OnMouseEnter() {
            player.hoveredTile = this;
        }

        public void FindNeighbors(){
            TileObject[] tiles = FindObjectsOfType<TileObject>();
            foreach(var tile in tiles){
                if(this == tile)continue;
                if(Vector2Int.Distance(gridPosition, tile.gridPosition) < 2){
                    adjacentTiles.Add(tile);
                }
                if(Vector2Int.Distance(gridPosition, tile.gridPosition) < 3){
                    reachableTiles.Add(tile);
                }
            }
        }

        public void BlockTile(bool isBlocked){
            isDisabled = isBlocked;
            if(isDisabled){
                blockade.SetActive(true);
            }

        }

        public void SelectTile(bool isSelected){
            foreach(var tile in reachableTiles){
                if(tile.isDisabled || tile.piece != null)continue;
                if(isSelected)tile.sr.color = ColorManager.reachableTile;
                if(!isSelected)tile.sr.color = ColorManager.tileColor;
            }
            foreach(var tile in adjacentTiles){
                if(tile.isDisabled || tile.piece != null)continue;
                if(isSelected && piece != null)tile.sr.color = ColorManager.adjacentTile;
                if(!isSelected)tile.sr.color = ColorManager.tileColor;
            }
        }
    }
}

