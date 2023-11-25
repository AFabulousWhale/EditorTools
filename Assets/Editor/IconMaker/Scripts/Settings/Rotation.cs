using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Rotation : Vector3Type
{
    public Rotation(VisualElement root)
    {
        elementName = "Rot";
        defaultValue = new(0, 150, 0);
        this.root = root;
        base.currentSetting = this;
        base.Init();
    }
    public override void ChangeSingleValues(GameObject objectToUpdate)
    {
        objectToUpdate.transform.rotation = Quaternion.Euler(defaultValue + field.value);
        base.ChangeSingleValues(objectToUpdate);
    }

    public override void SaveIndividualData(Data objectToSave)
    {
        objectToSave.rotOffset = field.value;
    }
}
