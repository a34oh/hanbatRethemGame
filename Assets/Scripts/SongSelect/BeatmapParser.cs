using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class BeatmapParser
{
  //  private Dictionary<string, List<Beatmap>> allBbeatmaps = new Dictionary<string, List<Beatmap>>();
  //  private HashSet<string> loadedBeatmapPaths = new HashSet<string>();

    public async Task<List<Beatmap>> ParserAllBeatmapsAsync()
    {
        List<Beatmap> beatmaps = new List<Beatmap>();
        List<string> audioPaths = new List<string>();
        List<string> imagePaths = new List<string>();

        try
        {
            string songsDirectory = Path.Combine(Application.persistentDataPath, "Songs").Replace("\\", "/");
            if (!Directory.Exists(songsDirectory))
            {
                Debug.LogWarning("Songs ���丮�� �������� �ʽ��ϴ�.");
                return beatmaps;
            }
            Debug.Log(songsDirectory);
            DirectoryInfo dirInfo = new DirectoryInfo(songsDirectory);
            DirectoryInfo[] songFolders = dirInfo.GetDirectories();

            foreach (DirectoryInfo songFolder in songFolders)
            {
                FileInfo[] txtFiles = songFolder.GetFiles("*.txt");

                foreach (FileInfo txtFile in txtFiles)
                {
                   /* if (loadedBeatmapPaths.Contains(txtFile.FullName))
                    {
                        Debug.Log("�̹� �ε� �� ���� : " + txtFile.FullName);
                        continue; // �̹� �ε�� ���� ��ŵ
                    }*/
                    Beatmap beatmap = await ParseBeatmapFileAsync(txtFile.FullName);

                    if (beatmap != null)
                    {
                        beatmap.localAudioPath = Path.Combine(songFolder.FullName, $"{beatmap.audioName}").Replace("\\", "/");
                        beatmap.localImagePath = Path.Combine(songFolder.FullName, $"{beatmap.imageName}").Replace("\\", "/");

                        audioPaths.Add(beatmap.localAudioPath);
                        imagePaths.Add(beatmap.localImagePath);

                        beatmaps.Add(beatmap);
                    }
                    /*
                    if (!allBbeatmaps.ContainsKey(beatmap.id))
                    {
                        allBbeatmaps[beatmap.id] = new List<Beatmap>();
                    }
                    allBbeatmaps[beatmap.id].Add(beatmap);
                    loadedBeatmapPaths.Add(txtFile.FullName);
                    Debug.Log("���� �ε��ϴ� ���� : " + txtFile.FullName);*/
                }
            }

            await GameManager.ResourceCache.PreloadBeatmapResourcesAsync(audioPaths, imagePaths);

        }
        catch (Exception ex)
        {
            Debug.LogError("�� �ε� �� ���� �߻�: " + ex.Message);
            throw;
        }

        return beatmaps;
    }

    private async Task<Beatmap> ParseBeatmapFileAsync(string filePath)
    {
        try
        {
            string[] lines = await File.ReadAllLinesAsync(filePath);
            Beatmap beatmap = new Beatmap();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                // ù ��° �ݷ� ��ġ ã��
                int colonIndex = line.IndexOf(':');
                if (colonIndex == -1) continue;  // �ݷ��� ������ �ǳʶ�

                string key = line.Substring(0, colonIndex).Trim();
                string value = line.Substring(colonIndex + 1).Trim();

                switch (key)
                {
                    case "Title":
                        beatmap.title = value;
                        break;
                    case "Artist":
                        beatmap.artist = value;
                        break;
                    case "Creator":
                        beatmap.creator = value;
                        break;
                    case "Version":
                        beatmap.version = value;
                        break;
                    case "Audioname":
                        beatmap.audioName = value;
                        break;
                    case "Imagename":
                        beatmap.imageName = value;
                        break;
                    case "PreviewTime":
                        beatmap.previewTime = int.Parse(value);
                        break;
                    case "DateAdded":
                        try
                        {
                            beatmap.dateAdded = DateTime.ParseExact(
                                value,
                                "yyyy-MM-dd HH:mm:ss",
                                null
                            );
                        }
                        catch (FormatException)
                        {
                            Debug.LogWarning($"DateAdded ������ �ùٸ��� ����: {value}");
                        }
                        break;
                    // �ʿ��� ��� �߰����� Ű ó�� (��: BPM, EndTime ��)
                    default:
                        break;
                }
            }

            return beatmap;
        }
        catch (Exception ex)
        {
            Debug.LogError("�� �Ľ� �� ���� �߻�: " + ex.Message);
            return null;
        }
    }

}
