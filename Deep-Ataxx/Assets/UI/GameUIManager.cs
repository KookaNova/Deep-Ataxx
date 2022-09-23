using UnityEngine.UIElements;
using Cox.Infection.Management;
using Cox.Infection.Utilities;
using UnityEngine.SceneManagement;
using System;

public class GameUIManager : VisualElement
{
    GameManager gm;

    VisualElement gameOver;
    VisualElement playScreen;

    public new class UxmlFactory : UxmlFactory<GameUIManager, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits{ }

    public GameUIManager(){
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        gm = GameManager.FindObjectOfType<GameManager>();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        gameOver = this.Q("GameOver");
        playScreen = this.Q("PlayScreen");
        playScreen.Q("RC").style.backgroundColor = new StyleColor(ColorManager.playerOne);
        playScreen.Q("GC").style.backgroundColor = new StyleColor(ColorManager.playerTwo);

        playScreen.Q<Button>("Reset").RegisterCallback<ClickEvent>(ev => Reset());
        playScreen.Q<Button>("Undo").RegisterCallback<ClickEvent>(ev => gm.Undo());
        playScreen.Q<Button>("Quit").RegisterCallback<ClickEvent>(ev => Quit());
    }

    void DeactivateAllScreens(){
        gameOver.style.display  = DisplayStyle.None;
        playScreen.style.display = DisplayStyle.None;
    }
    void ActivateScreen(VisualElement screen){
        DeactivateAllScreens();
        screen.style.display = DisplayStyle.Flex;
    }


    private void Reset()
    {
        UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void Quit(){
        UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
    }

    public void ChangeTurn(){
        if(gm.turnNumber == 0)this.Q<Label>("ActivePlayer").text = "Red's Turn";
        if(gm.turnNumber == 1)this.Q<Label>("ActivePlayer").text = "Green's Turn";
    }

    public void UpdateScore(){
        this.Q<Label>("RedCounter").text = gm.p1_Pieces.Count.ToString("00");
        this.Q<Label>("GreenCounter").text = gm.p2_Pieces.Count.ToString("00");
    }
    public void HideScore(){
        this.Q<Label>("ActivePlayer").style.visibility = Visibility.Hidden;
        playScreen.Q("RC").style.visibility = Visibility.Hidden;
        playScreen.Q("GC").style.visibility = Visibility.Hidden;
    }
    public void RevealScore(){
        this.Q<Label>("ActivePlayer").style.visibility = Visibility.Visible;
        playScreen.Q("RC").style.visibility = Visibility.Visible;
        playScreen.Q("GC").style.visibility = Visibility.Visible;
    }

    public void GameOver(string winner){
        ActivateScreen(gameOver);
        RevealScore();
        gameOver.Q<Label>("WinText").text = winner + " WINS.";
        gameOver.Q("Red").style.backgroundColor = new StyleColor(ColorManager.playerOne);
        gameOver.Q("Green").style.backgroundColor = new StyleColor(ColorManager.playerTwo);
        gameOver.Q<Label>("RedFinal").text = gm.p1_Pieces.Count.ToString("00");
        gameOver.Q<Label>("GreenFinal").text = gm.p2_Pieces.Count.ToString("00");
        gameOver.Q<Button>("Reset").RegisterCallback<ClickEvent>(ev => Reset());
        gameOver?.Q<Button>("ViewBoard").RegisterCallback<ClickEvent>(ev => {
            DeactivateAllScreens();
            ActivateScreen(playScreen);
            this.Q<Label>("ActivePlayer").text = winner + " WINS.";
            
            
        });
        gameOver.Q<Button>("Quit").RegisterCallback<ClickEvent>(ev => Quit());
    }

}
