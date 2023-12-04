using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class EditorWindowMain : EditorWindow
{
    string[] names = { "Icon", "Menu", "Database" };
    string tabSuffix = "Tab";
    string contentSuffix = "Content";
    int tabIndex = 0;

    string tabClass = "currentlySelectedTab";

    List<Label> tabs = new();
    List<VisualElement> content = new();

    [SerializeField]
    private VisualTreeAsset tree = default;

    public static IconMaker iconMaker;
    public static MenuMaker menuMaker;
    public static DatabaseMaker databaseMaker;

    [MenuItem("Editor Tools/Icon, Menu and Database Creator")]
    public static void ShowExample()
    {
        EditorWindow window = GetWindow<EditorWindowMain>("Icon, Menu and Database Creator"); //shows the icon window if not already displayed
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement UXMLFile = tree.Instantiate();
        root.Add(UXMLFile);

        iconMaker = new(root);
        iconMaker.CreateGUI();

        menuMaker = new(root);
        menuMaker.CreateGUI();

        databaseMaker = new(root);
        databaseMaker.CreateGUI();

        databaseMaker.iconMaker = iconMaker;
        databaseMaker.menuMaker = menuMaker;
        iconMaker.databaseMaker = databaseMaker;
        menuMaker.iconMaker = iconMaker;
        menuMaker.databaseMaker = databaseMaker;

        databaseMaker.ButtonShowing();

        for (int i = 0; i < names.Length; i++)
        {
            //finds the visual elements containing the content
            string veName = names[i] + contentSuffix;
            VisualElement contentVE = root.Q<VisualElement>(veName);
            content.Add(contentVE);

            //finds the labels of the tab name
            string lName = names[i] + tabSuffix;
            Label tabName = root.Q<Label>(lName);
            tabName.RegisterCallback<ClickEvent, int>(SelectTab, i);
            tabs.Add(tabName);
        }
    }

    public void SelectTab(ClickEvent evt, int index)
    {
        DeselectTab();
        tabIndex = index;

        tabs[tabIndex].AddToClassList(tabClass); //this tab is selected
        content[tabIndex].style.display = DisplayStyle.Flex; //this content selected
    }

    public void DeselectTab()
    {
        tabs[tabIndex].RemoveFromClassList(tabClass); //this tab is no longer selected
        content[tabIndex].style.display = DisplayStyle.None; //this content is no longer selected
    }

    private void OnSelectionChange()
    {
        iconMaker.OnSelectionChange();
        menuMaker.OnSelectionChange();
        databaseMaker.OnSelectionChange();
    }

    private void OnGUI()
    {
        iconMaker.OnGUI();
        menuMaker.OnGUI();
    }

    void OnInspectorUpdate()
    {
        iconMaker.OnInspectorUpdate();
    }
}
