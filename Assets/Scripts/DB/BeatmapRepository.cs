using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatmapRepository
{
    private List<Beatmap> beatmaps = new List<Beatmap>();

    public IReadOnlyList<Beatmap> Beatmaps => beatmaps.AsReadOnly();

    public void AddBeatmaps(IEnumerable<Beatmap> newBeatmaps)
    {
        beatmaps.AddRange(newBeatmaps);
    }

    public void Clear()
    {
        beatmaps.Clear();
    }

    public bool IsEmpty()
    {
        return beatmaps.Count == 0;
    }
}