using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cox.Infection.Management;

public class GridVector2Control : VisualElement
{
    GridTileControl[,] grid = new GridTileControl[0,0];
    VisualElement[] containers = new VisualElement[0];

    SliderInt columns;
    SliderInt rows;

    public new class UxmlFactory : UxmlFactory<GridVector2Control, UxmlTraits> { }

    public GridVector2Control(){
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        columns = parent?.Q<SliderInt>("Columns");
        rows = parent?.Q<SliderInt>("Columns");
        columns?.RegisterCallback<ChangeEvent<int>>(ev => GenerateGrid(columns.value, rows.value));
        rows?.RegisterCallback<ChangeEvent<int>>(ev => GenerateGrid(columns.value, rows.value));

    }

    public void GenerateGrid(int _columns, int _rows){

        foreach(var container in  containers){
            container.RemoveFromHierarchy();
        }
        foreach(var tile in grid){
            tile.RemoveFromHierarchy();
        }
        containers = new VisualElement[_columns];
        grid = new GridTileControl[_columns,_rows];
        for(int i = 0; i < _columns; i++){
            var column = new VisualElement();
            column.style.flexDirection = FlexDirection.Column;
            containers[i] = column;
            Add(column);
            for(int j = 0; j < _rows; j++){
                var tile = new GridTileControl();
                this.Add(tile);
                column.Add(tile);
                grid[i,j] = tile;
            }
        }

        Debug.Log(containers.Length);
    }
}
