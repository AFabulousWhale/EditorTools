using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectDataBaseSO : ScriptableObject
{
    [SerializeField]
    public List<ObjectData> objectDatabase = new();
}

[Serializable]
public class ObjectData
{
    [SerializeField]
    public int cost;

    [SerializeField]
    public GameObject prefab;

    [SerializeField]
    public Sprite icon;

    [SerializeField]
    public string name;
}
