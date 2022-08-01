using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cox.Infection.Management;
using UnityEngine.UIElements;

public class ListMenuData : MonoBehaviour
{
    [SerializeField] PlayerPersistantChoice data;
    public Level[] levelList;
    public CharacterObject[] characterList;

    VisualElement root;

    VisualElement twoPlayerMenu;
    ScrollView twoPlayerList;
    GridGenControl[] gridSource;

    VisualElement m_character_select;
    ScrollView arcadeCharacterList;
    

    private void Awake() {
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
        gridSource = new  GridGenControl[levelList.Length];
        foreach(Level level in levelList){
            var grid = new GridGenControl(level);
            gridSource[index] = grid;
            twoPlayerList.Add(grid);
            grid.RegisterCallback<FocusEvent>(ev => data.selectedLevel = grid.selectedLevel);
            index++;
        }
    }

    private void CreateCharacterList()
    {
        int index = 0;
        foreach(CharacterObject character in characterList){
            var control = new CharacterControl(character);
            arcadeCharacterList.Add(control);
            index++;
        }
    }
}
