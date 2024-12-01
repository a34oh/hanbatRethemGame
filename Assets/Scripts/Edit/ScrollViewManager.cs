using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewManager : MonoBehaviour
{ 
    public Audio a;
    public SheetEditor sheetEditor;
    public GridGenerator gridGenerator;

    public void InitializeWithBeatmap(Beatmap beatmap)
    {
        a.Init(beatmap);
        sheetEditor.Init();
        gridGenerator.Init();
     //   noteGenerator.GenNote();
    }
}
