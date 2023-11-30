using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.IO;
using Unity.EditorCoroutines.Editor;

public class MenuMaker : EditorWindow
{
    SetValueMenu setValueClass => new(root);
    SelectionMenu selectionClass => new(root);
    public DatabaseMenu databaseClass => new(root, this);
    List<MenuSettingsMain> settings => new() {setValueClass, selectionClass, databaseClass };

    private readonly VisualElement root;

    public ObjectDataBaseSO dataBase;

    public EnumField menuTypeField;
    public SliderInt buttonsPerRow;
    public VisualElement row, popup;
    public TextField buttonClass, VEName;
    public ObjectField UIObject;
    public Button createMenuButton;
    public Toggle menuIconToggle, DBIcons;

    public IconMaker iconMaker;
    public DatabaseMaker databaseMaker;

    public TextField saveLocation;

    int enumInt;

    EditorPopup menuPopup = new();

    public MenuMaker(VisualElement root)
    {
        this.root = root;
    }

    public enum MenuType
    {
        SetValues = 0,
        SelectedObjects,
        Database
    }

    #region Setting Up
    public void CreateGUI()
    {
        menuTypeField = root.Q<EnumField>("TypeOfMenu");

        buttonsPerRow = root.Q<SliderInt>("ButtonsPerRow");

        buttonClass = root.Q<TextField>("ButtonStyleName");
        VEName = root.Q<TextField>("UIName");
        UIObject = root.Q<ObjectField>("UIObject");

        createMenuButton = root.Q<Button>("CreateMenuButton");

        menuIconToggle = root.Q<Toggle>("MenuIconToggle");
        DBIcons = root.Q<Toggle>("UseIconsFromDBToggle");

        saveLocation = root.Q<TextField>("MenuSaveLocation");

        popup = root.Q<VisualElement>("PopupLayout");

        createMenuButton.RegisterCallback<ClickEvent>(MenuChecks);
    }

    public void OnGUI()
    {
        enumInt = Convert.ToInt32(menuTypeField.value);
        //loops through the settings and shows the elements based on the index of the enum as it's the same as the list
        for (int i = 0; i < settings.Count; i++)
        {
            if (i == enumInt)
            {
                settings[i].element.style.display = DisplayStyle.Flex;

                if (enumInt == 1) //is the selection enum
                {
                    selectionClass.SelectionCheck();
                }
                if(enumInt == 2) //if the database enum
                {
                    databaseClass.IconCheck();
                }

                buttonsPerRow.RegisterValueChangedCallback(settings[i].UpdateButtonDisplay); //calls the updatebutton display on that setting menu
                settings[i].ChangeRowDisplay();
            }
            else
            {
                settings[i].element.style.display = DisplayStyle.None;
            }
        }

        Debug.Log(settings[enumInt].rowDisplayInt);
        if (settings[enumInt].rowDisplayInt > 0)
        {
            createMenuButton.style.display = DisplayStyle.Flex;
        }
        else
        {
            createMenuButton.style.display = DisplayStyle.None;
        }
    }
    #endregion End - Setting Up

    public void OnSelectionChange()
    {
        if (enumInt == 1)
        {
            settings[1].OnSelectionChange();
        }
    }

    #region Checks
    /// <summary>
    /// checks if a menu can be generated
    /// </summary>
    /// <param name="evt"></param>
    public void MenuChecks(ClickEvent evt)
    {
        //will create popups
        if (UIObject.value == null) //if no UI object has been selected
        {
            ErrorPopup("There is No UI Doccument selected, please select one and try again");
        }
        else
        {
            if (VEName.value == "") //if no name for the visualelement has been entered
            {
                ErrorPopup("There is no Visual Element to attach to, please enter a name and try again");
            }
            else
            {
                //checks the name of the visual element to see if it's valid
                UIDocument doc = (UIDocument)UIObject.value;
                VisualElement elementToCreateUION = doc.rootVisualElement.Q<VisualElement>(VEName.value);
                if (elementToCreateUION == null)
                {
                    ErrorPopup("The Visual Element name you have entered doesn't exist on the specified UI Document, please enter a different one");
                }
                else
                {
                    //checks if text has been entered for the button class
                    if(buttonClass.value == "")
                    {
                        ErrorPopup("There is no button style class entered, please enter one and try again!");
                    }
                    else
                    {
                        GenerateMenu();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks if the save location entered exists
    /// </summary>
    /// <returns></returns>
    bool FilePathCheck(string path)
    {
        //check if directory doesn't exit
        if (Directory.Exists(path))
        {
            return true;
        }
        return false;
    }
    #endregion End - Checks

    #region Menu Generation
    void ErrorPopup(string errorText)
    {
        menuPopup.labelText = errorText;
        UnityEditor.PopupWindow.Show(popup.worldBound, menuPopup);
    }
    /// <summary>
    /// Function to generate menu buttons based on set amount
    /// </summary>
    void GenerateMenu()
    {
        if (FilePathCheck(saveLocation.value) && FilePathCheck(iconMaker.saveLocation.value))
        {
            if (menuIconToggle.value)
            {
                iconMaker.currentObjectIndex = 0;
                iconMaker.ObjectsSelected();
            }
            MenuUISO menu = ScriptableObject.CreateInstance<MenuUISO>(); //creates a new scriptable object with all of the button data for the menus
            menu.rowCount = buttonsPerRow.value;
            menu.buttonStyle = buttonClass.value;

            for (int i = 0; i < settings[enumInt].rowDisplayInt; i++)
            {
                ButtonData button = new();
                button.name = "button" + i;

                if (menuIconToggle.value)
                {
                    CreateIcon(button);
                }
                if (DBIcons.value)
                {
                    if (dataBase.objectDatabase[i].icon != null)
                    {
                        button.icon = dataBase.objectDatabase[i].icon;
                    }
                }
                EditorUtility.SetDirty(menu);
                menu.buttons.Add(button);
            }

                AssetDatabase.CreateAsset(menu, $"{saveLocation.value}/MenuSO.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                UIController controller = GameObject.FindAnyObjectByType<UIController>();
                controller.UI = menu;
                controller.AddButtons();
        }
        if (!FilePathCheck(iconMaker.saveLocation.value))
        {
            ErrorPopup("The specified save location does not exist for the icons, please enter a valid file path");
        }
        if (!FilePathCheck(saveLocation.value))
        {
            ErrorPopup("The specified save location does not exist for the menu, please enter a valid file path");
        }
    }

    /// <summary>
    /// calls the generate icon function on the icon maker and sets it in the database linked to the specific prefab
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="data"></param>
    void CreateIcon(ButtonData data)
    {
        iconMaker.GetIcon(iconMaker.GetRenderTexture());
        data.icon = iconMaker.iconSprite;
        if (settings[enumInt].rowDisplayInt > 1)
        {
            iconMaker.ProgressIcon(1);
        }
    }
    #endregion End - Menu Generation
}
