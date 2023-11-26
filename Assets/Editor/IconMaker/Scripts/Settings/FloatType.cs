using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FloatType : IconSettingsMain
{
    public FloatField field;
    public float defaultValue;
    public override void Init()
    {
        field = root.Q<FloatField>($"{elementName}{elementPrefix}");
        field.RegisterValueChangedCallback(ValueChangeDetection);
        toggle = root.Q<Toggle>($"{elementName}{togglePrefix}");
        base.currentSetting = this.currentSetting;
        base.Init();
    }
}
