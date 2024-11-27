using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditScene : BaseScene
{
    public BeatmapCreator beatmapCreator;
    public ScrollViewManager scrollViewManager;
    protected override void Init()
    {
        base.Init();

        SceneType = SceneType.Editor;
        beatmapCreator.OnBeatmapCreated += scrollViewManager.InitializeWithBeatmap;

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
