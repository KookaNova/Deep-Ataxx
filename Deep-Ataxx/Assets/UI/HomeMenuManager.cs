using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Cox.Infection.Management;

public class HomeMenuManager : VisualElement
{

    public PlayerPersistentChoice data;

    VisualElement m_home;
    VisualElement m_story;
    VisualElement m_twoPlayer;
    VisualElement m_arcade;
    VisualElement m_character_select;

    VisualElement back_destination;

    public new class UxmlFactory : UxmlFactory<HomeMenuManager, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits{ }

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

        m_home?.Q<Button>("b-story").RegisterCallback<ClickEvent>(ev => {
            data.enableAI = true;
            data.selectedMode = PlayerPersistentChoice.Mode.story;
            OpenMenu(m_story);
            });
        m_home?.Q<Button>("b-two-player").RegisterCallback<ClickEvent>(ev => {
            data.enableAI = false;
            data.selectedMode = PlayerPersistentChoice.Mode.twoPlayer;
            OpenMenu(m_twoPlayer);
            });
        m_home?.Q<Button>("b-arcade").RegisterCallback<ClickEvent>(ev => {
            data.enableAI = true;
            OpenMenu(m_arcade);
            });

        m_arcade?.Q<Button>("b-arcade-classic").RegisterCallback<ClickEvent>(ev => {
            data.selectedMode = PlayerPersistentChoice.Mode.arcadeClassic;
            data.SelectRandomLevel();
            OpenMenu(m_character_select, m_arcade);
            });
        m_arcade?.Q<Button>("b-arcade-untimed").RegisterCallback<ClickEvent>(ev => {
            data.selectedMode = PlayerPersistentChoice.Mode.arcadeUntimed;
            data.SelectRandomLevel();
            OpenMenu(m_character_select, m_arcade);
            });

        m_character_select?.Q<Button>("b-play").RegisterCallback<ClickEvent>(ev => SceneLoader("Play"));
 
        m_twoPlayer?.Q<Button>("b-play").RegisterCallback<ClickEvent>(ev => SceneLoader("Play"));

        this?.Q<Button>("b-back").RegisterCallback<ClickEvent>(ev => OpenMenu(back_destination)); //universal back button
    }


    void CloseAll(){
        m_home.style.display = DisplayStyle.None;
        m_story.style.display = DisplayStyle.None;
        m_twoPlayer.style.display = DisplayStyle.None;
        m_arcade.style.display = DisplayStyle.None;
        m_character_select.style.display = DisplayStyle.None;

        this.Q<Button>("b-back").style.display = DisplayStyle.None;
        this.Q<Button>("b-options").style.display = DisplayStyle.None;

    }
    //This allows us to close all menus while also opening the selected menu. 
    //Meaning instead of a new function for each menu, we only need one.
    void OpenMenu(VisualElement menu, VisualElement backDestination){
        CloseAll();
        back_destination = backDestination;
        menu.style.display = DisplayStyle.Flex;
        //turns on back button automatically where applicable.
        if(menu == m_home){
            this.Q<Button>("b-options").style.display = DisplayStyle.Flex;
            this.Q<Button>("b-back").style.display = DisplayStyle.None;
        }
        else{
            this.Q<Button>("b-options").style.display = DisplayStyle.None;
            this.Q<Button>("b-back").style.display = DisplayStyle.Flex;
        }
    }

    void OpenMenu(VisualElement menu){
        CloseAll();
        back_destination = m_home;
        menu.style.display = DisplayStyle.Flex;
        //turns on back button automatically where applicable.
        if(menu == m_home){
            this.Q<Button>("b-options").style.display = DisplayStyle.Flex;
            this.Q<Button>("b-back").style.display = DisplayStyle.None;
        }
        else{
            this.Q<Button>("b-options").style.display = DisplayStyle.None;
            this.Q<Button>("b-back").style.display = DisplayStyle.Flex;
        }
    }

    void SceneLoader(string scene){
        var loadedScene = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);

    }
}
