using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.UIElements;

public class IconMakerEditorWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset tree = default;

    string iconName;
    Camera cam;
    int maxButtons, buttonsPerRow;

    List<Sprite> spritesGenerated;
    bool generatingBoth, generatingAll;

    string databaseName;

    Label menuObjectCount, iconObjectCount, gridText, gridTextExtra;
    TextField nameOfDB;
    bool usingObjects;
    Toggle usingObjectsToggle;
    SliderInt maxButtonCountText, buttonsPerRowText;
    Button generateMenu, generateIcons, generateBoth, generateDatabase, addDatabase, clearDatabase, generateAll, generateMenuFromDatabase;

    ObjectDataBaseSO database, dataTest;

    RenderTexture renderTexture;
    VisualElement camera;

    [MenuItem("Window/EditorTools/Icon&ButtonMaker")]
    public static void ShowExample()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow<IconMakerEditorWindow>("Icon and Button Maker"); //shows the icon window if not already displayed
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement UXMLFile = tree.Instantiate();
        root.Add(UXMLFile);

        //sets up all the visual element variables based on the Icon&Button Maker UXMl file
        menuObjectCount = root.Q<Label>("MenuObjectCount");
        iconObjectCount = root.Q<Label>("IconObjectCount");
        maxButtonCountText = root.Q<SliderInt>("MaxButtonCountSlider");
        buttonsPerRowText = root.Q<SliderInt>("ButtonsPerRowSlider");
        usingObjectsToggle = root.Q<Toggle>("UsingObjectToggle");
        generateMenu = root.Q<Button>("GenerateMenuButton");
        generateIcons = root.Q<Button>("GenerateIconsButton");
        generateBoth = root.Q<Button>("GenerateBothButtons");
        gridText = root.Q<Label>("GridSize");
        gridTextExtra = root.Q<Label>("GridSizeExtra");
        nameOfDB = root.Q<TextField>("DatabaseName");
        generateDatabase = root.Q<Button>("GenerateNewDatabase");
        addDatabase = root.Q<Button>("AddToDatabase");
        clearDatabase = root.Q<Button>("ClearDatabase");
        generateAll = root.Q<Button>("GenerateDatabaseIconsMenu");
        generateMenuFromDatabase = root.Q<Button>("GenerateFromDatabase");

        camera = root.Q<VisualElement>("CameraView");

        //sets up the buttons to have click events - generating the desired things
        generateMenu.RegisterCallback<ClickEvent>(GenerateMenu);
        generateIcons.RegisterCallback<ClickEvent>(IconGeneration);
        generateBoth.RegisterCallback<ClickEvent>(GenerateBoth);
        generateDatabase.RegisterCallback<ClickEvent>(GenerateDatabase);
        addDatabase.RegisterCallback<ClickEvent>(GenerateDatabase);
        clearDatabase.RegisterCallback<ClickEvent>(ClearDatabase);
        generateAll.RegisterCallback<ClickEvent>(GenerateAll);
        generateMenuFromDatabase.RegisterCallback<ClickEvent>(MenuFromDatabase);

        GameObject camPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/IconMaker/IconCamera.prefab"); //instantiates a camera to render the objects
        GameObject camOBJ = (GameObject)PrefabUtility.InstantiatePrefab(camPrefab);
        camOBJ.transform.position = new Vector3(-500, 0, 0);
        cam = camOBJ.GetComponent<Camera>(); //spawns in a new camera prefab to generate the icon from
    }

    public void Awake()
    {
        renderTexture = new RenderTexture((int)position.width,
            (int)position.height,
            (int)RenderTextureFormat.ARGB32);
    }


    public void Update()
    {
        if (cam != null)
        {
            cam.targetTexture = renderTexture;
            cam.Render();
            cam.targetTexture = null;
        }
        if (renderTexture.width != position.width ||
            renderTexture.height != position.height)
            renderTexture = new RenderTexture((int)position.width,
                (int)position.height,
                (int)RenderTextureFormat.ARGB32);
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), renderTexture);
        //setting the bool values to the values within the uxml file
        usingObjects = usingObjectsToggle.value;
        maxButtons = maxButtonCountText.value;
        buttonsPerRow = buttonsPerRowText.value;
        databaseName = nameOfDB.value;


        //displaying the number of objects per row (depending if you have decided to select objects for the menu or not)
        int rowNumber = 0;
        int buttonNumber = 0;
        if(usingObjects)
        {
            if (Selection.count < buttonsPerRow)
            {
                rowNumber = 1;
                buttonNumber = Selection.count;
            }
            else
            {
                rowNumber = Selection.count / buttonsPerRow;
                buttonNumber = buttonsPerRow;
            }
        }
        else
        {
            if (maxButtons < buttonsPerRow)
            {
                rowNumber = 1;
                buttonNumber = maxButtons;
            }
            else
            {
                rowNumber = maxButtons / buttonsPerRow;
                buttonNumber = buttonsPerRow;
            }
        }

        gridText.text = $"This will create a Grid of {rowNumber} rows, each with {buttonNumber} buttons";


        //calculates how many rows will be needed depending on the max objects and the specified row count
        int buttonDiff = rowNumber * buttonsPerRow;
        if(buttonDiff == maxButtons || buttonDiff == Selection.count)
        {
            gridTextExtra.style.display = DisplayStyle.None;
        }
        else
        {
            if (buttonNumber >= buttonsPerRow)
            {
                gridTextExtra.style.display = DisplayStyle.Flex;
                if (usingObjects)
                {
                    gridTextExtra.text = $"With an extra row of {Selection.count - buttonDiff} buttons";
                }
                else
                {
                    gridTextExtra.text = $"With an extra row of {maxButtons - buttonDiff} buttons";
                }
            }
        }

        //hides and displays certain visual elements within the UI depending if booleans are true/false
        if(usingObjects)
        {
            menuObjectCount.style.display = DisplayStyle.Flex;
            maxButtonCountText.style.display = DisplayStyle.None;

            //if you are using objects, you can't generate a menu if no objects are selected so the button is hidden
            if (Selection.gameObjects.Length == 0)
            {
                gridText.style.display = DisplayStyle.None;
                gridTextExtra.style.display = DisplayStyle.None;
                generateMenu.style.display = DisplayStyle.None;
            }
            else
            {
                gridText.style.display = DisplayStyle.Flex;
                generateMenu.style.display = DisplayStyle.Flex;
            }
        }
        else
        {
            menuObjectCount.style.display = DisplayStyle.None;
            maxButtonCountText.style.display = DisplayStyle.Flex;
            generateMenu.style.display = DisplayStyle.Flex;
            gridText.style.display = DisplayStyle.Flex;
        }    

        if(NewDatabase())
        {
            generateMenuFromDatabase.style.display = DisplayStyle.None;
        }
        else if (!NewDatabase())
        {
            generateMenuFromDatabase.style.display = DisplayStyle.Flex;
        }
    }

    //this is called when selection of gameobjects have been updated
    private void OnSelectionChange()
    {
        if (Selection.gameObjects.Length == 0)
        {
            menuObjectCount.text = "No Object Currently Selected";
            iconObjectCount.text = "No Object Currently Selected";
            generateIcons.style.display = DisplayStyle.None;
            generateBoth.style.display = DisplayStyle.None;
            addDatabase.style.display = DisplayStyle.None;
            clearDatabase.style.display = DisplayStyle.None;
            generateDatabase.style.display = DisplayStyle.None;
            generateAll.style.display = DisplayStyle.None;
        }
        else
        {
            menuObjectCount.text = $"{Selection.gameObjects.Length} Currently Selected Objects";
            iconObjectCount.text = $"{Selection.gameObjects.Length} Currently Selected Objects";
            generateIcons.style.display = DisplayStyle.Flex;
            generateBoth.style.display = DisplayStyle.Flex;

            //displays whether to add to database or create a new one based on if the name already exists
            if (NewDatabase())
            {
                generateDatabase.style.display = DisplayStyle.Flex;
                generateAll.style.display = DisplayStyle.Flex;
                addDatabase.style.display = DisplayStyle.None;
                clearDatabase.style.display = DisplayStyle.None;
            }
            else if (!NewDatabase())
            {
                generateDatabase.style.display = DisplayStyle.None;
                generateAll.style.display = DisplayStyle.None;
                addDatabase.style.display = DisplayStyle.Flex;
                clearDatabase.style.display = DisplayStyle.Flex;
            }
        }
    }

    /// <summary>
    /// geneates both icons and a menu for the icons
    /// </summary>
    /// <param name="evt"></param>
    void GenerateBoth(ClickEvent evt)
    {
        generatingBoth = true;
        maxButtons = Selection.count;
        IconGeneration(evt);
        GenerateMenu(evt);
    }

    /// <summary>
    /// generates a database and menu
    /// </summary>
    void GenerateAll(ClickEvent evt)
    {
        generatingAll = true;
        GenerateDatabase(evt);
        maxButtons = database.objectDatabase.Count;
        GenerateMenu(evt);
    }

    /// <summary>
    /// generates a menu from an exisitng database
    /// </summary>
    void MenuFromDatabase(ClickEvent evt)
    {
        generatingAll = true;
        maxButtons = database.objectDatabase.Count;
        GenerateMenu(evt);
    }

    /// <summary>
    /// Generates icons for objects in the game
    /// </summary>
    private void IconGeneration(ClickEvent evt)
    {
        if (spritesGenerated.Count > 0)
        {
            spritesGenerated.Clear();
        }

        foreach(var item in Selection.gameObjects)
        {
            iconName = item.name;
            GameObject camPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/IconMaker/IconCamera.prefab"); //instantiates a camera to render the objects
            GameObject camOBJ = (GameObject)PrefabUtility.InstantiatePrefab(camPrefab);
            camOBJ.transform.position = new Vector3(-500, 0, 0);
            cam = camOBJ.GetComponent<Camera>(); //spawns in a new camera prefab to generate the icon from
            GameObject newPrefab = (GameObject)PrefabUtility.InstantiatePrefab(item); //spawns in a new object in from of the camera for the camera to render
            newPrefab.transform.position = new Vector3(-500, 0, 6);
            newPrefab.transform.rotation = Quaternion.Euler(0, 150, 0);

            GetIcon(newPrefab);

            spritesGenerated.Add(AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Editor/IconMaker/Icons/{iconName} sprite.png")); //adds each made sprite to a list so the scriptable object can asign it for the buttons

            DestroyImmediate(newPrefab);
            DestroyImmediate(camOBJ);
        }
    }

    /// <summary>
    /// creates the Icon based on the camera renderer
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    Sprite GetIcon(GameObject obj)
    {
        //get dimensions for the screen
        int resX = cam.pixelWidth;
        int resY = cam.pixelHeight;

        //vairbales for making the image a square
        int clipX = 0;
        int clipY = 0;

        if (resX > resY) //if in landscape mode
            clipX = resX - resY; //gets the difference betweeen the width and height values
        else if (resY > resX) //in portrait mode
            clipY = resY - resX;
        //else image is already square

        //initialising the variables
        Texture2D tex = new(resX - clipX, resY - clipY, TextureFormat.RGBA32, false); //counts for screen resoultion to make image square
        RenderTexture rt = new(resX, resY, 24); //used for the camera
        cam.targetTexture = rt; //draw to the texture instead of screen
        RenderTexture.active = rt;

        cam.Render(); //telling camera to render now instead of waiting
        tex.ReadPixels(new Rect(clipX / 2, clipY / 2, resX - clipX, resY - clipY), 0, 0); //texture = rendertexture
        //starts in the middle of the screen to crop both side equally to make image square
        tex.Apply();

        //clean up
        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        string filePath = $"Assets/Editor/IconMaker/Icons/{iconName} sprite.png";
        // Encode texture into PNG
        byte[] bytes = ImageConversion.EncodeToPNG(tex);

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes(filePath, bytes);

        AssetDatabase.Refresh();

        //used to make the texture a sprite 2D so it can be used for the icons of each button
        TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("Could not TextureImport from path: " + filePath);
        }
        else
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }

    /// <summary>
    /// Function to generate menu buttons based on set amount
    /// </summary>
    void GenerateMenu(ClickEvent evt)
    {
        MenuUISO menu = ScriptableObject.CreateInstance<MenuUISO>(); //creates a new scriptable object with all of the button data for the menus
        menu.rowCount = buttonsPerRow;

        for (int i = 0; i < maxButtons; i++)
        {
            ButtonData button = new();
            button.name = "button" + i;

            if(generatingAll)
            {
                button.icon = database.objectDatabase[i].icon;
            }

            if (generatingBoth)
            {
                button.icon = spritesGenerated[i];
            }

            menu.buttons.Add(button);
        }
        generatingBoth = false;
        generatingAll = false;

        AssetDatabase.CreateAsset(menu, "Assets/Editor/IconMaker/MenuSO.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UIController controller = GameObject.FindAnyObjectByType<UIController>();
        controller.UI = menu;
        controller.AddButtons();
    }

    /// <summary>
    /// checking the validity of the database and whether the name already exists
    /// </summary>
    /// <returns></returns>
    bool NewDatabase()
    {
        dataTest = AssetDatabase.LoadAssetAtPath<ObjectDataBaseSO>($"Assets/Editor/IconMaker/{databaseName}.asset");

        if (dataTest == null)
        {
            dataTest = ScriptableObject.CreateInstance<ObjectDataBaseSO>();
            return true;
        }
        return false;
    }

    /// <summary>
    /// will generate a database for the selected objects
    /// </summary>
    void GenerateDatabase(ClickEvent evt)
    {
        IconGeneration(evt);
        for (int i = 0; i < Selection.count; i++)
        {
            ObjectData data = new();
            data.prefab = Selection.gameObjects[i];
            data.name = data.prefab.name;
            data.icon = spritesGenerated[i];

            bool inDataBase = false;
            //checking if the database already contains the selected item, if it does then continue to the next one
            if (!NewDatabase())
            {
                database = dataTest;
                foreach (ObjectData objects in database.objectDatabase)
                {
                    if (objects.prefab == data.prefab)
                    {
                        inDataBase = true;
                        break;
                    }
                }
            }
            database = dataTest;

            if (!inDataBase)
            {
                database.objectDatabase.Add(data);
            }
        }
        //create a new scriptable object if it's a new name
        if (NewDatabase())
        {
            AssetDatabase.CreateAsset(database, $"Assets/Editor/IconMaker/{databaseName}.asset");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void ClearDatabase(ClickEvent evt)
    {
        AssetDatabase.DeleteAsset($"Assets/Editor/IconMaker/{databaseName}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
