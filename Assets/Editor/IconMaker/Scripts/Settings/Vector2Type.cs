using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Vector2Type : IconSettingsMain
{
    public Vector2Field field;
    public Vector2 defaultValue;

    public override void Init()
    {
        field = root.Q<Vector2Field>($"{elementName}{elementPrefix}");
        field.RegisterValueChangedCallback(ValueChangeDetection);
        toggle = root.Q<Toggle>($"{elementName}{togglePrefix}");
        base.currentSetting = this.currentSetting;
        base.Init();
    }
}
