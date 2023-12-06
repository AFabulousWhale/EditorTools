using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Position : Vector2Type
{ 
    public Position(VisualElement root)
    {
        elementName = "Pos";
        defaultValue = new(-500, 0);
        this.root = root;
        base.currentSetting = this;
        base.Init();
    }

    public override void ChangeSingleValues(GameObject objectToUpdate)
    {
        objectToUpdate.transform.position = new Vector3((-500 + field.value.x), (0 + field.value.y), 6);
        base.ChangeSingleValues(objectToUpdate);
    }

    public override void SaveIndividualData(Data objectToSave)
    {
        objectToSave.posOffset = field.value;
    }
}
