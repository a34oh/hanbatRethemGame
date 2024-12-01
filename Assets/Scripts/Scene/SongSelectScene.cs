using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongSelectScene : BaseScene
{
    public BeatmapSetManager beatmapSetManager;
    protected override void Init()
    {
        base.Init();
        ParserAndUpdateScrollView();   
    }

    async void ParserAndUpdateScrollView()
    {
        await GameManager.BeatmapParser.ParserAllBeatmapsAsync();
        beatmapSetManager.UpdateScrollView();
    }
}
