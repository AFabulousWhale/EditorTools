using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectionMenu : MenuSettingsMain
{
    Label GOCount;
    public SelectionMenu(VisualElement root)
    {
        this.root = root;
        this.element = root.Q<VisualElement>("SelectionMenu");
        base.currentSetting = this;
        GOCount = root.Q<Label>("SelectedGOCount");
        GOCount.text = $"You have {Selection.gameObjects.Length} selected objects";
        this.rowDisplayInt = Selection.gameObjects.Length;
        base.Init();
    }

    public override void OnSelectionChange()
    {
        GOCount.text = $"You have {Selection.gameObjects.Length} selected objects";
        ChangeRowDisplay();
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
