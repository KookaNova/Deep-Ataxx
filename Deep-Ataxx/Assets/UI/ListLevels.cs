using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cox.Infection.Management;
using UnityEngine.UIElements;

public class ListLevels : MonoBehaviour
{
    [SerializeField] PlayerPersistantChoice data;
    public Level[] levelList;

    VisualElement root;
    VisualElement twoPlayerMenu;
    ScrollView twoPlayerList;
    GridGenControl[] gridSource;
    

    private void Awake() {
        root = FindObjectOfType<UIDocument>().rootVisualElement;
        twoPlayerMenu = root?.Q("TwoPlayerMenu");
        twoPlayerList = twoPlayerMenu.Q<ScrollView>("2PLevelList");

        CreateLevelList();
    }

    public void CreateLevelList(){
        int index = 0;
        gridSource = new  GridGenControl[levelList.Length];
        foreach(Level level in levelList){
            var grid = new GridGenControl(level);
            gridSource[index] = grid;
            twoPlayerList.Add(grid);
            grid.RegisterCallback<FocusEvent>(ev => data.selectedLevel = grid.selectedLevel);
            index++;
        }
    }
}
