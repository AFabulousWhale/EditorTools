using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TextType : IconSettingsMain
{
    public TextField field;
    public string defaultValue;

    public override void Init()
    {
        field = root.Q<TextField>($"{elementName}{elementPrefix}");
        field.labelElement.RegisterCallback<MouseDownEvent>(ShowPopup, TrickleDown.TrickleDown);
        field.RegisterValueChangedCallback(ValueChangeDetection);
        base.currentSetting = this.currentSetting;
        base.Init();
    }
}
