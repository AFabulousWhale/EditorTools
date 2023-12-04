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

    List<string> iconStages = new() {"Position", "Rotation", "Scale", "Background" };
    List<string> menuStages;
    List<string> DBStages;

    List<string> iconDesc = new() { "The Position Offset lets you change the location of the object within the icon, across both the X and Y axis.",
                                    "The Rotation Offset lets you rotate the object within the icon, which can be done along any axis.",
                                    "The Scale Multiplier lets you change the size of the object within the icon, making it either bigger or smaller.",
                                    "The Background Color lets you change the background of the icon, changing the colour and opacity to anything you wish"};
    List<string> menuDesc = new() { };
    List<string> DBDesc = new() { };

    List<List<string>> stages => new() { iconStages, menuStages, DBStages };
    List<List<string>> descriptions => new() { iconDesc, menuDesc, DBDesc };

    string selectedMenu;
    int selectedMenuIndex;

    ProgressBar progress;

    Button iconMaker, menuMaker, DBMaker;
    List<Button> tabs => new() { iconMaker, menuMaker, DBMaker };
    string tabClass = "selectedTAB";

    VisualElement lArrow, rArrow;
    VisualElement imageDisplay;
    Label sectionTitle, Desc;

    Animator anim;
    GameObject sprite;

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

        rArrow = root.Q<VisualElement>("RArrow");
        lArrow = root.Q<VisualElement>("LArrow");

        sectionTitle = root.Q<Label>("SectionTitle");
        Desc = root.Q<Label>("Description");

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
            anim.Play($"{stages[selectedMenuIndex][(int)progress.value]}");
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

            switch (type)
            {
                case "icon":
                    SelectTab(0);
                    progress.highValue = iconStages.Count;
                    break;
                case "menu":
                    SelectTab(1);
                    progress.highValue = menuStages.Count;
                    break;
                case "DB":
                    SelectTab(2);
                    progress.highValue = DBStages.Count;
                    break;
            }
            progress.title = $"1/{progress.highValue}";
        }
    }

    void ProgressStage(ClickEvent evt, int value)
    {
        progress.value = progress.value + value;
        progress.value = Mathf.Clamp(progress.value, 1, progress.highValue);

        lArrow.style.visibility = Visibility.Visible;
        rArrow.style.visibility = Visibility.Visible;

        if (progress.value == 1)
        {
            lArrow.style.visibility = Visibility.Hidden;
        }
        if(progress.value == progress.highValue)
        {
            rArrow.style.visibility = Visibility.Hidden;
        }

        Desc.text = $"{descriptions[selectedMenuIndex][((int)progress.value - 1)]}";
        sectionTitle.text = $"{stages[selectedMenuIndex][((int)progress.value - 1)]}";
        progress.title = $"{progress.value}/{progress.highValue}";
        anim.Play($"{stages[selectedMenuIndex][((int)progress.value - 1)]}"); //plays the specified animation name "for the gif to play"
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
    }
}
