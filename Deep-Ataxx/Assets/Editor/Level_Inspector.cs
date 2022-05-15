using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Cox.Infection.Management;

[CustomEditor(typeof(Level))]
public class Level_Inspector : Editor
{
    public VisualTreeAsset m_InspectorAsset;

    public override VisualElement CreateInspectorGUI(){
	    VisualElement customInspector = new VisualElement(); //Create new Visual Element as root
	    m_InspectorAsset.CloneTree(customInspector); //Load and clone from public asset
        customInspector?.Q<GridVector2Control>().GenerateGrid(serializedObject.FindProperty("columns").intValue, serializedObject.FindProperty("rows").intValue);
        Debug.Log(serializedObject.FindProperty("columns").intValue);
        VisualElement inspectorFoldout = customInspector.Q("Default_Inspector"); //Gets reference to default inspector
        InspectorElement.FillDefaultInspector(inspectorFoldout, serializedObject, this); //Fills defaults*/
	    return customInspector; //Returns the new inspector, now in only 3 lines.
    }
}