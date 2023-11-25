using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SetValueMenu : MenuSettingsMain
{
    public SliderInt maxButtons;
    public SetValueMenu(VisualElement root)
    {
        this.root = root;
        this.element = root.Q<VisualElement>("SetMenu");
        base.currentSetting = this;
        maxButtons = root.Q<SliderInt>("NumberOfButtons");
        maxButtons.RegisterValueChangedCallback(UpdateButtonDisplay); //calls the updatebutton display on that setting menu
        this.rowDisplayInt = maxButtons.value;
        base.Init();
    }

    public override void UpdateButtonDisplay(IChangeEvent evt)
    {
        ChangeRowDisplay();
    }

    public override void ChangeRowDisplay()
    {
        this.rowDisplayInt = maxButtons.value;
        base.UpdateButtonFunction();
    }
}
