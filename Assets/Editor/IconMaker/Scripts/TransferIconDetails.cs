using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TransferIconDetails : PopupWindowContent
{
    Button reset, copy, paste;
    public override Vector2 GetWindowSize()
    {
        return new Vector2(85, 65);
    }
    public override void OnGUI(Rect rect)
    {
        reset = editorWindow.rootVisualElement.Q<Button>("Reset");
        copy = editorWindow.rootVisualElement.Q<Button>("Copy");
        paste = editorWindow.rootVisualElement.Q<Button>("Paste");
    }

    public void Copy()
    {

    }

    public void Paste()
    {

    }

    public void Reset()
    {

    }

    public override void OnOpen()
    {
        Debug.Log("Popup opened: " + this);
        var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/IconMaker/TransferIconDetails.uxml");
        visualTreeAsset.CloneTree(editorWindow.rootVisualElement);
    }

    public override void OnClose()
    {
        Debug.Log("Popup closed: " + this);
    }
}
