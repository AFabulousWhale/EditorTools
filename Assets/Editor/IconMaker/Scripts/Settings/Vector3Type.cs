using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Vector3Type : IconSettingsMain
{
    public Vector3Field field;
    public Vector3 defaultValue;

    public override void Init()
    {
        field = root.Q<Vector3Field>($"{elementName}{elementPrefix}");
        field.RegisterValueChangedCallback(ValueChangeDetection);
        toggle = root.Q<Toggle>($"{elementName}{togglePrefix}");
        base.currentSetting = this.currentSetting;
        base.Init();
    }
}
