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

    public new class UxmlFactory : UxmlFactory<GridVector2Control, UxmlTraits> { }

    public GridVector2Control(){
        
    }

    public void GenerateGrid(int columns, int rows){

        foreach(var container in  containers){
            container.RemoveFromHierarchy();
        }
        foreach(var tile in grid){
            tile.RemoveFromHierarchy();
        }
        containers = new VisualElement[columns];
        grid = new GridTileControl[columns,rows];
        for(int i = 0; i < columns; i++){
            var column = new VisualElement();
            column.style.flexDirection = FlexDirection.Column;
            containers[i] = column;
            for(int j = 0; j < rows; j++){
                var tile = new GridTileControl();
                Add(tile);
                column.Add(tile);
                grid[i,j] = tile;
            }
        }
    }
}
