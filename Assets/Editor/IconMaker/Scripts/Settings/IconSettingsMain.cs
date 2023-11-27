using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class IconSettingsMain
{
    protected VisualElement root;
    protected string elementName;
    protected string togglePrefix = "All";
    protected string elementPrefix = "IconChanges";

    public Toggle toggle;

    public GameObject currentPrefab;

    protected VisualElement cameraView;
    public IconMaker iconMaker;

    protected IconSettingsMain currentSetting;

    public Camera cam;

    /// <summary>
    /// any changes made to these values will update the prefab
    /// </summary>
    /// <param name="evt"></param>
    public virtual void ValueChangeDetection(IChangeEvent evt)
    {
        if (currentSetting.currentPrefab != null)
        {
            ChangeSingleValues(currentPrefab);
            cameraView.style.backgroundImage = iconMaker.GetRenderTexture();
        }
    }

    /// <summary>
    /// for each setting this is where it updates things such as the position or rotation of the object passed in
    /// </summary>
    /// <param name="objectToUpdate"></param>
    public virtual void ChangeSingleValues(GameObject objectToUpdate)
    {
    }

    /// <summary>
    /// this checks if there are multiple objects that need data saving or if it is just the one based on if the toggle is checked or not
    /// </summary>
    /// <param name="objectToSave"></param>
    /// <param name="currentSetting"></param>
    public virtual void SaveIconDataCheck(Data objectToSave, IconSettingsMain currentSetting)
    {
        //if the toggle for the current setting is true then it will save the data for that setting for each selected object
        if (currentSetting.toggle != null)
        {
            if (currentSetting.toggle.value)
            {
                foreach (var item in currentSetting.iconMaker.iconObjects)
                {
                    Data data = new();
                    data.prefab = item;
                    data.name = item.name;
                    data = currentSetting.iconMaker.SaveData(data);
                    currentSetting.SaveIndividualData(data);
                    currentSetting.iconMaker.SaveIconData(data);

                }
            }
        }
        currentSetting.SaveIndividualData(objectToSave);
        currentSetting.iconMaker.SaveIconData(objectToSave);
    }

    /// <summary>
    /// this does the saving for each individual setting
    /// </summary>
    /// <param name="objectToSave"></param>
    public virtual void SaveIndividualData(Data objectToSave)
    {
    }

    /// <summary>
    /// setting up varaible values and finding the elements within the editor window root
    /// </summary>
    public virtual void Init()
    {
        //if the setting has a toggle then it will be something that updates the camera
        if (currentSetting.toggle != null)
        {
            currentSetting.toggle = root.Q<Toggle>($"{elementName}{togglePrefix}");
        }
        currentSetting.cameraView = root.Q<VisualElement>("CameraView");
        iconMaker = EditorWindowMain.iconMaker;
    }
}
