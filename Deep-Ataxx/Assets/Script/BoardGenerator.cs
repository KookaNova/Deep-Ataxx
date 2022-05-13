using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    public class BoardGenerator : MonoBehaviour
    {
        GameManager gm;
        public Level level;

        Vector2 offset = Vector2.zero;
        TileObject[,] grid;
        float cameraOffset = 2;

        

        private void Start() {
            gm = FindObjectOfType<GameManager>();
            GenerateBoard();
        }

        void GenerateBoard(){
            offset = new Vector2(level.columns/2, level.rows/2);

            grid = new TileObject[level.columns,level.rows];
            for(int i = 0; i < level.columns; i++){
                for(int j = 0; j < level.rows; j++){
                    TileObject newTile = Instantiate(level.tile, new Vector3((i - offset.x)* level.padding,(-j + offset.y) * level.padding,0), Quaternion.identity);
                    grid[i,j] = newTile;
                    newTile.gridPosition = new Vector2Int(i,j);
                    //find blocked tiles and block them.
                    if(level.blockedTiles != null){
                        foreach (var blockedPosition in level.blockedTiles){
                            if(newTile.gridPosition == blockedPosition){
                                newTile.BlockTile(true);
                            }
                        }
                    }
                    
                    newTile.transform.SetParent(transform);
                    newTile.name = "Tile[" + i + "," + j + "]";
                }
            }
            //allow tiles to know about grid
            foreach(var tile in grid){
                tile.FindNeighbors();
            }
            
            PositionCamera();
            PlaceStarterPieces();
        }

        void PositionCamera(){
            //makes sure board is always on screen.
            if(level.rows > level.columns){
                Camera.main.orthographicSize = level.columns + 2 + ((level.rows - level.columns) * 0.5f);
            }
            else{
                Camera.main.orthographicSize = level.columns + 2;
            }
            //centers board to screen with offset.
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y + cameraOffset, -10);
        }

        void PlaceStarterPieces(){
            //Places red starter pieces
            for(int i = 0; i < level.redStartTiles.Length; i++){
                var p = Instantiate(level.piece, grid[level.redStartTiles[i].x, level.redStartTiles[i].y].transform.position, Quaternion.identity);
                p.ChangeTeam(Team.RedTeam);
                p.homeTile = grid[level.redStartTiles[i].x, level.redStartTiles[i].y];
                p.homeTile.piece = p;
            }
            //Places blue starter pieces
            for(int i = 0; i < level.blueStartTiles.Length; i++){
                var p = Instantiate(level.piece, grid[level.blueStartTiles[i].x, level.blueStartTiles[i].y].transform.position, Quaternion.identity);
                p.ChangeTeam(Team.GreenTeam);
                p.homeTile = grid[level.blueStartTiles[i].x, level.blueStartTiles[i].y];
                p.homeTile.piece = p;
            }

            gm.StartGame();


        }

    }
}

