using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Scale : FloatType
{
    public Scale(VisualElement root)
    {
        elementName = "Scale";
        defaultValue = 1;
        this.root = root;
        base.currentSetting = this;
        base.Init();
    }
    public override void ChangeSingleValues(GameObject objectToUpdate)
    {
        objectToUpdate.transform.localScale = new(defaultValue * field.value, defaultValue * field.value, defaultValue * field.value);
        base.ChangeSingleValues(objectToUpdate);
    }

    public override void SaveIndividualData(Data objectToSave)
    {
        objectToSave.scaleOffset = field.value;
    }
}
