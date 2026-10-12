using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cox.Infection.Management;

[UxmlElement]
public partial class HomeMenuManager : VisualElement
{

    public PlayerPersistentChoice data;

    VisualElement m_home;
    VisualElement m_story;
    VisualElement m_twoPlayer;
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
        m_twoPlayer = this?.Q("m-two-player");
        m_arcade = this?.Q("m-arcade");
        m_character_select = this?.Q("m-character-select");

        // UI Builder preview
        if (!Application.isPlaying)
        {
            //return;
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
        var bTwoPlayer = m_home?.Q<Button>("b-two-player");
        var bArcade = m_home?.Q<Button>("b-arcade");

        bStory?.RegisterCallback<ClickEvent>(ev => {
            if (data != null) { data.enableAI = true; data.selectedMode = PlayerPersistentChoice.Mode.story; }
            OpenMenu(m_story);
        });

        bTwoPlayer?.RegisterCallback<ClickEvent>(ev => {
            if (data != null) { data.enableAI = false; data.selectedMode = PlayerPersistentChoice.Mode.twoPlayer; }
            OpenMenu(m_twoPlayer);
        });

        bArcade?.RegisterCallback<ClickEvent>(ev => {
            if (data != null) { data.enableAI = true; }
            OpenMenu(m_arcade);
        });

        m_arcade?.Q<Button>("b-arcade-classic").RegisterCallback<ClickEvent>(ev => {
            if (data != null) { data.selectedMode = PlayerPersistentChoice.Mode.arcadeClassic; data.SelectRandomLevel(); }
            OpenMenu(m_character_select, m_arcade);
        });

        m_arcade?.Q<Button>("b-arcade-untimed").RegisterCallback<ClickEvent>(ev => {
            if (data != null) { data.selectedMode = PlayerPersistentChoice.Mode.arcadeUntimed; data.SelectRandomLevel(); }
            OpenMenu(m_character_select, m_arcade);
        });

        m_character_select?.Q<Button>("b-play").RegisterCallback<ClickEvent>(ev => { if (Application.isPlaying) SceneLoader("Play"); });
        m_twoPlayer?.Q<Button>("b-play").RegisterCallback<ClickEvent>(ev => { if (Application.isPlaying) SceneLoader("Play"); });

        this?.Q<Button>("b-back").RegisterCallback<ClickEvent>(ev => OpenMenu(back_destination)); //universal back button
    }


    void CloseAll(){
        if (m_home != null) m_home.style.display = DisplayStyle.None;
        if (m_story != null) m_story.style.display = DisplayStyle.None;
        if (m_twoPlayer != null) m_twoPlayer.style.display = DisplayStyle.None;
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
