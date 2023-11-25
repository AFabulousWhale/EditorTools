using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class IconSaveLocation : TextType
{
    public IconSaveLocation(VisualElement root)
    {
        elementName = "IconSaveLocation";
        defaultValue = "Assets/Editor/IconMaker/Icons";
        this.root = root;
        base.currentSetting = this;
        base.Init();
    }
    public override void ChangeSingleValues(GameObject objectToUpdate)
    {
    }
    public override void SaveIndividualData(Data objectToSave)
    {
        objectToSave.spriteName = field.value;
    }
}
