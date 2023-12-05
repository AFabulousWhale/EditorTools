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
    #region Database
    List<string> newDBSettings = new() { "Default Database", "Creating Database" };
    List<string> newDBDesc = new()
    {
        "If you don't have any objects selected then you can't create a database.",
        "Once selecting objects, you will be given the option to add these items to a scriptable object database."
    };
    List<string> existingDBSettings = new() { "Already In Database", "Add To Database", "Clear Database" };
    List<string> existingDBDesc = new()
    {
        "If an object is already within the database, you will get a message saying they won't be re-added. This doesn't affect icons, just the object itself.",
        "The Database can be changed, and more objects can be added at any time.",
        "You can also clear the database of all of its content."
    };
    List<string> DBIconSettings = new() { "Creation Icons", "Selected Object Icons", "All Icons", "Database Icons" };
    List<string> DBIconDesc = new()
    {
        "When first creating the database, you can also create icons with the objects, that can be customised in the 'Icon Maker' TAB.",
        "When adding new items to the database, you can choose to generate icons only for the new objects you are adding",
        "Or you can generate icons for everything within the database AND the new objects.",
        "Even if objects aren't selected, you can choose to generate icons for the objects currently within the database!"
    };
    #endregion Database
    List<List<string>> iconStages => new() { iconBasicSettings, iconAnimationSettings, multipleIconSettings };
    List<List<string>> menuStages => new() { setMenuSettings, selectionMenuSettings, DBMenuSettings};
    List<List<string>> DBStages => new() { newDBSettings, existingDBSettings, DBIconSettings };

    List<List<string>> iconDesc => new() { iconBasicDesc, iconAnimDesc, iconMultiDesc };
    List<List<string>> menuDesc => new() { setMenuDesc, selectionMenuDesc, DBMenuDesc };
    List<List<string>> DBDesc => new() { newDBDesc, existingDBDesc, DBIconDesc};

    List<string> iconSubTabNames = new() { "Basic Settings", "Animation Settings", "Multiple Icons"};
    List<string> menuSubTabNames = new() { "Set Values", "Selected Objects", "Database"};
    List<string> DBSubTabNames = new() { "New Database", "Existing Database", "Icons"};

    List<List<List<string>>> stages => new() { iconStages, menuStages, DBStages};
    List<List<List<string>>> descriptions => new() { iconDesc, menuDesc, DBDesc};

    List<List<string>> subTabNames => new() { iconSubTabNames, menuSubTabNames, DBSubTabNames };
    #endregion Menu Setup

    string selectedMenu;
    int selectedMenuIndex;
    int selectedSubMenuIndex = 0;

    ProgressBar progress;

    Button homePage, iconMaker, menuMaker, DBMaker;
    List<Button> tabs => new() { homePage, iconMaker, menuMaker, DBMaker };
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

        homePage = root.Q<Button>("MainMenu");
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

        homePage.RegisterCallback<ClickEvent>(MainMenu);
        iconMaker.RegisterCallback<ClickEvent, string>(ChangeMakerType, "icon");
        menuMaker.RegisterCallback<ClickEvent, string>(ChangeMakerType, "menu");
        DBMaker.RegisterCallback<ClickEvent, string>(ChangeMakerType, "DB");

        DestroyImmediate(GameObject.Find("DoccumentationGif"));

        if (!sprite)
        {
            sprite = (GameObject)PrefabUtility.InstantiatePrefab(VideoPlayerGO);
            anim = sprite.GetComponent<Animator>();
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
                    SelectTab(1);
                    break;
                case "menu":
                    SelectTab(2);
                    break;
                case "DB":
                    SelectTab(3);
                    break;
            }
            progress.title = $"1/{progress.highValue}";
            mainDisplay.style.display = DisplayStyle.Flex;
            startDisplay.style.display = DisplayStyle.None;
            subTabs.style.visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// called by the homepage button to show the start "menu"
    /// </summary>
    /// <param name="evt"></param>
    void MainMenu(ClickEvent evt)
    {
        mainDisplay.style.display = DisplayStyle.None;
        startDisplay.style.display = DisplayStyle.Flex;
        subTabs.style.visibility = Visibility.Hidden;
        SelectTab(0);
    }

    /// <summary>
    /// changed which sub menu is selected and shows the correct image/description
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="index"></param>
    void ChangeSubMenu(ClickEvent evt, int index)
    {
        SelectSubTab(index);
        progress.value = 1;
        ChangeDisplayValues();
        ArrowChange();
    }

    /// <summary>
    /// progresses the stage in each sub menu
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="value"></param>
    void ProgressStage(ClickEvent evt, int value)
    {
        progress.value = progress.value + value;
        progress.value = Mathf.Clamp(progress.value, 1, progress.highValue);
        ArrowChange();
        ChangeDisplayValues();
    }

    /// <summary>
    /// decides what arrows need to be shown/hidden
    /// </summary>
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

    /// <summary>
    /// changed what's displayed and updates the progress bar
    /// </summary>
    void ChangeDisplayValues()
    {
        Debug.Log(selectedMenuIndex);
        int currentTab = (selectedMenuIndex - 1);
        Debug.Log(currentTab);
        subTab1.text = subTabNames[currentTab][0];
        subTab2.text = subTabNames[currentTab][1];
        subTab3.text = subTabNames[currentTab][2];

        progress.highValue = stages[currentTab][selectedSubMenuIndex].Count;
        Desc.text = $"{descriptions[currentTab][selectedSubMenuIndex][((int)progress.value - 1)]}";
        sectionTitle.text = $"{stages[currentTab][selectedSubMenuIndex][((int)progress.value - 1)]}";
        progress.title = $"{progress.value}/{progress.highValue}";
        anim.Play($"{stages[currentTab][selectedSubMenuIndex][((int)progress.value - 1)]}"); //plays the specified animation name "for the gif to play"
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
        if (index != 0)
        {
            ChangeDisplayValues();
        }
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
