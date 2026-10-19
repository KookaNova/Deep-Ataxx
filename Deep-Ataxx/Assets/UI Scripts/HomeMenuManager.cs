using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cox.Infection.Management;
using Cox.Infection.Utilities;

[UxmlElement]
public partial class HomeMenuManager : VisualElement
{

    public PlayerPersistentChoice data;

    VisualElement m_home;
    VisualElement m_story;
    VisualElement m_level_select;
    VisualElement m_arcade;
    VisualElement m_character_select;

    VisualElement back_destination;

    public HomeMenuManager(){
        this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        m_home = this?.Q("m-home");
        m_story = this?.Q("m-story");
        m_level_select = this?.Q("m-level-select");
        m_arcade = this?.Q("m-arcade");
        m_character_select = this?.Q("m-character-select");

        // UI Builder preview
        if (!Application.isPlaying)
        {
            return;
            // Ensure a sensible preview state so the UI Builder shows the main menu
            CloseAll();
            if (m_home != null) m_home.style.display = DisplayStyle.Flex;
            var bOptions = this.Q<Button>("b-options");
            if (bOptions != null) bOptions.style.display = DisplayStyle.Flex;
            var bBack = this.Q<Button>("b-back");
            if (bBack != null) bBack.style.display = DisplayStyle.None;
            return;
        }

        // At runtime register interactive callbacks.
        var bStory = m_home?.Q<Button>("b-story");
        var bMultiplayer = m_home?.Q<Button>("b-multiplayer");
        var bArcade = m_home?.Q<Button>("b-arcade");
        var bFirstMove = m_level_select?.Q<Button>("b-first-move");

        bStory?.RegisterCallback<ClickEvent>(ev => {
            if (data != null) { data.enableAI = true; data.selectedMode = PlayerPersistentChoice.Mode.story; }
            OpenMenu(m_story);
        });

        bMultiplayer?.RegisterCallback<ClickEvent>(ev => {
            if (data != null) { 
                data.enableAI = false; 
                data.selectedMode = PlayerPersistentChoice.Mode.twoPlayer; 
            }
            OpenMenu(m_level_select);
        });

        bArcade?.RegisterCallback<ClickEvent>(ev => {
            if (data != null) { data.enableAI = true; }
            OpenMenu(m_arcade);
        });

        m_arcade?.Q<Button>("b-arcade-classic").RegisterCallback<ClickEvent>(ev => {
            if (data != null) { data.selectedMode = PlayerPersistentChoice.Mode.arcadeClassic; }
            OpenMenu(m_character_select, m_arcade);
        });

        m_arcade?.Q<Button>("b-arcade-untimed").RegisterCallback<ClickEvent>(ev => {
            if (data != null) { data.selectedMode = PlayerPersistentChoice.Mode.arcadeUntimed; }
            OpenMenu(m_character_select, m_arcade);
        });

        m_character_select?.Q<Button>("b-continue").RegisterCallback<ClickEvent>(ev => { OpenMenu(m_level_select); });
        m_level_select?.Q<Button>("b-randomize").RegisterCallback<ClickEvent>(ev => { if (Application.isPlaying) data.SelectRandomLevel(); });
        m_level_select?.Q<Button>("b-play").RegisterCallback<ClickEvent>(ev => { if (Application.isPlaying) SceneLoader("Play"); });
        bFirstMove.text = data.isRedFirst ? "Red First" : "Green First";
        bFirstMove.style.color = data.isRedFirst ? new StyleColor(ColorManager.playerOne) : new StyleColor(ColorManager.playerTwo);
        bFirstMove.style.borderBottomColor = data.isRedFirst ? new StyleColor(ColorManager.playerOne) : new StyleColor(ColorManager.playerTwo);
        bFirstMove.style.borderLeftColor = data.isRedFirst ? new StyleColor(ColorManager.playerOne) : new StyleColor(ColorManager.playerTwo);
        bFirstMove.style.borderRightColor = data.isRedFirst ? new StyleColor(ColorManager.playerOne) : new StyleColor(ColorManager.playerTwo);
        bFirstMove.style.borderTopColor = data.isRedFirst ? new StyleColor(ColorManager.playerOne) : new StyleColor(ColorManager.playerTwo);

        bFirstMove?.RegisterCallback<ClickEvent>(ev => {
            data.isRedFirst = !data.isRedFirst;
            bFirstMove.text = data.isRedFirst ? "Red First" : "Green First";
            bFirstMove.style.color = data.isRedFirst ? new StyleColor(ColorManager.playerOne) : new StyleColor(ColorManager.playerTwo);
            bFirstMove.style.borderBottomColor = data.isRedFirst ? new StyleColor(ColorManager.playerOne) : new StyleColor(ColorManager.playerTwo);
            bFirstMove.style.borderLeftColor = data.isRedFirst ? new StyleColor(ColorManager.playerOne) : new StyleColor(ColorManager.playerTwo);
            bFirstMove.style.borderRightColor = data.isRedFirst ? new StyleColor(ColorManager.playerOne) : new StyleColor(ColorManager.playerTwo);
            bFirstMove.style.borderTopColor = data.isRedFirst ? new StyleColor(ColorManager.playerOne) : new StyleColor(ColorManager.playerTwo); ;
        });

        this?.Q<Button>("b-back").RegisterCallback<ClickEvent>(ev => OpenMenu(back_destination)); //universal back button
    }


    void CloseAll(){
        if (m_home != null) m_home.style.display = DisplayStyle.None;
        if (m_story != null) m_story.style.display = DisplayStyle.None;
        if (m_level_select != null) m_level_select.style.display = DisplayStyle.None;
        if (m_arcade != null) m_arcade.style.display = DisplayStyle.None;
        if (m_character_select != null) m_character_select.style.display = DisplayStyle.None;

        var bBack = this.Q<Button>("b-back");
        if (bBack != null) bBack.style.display = DisplayStyle.None;
        var bOptions = this.Q<Button>("b-options");
        if (bOptions != null) bOptions.style.display = DisplayStyle.None;

    }
    //This allows us to close all menus while also opening the selected menu. 
    //Meaning instead of a new function for each menu, we only need one.
    void OpenMenu(VisualElement menu, VisualElement backDestination){
        CloseAll();
        back_destination = backDestination;
        if (menu != null) menu.style.display = DisplayStyle.Flex;
        //turns on back button automatically where applicable.
        if(menu == m_home){
            var bOptions = this.Q<Button>("b-options"); if (bOptions != null) bOptions.style.display = DisplayStyle.Flex;
            var bBack = this.Q<Button>("b-back"); if (bBack != null) bBack.style.display = DisplayStyle.None;
        }
        else{
            var bOptions = this.Q<Button>("b-options"); if (bOptions != null) bOptions.style.display = DisplayStyle.None;
            var bBack = this.Q<Button>("b-back"); if (bBack != null) bBack.style.display = DisplayStyle.Flex;
        }
    }

    void OpenMenu(VisualElement menu){
        CloseAll();
        back_destination = m_home;
        if (menu != null) menu.style.display = DisplayStyle.Flex;
        //turns on back button automatically where applicable.
        if(menu == m_home){
            var bOptions = this.Q<Button>("b-options"); if (bOptions != null) bOptions.style.display = DisplayStyle.Flex;
            var bBack = this.Q<Button>("b-back"); if (bBack != null) bBack.style.display = DisplayStyle.None;
        }
        else{
            var bOptions = this.Q<Button>("b-options"); if (bOptions != null) bOptions.style.display = DisplayStyle.None;
            var bBack = this.Q<Button>("b-back"); if (bBack != null) bBack.style.display = DisplayStyle.Flex;
        }
    }

    void SceneLoader(string scene){
        var loadedScene = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);

    }
}
