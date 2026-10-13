using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cox.Infection.Management;
[UxmlElement]
public partial class GridGenControl : VisualElement
{
    GridTileControl[,] grid;
    VisualElement gridContainer;
    VisualElement descriptionBox;
    public Level selectedLevel;

    #region UI Builder Test Control Only
    int columns = 7, rows = 7; //These numbers are only for testing purposes

    /// <summary>
    /// This variant of the grid is meant to be used specifically
    /// in the UI Builder to offer an easy visual display for the code.
    /// </summary>
    public GridGenControl(){
        SetupStyle();
        //Generate grid
        grid = new GridTileControl[columns, rows];
        for(int i = 0; i < columns; i++){
            var columnContainer = new VisualElement();
            gridContainer.Add(columnContainer);
            columnContainer.style.flexDirection = FlexDirection.Column;
            for(int j = 0; j < rows; j++){
                var tile = new GridTileControl();
                columnContainer.Add(tile);
                grid[i,j] = tile;
                tile.locationOnGrid = new Vector2Int(i,j);
            }
        }
        //Add additional elements
        Label title = new Label("Test Title");
        title.AddToClassList("menuText");
        Label opponent = new Label("Opponent");
        opponent.AddToClassList("menuText2");
        Label firstMove = new Label("Gray First");
        firstMove.AddToClassList("menuText2");
        Label size = new Label(columns + "x" + rows);
        size.AddToClassList("menuText2");
        descriptionBox.Add(title);
        descriptionBox.Add(opponent);
        descriptionBox.Add(firstMove);
        descriptionBox.Add(size);
    }
    #endregion

    /// <summary>
    /// Control that creates a UI Grid from a given Level asset, 
    /// allowing the level layout and info to be displayed easily 
    /// in a UXML document.
    /// </summary>
    /// <param name="level"></param>
    public GridGenControl(Level level){
        selectedLevel = level;
        SetupStyle();

        grid = new GridTileControl[level.columns, level.rows];
        for (int i = 0; i < level.columns; i++) {
            var columnContainer = new VisualElement();
            gridContainer.Add(columnContainer);
            columnContainer.style.flexDirection = FlexDirection.Column;
            for (int j = 0; j < level.rows; j++) {
                var tile = new GridTileControl();
                columnContainer.Add(tile);
                grid[i, j] = tile;
                tile.locationOnGrid = new Vector2Int(i, j);
                foreach (var position in level.piecePositions) {
                    //assign the tile states based on level information
                    if (tile.locationOnGrid == new Vector2(position.x, position.y)) {
                        tile.SetState(position.z);
                    }
                }
            }
        }

        /*
        grid = new GridTileControl[level.columns, level.rows];
        for (int y = level.rows; y > level.rows; y--){
            //creates a row along each y, adds it to the grid container, and makes style adjustments
            var rowContainer = new VisualElement();
            gridContainer.Add(rowContainer);
            rowContainer.style.flexDirection = FlexDirection.Row;
            //for each spot in the row, make a tile and add it to the row, then assign its location
            for (int x = 0; x < level.columns; x++){
                var tile = new GridTileControl();
                rowContainer.Add(tile);
                grid[x,y] = tile;
                tile.locationOnGrid = new Vector2Int(x,y);
                foreach(var position in level.piecePositions){
                    //assign the tile states based on level information
                    if (tile.locationOnGrid == new Vector2(position.x, position.y)) {
                        tile.SetState(position.z);
                    }
                }
            }
        }*/

        //Add additional elements
        Label title = new Label(level.levelName);
        title.AddToClassList("menuText");
        Label opponent = new Label("Red Haze"); //add label to opponent?
        opponent.AddToClassList("menuText2");
        Label size = new Label(level.columns + "x" + level.rows);
        size.AddToClassList("menuText2");
        descriptionBox.Add(title);
        descriptionBox.Add(opponent); //I don't think this is implemented yet?
        descriptionBox.Add(size);
    }

    void SetupStyle(){
        gridContainer = new VisualElement();
        gridContainer.style.alignItems = Align.Center;
        gridContainer.style.flexDirection = FlexDirection.Row;
        Add(gridContainer);

        descriptionBox = new VisualElement();
        Add(descriptionBox);
        descriptionBox.style.flexDirection = FlexDirection.Column;
        descriptionBox.style.justifyContent = Justify.FlexEnd;
        
        AddToClassList("gridClass");
        focusable = true;
    }
}
