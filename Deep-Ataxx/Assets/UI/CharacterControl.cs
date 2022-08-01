using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cox.Infection.Management;

public class CharacterControl : VisualElement
{
    VisualElement pictureContainer;
    VisualElement descriptionBox;

    int columns = 7, rows = 7; //These are only for testing purposes
    public new class UxmlFactory : UxmlFactory<CharacterControl> { }

    /// <summary>
    /// This variant of the grid is meant to be used specifically
    /// in the UI Builder to offer an easy visual display for the code.
    /// </summary>
    public CharacterControl(){
        SetupStyle();
        //Add additional elements
        VisualElement pic = new VisualElement();
        pic.style.backgroundColor = new StyleColor(Color.cyan);
        pic.style.flexGrow = 1;
        pic.style.height = new StyleLength(Length.Percent(100));
        Label charName = new Label("Test Name");
        charName.AddToClassList("menuText");
        Label difficulty = new Label("Diff");
        difficulty.AddToClassList("menuText2");
        Label firstMove = new Label("Moves First");
        firstMove.AddToClassList("menuText2");
        descriptionBox.Add(charName);
        descriptionBox.Add(difficulty);
        descriptionBox.Add(firstMove);
        pictureContainer.Add(pic);
    }

    public CharacterControl(CharacterObject character){
        SetupStyle();
        //Add additional elements
        VisualElement pic = new VisualElement();
        pic.style.backgroundImage = character.art;
        pic.style.flexGrow = 1;
        pic.style.height = new StyleLength(Length.Percent(100));
        pic.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        Label charName = new Label(character.name);
        charName.AddToClassList("menuText");
        Label difficulty = new Label(character.thinkDepth.ToString());
        difficulty.AddToClassList("menuText2");
        Label firstMove = new Label("Moves First");
        firstMove.AddToClassList("menuText2");
        descriptionBox.Add(charName);
        descriptionBox.Add(difficulty);
        descriptionBox.Add(firstMove);
        pictureContainer.Add(pic);
    }

    void SetupStyle(){
        pictureContainer = new VisualElement();
        pictureContainer.style.alignItems = Align.Center;
        pictureContainer.AddToClassList("characterIcon");
        Add(pictureContainer);
        descriptionBox = new VisualElement();
        Add(descriptionBox);
        descriptionBox.style.flexDirection = FlexDirection.Column;
        descriptionBox.style.justifyContent = Justify.FlexEnd;
        AddToClassList("gridClass");
        pictureContainer.style.flexDirection = FlexDirection.Row;
        focusable = true;
    }
}
