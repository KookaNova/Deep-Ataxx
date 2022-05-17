using UnityEngine;
using UnityEngine.UIElements;

namespace Cox.Infection.Management{
    public class BoardGenerator : MonoBehaviour
    {
        GameManager gm;
        public Level level;

        Vector2 offset = Vector2.zero;
        TileObject[,] grid;
        float cameraOffset = 2;

        

        private void Awake() {
            var root = FindObjectOfType<UIDocument>().rootVisualElement;
            root.Q<Button>("Flag").RegisterCallback<ClickEvent>(ev => level.isFlagged = !level.isFlagged);
            gm = FindObjectOfType<GameManager>();
            level = FindObjectOfType<PersistentData>().selectedLevel;
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
                    if(level.blockPositions != null){
                        foreach (var blockedPosition in level.blockPositions){
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
            for(int i = 0; i < level.redPositions.Length; i++){
                var p = Instantiate(level.piece, grid[level.redPositions[i].x, level.redPositions[i].y].transform.position, Quaternion.identity);
                p.ChangeTeam(Team.RedTeam);
                p.homeTile = grid[level.redPositions[i].x, level.redPositions[i].y];
                p.homeTile.piece = p;
            }
            //Places blue starter pieces
            for(int i = 0; i < level.greenPositions.Length; i++){
                var p = Instantiate(level.piece, grid[level.greenPositions[i].x, level.greenPositions[i].y].transform.position, Quaternion.identity);
                p.ChangeTeam(Team.GreenTeam);
                p.homeTile = grid[level.greenPositions[i].x, level.greenPositions[i].y];
                p.homeTile.piece = p;
            }

            gm.StartGame();


        }

    }
}

