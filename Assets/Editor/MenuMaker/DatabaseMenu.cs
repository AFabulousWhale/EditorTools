using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DatabaseMenu : MenuSettingsMain
{
    Toggle DBIcons;
    public DatabaseMenu(VisualElement root, MenuMaker menuMaker)
    {
        this.menuMaker = menuMaker;
        this.root = root;
        this.element = root.Q<VisualElement>("DatabaseMenu");
        DBIcons = root.Q<Toggle>("UseIconsFromDBToggle");
        rowDisplayInt = menuMaker.dataBase.objectDatabase.Count;
        base.currentSetting = this;
        base.Init();
    }

    /// <summary>
    /// checks if the database has icons and will display the toggle to use them
    /// </summary>
    public void IconCheck()
    {
        if(menuMaker.dataBase != null)
        {
            if (menuMaker.dataBase.objectDatabase.Count != 0)
            {
                if (DBHasIcons())
                {
                    DBIcons.style.display = DisplayStyle.Flex;
                }
                else
                {
                    DBIcons.style.display = DisplayStyle.None;
                }
            }
        }
    }

    bool DBHasIcons()
    {
        foreach (var item in menuMaker.dataBase.objectDatabase)
        {
            if(item.icon != null)
            {
                return true; //returns true if at least 1 icon exists
            }
        }
        return false;
    }

    public override void UpdateButtonDisplay(IChangeEvent evt)
    {
        ChangeRowDisplay();
    }

    public override void ChangeRowDisplay()
    {
        if (menuMaker.dataBase != null)
        {
            if (menuMaker.dataBase.objectDatabase.Count != 0)
            {
                this.rowDisplayInt = menuMaker.dataBase.objectDatabase.Count;
                Debug.Log($"row {this.rowDisplayInt}");
                base.UpdateButtonFunction();
            }
            else
            {
                buttonCountDisplay.text = "There is no items within the database";
            }
        }
    }
}
