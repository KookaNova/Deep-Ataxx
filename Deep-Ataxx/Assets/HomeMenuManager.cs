using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class HomeMenuManager : VisualElement
{
    VisualElement HomeMenu;
    VisualElement PlayMenu;


    public new class UxmlFactory : UxmlFactory<HomeMenuManager, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits{ }

    public HomeMenuManager(){
        this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

        
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        HomeMenu = this?.Q("HomeMenu");
        PlayMenu = this?.Q("PlayMenu");
        this?.Q<Button>("Debug").RegisterCallback<ClickEvent>(ev => SceneManager.LoadScene("SampleScene"));
        this?.Q<Button>("Story").RegisterCallback<ClickEvent>(ev => OpenMenu(PlayMenu));
        this?.Q<Button>("Back").RegisterCallback<ClickEvent>(ev => OpenMenu(HomeMenu));
    }

    void CloseAll(){
        HomeMenu.style.display = DisplayStyle.None;
        PlayMenu.style.display = DisplayStyle.None;

    }

    void OpenMenu(VisualElement menu){
        CloseAll();
        menu.style.display = DisplayStyle.Flex;
    }
}
