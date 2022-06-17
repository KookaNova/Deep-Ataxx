using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class HomeMenuManager : VisualElement
{
    VisualElement HomeMenu;
    VisualElement PlayMenu;
    VisualElement TwoPlayerMenu;


    public new class UxmlFactory : UxmlFactory<HomeMenuManager, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits{ }

    public HomeMenuManager(){
        this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        HomeMenu = this?.Q("HomeMenu");
        PlayMenu = this?.Q("PlayMenu");
        TwoPlayerMenu = this?.Q("TwoPlayerMenu");
        this?.Q<Button>("Story").RegisterCallback<ClickEvent>(ev => OpenMenu(PlayMenu));
        this?.Q<Button>("TwoPlayer").RegisterCallback<ClickEvent>(ev => OpenMenu(TwoPlayerMenu));
        this?.Q<Button>("Back").RegisterCallback<ClickEvent>(ev => OpenMenu(HomeMenu));

        TwoPlayerMenu?.Q<Button>("Play").RegisterCallback<ClickEvent>(ev => SceneLoader("Play"));
    }

    void CloseAll(){
        HomeMenu.style.display = DisplayStyle.None;
        PlayMenu.style.display = DisplayStyle.None;
        TwoPlayerMenu.style.display = DisplayStyle.None;

        this.Q<Button>("Back").style.display = DisplayStyle.None;
        this.Q<Button>("Options").style.display = DisplayStyle.None;

    }
    //This allows us to close all menus while also opening the selected menu. 
    //Meaning instead of a new function for each menu, we only need one.
    void OpenMenu(VisualElement menu){
        CloseAll();
        menu.style.display = DisplayStyle.Flex;
        //turns on back button automatically where applicable.
        if(menu == HomeMenu){
            this.Q<Button>("Options").style.display = DisplayStyle.Flex;
            this.Q<Button>("Back").style.display = DisplayStyle.None;
        }
        else{
            this.Q<Button>("Options").style.display = DisplayStyle.None;
            this.Q<Button>("Back").style.display = DisplayStyle.Flex;
        }
    }

    void SceneLoader(string scene){
        var loadedScene = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);

    }
}
