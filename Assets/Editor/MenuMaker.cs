using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

public class MenuMaker : EditorWindow
{
    SetValueMenu setValueClass => new(root);
    SelectionMenu selectionClass => new(root);
    DatabaseMenu databaseClass => new(root);
    List<MenuSettingsMain> settings => new() {setValueClass, selectionClass, databaseClass };

    private readonly VisualElement root;

    public EnumField menuTypeField;
    public SliderInt buttonsPerRow;
    public VisualElement row;
    public TextField buttonClass, VEName;
    public ObjectField UIObject;
    public Button createMenuButton;

    int enumInt;

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

    public void CreateGUI()
    {
        menuTypeField = root.Q<EnumField>("TypeOfMenu");

        buttonsPerRow = root.Q<SliderInt>("ButtonsPerRow");

        buttonClass = root.Q<TextField>("ButtonStyleName");
        VEName = root.Q<TextField>("UIName");
        UIObject = root.Q<ObjectField>("UIObject");

        createMenuButton = root.Q<Button>("CreateMenuButton");

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
                buttonsPerRow.RegisterValueChangedCallback(settings[i].UpdateButtonDisplay); //calls the updatebutton display on that setting menu
                settings[i].ChangeRowDisplay();
            }
            else
            {
                settings[i].element.style.display = DisplayStyle.None;
            }
        }

        if(settings[enumInt].rowDisplayInt > 0)
        {
            createMenuButton.style.display = DisplayStyle.Flex;
        }
        else
        {
            createMenuButton.style.display = DisplayStyle.None;
        }
    }

    public void OnSelectionChange()
    {
        if (enumInt == 1)
        {
            settings[1].OnSelectionChange();
        }
    }


    /// <summary>
    /// checks if a menu can be generated
    /// </summary>
    /// <param name="evt"></param>
    public void MenuChecks(ClickEvent evt)
    {
        //will create popups
        if (UIObject.value == null) //if no UI object has been selected
        {
            Debug.Log("No UI Selected");
        }
        else
        {
            if (VEName.value == "") //if no name for the visualelement has been entered
            {
                Debug.Log("No Visual Element Name entered");
            }
            else
            {
                //checks the name of the visual element to see if it's valid
                UIDocument doc = (UIDocument)UIObject.value;
                VisualElement elementToCreateUION = doc.rootVisualElement.Q<VisualElement>(VEName.value);
                if (elementToCreateUION == null)
                {
                    Debug.Log("VEName doesn't exist");
                }
                else
                {
                    //checks if text has been entered for the button class
                    if(buttonClass.value == "")
                    {
                        Debug.Log("No class entered");
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
    /// Function to generate menu buttons based on set amount
    /// </summary>
    void GenerateMenu()
    {
        MenuUISO menu = ScriptableObject.CreateInstance<MenuUISO>(); //creates a new scriptable object with all of the button data for the menus
        menu.rowCount = buttonsPerRow.value;
        menu.buttonStyle = buttonClass.value;

        Debug.Log(settings[enumInt].rowDisplayInt);

        for (int i = 0; i < settings[enumInt].rowDisplayInt; i++)
        {
            ButtonData button = new();
            button.name = "button" + i;

            menu.buttons.Add(button);
        }

        AssetDatabase.CreateAsset(menu, "Assets/Editor/IconMaker/MenuSO.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UIController controller = GameObject.FindAnyObjectByType<UIController>();
        controller.UI = menu;
        controller.AddButtons();
    }
}
