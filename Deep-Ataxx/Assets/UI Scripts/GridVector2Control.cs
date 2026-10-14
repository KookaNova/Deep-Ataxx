using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
[UxmlElement]
public partial class GridVector2Control : VisualElement
{
    GridTileControl[,] grid = new GridTileControl[0,0];
    VisualElement[] containers = new VisualElement[0];

    SliderInt columns;
    SliderInt rows;
    Button reset;
    VisualElement gridContainer;
    Label levelName;

    public SerializedObject serializedObject;
    List<GridTileControl> red, green, block;

    /// <summary>
    /// Constructor that builds the runtime UI for the grid editor control.
    /// It creates the visual container and label, adds a Reset button,
    /// registers geometry/layout callbacks, and schedules an initial lookup
    /// for the Columns and Rows slider controls so the grid can be generated
    /// to match the serialized Level data (via the SerializedObject `so`).
    /// </summary>
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

        reset = new Button(() => ResetTiles()); //creates a reset button and assigns the ResetTiles method to its click event
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

        if(columns != null && rows != null && serializedObject != null){
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
        //generate grid
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
                tile.locationOnGrid = new Vector2Int(i,j);
                grid[i,j] = tile;
                tile.RegisterCallback<ClickEvent>(ev => GridLeftClicked(tile));
                tile.RegisterCallback<ContextClickEvent>(ev => GridRightClicked(tile));
            }
        }
        SetTileStates();
    }

    void SetTileStates(){
        SerializedProperty piecePositions = serializedObject.FindProperty("piecePositions");

        for(int i = 0; i < piecePositions?.arraySize; i++){
            foreach(var tile in grid){
                Vector3Int piecePlace = piecePositions.GetArrayElementAtIndex(i).vector3IntValue;
                if ( new Vector2Int(piecePlace.x, piecePlace.y) == tile.locationOnGrid){
                    tile.SetState(piecePlace.z);
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
        foreach(var cell in grid){
            if(cell.gridState == 0)continue;
            if(cell.gridState == 1)red.Add(cell);
            if(cell.gridState == 2)green.Add(cell);
            if(cell.gridState == 3)block.Add(cell);
        }
        ApplyGridChanges();
    }

    void ApplyGridChanges(){
        SerializedProperty piecePositions = serializedObject.FindProperty("piecePositions");

        int total = red.Count + green.Count + block.Count;
        piecePositions.arraySize = total;

        int i = 0;
        if (red != null) {
            foreach (var t in red) {
                piecePositions.GetArrayElementAtIndex(i).vector3IntValue = new Vector3Int(t.locationOnGrid.x, t.locationOnGrid.y, 1);
                i++;
            }
        }
        if (green != null) {
            foreach (var t in green) {
                piecePositions.GetArrayElementAtIndex(i).vector3IntValue = new Vector3Int(t.locationOnGrid.x, t.locationOnGrid.y, 2);
                i++;
            }
        }
        if (block != null) {
            foreach (var t in block) {
                piecePositions.GetArrayElementAtIndex(i).vector3IntValue = new Vector3Int(t.locationOnGrid.x, t.locationOnGrid.y, 3);
                i++;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
