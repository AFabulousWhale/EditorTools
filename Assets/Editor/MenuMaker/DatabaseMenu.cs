using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DatabaseMenu : MenuSettingsMain
{
    public DatabaseMenu(VisualElement root)
    {
        this.root = root;
        this.element = root.Q<VisualElement>("DatabaseMenu");
        base.currentSetting = this;
        base.Init();
    }
}
