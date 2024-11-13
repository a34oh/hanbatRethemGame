using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class BeatmapSet : IEnumerable<Beatmap>
{
    [SerializeField]
    private List<Beatmap> beatmaps;

    // ������
    public BeatmapSet(List<Beatmap> songs)
    {
        this.beatmaps = songs ?? new List<Beatmap>();
    }

    public int Count => beatmaps.Count;

    // Ư�� �ε����� Beatmap�� ��ȯ
    public Beatmap Get(int index)
    {
        if (index < 0 || index >= beatmaps.Count)
            Debug.LogError("Beatmap index is out of range.");
        return beatmaps[index];
    }

    // Ư�� �ε����� Beatmap�� �����ϰ� ��ȯ
    public Beatmap Remove(int index)
    {
        if (index < 0 || index >= beatmaps.Count)
            Debug.LogError("Beatmap index is out of range.");
        Beatmap removed = beatmaps[index];
        beatmaps.RemoveAt(index);
        return removed;
    }

    // Beatmap ���� �迭 ��ȯ
    public string[] GetInfo(int index)
    {
        if (index < 0 || index >= beatmaps.Count)
            Debug.LogError("Invalid index");

        Beatmap beatmap = Get(index);

        string[] info = new string[6];
        info[0] = $"Title: {beatmap.title}";
        info[1] = $"Artist: {beatmap.artist}";
        info[2] = $"Creator: {beatmap.creator}";
        info[3] = $"Version: {beatmap.version}";
        info[4] = $"AudioFilename: {beatmap.audioFilename}";
        info[5] = $"PreviewTime: {beatmap.previewTime}";
        info[6] = $"Tags: {beatmap.tags}";

        return info;      
    }

    // �˻���� ��Ī�Ǵ��� Ȯ��
    public bool Matches(string query)
    {
        if (string.IsNullOrEmpty(query))
            return false;

        query = query.ToLower();

        // ù ��° Beatmap���� �˻�
        Beatmap beatmap = beatmaps[0];
        if ((beatmap.title != null && beatmap.title.ToLower().Contains(query)) ||
            (beatmap.artist != null && beatmap.artist.ToLower().Contains(query)) ||
            (beatmap.creator != null && beatmap.creator.ToLower().Contains(query)) ||
            (beatmap.version != null && beatmap.version.ToLower().Contains(query)) ||
            (beatmap.tags != null && beatmap.tags.ToLower().Contains(query)))
        {
            return true;
        }

        // ������ Beatmap���� �˻�
        for (int i = 1; i < beatmaps.Count; i++)
        {
            beatmap = beatmaps[i];
            if ((beatmap.version != null && beatmap.version.ToLower().Contains(query)) ||
                (beatmap.tags != null && beatmap.tags.ToLower().Contains(query)))
            {
                return true;
            }
        }

        return false;
    }

    // ���ǰ� ��Ī�Ǵ��� Ȯ�� (��: bpm, length ��)
    public bool Matches(string type, string operatorStr, float value)
    {
        foreach (Beatmap beatmap in beatmaps)
        {
            float v;
            switch (type)
            {
                case "bpm":
                    v = beatmap.bpm;
                    break;
                case "length":
                    v = beatmap.endTime / 1000f;
                    break;
                default:
                    return false;
            }

            bool met;
            switch (operatorStr)
            {
                case "=":
                case "==":
                    met = (v == value);
                    break;
                case ">":
                    met = (v > value);
                    break;
                case ">=":
                    met = (v >= value);
                    break;
                case "<":
                    met = (v < value);
                    break;
                case "<=":
                    met = (v <= value);
                    break;
                default:
                    return false;
            }

            if (met)
                return true;
        }

        return false;
    }


    // IEnumerable �������̽� ����
    public IEnumerator<Beatmap> GetEnumerator()
    {
        return beatmaps.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // BeatmapSet�� ���ڿ� ǥ�� ��ȯ
    public override string ToString()
    {
        if (beatmaps.Count == 0)
            return "���� �����ϴ�.";
        Beatmap beatmap = beatmaps[0];
        return $"{beatmap.artist} - {beatmap.title}";
    }

    // ���ã�� ���� Ȯ��
    public bool IsFavorite()
    {
        foreach (var map in beatmaps)
        {
            if (map.favorite)
                return true;
        }
        return false;
    }

    // ���ã�� ���� ����
    public void SetFavorite(bool flag)
    {
        foreach (var map in beatmaps)
        {
            map.favorite = flag;
            // BeatmapDB.UpdateFavoriteStatus(map); // �����ͺ��̽� ���� �� Ȱ��ȭ
        }
    }
}
