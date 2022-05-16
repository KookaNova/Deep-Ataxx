using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
public class GridVector2Control : VisualElement
{
    GridTileControl[,] grid = new GridTileControl[0,0];
    VisualElement[] containers = new VisualElement[0];

    SliderInt columns;
    SliderInt rows;
    Button reset;
    VisualElement gridContainer;
    Label levelName;

    public SerializedObject so;
    List<GridTileControl> red, green, block;

    public new class UxmlFactory : UxmlFactory<GridVector2Control, UxmlTraits> { }

    public GridVector2Control(){
        gridContainer = new VisualElement();
        gridContainer.style.flexDirection = FlexDirection.Row;
        gridContainer.style.justifyContent = Justify.Center;
        gridContainer.style.width = Length.Percent(100);
        gridContainer.style.height = 130;
        gridContainer.style.marginTop = 5;
        gridContainer.style.marginBottom = 50;
        levelName = new Label();
        Add(levelName);

        Add(gridContainer);


        reset = new Button(() => ResetTiles());
        reset.text = "Reset";
        Add(reset);
        
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        //Find the starting state of the grid and display it.
        schedule.Execute(() => FindColumnsAndRows()).Until(() => columns != null && rows != null);
        
    }

    private void ResetTiles()
    {
        foreach(var tile in grid){
            tile.SetState(0);
        }
        CreateLists();
    }

    private void FindColumnsAndRows()
    {
        columns = parent?.Q<SliderInt>("Columns");
        rows = parent?.Q<SliderInt>("Rows");

        if(columns != null && rows != null && so != null){
           GenerateGrid(columns.value, rows.value);
        } 
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        columns = parent?.Q<SliderInt>("Columns");
        rows = parent?.Q<SliderInt>("Rows");
        columns?.RegisterCallback<ChangeEvent<int>>(ev => GenerateGrid(columns.value, rows.value));
        rows?.RegisterCallback<ChangeEvent<int>>(ev => GenerateGrid(columns.value, rows.value));

    }

    public void GenerateGrid(int _columns, int _rows){
        foreach(var container in containers){
            container.Clear(); //this is required to prevent warning about accidentally destroying objects
            container.RemoveFromHierarchy();
        }
        foreach(var tile in grid){
            tile.UnregisterCallback<ClickEvent>(ev => GridLeftClicked(tile));
            tile.UnregisterCallback<ContextClickEvent>(ev => GridRightClicked(tile));
            tile.RemoveFromHierarchy();
        }
        //generate  grid
        containers = new VisualElement[_columns];
        grid = new GridTileControl[_columns,_rows];
        for(int i = 0; i < _columns; i++){
            var column = new VisualElement();
            column.style.flexDirection = FlexDirection.Column;
            containers[i] = column;
            gridContainer.Add(column);
            for(int j = 0; j < _rows; j++){
                var tile = new GridTileControl();
                this.Add(tile);
                column.Add(tile);
                tile.gridPlace = new Vector2Int(i,j);
                grid[i,j] = tile;
                tile.RegisterCallback<ClickEvent>(ev => GridLeftClicked(tile));
                tile.RegisterCallback<ContextClickEvent>(ev => GridRightClicked(tile));
            }
        }
        SetTileStates();
    }

    void SetTileStates(){
        var redPositions = so.FindProperty("redPositions");
        var greenPositions = so.FindProperty("greenPositions");
        var blockPositions = so.FindProperty("blockPositions");

        for(int i = 0; i < redPositions.arraySize; i++){
            foreach(var tile in grid){
                if(redPositions.GetArrayElementAtIndex(i).vector2IntValue == tile.gridPlace){
                    tile.SetState(1);
                }
            }
        }
        for(int i = 0; i < greenPositions.arraySize; i++){
            foreach(var tile in grid){
                if(greenPositions.GetArrayElementAtIndex(i).vector2IntValue == tile.gridPlace){
                    tile.SetState(2);
                }
            }
        }
        for(int i = 0; i < blockPositions.arraySize; i++){
            foreach(var tile in grid){
                if(blockPositions.GetArrayElementAtIndex(i).vector2IntValue == tile.gridPlace){
                    tile.SetState(3);
                }
            }
        }
        CreateLists();
    }

    void GridLeftClicked(GridTileControl tile){
        tile.IncrementState();
        CreateLists();
    }
    void GridRightClicked(GridTileControl tile){
        tile.DecrementState();
        CreateLists();
    }

    void CreateLists(){
        red = new List<GridTileControl>();
        green = new List<GridTileControl>();
        block = new List<GridTileControl>();
        foreach(var item in grid){
            if(item.gridState == 0)continue;
            if(item.gridState == 1)red.Add(item);
            if(item.gridState == 2)green.Add(item);
            if(item.gridState == 3)block.Add(item);
        }
        ApplyGridChanges();
    }

    void ApplyGridChanges(){
        var redPositions = so.FindProperty("redPositions");
        redPositions.arraySize = red.Count;
        for(int i = 0; i < redPositions.arraySize; i++){
            redPositions.GetArrayElementAtIndex(i).vector2IntValue = red[i].gridPlace;
        }
        var greenPositions = so.FindProperty("greenPositions");
        greenPositions.arraySize = green.Count;
        for(int i = 0; i < greenPositions.arraySize; i++){
            greenPositions.GetArrayElementAtIndex(i).vector2IntValue = green[i].gridPlace;
        }
        var blockPositions = so.FindProperty("blockPositions");
        blockPositions.arraySize = block.Count;
        for(int i = 0; i < blockPositions.arraySize; i++){
            blockPositions.GetArrayElementAtIndex(i).vector2IntValue = block[i].gridPlace;
        }

        so.ApplyModifiedProperties();
    }
}
#endif
