using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Unity.EditorCoroutines.Editor;

public class DatabaseMaker : EditorWindow
{
    private readonly VisualElement root;

    public ObjectDataBaseSO database, dataTest;
    TextField databaseName;
    Toggle generateAllIconsToggle, generateNewIconsToggle, generateEveryIconToggle;
    VisualElement settings, GOLabels, popup, buttonsVE;
    Button addToDatabase, clearDatabase, newDatabase;
    Label allIconsLabel, newIconsLabel, everyIconLabel, inDBLabel;

    int inDBCount = 0;

    string filePath = "Assets/Editor/DatabaseMaker/";

    public IconMaker iconMaker;

    EditorPopup databasePopup = new();

    public DatabaseMaker(VisualElement root)
    {
        this.root = root;
    }

    #region Editor Window Setup
    public void CreateGUI()
    {
        databaseName = root.Q<TextField>("DatabaseName");
        settings = root.Q<VisualElement>("DatabaseSettings");
        GOLabels = root.Q<VisualElement>("DatabaseGOLabels");

        addToDatabase = root.Q<Button>("AddToDatabaseButton");
        clearDatabase = root.Q<Button>("ClearDatabaseButton");
        newDatabase = root.Q<Button>("CreateDatabaseButton");
        buttonsVE = root.Q<VisualElement>("DatabaseButtons");

        generateAllIconsToggle = root.Q<Toggle>("DatabaseAllIcons");
        generateEveryIconToggle = root.Q<Toggle>("DatabaseEveryIcons");
        generateNewIconsToggle = root.Q<Toggle>("DatabaseNewIcons");

        allIconsLabel = root.Q<Label>("DatabaseAllIconsLabel");
        everyIconLabel = root.Q<Label>("DatabaseEveryIconsLabel");
        newIconsLabel = root.Q<Label>("DatabaseNewIconsLabel");

        popup = root.Q<VisualElement>("PopupLayout");

        inDBLabel = root.Q<Label>("InDatabaseLabel");

        databaseName.RegisterValueChangedCallback(DatabaseNameChange);
        newDatabase.RegisterCallback<ClickEvent, string>(PopUpCheck, "create");
        addToDatabase.RegisterCallback<ClickEvent, string>(PopUpCheck, "add");
        clearDatabase.RegisterCallback<ClickEvent, string>(PopUpCheck, "clear");

        ButtonShowing();
    }

    public void OnGUI()
    {
        ButtonShowing();
    }

    public void OnSelectionChange()
    {
        ButtonShowing();
        inDBLabel.style.display = DisplayStyle.None;
        if (Selection.gameObjects.Length > 0)
        {
            GOLabels.style.display = DisplayStyle.None;
            settings.style.display = DisplayStyle.Flex;
            buttonsVE.style.display = DisplayStyle.Flex;

            //loops through the selection of gameobjects to see if any are already in the database
            inDBCount = 0;
            foreach (var item in Selection.gameObjects)
            {
                if(AlreadyInDatabaseCheck(item))
                {
                    inDBCount++;
                }
            }

            if(inDBCount > 0)
            {
                inDBLabel.style.display = DisplayStyle.Flex;
                if (Selection.gameObjects.Length != inDBCount)
                {
                    inDBLabel.text = $"{inDBCount} selected objects are already in the database and won't be re-added";
                    settings.style.display = DisplayStyle.Flex;
                    buttonsVE.style.display = DisplayStyle.Flex;
                }
                else
                {
                    inDBLabel.text = $"The objects you have selected is already in the database, please select another";
                    settings.style.display = DisplayStyle.None;
                    buttonsVE.style.display = DisplayStyle.None;
                }
            }
        }
        else
        {
            GOLabels.style.display = DisplayStyle.Flex;
            settings.style.display = DisplayStyle.None;          
        }
    }
    #endregion End - Editor Window Setup

    #region Checks
    /// <summary>
    /// when the database name is changed it will update the button display
    /// </summary>
    /// <param name="evt"></param>
    public void DatabaseNameChange(IChangeEvent evt)
    {
        generateAllIconsToggle.value = false;
        generateEveryIconToggle.value = false;
        generateNewIconsToggle.value = false;
        ButtonShowing();
    }

    /// <summary>
    /// shows the buttons to create/add/clear the database when the object selection is more than 0
    /// </summary>
    public void ButtonShowing()
    {
        switch (NewDatabase())
        {
            case true: //if it's a new database and the selected objects are over 0 then can create a database from objects
                addToDatabase.style.display = DisplayStyle.None;
                clearDatabase.style.display = DisplayStyle.None;

                generateAllIconsToggle.style.display = DisplayStyle.Flex;
                allIconsLabel.style.display = DisplayStyle.Flex;
                generateEveryIconToggle.style.display = DisplayStyle.None;
                everyIconLabel.style.display = DisplayStyle.None;
                generateNewIconsToggle.style.display = DisplayStyle.None;
                newIconsLabel.style.display = DisplayStyle.None;

                if (Selection.gameObjects.Length > 0)
                {
                    newDatabase.style.display = DisplayStyle.Flex;
                }
                else
                {
                    newDatabase.style.display = DisplayStyle.None;
                }
                break;
            case false: //if database already exists
                newDatabase.style.display = DisplayStyle.None;
                clearDatabase.style.display = DisplayStyle.Flex;

                generateAllIconsToggle.style.display = DisplayStyle.None;
                allIconsLabel.style.display = DisplayStyle.None;


                if (Selection.gameObjects.Length > 0) //if there are objects selected then you can add them to the database
                {
                    GOLabels.style.display = DisplayStyle.None;
                    generateEveryIconToggle.style.display = DisplayStyle.Flex;
                    everyIconLabel.style.display = DisplayStyle.Flex;
                    generateNewIconsToggle.style.display = DisplayStyle.Flex;
                    newIconsLabel.style.display = DisplayStyle.Flex;
                    addToDatabase.style.display = DisplayStyle.Flex;
                }
                else
                {
                    GOLabels.style.display = DisplayStyle.Flex;
                    generateEveryIconToggle.style.display = DisplayStyle.None;
                    everyIconLabel.style.display = DisplayStyle.None;
                    generateNewIconsToggle.style.display = DisplayStyle.None;
                    newIconsLabel.style.display = DisplayStyle.None;
                    addToDatabase.style.display = DisplayStyle.None;
                }
                break;
        }

    }

    /// <summary>
    /// Checks if popup was canceled/confirmed
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="buttonType"></param>
    public void PopUpCheck(ClickEvent evt, string buttonType)
    {
        databasePopup.twoButtons = true;
        switch (buttonType) //used to change the text displayed on the popup
        {
            case "create":
                databasePopup.labelText = $"This will create a new database with all {Selection.gameObjects.Length} selected objects";
                if(generateAllIconsToggle.value)
                {
                    databasePopup.extraText = true;
                    databasePopup.labelTextExtra = $"And create Icons for the {Selection.gameObjects.Length} selected objects";
                }
                else
                {
                    databasePopup.extraText = false;
                }
                break;
            case "add":
                databasePopup.labelText = $"This will add the {Selection.gameObjects.Length} selected objects into the database";
                if(generateEveryIconToggle.value || generateNewIconsToggle.value)
                {
                    databasePopup.extraText = true;


                    if (generateEveryIconToggle.value)
                    {
                        databasePopup.labelTextExtra = $"And create Icons for the {database.objectDatabase.Count} database items as well " +
                            $"as the {Selection.gameObjects.Length} selected objects";
                    }
                    if (generateNewIconsToggle.value)
                    {
                        databasePopup.labelTextExtra = $"And create Icons for the {Selection.gameObjects.Length} selected objects";
                    }
                }
                else
                {
                    databasePopup.extraText = false;
                }
                break;
            case "clear":
                NewDatabase();
                database = dataTest;
                databasePopup.labelText = $"This will clear the database of {database.objectDatabase.Count} items";
                break;
        }
        EditorCoroutineUtility.StartCoroutineOwnerless(StartCheck());
        IEnumerator StartCheck() //waits until the popup has either been cancelled or confirmed 
        {
            UnityEditor.PopupWindow.Show(popup.worldBound, databasePopup);
            yield return new WaitUntil(() => databasePopup.popUpStage != "none");

            if(databasePopup.popUpStage == "confirm")
            {
                switch (buttonType) //used to call the specific functions
                {
                    case "create":
                        GenerateDatabase(evt);
                        break;
                    case "add":
                        GenerateDatabase(evt);
                        break;
                    case "clear":
                        ClearDatabase();
                        break;
                }
            }
        }
    }

    bool AlreadyInDatabaseCheck(GameObject objectToCheck)
    {
        if (!NewDatabase())
        {
            database = dataTest;
            for (int i = 0; i < database.objectDatabase.Count; i++)//for each item already saved
            {
                //updates the icon if the prefab already exists
                if (database.objectDatabase[i].prefab == objectToCheck)
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion End - Checks

    #region Database Creation
    /// <summary>
    /// creates a new database
    /// </summary>
    /// <param name="iconGeneration"></param>
    /// <param name="objectsToAddToDatabase"></param>
    /// <param name="evt"></param>
    /// <param name="scriptableObject"></param>
    void GenerateDatabase(ClickEvent evt)
    {
        bool iconGeneration = false;
        if (generateAllIconsToggle.value || generateEveryIconToggle.value || generateNewIconsToggle.value) //if any of the toggles are on then icons will be generated
        {
            iconMaker.currentObjectIndex = 0;
            iconMaker.ObjectsSelected();
            iconGeneration = true;
        }
        for (int i = 0; i < iconMaker.iconObjects.Count; i++)
        {
            ObjectData data = new();
            data.prefab = iconMaker.iconObjects[i];
            data.name = data.prefab.name;

            Debug.Log(iconMaker.currentObjectIndex);
            if (iconGeneration) //generate icons if any of the toggles are true
            {
                CreateIcon(evt, data);
            }

            //this is the database that has been found/a new one
            if(AlreadyInDatabaseCheck(data.prefab))
            {
                database.objectDatabase[i].icon = data.icon;
            }

            EditorUtility.SetDirty(database);

            if (!AlreadyInDatabaseCheck(data.prefab))
            {
                database.objectDatabase.Add(data); //adds the current data to the database to be saved
            }
        }
        databasePopup.twoButtons = false;
        databasePopup.extraText = false;

        if (!NewDatabase())
        {
            if (iconGeneration)
            {
                databasePopup.labelText = "Database and Icons Have Been Updated Sucessfully!";
            }
            else
            {
                databasePopup.labelText = "Database Has Been Updated Sucessfully!";
            }
        }
        if (NewDatabase())
        {
            AssetDatabase.CreateAsset(database, $"{filePath}{databaseName.value}.asset");
            if (iconGeneration)
            {
                databasePopup.labelText = "Database and Icons Have Been Created Sucessfully!";
            }
            else
            {
                databasePopup.labelText = "Database Has Been Created Sucessfully!";
            }
        }
        EditorCoroutineUtility.StartCoroutineOwnerless(SaveDatabase());
    }

    /// <summary>
    /// calls the generate icon function on the icon maker and sets it in the database linked to the specific prefab
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="data"></param>
    void CreateIcon(ClickEvent evt, ObjectData data)
    {
        iconMaker.GetIcon(iconMaker.GetRenderTexture());
        data.icon = iconMaker.iconSprite;
        iconMaker.NextIcon(evt, 1);
    }

    /// <summary>
    /// Popup once database has been edited
    /// </summary>
    /// <returns></returns>
    IEnumerator SaveDatabase() //waits a second before saving so the popup will show if completion was quick
    {
        yield return new WaitForSecondsRealtime(1);
        UnityEditor.PopupWindow.Show(popup.worldBound, databasePopup);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ButtonShowing();
    }

    /// <summary>
    /// Clears the database by the current name - if it already exists
    /// </summary>
    /// <param name="evt"></param>
    void ClearDatabase()
    {
        AssetDatabase.DeleteAsset($"{filePath}{databaseName.value}.asset");
        databasePopup.twoButtons = false;
        databasePopup.extraText = false;
        databasePopup.labelText = "Database Has Been Deleted Sucessfully!";
        EditorCoroutineUtility.StartCoroutineOwnerless(SaveDatabase());
    }

    /// <summary>
    /// checking the validity of the database and whether the name already exists
    /// </summary>
    /// <returns></returns>
    public bool NewDatabase()
    {
        dataTest = AssetDatabase.LoadAssetAtPath<ObjectDataBaseSO>($"{filePath}{databaseName.value}.asset");

        if (dataTest == null)
        {
            dataTest = ScriptableObject.CreateInstance<ObjectDataBaseSO>();
            return true;
        }
        return false;
    }
    #endregion End - Database Creation
}
