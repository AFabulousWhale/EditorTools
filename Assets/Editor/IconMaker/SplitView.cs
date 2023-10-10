using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class SplitView : TwoPaneSplitView
{
    //used to make the script usable within ui builder
    public new class UxmlFactory : UxmlFactory<SplitView, TwoPaneSplitView.UxmlTraits> { }
}
