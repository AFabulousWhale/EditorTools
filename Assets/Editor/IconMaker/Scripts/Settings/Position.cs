using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Position : Vector3Type
{ 
    public Position(VisualElement root)
    {
        elementName = "Pos";
        defaultValue = new(-500, 0, 10);
        this.root = root;
        base.currentSetting = this;
        base.Init();
    }

    public override void ChangeSingleValues(GameObject objectToUpdate)
    {
        objectToUpdate.transform.position = defaultValue + field.value;
        base.ChangeSingleValues(objectToUpdate);
    }

    public override void SaveIndividualData(Data objectToSave)
    {
        objectToSave.posOffset = field.value;
    }
}
