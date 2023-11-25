using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IconDataSO : ScriptableObject
{
    [SerializeField]
    public List<Data> IconData = new();
}

[Serializable]
public class Data
{
    public string name;
    public GameObject prefab;
    public string spriteName;
    public Vector2 posOffset;
    public Vector3 rotOffset;
    public float scaleOffset;
    public Color32 BGColor;
}
