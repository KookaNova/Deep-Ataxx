using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    public class BoardGenerator : MonoBehaviour
    {
        GameManager gm;
        public Rules rules;

        Vector2 offset = Vector2.zero;
        TileObject[,] grid;
        float cameraOffset = 2;

        

        private void Start() {
            gm = FindObjectOfType<GameManager>();
            GenerateBoard();
        }

        void GenerateBoard(){
            offset = new Vector2(rules.columns/2, rules.rows/2);

            grid = new TileObject[rules.columns,rules.rows];
            for(int i = 0; i < rules.columns; i++){
                for(int j = 0; j < rules.rows; j++){
                    TileObject newTile = Instantiate(rules.tile, new Vector3((i - offset.x)* rules.padding,(-j + offset.y) * rules.padding,0), Quaternion.identity);
                    grid[i,j] = newTile;
                    newTile.gridPosition = new Vector2Int(i,j);
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
            if(rules.rows > rules.columns){
                Camera.main.orthographicSize = rules.columns + 2 + ((rules.rows - rules.columns) * 0.5f);
            }
            else{
                Camera.main.orthographicSize = rules.columns + 2;
            }
            //centers board to screen with offset.
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y + cameraOffset, -10);
        }

        void PlaceStarterPieces(){
            //Places red starter pieces
            for(int i = 0; i < rules.redStartTiles.Length; i++){
                var p = Instantiate(rules.piece, grid[rules.redStartTiles[i].x, rules.redStartTiles[i].y].transform.position, Quaternion.identity);
                p.ChangeTeam(Team.RedTeam);
                p.homeTile = grid[rules.redStartTiles[i].x, rules.redStartTiles[i].y];
                p.homeTile.piece = p;
            }
            //Places blue starter pieces
            for(int i = 0; i < rules.blueStartTiles.Length; i++){
                var p = Instantiate(rules.piece, grid[rules.blueStartTiles[i].x, rules.blueStartTiles[i].y].transform.position, Quaternion.identity);
                p.ChangeTeam(Team.GreenTeam);
                p.homeTile = grid[rules.blueStartTiles[i].x, rules.blueStartTiles[i].y];
                p.homeTile.piece = p;
            }

            gm.StartGame();


        }

    }
}

