using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cox.Infection.Utilities;

public class GridTileControl : VisualElement
{
    public Vector2Int gridPlace;
    public int gridState = 0; //0 clear, 1 red, 2 green, 3 blocked;

    public new class UxmlFactory : UxmlFactory<GridTileControl> { }

    public GridTileControl(){
        style.backgroundColor = new StyleColor(Color.black);
        style.width = new StyleLength(10);
        style.height = new StyleLength(10);
        style.marginBottom = new StyleLength(1);
        style.marginLeft = new StyleLength(1);
        style.marginRight = new StyleLength(1);
        style.marginTop = new StyleLength(1);


    }

    public void IncrementState(){
        gridState++;
        AssignColor();
    }
    public void DecrementState(){
        gridState--;
        AssignColor();
    }
    public void SetState(int state){
        gridState = state;
        AssignColor();
        
    }

    void AssignColor(){
        if(gridState > 3){
            gridState = 0;
        }
        if(gridState < 0){
            gridState = 3;
        }

        if(gridState == 0){
            style.backgroundColor = new StyleColor(Color.black);
        }
        else if(gridState == 1){
            style.backgroundColor = new StyleColor(ColorManager.playerOne);
        }
        else if(gridState == 2){
            style.backgroundColor = new StyleColor(ColorManager.playerTwo);
        }
        else if(gridState == 3){
            style.backgroundColor = new StyleColor(Color.blue);
        }
    }
    
}
