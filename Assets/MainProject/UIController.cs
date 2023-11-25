using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// this script is placed on a gameobject in the scene, which contains a UI document component with the main UI Toolkit Document to display
/// </summary>
public class UIController : MonoBehaviour
{
    [SerializeField]
    public MenuUISO UI; //this is the scriptable menu object which will have to be passed in the inspector
    VisualElement row, root;

    List<VisualElement> currentRows = new();

    void Start()
    {
        AddButtons();
    }

    public void AddButtons()
    {
        root = this.GetComponent<UIDocument>().rootVisualElement; //setting the root element to be the main UI root
        RemoveButtons();
        AddRow();
        for (int i = 0; i < UI.buttons.Count; i++)
        {
            Button button = new(); //creates a new button for each element in the menu based on the max amount of buttons set
            button.name = UI.buttons[i].name;
            button.AddToClassList(UI.buttonStyle);

            if (UI.buttons[i].icon != null)
            {
                button.style.backgroundImage = new StyleBackground(UI.buttons[i].icon);
            }

            if (row.childCount >= UI.rowCount) //if the current row has reached the desired amount then make a new row
            {
                AddRow();
            }
            row.Add(button); //adding the button to the visual menu
            Debug.Log("added button");
        }
    }

    void AddRow()
    {
        row = new();
        root.Add(row);
        currentRows.Add(row);
        row.AddToClassList("row-style");
    }

    void RemoveButtons()
    {
        if(currentRows.Count > 0)
        {
            for (int i = 0; i < currentRows.Count; i++)
            {
                currentRows[i].Clear();
                root.Remove(currentRows[i]);
            }
            currentRows.Clear();
        }
    }
}
