using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditScene : BaseScene
{
    public BeatmapCreator beatmapCreator;
    public ScrollViewManager scrollViewManager;
    public SheetEditor sheetEditor;
    public Button BeatmapCreateCanvasButton;
    public Button SaveEditorButton;
    public GameObject BeatmapCreateCanvas;
    protected override void Init()
    {
        base.Init();

        SceneType = SceneType.EditorScene;
        beatmapCreator.OnBeatmapCreated += scrollViewManager.InitializeWithBeatmap;
        SaveEditorButton.onClick.AddListener(sheetEditor.OnSaveNotesButtonClick);
        BeatmapCreateCanvasButton.onClick.AddListener(OnBeatmapCreateCanvasButton);
     //   scrollViewManager.InitializeWithBeatmap(new Beatmap());

    }
    public void OnBeatmapCreateCanvasButton()
    {
        BeatmapCreateCanvas.SetActive(true);
    }


}
