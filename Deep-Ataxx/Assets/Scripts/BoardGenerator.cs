using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cox.Infection.Management{
    public class BoardGenerator : MonoBehaviour
    {
        GameManager gm;
        public PlayerPersistentChoice data;

        Level level;
        TileObject[,] grid;
        float timeDelay = 2f;
        const string alphabet = "abcdefghijklmnopqrstuvwxyz"; //create an alphabet for the notation

        private void Awake() {

            var root = FindAnyObjectByType<UIDocument>()?.rootVisualElement;
            level = data.selectedLevel;
            gm = FindAnyObjectByType<GameManager>();
            gm.data = data;
            gm.boardSize = new Vector2Int(data.selectedLevel.rows, data.selectedLevel.columns); //finds the max for these.
            timeDelay = 2f / (level.rows * level.columns);
            StartCoroutine(GenerateBoard());
        }

        private IEnumerator GenerateBoard(){
            PositionCamera();
            Vector2 offset = new Vector2(level.columns / 2, (level.rows / 2)+1); //offset the board to 0,0 in the scene, columns for the x, rows for y
            float padding = 1.27f;


            //generate the board
            grid = new TileObject[level.columns, level.rows];
            

            for (int y = 0; y < level.rows; y++){
                for(int x = 0; x < level.columns; x++){
                    yield return new WaitForSeconds(timeDelay);
                    TileObject newTile = Instantiate(level.tile, new Vector3((x - offset.x)* padding, (-y + offset.y)* padding, .25f), Quaternion.identity);
                    grid[x,y] = newTile;
                    newTile.gridPosition = new Vector2Int(x,y);
                    newTile.transform.SetParent(transform);
                    //Assigns a name with our battleship-like notation (letter, number)
                    char letter = alphabet[y];
                    newTile.name = letter.ToString() + x;
                }
            }
            
            //allow tiles to know about their neighbors after the grid is completed.
            foreach(var tile in grid){
                tile.FindNeighbors();
            }
            StartCoroutine(PlaceInitialPieces());
        }

        void PositionCamera(){
            //makes sure board is always on screen. 
            float zoom = 3.5f; //Higher number = Zoomed Out
            if (level.rows > level.columns){
                Camera.main.orthographicSize = level.columns + zoom;
            }
            else{
                Camera.main.orthographicSize = level.columns + zoom; 
            }


            //centers board to screen with offset.
            float cameraOffsetY = 2;
            Camera.main.transform.position = new Vector3(0, transform.position.y + cameraOffsetY, -10);
        }

        private IEnumerator PlaceInitialPieces(){
            //Places pieces
            List<Vector2Int> blocks = new List<Vector2Int>();
            for (int i = 0; i < level.piecePositions.Length; i++){
                yield return new WaitForSeconds(timeDelay*6.5f);
                //create piece is z value matches
                if (level.piecePositions[i].z < 3) {
                    var p = Instantiate(level.piece, grid[level.piecePositions[i].x, level.piecePositions[i].y].transform.position, Quaternion.identity);
                    p.SetPlayerId(level.piecePositions[i].z-1); //sets playerID and thus color
                    p.homeTile = grid[level.piecePositions[i].x, level.piecePositions[i].y]; //uses input level to place correctly
                    p.homeTile.piece = p;
                    p.name = p.homeTile.name + ".piece";
                }
                //if z is 2, place a block instead
                if (level.piecePositions[i].z == 3) {
                    grid[level.piecePositions[i].x, level.piecePositions[i].y].BlockTile(true);
                    blocks.Add(new Vector2Int(level.piecePositions[i].x, level.piecePositions[i].y));
                }
            }
            gm.blockedSpaces = blocks.ToArray();
            gm.StartGame();
        }

    }
}

