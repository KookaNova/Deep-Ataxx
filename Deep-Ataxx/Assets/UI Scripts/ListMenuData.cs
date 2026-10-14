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

    VisualElement levelSelectMenu;
    ScrollView levelList;

    VisualElement m_character_select;
    ScrollView arcadeCharacterList;
    

    private void Awake() {
        data.levelList = arcadeLevels;
        data.characterList = characterList;

        root = FindAnyObjectByType<UIDocument>().rootVisualElement;
        root.Q<HomeMenuManager>().data = data;

        levelSelectMenu = root?.Q("m-level-select");
        levelList = levelSelectMenu.Q<ScrollView>("list-levels");
        if (Application.isEditor) {
            levelList.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
        }
        root?.Q<Button>("b-options").RegisterCallback<ClickEvent>(ev => data.enableAI = !data.enableAI);
        levelList.Focus();

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
            levelList.Add(grid); //add the control to the ScrollView
            grid.RegisterCallback<FocusEvent>(ev => data.selectedLevel = grid.selectedLevel);
            grid.RegisterCallback<ClickEvent>(ev => data.selectedLevel = grid.selectedLevel);

            i++;
        }
    }

    private void CreateCharacterList()
    {
        int index = 0;
        foreach(CharacterObject character in characterList){
            var control = new CharacterControl(character);
            arcadeCharacterList.Add(control);
            control.RegisterCallback<FocusEvent>(ev => data.opponent = character);
            control.RegisterCallback<ClickEvent>(ev => data.opponent = character);
            index++;
        }
    }

    private void CreateZoneList(){
        
    }
}
