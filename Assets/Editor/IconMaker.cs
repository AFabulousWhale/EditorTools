using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using Unity.EditorCoroutines.Editor;
using UnityEditor.Animations;

public class IconMaker : EditorWindow
{
    string selectedObjectCount = "No Object Currently Selected";
    Camera cam;

    GameObject camOBJ, newPrefab, selectedOBJ;

    public int currentViewedObject = 0;

    Label selectedGOLabel, iconNameLabel, iconUsedLabel, animationLabel;
    ProgressBar iconAmount;
    Button leftButton, rightButton, generateIcon, generateAllIcons, autoNameButton;
    VisualElement iconPreviewBox, cameraView, popup, buttons, applyToAllToggles, iconLabels, animationSettings;
    TextField saveLocation;

    Toggle databaseAllToggle, databaseDatabaseToggle, databaseNewToggle;
    DropdownField animations;
    Slider animationFrame;
    ObjectField animationController;

    public DatabaseMaker databaseMaker;

    Data dataCheck;

    IconDataSO iconData, dataTest;

    EditorPopup iconPopup = new();

    Position pos => new(root);
    Rotation rot => new(root);
    Scale scale => new(root);
    BG BG => new(root);
    SpriteName spriteName => new(root);

    List<IconSettingsMain> settings => new() { pos, rot, scale, BG, spriteName };

    public List<GameObject> objectForItems = new();

    List<AnimationClip> animClips = new();
    List<string> animNames = new();
    List<AnimatorState> animStates = new();
    AnimatorState currentAnimatorState;
    Animator anim;
    string currentAnimName;

    Renderer prefabRend;
    GameObject rendererObject;

    public Sprite iconSprite;

    bool usingOtherMaker;

    private readonly VisualElement root;

    public IconMaker(VisualElement root)
    {
        this.root = root;
    }

    #region Editor Window Setup
    /// <summary>
    /// setting up values for the IconMaker TAB
    /// </summary>
    public void CreateGUI()
    {
        selectedGOLabel = root.Q<Label>("SelectedGOLabel");
        iconAmount = root.Q<ProgressBar>("DisplayNumber");
        iconNameLabel = root.Q<Label>("IconName");
        leftButton = root.Q<Button>("LeftArrow");
        rightButton = root.Q<Button>("RightArrow");
        iconPreviewBox = root.Q<VisualElement>("IconPreviewBox");
        cameraView = root.Q<VisualElement>("CameraView");
        generateIcon = root.Q<Button>("GenerateIconButton");
        generateAllIcons = root.Q<Button>("GenerateAllIconsButton");
        popup = root.Q<VisualElement>("PopupLayout");
        autoNameButton = root.Q<Button>("AutoNameButton");
        applyToAllToggles = root.Q<VisualElement>("IconToggles");
        iconLabels = root.Q<VisualElement>("IconLabels");
        saveLocation = root.Q<TextField>("IconSaveLocation");
        
        animationSettings = root.Q<VisualElement>("AnimationSettings");
        animations = root.Q<DropdownField>("Animation");
        animationFrame = root.Q<Slider>("AnimationFrame");
        animationController = root.Q<ObjectField>("AnimationController");
        animationLabel = root.Q<Label>("AnimationLabel");

        animationFrame.RegisterValueChangedCallback(AnimSliderCaller);
        animations.RegisterValueChangedCallback(AnimationUpdateCaller);
        animationController.RegisterValueChangedCallback(AnimControllerCaller);

        iconUsedLabel = root.Q<Label>("IconUsedLabel");
        buttons = root.Q<VisualElement>("IconButtons");

        databaseAllToggle = root.Q<Toggle>("DatabaseAllIcons");
        databaseDatabaseToggle = root.Q<Toggle>("DatabaseEveryIcons");
        databaseNewToggle = root.Q<Toggle>("DatabaseNewIcons");

        leftButton.RegisterCallback<ClickEvent, int>(NextIcon, -1);
        rightButton.RegisterCallback<ClickEvent, int>(NextIcon, 1);
        generateIcon.RegisterCallback<ClickEvent>(IconGeneration);
        generateAllIcons.RegisterCallback<ClickEvent>(GenerateAllIcons);
        autoNameButton.RegisterCallback<ClickEvent>(ResetName);

        databaseAllToggle.RegisterValueChangedCallback(ToggleChange);
        databaseDatabaseToggle.RegisterValueChangedCallback(ToggleChange);
        databaseNewToggle.RegisterValueChangedCallback(ToggleChange);

        iconPopup.twoButtons = false;
        iconPopup.labelText = "All Icons Have Been Created Sucessfully!";

        DatabaseToggleCheck();

        //if objects are still there from testing then destroy them so it won't mess up the new icon display
        DestroyImmediate(GameObject.Find("IconObjectPrefab"));
        DestroyImmediate(GameObject.Find("IconCamera"));
    }

    public void OnGUI()
    {
        if (cam != null)
        {
            cam.backgroundColor = BG.field.value; //setting the camera background color to the selected color for the icon
        }
    }
    #endregion End - Editor Window Setup

    #region Database and Menu Manager
    void ToggleChange(IChangeEvent evt)
    {
        databaseMaker.NewDatabase();
        databaseMaker.database = databaseMaker.dataTest;
        DatabaseToggleCheck();
        ObjectsSelected();
    }
    #endregion End - Database and Menu Manager

    #region Selected Object Changes
    /// <summary>
    /// Checks if any gameobjects are being selected
    /// </summary>
    public void OnSelectionChange()
    {
        objectForItems.Clear();

        foreach (var item in Selection.gameObjects)
        {
            if (ChildRendCheck(item)) //checking if the selected object has a mesh renderer
            {
                objectForItems.Add(item);
            }
        }
        currentViewedObject = 0;
        //displaying how many gameobjects have been selected
        if (objectForItems.Count > 0)
        {
            DatabaseToggleCheck();
            ObjectsSelected();
        }
        else if (objectForItems.Count == 0) //resets everything if no gameobjects are selected
        {
            iconLabels.style.display = DisplayStyle.Flex;
            generateAllIcons.style.display = DisplayStyle.None;
            rightButton.style.visibility = Visibility.Hidden;
            leftButton.style.visibility = Visibility.Hidden;
            if (selectedOBJ != null)
            {
                CheckApplyAll(selectedOBJ);
            }
            selectedOBJ = null;
            iconPreviewBox.style.display = DisplayStyle.None;
            selectedObjectCount = "No Object Currently Selected";
            if (camOBJ != null && newPrefab != null)
            {
                DestroyImmediate(camOBJ);
                DestroyImmediate(newPrefab);
            }
        }
        selectedGOLabel.text = selectedObjectCount;
    }

    /// <summary>
    /// Changes made when objects are selected - changing the name displayed and showing the camera preview
    /// </summary>
    public void ObjectsSelected()
    {
        iconLabels.style.display = DisplayStyle.None;
        generateAllIcons.style.display = DisplayStyle.None;
        //hides all the toggles if only 1 game object is selected
        applyToAllToggles.style.display = DisplayStyle.None;
        rightButton.style.visibility = Visibility.Hidden;
        leftButton.style.visibility = Visibility.Hidden;

        if (objectForItems.Count > 1)
        {
            //shows all the toggles if more than 1 game object is selected
            applyToAllToggles.style.display = DisplayStyle.Flex;
            if (!usingOtherMaker) //showing the right arrow
            {
                generateAllIcons.style.display = DisplayStyle.Flex;
                rightButton.style.visibility = Visibility.Visible; //shows right arrow as there is more than one object selected
                leftButton.style.visibility = Visibility.Hidden;
            }
            else //shows the left arrow because it starts from the selected objects
            {
                generateAllIcons.style.display = DisplayStyle.Flex;
                rightButton.style.visibility = Visibility.Hidden;
                leftButton.style.visibility = Visibility.Visible;
            }
        }
        if (objectForItems.Count > 0)
        {
            //spawns a camera and sets all the values to show the object currently selected
            SpawnCamera();
            if (selectedOBJ != null)
            {
                CheckApplyAll(selectedOBJ);
                //ResetData();
            }
            selectedOBJ = objectForItems[currentViewedObject];
            iconPreviewBox.style.display = DisplayStyle.Flex;
            iconAmount.lowValue = 1;
            iconAmount.highValue = objectForItems.Count;
            iconAmount.value = (currentViewedObject + 1);
            iconNameLabel.text = objectForItems[currentViewedObject].name;
            iconAmount.title = $"{iconAmount.value}/{iconAmount.highValue}";
            CheckDatabase();
        }
    }
    #endregion End - Selected Object Changes

    #region Animation Settings
    void DisplayAnimSettings()
    {
        //shows animation settings if the prefab has an animator
        if (AnimationCheck(newPrefab))
        {
            animationSettings.style.display = DisplayStyle.Flex;
            animationLabel.style.display = DisplayStyle.None;
            anim = newPrefab.GetComponent<Animator>();
            UpdateController();
        }
        else
        {
            animationSettings.style.display = DisplayStyle.None;
            animationLabel.style.display = DisplayStyle.Flex;
            ResetAnimData();
        }
    }

    //all called by the fields changing the values to update them
    void AnimControllerCaller(IChangeEvent evt)
    {
        UpdateController();
    }

    void AnimationUpdateCaller(IChangeEvent evt)
    {
        SetAnimStage();
    }

    void AnimSliderCaller(IChangeEvent evt)
    {
        ChangeAnimStage();
    }

    /// <summary>
    /// updates the drop down display for the clips you can play
    /// </summary>
    /// <param name="evt"></param>
    void UpdateController()
    {
        if (animationController.value != null)
        {
            anim.runtimeAnimatorController = (RuntimeAnimatorController)animationController.value;
            animClips.Clear();
            animNames.Clear();
            animStates.Clear();
            animations.choices.Clear();
            for (int i = 0; i < anim.runtimeAnimatorController.animationClips.Length; i++)
            {
                Debug.Log("adding components");
                animClips.Add(anim.runtimeAnimatorController.animationClips[i]);

                AnimatorController ac = anim.runtimeAnimatorController as AnimatorController;
                AnimatorControllerLayer[] acLayers = ac.layers;

                //loops through the anim states on the controller and adds them to a list, in case there is more than one so they can be selected from the drop down
                foreach (AnimatorControllerLayer layer in acLayers)
                {
                    ChildAnimatorState[] animStatesLocal = layer.stateMachine.states;
                    foreach (ChildAnimatorState j in animStatesLocal)
                    {
                        animStates.Add(j.state);
                    }
                }
                animNames.Add(ac.animationClips[i].name);
            }
            animations.choices = animNames;
            foreach (var item in animNames)
            {
                if(item == currentAnimName)
                {
                    animations.value = currentAnimName;
                    break;
                }
                else
                {
                    animations.value = animNames[0];
                }
            }
            SetAnimStage();
        }
    }

    /// <summary>
    /// makes the slider values the same length of the clip
    /// </summary>
    /// <param name="evt"></param>
    void SetAnimStage()
    {
        if (animations.value != null)
        {
            for (int i = 0; i < animNames.Count; i++)
            {
                if (animNames[i] == animations.value)
                {
                    Debug.Log("found anim");
                    animationFrame.highValue = animClips[i].length;
                    currentAnimatorState = animStates[i];
                }
            }
            ChangeAnimStage();
        }
    }

    /// <summary>
    /// sets the frame of the animation based on the slider
    /// </summary>
    /// <param name="evt"></param>
    void ChangeAnimStage()
    {
        if (animationController.value != null && animations.value != null)
        {
            Debug.Log("updating anim");
            anim.speed = 0.0f;
            anim.Play(currentAnimatorState.name, 0, animationFrame.value);
            anim.Update(Time.deltaTime);

            cameraView.style.backgroundImage = GetRenderTexture();
        }
    }
    #endregion End - Animation Settings

    #region Icon Setup
    /// <summary>
    /// if the camera doesn't already exist then spawns a new one for icon creation
    /// </summary>
    void SpawnCamera()
    {
        //spawns in the camera object if it doesn't already exist
        if (camOBJ == null)
        {
            GameObject camPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/IconMaker/IconCamera.prefab"); //instantiates a camera to render the objects
            camOBJ = (GameObject)PrefabUtility.InstantiatePrefab(camPrefab);
            camOBJ.transform.position = new Vector3(-500, 0, 0);
            cam = camOBJ.GetComponent<Camera>(); //spawns in a new camera prefab to generate the icon from
        }
        SpawnPrefab();
    }

    /// <summary>
    /// spawns a new prefab for each object when it is currently selected to view
    /// </summary>
    void SpawnPrefab()
    {
        DestroyImmediate(newPrefab);
        newPrefab = (GameObject)PrefabUtility.InstantiatePrefab(objectForItems[currentViewedObject]); //spawns in a new object in from of the camera for the camera to render
        newPrefab.transform.position = pos.defaultValue;
        newPrefab.transform.rotation = Quaternion.Euler(rot.defaultValue);
        newPrefab.transform.localScale = new Vector3(scale.defaultValue, scale.defaultValue, scale.defaultValue);
        newPrefab.name = "IconObjectPrefab";
        anim = newPrefab.GetComponent<Animator>();

        foreach (var item in settings)
        {
            item.currentPrefab = newPrefab;
            item.cam = cam;
        }
        prefabRend = rendererObject.GetComponent<Renderer>();
        cam.orthographicSize = prefabRend.bounds.extents.y + prefabRend.bounds.extents.x;
        EditorCoroutineUtility.StartCoroutineOwnerless(WaitForObjectToSpawn());
        IEnumerator WaitForObjectToSpawn()
        {
            yield return new WaitUntil(() => newPrefab != null);
            DisplayAnimSettings();
        }
    }
    #endregion End - Icon Setup

    #region Data Handling

    /// <summary>
    /// saves the changed values for the prefab to a scriptable object so it can be accessed again
    /// </summary>
    public void SaveIconData(Data data)
    {
        //checking if object already exists in the database and the database already exists
        if (!NewDatabase())
        {
            iconData = dataTest; //this is the database that has been found/a new one
            for (int i = 0; i < iconData.IconData.Count; i++)//for each item already saved
            {
                //updates the data if it already exists in the database
                if (iconData.IconData[i].prefab == data.prefab)
                {
                    if (iconData.IconData[i] != data) //checks if the data has been changed
                    {
                        EditorUtility.SetDirty(iconData);
                        iconData.IconData[i] = data;
                    }
                    break;
                }
            }
        }
        iconData = dataTest;

        if (!DatabaseCheck(data.prefab))
        {
            EditorUtility.SetDirty(iconData);
            iconData.IconData.Add(data); //adds the new data to the list
        }

        if (NewDatabase())
        {
            AssetDatabase.CreateAsset(iconData, "Assets/Editor/IconMaker/IconData.asset");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    void ResetData()
    {
        rot.field.value = new Vector3(0, 0, 0);
        pos.field.value = new Vector3(0, 0, 0);
        scale.field.value = 1;
        BG.field.value = new Color32(255, 255, 255, 0);
        spriteName.field.value = selectedOBJ.name;
        ResetAnimData();
    }

    void ResetAnimData()
    {
        if (AnimationCheck(selectedOBJ))
        {
            animationController.value = null;
            animationFrame.value = 0;
            animations.choices.Clear();
            animations.value = null;
        }
    }

    void ResetName(ClickEvent evt)
    {
        spriteName.field.value = selectedOBJ.name;
    }
    #endregion End - Data Handling

    #region Checks

    /// <summary>
    /// Checks if the save location entered exists
    /// </summary>
    /// <returns></returns>
    bool FilePathCheck()
    {
        //check if directory doesn't exit
        if (Directory.Exists(saveLocation.value))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// checks if the passed in object has an animator
    /// </summary>
    /// <param name="objectToCheck"></param>
    /// <returns></returns>
    bool AnimationCheck(GameObject objectToCheck)
    {
        if(objectToCheck.GetComponent<Animator>())
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// checks the passed in object, if it's visible and has a renderer as well as the children of said object
    /// </summary>
    /// <param name="objectToCheck"></param>
    /// <returns></returns>
    public bool ChildRendCheck(GameObject objectToCheck)
    {
        if(RendererCheck(objectToCheck))
        {
            return true;
        }

        if (objectToCheck.transform.childCount != 0) //check children too if there is any
        {
            foreach (Transform child in objectToCheck.transform)
            {
                if (RendererCheck(child.gameObject))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// checks if the passed in object has a renderer
    /// </summary>
    /// <param name="objectToCheck"></param>
    /// <returns></returns>
    public bool RendererCheck(GameObject objectToCheck)
    {
        if(objectToCheck.activeSelf) //if the gameobject is visible
        {
            if(objectToCheck.GetComponent<Renderer>())
            {
                rendererObject = objectToCheck;
                return true;
            }
        }
        return false;
    }
    void DatabaseToggleCheck()
    {
        if (databaseDatabaseToggle.value)
        {
            usingOtherMaker = true;
            iconUsedLabel.text = "The 'Icon Maker' is being used by the 'Database Maker'";
            iconUsedLabel.style.display = DisplayStyle.Flex;
            buttons.style.display = DisplayStyle.None;
            objectForItems.Clear();
            foreach (var item in databaseMaker.database.objectDatabase)
            {
                objectForItems.Add(item.prefab);
            }
            foreach (var item in Selection.gameObjects)
            {
                if (ChildRendCheck(item)) //checking if the selected object has a mesh renderer
                {
                    objectForItems.Add(item);
                }
            }
            currentViewedObject = (objectForItems.Count - 1);
        }
        else
        {
            usingOtherMaker = false;
            iconUsedLabel.style.display = DisplayStyle.None;
            buttons.style.display = DisplayStyle.Flex;
            objectForItems.Clear();
            if (Selection.gameObjects.Length > 0)
            {
                foreach (var item in Selection.gameObjects)
                {
                    if (ChildRendCheck(item)) //checking if the selected object has a mesh renderer
                    {
                        objectForItems.Add(item);
                    }
                }
            }
            currentViewedObject = 0;
        }
    }

    /// <summary>
    /// checking the validity of the database and whether the name already exists
    /// </summary>
    /// <returns></returns>
    bool NewDatabase()
    {
        dataTest = AssetDatabase.LoadAssetAtPath<IconDataSO>($"Assets/Editor/IconMaker/IconData.asset");

        if (dataTest == null)
        {
            dataTest = ScriptableObject.CreateInstance<IconDataSO>();
            return true;
        }
        return false;
    }

    /// <summary>
    /// checks if prefab is in database and sets the instantiated prefab values to the saved ones
    /// </summary>
    public void CheckDatabase()
    {
        if(DatabaseCheck(selectedOBJ))
        {
            rot.field.value = dataCheck.rotOffset;
            pos.field.value = dataCheck.posOffset;
            scale.field.value = dataCheck.scaleOffset;
            BG.field.value = dataCheck.BGColor;
            spriteName.field.value = dataCheck.spriteName;
            if (AnimationCheck(selectedOBJ))
            {
                animationFrame.value = dataCheck.animFrame;
                animationController.value = dataCheck.animationController;
                currentAnimName = dataCheck.animName;
                UpdateController();
            }
            foreach (var setting in settings)
            {
                setting.ChangeSingleValues(newPrefab);
            }
        }
        else
        {
            ResetData();
            foreach (var setting in settings)
            {
                setting.ChangeSingleValues(newPrefab);
            }
        }
    }

    /// <summary>
    /// Sets the data passed in to the currently stored item versions and returns this data - this is done before the data is overwritten and saved so it doesn't overwrite the previously saved versions
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Data</returns>
    public Data CheckData(Data data)
    {
        if(DatabaseCheck(data.prefab))
        {
            data.rotOffset = dataCheck.rotOffset;
            data.posOffset = dataCheck.posOffset;
            data.scaleOffset = dataCheck.scaleOffset;
            data.BGColor = dataCheck.BGColor;
            data.spriteName = dataCheck.spriteName;
            if (AnimationCheck(data.prefab))
            {
                data.animationController = dataCheck.animationController;
                data.animFrame = dataCheck.animFrame;
                data.animName = dataCheck.animName;
            }
        }
        return data;
    }

    /// <summary>
    /// checks if the item passed in is currently in the database
    /// </summary>
    /// <param name="data"></param>
    /// <returns>bool</returns>
    public bool DatabaseCheck(GameObject obj)
    {
        //if item in database
        if (!NewDatabase())
        {
            iconData = dataTest;
            foreach (var item in iconData.IconData)
            {
                if (item.prefab == obj)
                {
                    dataCheck = item;
                    return true;
                }
            }
        }
        dataCheck = null;
        return false;
    }

    /// <summary>
    /// checks if the toggle in on for all objects to share the value
    /// </summary>
    /// <param name="obj"></param>
    void CheckApplyAll(GameObject obj)
    {
        Data data = new();
        data.prefab = obj;
        data.name = obj.name;

        foreach (var setting in settings)
        {
            setting.SaveIconDataCheck(data, setting);
        }
        if (AnimationCheck(obj))
        {
            data.animationController = (RuntimeAnimatorController)animationController.value;
            data.animFrame = animationFrame.value;
            data.animName = animations.value;
        }
    }
    #endregion End - Checks

    #region Icon Generation

    void ErrorPopup()
    {
        iconPopup.labelText = "The specified save location does not exist, please enter a valid file path";
        UnityEditor.PopupWindow.Show(popup.worldBound, iconPopup);
    }

    /// <summary>
    /// shows the next icon in the Gameobject Selection array
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="amount"></param>
    public void NextIcon(ClickEvent evt, int amount)
    {
        CheckApplyAll(selectedOBJ);
        currentViewedObject += amount;
        currentViewedObject = Mathf.Clamp(currentViewedObject, 0, (objectForItems.Count - 1));
        selectedOBJ = objectForItems[currentViewedObject];
        iconNameLabel.text = objectForItems[currentViewedObject].name; //setting the name display to the currently selected item within the list
        iconAmount.value = (currentViewedObject + 1);
        iconAmount.title = $"{iconAmount.value}/{iconAmount.highValue}";
        SpawnPrefab();
        CheckDatabase();

        if (currentViewedObject == objectForItems.Count - 1) //if you're at the end of the list then can't go right anymore but can go left
        {
            rightButton.style.visibility = Visibility.Hidden;
            leftButton.style.visibility = Visibility.Visible;
        }
        else if (currentViewedObject == 0) //if at the start of the list again then can go right but not left
        {
            leftButton.style.visibility = Visibility.Hidden;
            rightButton.style.visibility = Visibility.Visible;
        }
        else
        {
            rightButton.style.visibility = Visibility.Visible;
            leftButton.style.visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Generates all selected gameobjects as icons
    /// </summary>
    /// <param name="evt"></param>
    public void GenerateAllIcons(ClickEvent evt)
    {
        if (FilePathCheck())
        {
            iconPopup.labelText = "All Icons Have Been Created Sucessfully!";
            if (currentViewedObject != 0)
            {
                NextIcon(evt, -currentViewedObject);
            }
            IconGeneration(evt); //generates the first icon

            for (int i = 0; i < (objectForItems.Count); i++) //progresses the icon and generates it for every selected gameobject
            {
                NextIcon(evt, 1);
                IconGeneration(evt);
            }
            UnityEditor.PopupWindow.Show(popup.worldBound, iconPopup);
        }
        else
        {
            ErrorPopup();
        }
    }

    /// <summary>
    /// Generates icons for objects in the game
    /// </summary>
    public void IconGeneration(ClickEvent evt)
    {
        if (FilePathCheck())
        {
            iconPopup.labelText = "All Icons Have Been Created Sucessfully!";
            spriteName.field.value = selectedOBJ.name;
            GetIcon(GetRenderTexture());
            if (objectForItems.Count == 1)
            {
                UnityEditor.PopupWindow.Show(popup.worldBound, iconPopup);
            }
        }
        else
        {
            ErrorPopup();
        }
    }

    /// <summary>
    /// Returns the rendertexture as a texture2D of the camera so it can be displayed in the UI
    /// </summary>
    /// <returns></returns>
    public Texture2D GetRenderTexture()
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
        return tex;
    }

    /// <summary>
    /// creates the Icon based on the camera renderer
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public void GetIcon(Texture2D tex)
    {
        string filePath = $"{saveLocation.value}/{spriteName.field.value} sprite.png";
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
        iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
    }

#endregion End - Icon Generation

}
