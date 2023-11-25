using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorPopup : PopupWindowContent
{
    Button confirm, cancel;
    Label popUpLabel, popUpLabelExtra;
    public bool twoButtons, extraText;
    public string popUpStage = "none";
    public bool popupOpen = false;
    public string labelText, labelTextExtra;
    public override Vector2 GetWindowSize()
    {
        return new Vector2(350, 175);
    }
    public override void OnGUI(Rect rect)
    {
        confirm.RegisterCallback<ClickEvent, string>(CloseWindow, "confirm");
        cancel.RegisterCallback<ClickEvent, string>(CloseWindow, "cancel");
    }

    void CloseWindow(ClickEvent evt, string buttonType)
    {
        popUpStage = buttonType;
        editorWindow.Close();
    }

    public override void OnOpen()
    {
        popupOpen = true;
        popUpStage = "none";
        Debug.Log("Popup opened: " + this);
        var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/IconMaker/PopupWindow.uxml");
        visualTreeAsset.CloneTree(editorWindow.rootVisualElement);

        popUpLabel = editorWindow.rootVisualElement.Q<Label>("PopupText");
        popUpLabelExtra = editorWindow.rootVisualElement.Q<Label>("PopupTextExtra");
        confirm = editorWindow.rootVisualElement.Q<Button>("ConfirmButton");
        cancel = editorWindow.rootVisualElement.Q<Button>("CancelButton");

        popUpLabel.text = labelText;

        if (twoButtons)
        {
            cancel.style.display = DisplayStyle.Flex;
        }
        else
        {
            cancel.style.display = DisplayStyle.None;
        }

        if(extraText)
        {
            popUpLabelExtra.style.display = DisplayStyle.Flex;
            popUpLabelExtra.text = labelTextExtra;
        }
        else
        {
            popUpLabelExtra.style.display = DisplayStyle.None;
        }
    }

    public override void OnClose()
    {
        popupOpen = false;
        if(popUpStage == "none")
        {
            popUpStage = "cancel";
        }
        Debug.Log("Popup closed: " + this);
    }
}
