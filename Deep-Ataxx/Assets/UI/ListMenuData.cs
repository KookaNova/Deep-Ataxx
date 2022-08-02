using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cox.Infection.Management;
using UnityEngine.UIElements;

public class ListMenuData : MonoBehaviour
{
    [SerializeField] PlayerPersistentChoice data;
    public Level[] arcadeLevels; //used for arcade mode
    public CharacterObject[] characterList;
    public LevelPlaylist[] zones;

    VisualElement root;

    VisualElement twoPlayerMenu;
    ScrollView twoPlayerList;
    GridGenControl[] gridSource;

    VisualElement m_character_select;
    ScrollView arcadeCharacterList;
    

    private void Awake() {
        data.levelList = arcadeLevels;
        data.characterList = characterList;

        root = FindObjectOfType<UIDocument>().rootVisualElement;
        root.Q<HomeMenuManager>().data = data;

        twoPlayerMenu = root?.Q("m-two-player");
        twoPlayerList = twoPlayerMenu.Q<ScrollView>("list-levels");
        root?.Q<Button>("b-options").RegisterCallback<ClickEvent>(ev => data.enableAI = !data.enableAI);

        CreateLevelList();

        m_character_select = root?.Q("m-character-select");
        arcadeCharacterList = m_character_select.Q<ScrollView>("list-characters");

        CreateCharacterList();
    }

    public void CreateLevelList(){
        int index = 0;
        gridSource = new  GridGenControl[arcadeLevels.Length];
        foreach(Level level in arcadeLevels){
            var grid = new GridGenControl(level);
            gridSource[index] = grid;
            twoPlayerList.Add(grid);
            grid.RegisterCallback<FocusEvent>(ev => data.selectedLevel = grid.selectedLevel);
            grid.RegisterCallback<ClickEvent>(ev => data.selectedLevel = grid.selectedLevel);
            index++;
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
