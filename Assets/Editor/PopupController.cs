using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

public class PopupController
{
    public void CallPopup(bool extraText, bool twoButtons, string mainText, string secondText)
    {
        EditorPopup popup = new();
        popup.twoButtons = twoButtons;
        popup.extraText = extraText;
        popup.labelText = mainText;
        popup.labelTextExtra = secondText;

        if(twoButtons)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(WaitForPopupValue(popup));
        }
        else
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(DelayPopup(popup));
        }
    }

    IEnumerator WaitForPopupValue(EditorPopup popup)
    {
        UnityEditor.PopupWindow.Show(new Rect(0, 0, 1, 1), popup);
        yield return new WaitUntil(() => popup.popUpStage != "none");
    }

    IEnumerator DelayPopup(EditorPopup popup)
    {
        yield return new WaitForSecondsRealtime(1);
        UnityEditor.PopupWindow.Show(new Rect(0, 0, 1, 1), popup);
    }
}
