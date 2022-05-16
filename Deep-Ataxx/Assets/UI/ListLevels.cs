using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cox.Infection.Management;
using UnityEngine.UIElements;

public class ListLevels : MonoBehaviour
{
    public Level[] levelList;

    VisualElement root;
    VisualElement twoPlayerMenu;
    ScrollView twoPlayerList;

    GridGenControl[] gridSource;
    

    private void Awake() {
        root = GetComponent<UIDocument>().rootVisualElement;
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
            index++;
        }
        

    }
}
