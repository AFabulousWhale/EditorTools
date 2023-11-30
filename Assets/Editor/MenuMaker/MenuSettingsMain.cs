using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuSettingsMain
{
    protected VisualElement root;
    public VisualElement element;

    public int rowDisplayInt;
    public SliderInt buttonsPerRow;
    public Label buttonCountDisplay, buttonCountDisplayExtra;

    public MenuSettingsMain currentSetting;
    public MenuMaker menuMaker;

    /// <summary>
    /// setting up varaible values and finding the elements within the editor window root
    /// </summary>
    public virtual void Init()
    {
        buttonCountDisplay = root.Q<Label>("ButtonDisplayLabel");
        buttonCountDisplayExtra = root.Q<Label>("ButtonDisplayLabelExtra");
        buttonsPerRow = root.Q<SliderInt>("ButtonsPerRow");
    }

    public virtual void OnSelectionChange()
    {

    }

    /// <summary>
    /// called when changing enums to update the row display for how many buttons will be generated
    /// </summary>
    public virtual void ChangeRowDisplay()
    {

    }

    public void UpdateButtonFunction()
    {
        if (currentSetting.rowDisplayInt > buttonsPerRow.value) //if there will be more than 1 row
        {
            int rowCount = currentSetting.rowDisplayInt / buttonsPerRow.value; //works out how many rows will be created
            buttonCountDisplay.text = $"This will create {rowCount} rows each with {buttonsPerRow.value} buttons";

            int divisionRemainder = rowCount * buttonsPerRow.value; //works out if it is an even division or if there is a remainder for an extra row

            if (divisionRemainder != currentSetting.rowDisplayInt)
            {
                int difference = currentSetting.rowDisplayInt - divisionRemainder;
                buttonCountDisplayExtra.text = $"With an extra row of {difference} buttons";
                buttonCountDisplayExtra.style.display = DisplayStyle.Flex;
            }
            else
            {
                buttonCountDisplayExtra.style.display = DisplayStyle.None;
            }
        }
        else //if the max buttons is equal to or smaller than the buttons per row it will always create one row
        {
            buttonCountDisplayExtra.style.display = DisplayStyle.None;
            buttonCountDisplay.text = $"This will create 1 row with {currentSetting.rowDisplayInt} buttons";
        }
    }

    /// <summary>
    /// updates the label to display how many rows with buttons will be created
    /// </summary>
    public virtual void UpdateButtonDisplay(IChangeEvent evt)
    {
        UpdateButtonFunction();
    }
}
