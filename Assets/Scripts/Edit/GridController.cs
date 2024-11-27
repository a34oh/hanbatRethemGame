using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour
{
    public SheetEditor sheetEditor;
    public SheetEditorController sheetController;
    public GridGenerator gridGenerator;
    public AudioController audioController;

    public Button scrollUp;
    public Button scrollDown;
    public Button changeSnapUp;
    public Button changeSnapDown;

    private void Start()
    {
        scrollUp.onClick.AddListener(OnScrollUp);
        scrollDown.onClick.AddListener(OnScrollDown);
        changeSnapUp.onClick.AddListener(ChangeSnapAmountUp);
        changeSnapDown.onClick.AddListener(ChangeSnapAmountDown);
    }
    void OnScrollUp()
    {
        gridGenerator.ChangePos(1);
        audioController.Scroll(1);
    }

    void OnScrollDown()
    {
        gridGenerator.ChangePos(-1);
        audioController.Scroll(-1);
    }

    void ChangeSnapAmountUp()
    {
        gridGenerator.ScrollSnapAmount *= 2f;
        gridGenerator.ChangeSnap();
    }
    void ChangeSnapAmountDown()
    {
        gridGenerator.ScrollSnapAmount *= 0.5f;
        gridGenerator.ChangeSnap();
    }
    /*   void ChangeSnapAmount()
       {
           if (sheetController.isLeftCtrl && sheetController.isScrolling)
           {
               if (sheetController.scrollDir > 0)
                   gridGenerator.ScrollSnapAmount *= 0.5f;
               else if (sheetController.scrollDir < 0)
                   gridGenerator.ScrollSnapAmount *= 2f;

               gridGenerator.ChangeSnap();

               //Debug.Log("±×¸®µå ½º³À : " + gridGenerator.ScrollSnapAmount);
           }
       }
    */
}
