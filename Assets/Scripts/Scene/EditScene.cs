using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditScene : BaseScene
{
    public BeatmapCreator beatmapCreator;
    public ScrollViewManager scrollViewManager;
    public Button BeatmapCreateCanvasButton;
    public GameObject BeatmapCreateCanvas;
    protected override void Init()
    {
        base.Init();

        SceneType = SceneType.EditorScene;
        beatmapCreator.OnBeatmapCreated += scrollViewManager.InitializeWithBeatmap;
        BeatmapCreateCanvasButton.onClick.AddListener(OnBeatmapCreateCanvasButton);

    }
    public void OnBeatmapCreateCanvasButton()
    {
        BeatmapCreateCanvas.SetActive(true);
    }
}
