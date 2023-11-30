using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectionMenu : MenuSettingsMain
{
    Label GOCount, iconMenuLabel;
    VisualElement buttonCount;
    Toggle menuIconToggle;
    public SelectionMenu(VisualElement root)
    {
        this.root = root;
        this.element = root.Q<VisualElement>("SelectionMenu");
        base.currentSetting = this;
        GOCount = root.Q<Label>("SelectedGOCount");
        buttonCount = root.Q<VisualElement>("ButtonDisplayCount");
        menuIconToggle = root.Q<Toggle>("MenuIconToggle");
        iconMenuLabel = root.Q<Label>("IconMenuLabel");
        this.rowDisplayInt = Selection.gameObjects.Length;
        base.Init();
    }

    public override void OnSelectionChange()
    {
        SelectionCheck();
    }

    public void SelectionCheck()
    {
        if (Selection.gameObjects.Length > 0)
        {
            buttonCount.style.display = DisplayStyle.Flex;
            menuIconToggle.style.display = DisplayStyle.Flex;
            iconMenuLabel.style.display = DisplayStyle.Flex;
            GOCount.text = $"You have {Selection.gameObjects.Length} selected objects";
            ChangeRowDisplay();
        }
        else
        {
            buttonCount.style.display = DisplayStyle.None;
            menuIconToggle.style.display = DisplayStyle.None;
            iconMenuLabel.style.display = DisplayStyle.None;
            menuIconToggle.value = false;
            GOCount.text = $"Please select gameobjects to use as buttons";
        }
    }

    public override void UpdateButtonDisplay(IChangeEvent evt)
    {
        ChangeRowDisplay();
    }

    public override void ChangeRowDisplay()
    {
        this.rowDisplayInt = Selection.gameObjects.Length;
        base.UpdateButtonFunction();
    }
}
