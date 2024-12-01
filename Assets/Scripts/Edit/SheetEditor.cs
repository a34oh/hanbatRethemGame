using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SheetEditor : MonoBehaviour
{
    public bool isPlay = false;
    public SheetEditorController sheetController;
    public float Speed { get; set; } = 4;
    private int currentBarNumber;


    void DetectionPreNote()
    {
        if(sheetController.mRay.transform.gameObject.layer == (int)LayerTypes.Grid)
        {
            GameObject go = sheetController.mRay.transform.gameObject;
            Grid grid = go.GetComponent<Grid>();
            currentBarNumber = grid.barNumber;
        }
    }


}
