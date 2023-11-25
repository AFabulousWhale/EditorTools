using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BG : ColorType
{
    public BG(VisualElement root)
    {
        elementName = "BG";
        defaultValue = new(255, 255, 255, 0);
        this.root = root;
        base.currentSetting = this;
        base.Init();
    }
    public override void ChangeSingleValues(GameObject objectToUpdate)
    {
        if (cam != null)
        {
            cam.backgroundColor = field.value;
        }
        base.ChangeSingleValues(objectToUpdate);
    }
    public override void SaveIndividualData(Data objectToSave)
    {
        objectToSave.BGColor = field.value;
    }
}
