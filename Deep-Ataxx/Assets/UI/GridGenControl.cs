using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cox.Infection.Management;

public class GridGenControl : VisualElement
{
    public  int columns = 7, rows = 7;
    GridTileControl[,] grid;

    public new class UxmlFactory : UxmlFactory<GridGenControl> { }

    public GridGenControl(){
        SetupStyle();
        //Generate grid
        grid = new GridTileControl[columns, rows];
        for(int i = 0; i < columns; i++){
            var columnContainer = new VisualElement();
            Add(columnContainer);
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
        Add(title);
    }

    public GridGenControl(int _columns, int _rows, string title){
        SetupStyle();
        //Generate grid
        grid = new GridTileControl[_columns, _rows];
        for(int i = 0; i < _columns; i++){
            var columnContainer = new VisualElement();
            Add(columnContainer);
            columnContainer.style.flexDirection = FlexDirection.Column;
            for(int j = 0; j < _rows; j++){
                var tile = new GridTileControl();
                columnContainer.Add(tile);
                grid[i,j] = tile;
                tile.gridPlace = new Vector2Int(i,j);

            }
        }
        //Add additional elements
        Label _title = new Label(title);
        Add(_title);
    }

    public GridGenControl(Level level){
        SetupStyle();
        //Generate grid
        grid = new GridTileControl[level.columns, level.rows];
        for(int i = 0; i < level.columns; i++){
            var columnContainer = new VisualElement();
            Add(columnContainer);
            columnContainer.style.flexDirection = FlexDirection.Column;
            for(int j = 0; j < level.rows; j++){
                var tile = new GridTileControl();
                columnContainer.Add(tile);
                grid[i,j] = tile;
                tile.gridPlace = new Vector2Int(i,j);

                foreach(var position in level.redPositions){
                    if(tile.gridPlace == position)tile.SetState(1);
                }
                foreach(var position in level.greenPositions){
                    if(tile.gridPlace == position)tile.SetState(2);
                }
                foreach(var position in level.blockPositions){
                    if(tile.gridPlace == position)tile.SetState(3);
                }
            }
        }
        //Add additional elements
        Label title = new Label(level.name);
        Add(title);
    }

    void SetupStyle(){
        AddToClassList("gridClass");
        style.flexDirection = FlexDirection.Row;
        /*
        style.justifyContent = Justify.Center;
        
        style.marginBottom = 5;
        style.marginLeft = 5;
        style.marginRight = 5;
        style.marginTop = 5;*/
    }
}
