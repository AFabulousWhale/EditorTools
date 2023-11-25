using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpriteName : TextType
{
    public SpriteName(VisualElement root)
    {
        elementName = "SpriteName";
        defaultValue = "";
        this.root = root;
        base.currentSetting = this;
        base.Init();
    }
    public override void SaveIndividualData(Data objectToSave)
    {
        objectToSave.spriteName = field.value;
    }
}
