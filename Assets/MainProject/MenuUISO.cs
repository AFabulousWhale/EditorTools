using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MenuUISO : ScriptableObject
{
    [SerializeField]
    public int rowCount;
    [SerializeField]
    public string buttonStyle;
    [SerializeField]
    public List<ButtonData> buttons = new();
}

[Serializable]
public class ButtonData
{
    public Sprite icon;
    [SerializeField]
    public string name;
}
