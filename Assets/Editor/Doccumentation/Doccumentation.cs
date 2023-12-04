using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class Doccumentation : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset tree = default;

    [SerializeField]
    GameObject VideoPlayerGO = default;

    #region Menu Setup

    #region Icons
    List<string> iconBasicSettings = new() { "Position", "Rotation", "Scale", "Background" };
    List<string> iconBasicDesc = new()
    {
        "The Position Offset lets you change the location of the object within the icon, across both the X and Y axis.",
        "The Rotation Offset lets you rotate the object within the icon, which can be done along any axis.",
        "The Scale Multiplier lets you change the size of the object within the icon, making it either bigger or smaller.",
        "The Background Color lets you change the background of the icon, you can change the colour and opacity to anything you wish!"
    };
    List<string> iconAnimationSettings = new() { "Animation Default State", "Choosing An Animation Controller", "Choosing An Animation", "Playing The Animation" };
    List<string> iconAnimDesc = new()
    {
        "If the object selected has an animator attached to it, the settings shown in the GIF will appear under the 'Animation' Foldout menu.",
        "The Animation Controller is in charge of what animations can be played, they usually contain one, or a series of Animation clips for the object to be animated with.",
        "Once a controller has been selected, the first clip will automatically be selected, however the drop-down menu contains all the clips within that controller for you to use.",
        "The animation can then be set to any frame you wish to use within your icon, adding that bit more pazazz!"
    };
    List<string> multipleIconSettings = new() { "Multiple Icons", "Share One Value", "Share Multiple Values", "Share All Values" };
    List<string> iconMultiDesc = new()
    {
        "When having multiple objects selected, they can be very different from each other, but you may wish to have the objects share some values.",
        "If you tick one of the 'Apply To All' toggle boxes, that value will be shared between every object you have currently selected.",
        "You can even share multiple values at once if you wish, it's up to you how you want to modify your icons.",
        "Even sharing every value is possible, just check all the boxes and there you have it!"
    };
    #endregion Icons
    #region Menus
    List<string> setMenuSettings = new() { "Total Button Count", "Set Value Menu Example"};
    List<string> setMenuDesc = new()
    {
        "The Total Button Count Slider dertermins how many buttons you want within your menu in total.",
        "These are some example of menus created using the sliders."
    };
    List<string> selectionMenuSettings = new() { "Selection Menu", "Selection Menu Icons", "Selection Menu Example" };
    List<string> selectionMenuDesc = new()
    {
        "Instead of using a Slider to determin how many buttons you want within your menu, instead you can use the number of selected objects.",
        "With the objects you have selected you can also use the 'Icon Maker' to generate icons that will be used for the buttons.",
        "This is an example of a menu created with the icons used for the buttons."
    };
    List<string> DBMenuSettings = new() { "No Database", "Database Menu", "Database Menu With Icons", "Database Menu Example" };
    List<string> DBMenuDesc = new()
    {
        "If you haven't created a database using the 'Database Maker' then you will be prompted to create one, to be able to use it with the 'Menu Maker'.",
        "Once a database has been created, the total button count will be the number of items used within the database - which can be updated at any time.",
        "If you have created Icons for the objects within the database, then you will also be able to use these icons for the menu.",
        "This is an example of a menu created using a database and its icons!"
    };
    #endregion Menus
    List<List<string>> iconStages => new() { iconBasicSettings, iconAnimationSettings, multipleIconSettings };
    List<List<string>> menuStages => new() { setMenuSettings, selectionMenuSettings, DBMenuSettings};
    List<List<string>> DBStages;

    List<List<string>> iconDesc => new() { iconBasicDesc, iconAnimDesc, iconMultiDesc };
    List<List<string>> menuDesc => new() { setMenuDesc, selectionMenuDesc, DBMenuDesc };
    List<List<string>> DBDesc => new() { };

    List<string> iconSubTabNames = new() { "Basic Settings", "Animation Settings", "Multiple Icons"};
    List<string> menuSubTabNames = new() { "Set Values", "Selected Objects", "Database"};
    List<string> DBSubTabNames = new() { };

    List<List<List<string>>> stages => new() { iconStages, menuStages};
    List<List<List<string>>> descriptions => new() { iconDesc, menuDesc};

    List<List<string>> subTabNames => new() { iconSubTabNames, menuSubTabNames, DBSubTabNames };
    #endregion Menu Setup

    string selectedMenu;
    int selectedMenuIndex;
    int selectedSubMenuIndex = 0;

    ProgressBar progress;

    Button iconMaker, menuMaker, DBMaker;
    List<Button> tabs => new() { iconMaker, menuMaker, DBMaker };
    List<Button> subTabList => new() { subTab1, subTab2, subTab3 };
    string tabClass = "selectedTAB";

    VisualElement lArrow, rArrow;
    VisualElement imageDisplay, subTabs;
    Button subTab1, subTab2, subTab3;
    Label sectionTitle, Desc;

    Animator anim;
    GameObject sprite;

    VisualElement mainDisplay, startDisplay;

    [MenuItem("Window/UI Toolkit/Doccumentation")]
    public static void ShowExample()
    {
        Doccumentation wnd = GetWindow<Doccumentation>();
        wnd.titleContent = new GUIContent("Doccumentation");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement UXMLFile = tree.Instantiate();
        root.Add(UXMLFile);

        iconMaker = root.Q<Button>("IconMaker");
        menuMaker = root.Q<Button>("MenuMaker");
        DBMaker = root.Q<Button>("DatabaseMaker");

        progress = root.Q<ProgressBar>("Progress");

        imageDisplay = root.Q<VisualElement>("Image");

        mainDisplay = root.Q<VisualElement>("Main");
        startDisplay = root.Q<VisualElement>("Start");

        rArrow = root.Q<VisualElement>("RArrow");
        lArrow = root.Q<VisualElement>("LArrow");

        sectionTitle = root.Q<Label>("SectionTitle");
        Desc = root.Q<Label>("Description");

        subTabs = root.Q<VisualElement>("Middle-Panel");
        subTab1 = root.Q<Button>("FirstSection");
        subTab2 = root.Q<Button>("SecondSection");
        subTab3 = root.Q<Button>("ThirdSection");

        subTab1.RegisterCallback<ClickEvent, int>(ChangeSubMenu, 0);
        subTab2.RegisterCallback<ClickEvent, int>(ChangeSubMenu, 1);
        subTab3.RegisterCallback<ClickEvent, int>(ChangeSubMenu, 2);

        rArrow.RegisterCallback<ClickEvent, int>(ProgressStage, 1);
        lArrow.RegisterCallback<ClickEvent, int>(ProgressStage, -1);

        iconMaker.RegisterCallback<ClickEvent, string>(ChangeMakerType, "icon");
        menuMaker.RegisterCallback<ClickEvent, string>(ChangeMakerType, "menu");
        DBMaker.RegisterCallback<ClickEvent, string>(ChangeMakerType, "DB");

        DestroyImmediate(GameObject.Find("DoccumentationGif"));

        if (!sprite)
        {
            sprite = (GameObject)PrefabUtility.InstantiatePrefab(VideoPlayerGO);
            anim = sprite.GetComponent<Animator>();
            anim.Play($"{stages[selectedMenuIndex][selectedSubMenuIndex][(int)progress.value]}");
        }
    }

    private void OnInspectorUpdate()
    {
        if (anim != null)
        {
            anim.Update(Time.deltaTime);
            imageDisplay.style.backgroundImage = Background.FromSprite(sprite.GetComponent<SpriteRenderer>().sprite);
        }
    }

    /// <summary>
    /// will change what type of creator is being displayed and it's stages
    /// </summary>
    /// <param name="evt"></param>
    void ChangeMakerType(ClickEvent evt, string type)
    {
        if (selectedMenu != type) //if a different menu button has been pressed compared to the current one being shown
        {
            selectedMenu = type;
            progress.value = 1;
            SelectSubTab(0);
            ArrowChange();

            switch (type)
            {
                case "icon":
                    SelectTab(0);
                    break;
                case "menu":
                    SelectTab(1);
                    break;
                case "DB":
                    SelectTab(2);
                    break;
            }
            progress.title = $"1/{progress.highValue}";
            mainDisplay.style.display = DisplayStyle.Flex;
            startDisplay.style.display = DisplayStyle.None;
            subTabs.style.visibility = Visibility.Visible;
        }
    }

    void ChangeSubMenu(ClickEvent evt, int index)
    {
        SelectSubTab(index);
        progress.value = 1;
        ChangeDisplayValues();
        ArrowChange();
    }

    void ProgressStage(ClickEvent evt, int value)
    {
        progress.value = progress.value + value;
        progress.value = Mathf.Clamp(progress.value, 1, progress.highValue);
        ArrowChange();
        ChangeDisplayValues();
    }

    void ArrowChange()
    {
        lArrow.style.visibility = Visibility.Visible;
        rArrow.style.visibility = Visibility.Visible;

        if (progress.value == 1)
        {
            lArrow.style.visibility = Visibility.Hidden;
        }
        if (progress.value == progress.highValue)
        {
            rArrow.style.visibility = Visibility.Hidden;
        }
    }
    void ChangeDisplayValues()
    {
        subTab1.text = subTabNames[selectedMenuIndex][0];
        subTab2.text = subTabNames[selectedMenuIndex][1];
        subTab3.text = subTabNames[selectedMenuIndex][2];

        progress.highValue = stages[selectedMenuIndex][selectedSubMenuIndex].Count;
        Desc.text = $"{descriptions[selectedMenuIndex][selectedSubMenuIndex][((int)progress.value - 1)]}";
        sectionTitle.text = $"{stages[selectedMenuIndex][selectedSubMenuIndex][((int)progress.value - 1)]}";
        progress.title = $"{progress.value}/{progress.highValue}";
        anim.Play($"{stages[selectedMenuIndex][selectedSubMenuIndex][((int)progress.value - 1)]}"); //plays the specified animation name "for the gif to play"
    }

    void DeselectTab()
    {
        tabs[selectedMenuIndex].RemoveFromClassList(tabClass); //this tab is no longer selected
    }

    void SelectTab(int index)
    {
        DeselectTab();
        selectedMenuIndex = index;

        tabs[selectedMenuIndex].AddToClassList(tabClass); //this tab is selected
        ChangeDisplayValues();
    }

    void DeselectSubTab()
    {
        subTabList[selectedSubMenuIndex].RemoveFromClassList(tabClass); //this tab is no longer selected
    }

    void SelectSubTab(int index)
    {
        DeselectSubTab();
        selectedSubMenuIndex = index;

        subTabList[selectedSubMenuIndex].AddToClassList(tabClass); //this tab is selected
    }
}
