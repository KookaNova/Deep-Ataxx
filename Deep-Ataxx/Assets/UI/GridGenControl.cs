using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cox.Infection.Management;

public class GridGenControl : VisualElement
{
    GridTileControl[,] grid;
    VisualElement gridContainer;
    VisualElement descriptionBox;
    public Level selectedLevel;

    #region UI Builder Test Control Only
    int columns = 7, rows = 7; //These are only for testing purposes
    public new class UxmlFactory : UxmlFactory<GridGenControl> { }

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
                tile.gridPlace = new Vector2Int(i,j);
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
        //Generate grid
        grid = new GridTileControl[level.columns, level.rows];
        for(int i = 0; i < level.columns; i++){
            var columnContainer = new VisualElement();
            gridContainer.Add(columnContainer);
            columnContainer.style.flexDirection = FlexDirection.Column;
            for(int j = 0; j < level.rows; j++){
                var tile = new GridTileControl();
                columnContainer.Add(tile);
                grid[i,j] = tile;
                tile.gridPlace = new Vector2Int(i,j);

                foreach(var position in level.p1_Positions){
                    if(tile.gridPlace == position)tile.SetState(1);
                }
                foreach(var position in level.p2_Positions){
                    if(tile.gridPlace == position)tile.SetState(2);
                }
                foreach(var position in level.block_Positions){
                    if(tile.gridPlace == position)tile.SetState(3);
                }
            }
        }
        //Add additional elements
        Label title = new Label(level.levelName);
        title.AddToClassList("menuText");
        Label opponent = new Label("Red Haze");
        opponent.AddToClassList("menuText2");
        Label firstMove = new Label();
        if(level.redFirst)firstMove.text = "Red First";
        if(!level.redFirst)firstMove.text = "Green First";
        Label size = new Label(level.columns + "x" + level.rows);
        size.AddToClassList("menuText2");
        firstMove.AddToClassList("menuText2");
        descriptionBox.Add(title);
        descriptionBox.Add(opponent);
        descriptionBox.Add(firstMove);
        descriptionBox.Add(size);

        //is the level flagged?
        if(level.isFlagged){
            VisualElement yellowFlag = new VisualElement();
            yellowFlag.style.width = 20;
            yellowFlag.style.height = 20;
            yellowFlag.style.marginRight = 10;
            yellowFlag.style.backgroundColor = new StyleColor(Color.yellow);
            gridContainer.Add(yellowFlag);
            yellowFlag.SendToBack();

        }
    }

    void SetupStyle(){
        gridContainer = new VisualElement();
        gridContainer.style.alignItems = Align.Center;
        Add(gridContainer);
        descriptionBox = new VisualElement();
        Add(descriptionBox);
        descriptionBox.style.flexDirection = FlexDirection.Column;
        descriptionBox.style.justifyContent = Justify.FlexEnd;
        AddToClassList("gridClass");
        gridContainer.style.flexDirection = FlexDirection.Row;
        focusable = true;
    }
}
