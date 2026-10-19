using UnityEngine;
using UnityEngine.UIElements;
using Cox.Infection.Management;
using Cox.Infection.Utilities;
using UnityEngine.SceneManagement;
using System;

[UxmlElement]
public partial class GameUIManager : VisualElement
{
    GameManager gm;

    VisualElement gameOver;
    VisualElement playScreen;
    StyleColor indicatorOff = new StyleColor(Color.darkSlateGray);

    public GameUIManager(){
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        gm = GameManager.FindAnyObjectByType<GameManager>();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        gameOver = this.Q("GameOver");
        playScreen = this.Q("PlayScreen");

        // In UI Builder / Editor preview the visual tree may be incomplete.
        // If playScreen is missing and we're not in Play mode, bail out safely.
        if (playScreen == null)
        {
            if (!Application.isPlaying) return;
            // If we're playing and playScreen is still null, avoid NRE by returning.
            return;
        }

        var rc = playScreen.Q("RC");
        if (rc != null) rc.style.backgroundColor = new StyleColor(ColorManager.playerOne);
        var gc = playScreen.Q("GC");
        if (gc != null) gc.style.backgroundColor = new StyleColor(ColorManager.playerTwo);

        var resetBtn = playScreen.Q<Button>("Reset");
        if (resetBtn != null) resetBtn.RegisterCallback<ClickEvent>(ev => Reset());
        var undoBtn = playScreen.Q<Button>("Undo");
        if (undoBtn != null)
        {
            undoBtn.UnregisterCallback<ClickEvent>(OnUndoClicked);
            undoBtn.RegisterCallback<ClickEvent>(OnUndoClicked);
        }
        RefreshUndoButton();
        var quitBtn = playScreen.Q<Button>("Quit");
        if (quitBtn != null) quitBtn.RegisterCallback<ClickEvent>(ev => Quit());
        this.Q<VisualElement>("turn-indicator-p1").style.backgroundColor = indicatorOff;
        this.Q<VisualElement>("turn-indicator-p2").style.backgroundColor = indicatorOff;
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

    private void OnUndoClicked(ClickEvent evt)
    {
        gm?.Undo();
    }

    public void RefreshUndoButton()
    {
        if (playScreen == null) return;

        var undoButton = playScreen.Q<Button>("Undo");
        if (undoButton != null)
        {
            bool canUndo = gm != null && gm.CanUndo;
            undoButton.SetEnabled(canUndo);
            undoButton.style.display = canUndo ? DisplayStyle.Flex : DisplayStyle.None;
        }

    }

    public void ShowPlayScreen()
    {
        if (playScreen == null) return;

        ActivateScreen(playScreen);
        RevealScore();
        ChangeTurn();
        RefreshUndoButton();
    }

    public void ChangeTurn(){
        if (gm.currentPlayerId == 0) {
            this.Q<Label>("bot-message").text = "Player 1";
            this.Q<VisualElement>("turn-indicator-p1").style.backgroundColor = new StyleColor(ColorManager.playerOne);
            this.Q<VisualElement>("turn-indicator-p2").style.backgroundColor = indicatorOff;

        }
        if (gm.currentPlayerId == 1) {
            this.Q<Label>("bot-message").text = "Player 2";
            this.Q<VisualElement>("turn-indicator-p2").style.backgroundColor = new StyleColor(ColorManager.playerTwo);
            this.Q<VisualElement>("turn-indicator-p1").style.backgroundColor = indicatorOff;
        }
    }

    public void UpdateScore(){
        this.Q<Label>("points-p1").text = gm.p1_Pieces.Count.ToString("00");
        this.Q<Label>("points-p2").text = gm.p2_Pieces.Count.ToString("00");

    }
    public void HideScore(){
        playScreen.Q("panel-score").style.visibility = Visibility.Hidden;
    }
    public void RevealScore(){
        playScreen.Q("panel-score").style.visibility = Visibility.Visible;
    }

    public void GameOver(string winner){
        ActivateScreen(gameOver);
        RevealScore();
        gameOver.Q<Label>("WinText").text = winner + " WINS.";
        gameOver.Q<Label>("final-score-p1").text = gm.p1_Pieces.Count.ToString("00");
        gameOver.Q<Label>("final-score-p2").text = gm.p2_Pieces.Count.ToString("00");
        gameOver.Q<Button>("Reset").RegisterCallback<ClickEvent>(ev => Reset());
        gameOver?.Q<Button>("ViewBoard").RegisterCallback<ClickEvent>(ev => {
            DeactivateAllScreens();
            ActivateScreen(playScreen);
            this.Q<Label>("ActivePlayer").text = winner + " WINS.";
            RefreshUndoButton();
        });
        gameOver.Q<Button>("Quit").RegisterCallback<ClickEvent>(ev => Quit());
    }

}
