using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cox.Infection.Management;
using UnityEngine.UIElements;

public class ListMenuData : MonoBehaviour
{
    [SerializeField] PlayerPersistentChoice data;
    [SerializeField] Level[] arcadeLevels; // used for arcade mode and multiplayer
    [SerializeField] CharacterObject[] characterList;
    [SerializeField] LevelPlaylist[] zones;

    VisualElement root;

    VisualElement twoPlayerMenu;
    ScrollView twoPlayerList;

    VisualElement m_character_select;
    ScrollView arcadeCharacterList;
    

    private void Awake() {
        data.levelList = arcadeLevels;
        data.characterList = characterList;

        root = FindAnyObjectByType<UIDocument>().rootVisualElement;
        root.Q<HomeMenuManager>().data = data;

        twoPlayerMenu = root?.Q("m-two-player");
        twoPlayerList = twoPlayerMenu.Q<ScrollView>("list-levels");
        if (Application.isEditor) {
            twoPlayerList.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
        }
        root?.Q<Button>("b-options").RegisterCallback<ClickEvent>(ev => data.enableAI = !data.enableAI);
        twoPlayerList.Focus();

        CreateLevelList();

        m_character_select = root?.Q("m-character-select");
        arcadeCharacterList = m_character_select.Q<ScrollView>("list-characters");

        CreateCharacterList();
    }

    public void CreateLevelList() {
        int i = 0;
        GridGenControl[] gridSource = new GridGenControl[arcadeLevels.Length];
        foreach(Level level in arcadeLevels){
            var grid = new GridGenControl(level); //generate the control
            gridSource[i] = grid; //add it to array
            twoPlayerList.Add(grid); //add the control to the ScrollView
            grid.RegisterCallback<FocusEvent>(ev => data.selectedLevel = grid.selectedLevel);
            grid.RegisterCallback<ClickEvent>(ev => data.selectedLevel = grid.selectedLevel);
            i++;
        }
    }

    private void CreateCharacterList()
    {
        /*int index = 0;
        foreach(CharacterObject character in characterList){
            var control = new CharacterControl(character);
            arcadeCharacterList.Add(control);
            control.RegisterCallback<FocusEvent>(ev => data.opponent = character);
            control.RegisterCallback<ClickEvent>(ev => data.opponent = character);
            index++;
        }*/
    }

    private void CreateZoneList(){
        
    }
}
