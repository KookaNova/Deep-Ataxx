using UnityEngine.UIElements;
using Cox.Infection.Management;
using Cox.Infection.Utilities;
using System;
using UnityEngine.SceneManagement;

public class GameUIManager : VisualElement
{
    GameManager gm;

    public new class UxmlFactory : UxmlFactory<GameUIManager, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits{ }

    public GameUIManager(){
        this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        gm = GameManager.FindObjectOfType<GameManager>();

        
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        this.Q("RC").style.backgroundColor = new StyleColor(ColorManager.red);
        this.Q("GC").style.backgroundColor = new StyleColor(ColorManager.green);

        this.Q<Button>("Reset").RegisterCallback<ClickEvent>(ev => Reset());
        this.Q<Button>("Quit").RegisterCallback<ClickEvent>(ev => Quit());
    }

    private void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void Quit(){
        SceneManager.LoadScene("Home");
    }

    public void ChangeTurn(){
        if(gm.redsTurn)this.Q<Label>("ActivePlayer").text = "Red's Turn";
        if(!gm.redsTurn)this.Q<Label>("ActivePlayer").text = "Green's Turn";
    }

    public void UpdateScore(){
        this.Q<Label>("RedCounter").text = gm.redPieces.Count.ToString("00");
        this.Q<Label>("GreenCounter").text = gm.greenPieces.Count.ToString("00");
    }

    public void GameOver(string winner){
        this.Q<Label>("ActivePlayer").text = winner + " WINS.";
    }
}
