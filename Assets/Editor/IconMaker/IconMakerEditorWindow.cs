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


    Label selectedObjectCount, gridText, gridTextExtra;
    bool usingObjects;
    Toggle usingObjectsToggle;
    SliderInt buttonCountText, rowCountText;
    Button generateMenu;

    [MenuItem("Window/EditorTools/Icon&ButtonMaker")]
    public static void ShowExample()
    {
        EditorWindow.GetWindow<IconMakerEditorWindow>("Icon and Button Maker"); //shows the icon window if not already displayed
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement UXMLFile = tree.Instantiate();
        root.Add(UXMLFile);

        selectedObjectCount = root.Q<Label>("SelectedObjectCount");
        buttonCountText = root.Q<SliderInt>("MaxButtonCountSlider");
        rowCountText = root.Q<SliderInt>("ButtonsPerRowSlider");
        usingObjectsToggle = root.Q<Toggle>("UsingObjectToggle");
        generateMenu = root.Q<Button>("GenerateMenuButton");
        gridText = root.Q<Label>("GridSize");
        gridTextExtra = root.Q<Label>("GridSizeExtra");

        //else
        //{
        //    GUILayout.Label($"{Selection.gameObjects.Length} Currently Selected Objects");
        //    if (GUILayout.Button("Create Icon(s) From Selected Object(s)"))
        //    {
        //        IconGeneration();
        //    }
        //    if (GUILayout.Button("Create Icon(s) And Button(s) From Selected Object(s)"))
        //    {
        //        IconGeneration();
        //        GenerateButtons();
        //    }
        //}
    }

    private void OnGUI()
    {
        usingObjects = usingObjectsToggle.value;
        maxButtons = buttonCountText.value;
        buttonsPerRow = rowCountText.value;

        int rowNumber = maxButtons / buttonsPerRow;
        gridText.text = $"This will create a Grid of {rowNumber} rows, each with {buttonsPerRow} buttons";

        int buttonDiff = rowNumber * buttonsPerRow;
        if(buttonDiff == maxButtons)
        {
            gridTextExtra.style.display = DisplayStyle.None;
        }
        else
        {
            gridTextExtra.style.display = DisplayStyle.Flex;
            gridTextExtra.text = $"With an extra row of {maxButtons - buttonDiff} buttons";
        }

        if(usingObjects)
        {
            buttonCountText.style.display = DisplayStyle.None;
            rowCountText.style.display = DisplayStyle.None;
            gridText.style.display = DisplayStyle.None;
            gridTextExtra.style.display = DisplayStyle.None;

            if (Selection.gameObjects.Length == 0)
            {
                generateMenu.style.display = DisplayStyle.None;
            }
            else
            {
                generateMenu.style.display = DisplayStyle.Flex;
            }
        }
        else
        {
            buttonCountText.style.display = DisplayStyle.Flex;
            rowCountText.style.display = DisplayStyle.Flex;
            generateMenu.style.display = DisplayStyle.Flex;
            gridText.style.display = DisplayStyle.Flex;
        }    
    }

    //this is called when selection of gameobjects have been updated
    private void OnSelectionChange()
    {
        if (Selection.gameObjects.Length == 0)
        {
            selectedObjectCount.text = "No Object Currently Selected";
        }
        else
        {
            selectedObjectCount.text = $"{Selection.gameObjects.Length} Currently Selected Objects";
        }
    }

    /// <summary>
    /// Generates icons for objects in the game
    /// </summary>
    private void IconGeneration()
    {
        spritesGenerated.Clear();
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
    void GenerateButtons()
    {
        MenuUISO menu = ScriptableObject.CreateInstance<MenuUISO>(); //creates a new scriptable object with all of the button data for the menus
        menu.rowCount = buttonsPerRow;
        for (int i = 0; i < spritesGenerated.Count; i++)
        {
            ButtonData newButton = new();
            newButton.name = "button" + i;
            newButton.icon = spritesGenerated[i];
            menu.buttons.Add(newButton);
        }
        AssetDatabase.CreateAsset(menu, "Assets/Editor/IconMaker/MenuSO.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (Application.isPlaying) //regenerates buttons if in play mode - to update the menu visuals
        {
            UIController controller = GameObject.FindAnyObjectByType<UIController>();
            controller.UI = menu;
            controller.AddButtons();
        }
    }
}
