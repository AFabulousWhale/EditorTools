using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ColorType : IconSettingsMain
{
    public ColorField field;
    public Color32 defaultValue;
    public override void Init()
    {
        field = root.Q<ColorField>($"{elementName}{elementPrefix}");
        field.RegisterValueChangedCallback(ValueChangeDetection);
        toggle = root.Q<Toggle>($"{elementName}{togglePrefix}");
        base.currentSetting = this.currentSetting;
        base.Init();
    }
}
