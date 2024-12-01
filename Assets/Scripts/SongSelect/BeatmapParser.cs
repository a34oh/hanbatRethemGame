using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class BeatmapParser
{
   private HashSet<string> loadedBeatmapPaths = new HashSet<string>();

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
                Debug.LogWarning("Songs 디렉토리가 존재하지 않습니다.");
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
                     if (loadedBeatmapPaths.Contains(txtFile.FullName))
                     {
                         continue;
                     }
                    Beatmap beatmap = await ParseBeatmapFileAsync(txtFile.FullName);
                    if (beatmap != null)
                    {
                        beatmap.localAudioPath = Path.Combine(songFolder.FullName, $"{beatmap.audioName}").Replace("\\", "/");
                        beatmap.localImagePath = Path.Combine(songFolder.FullName, $"{beatmap.imageName}").Replace("\\", "/");

                        audioPaths.Add(beatmap.localAudioPath);
                        imagePaths.Add(beatmap.localImagePath);

                        beatmaps.Add(beatmap);
                    }
                    
                    loadedBeatmapPaths.Add(txtFile.FullName);
                    Debug.Log("새로 로드하는 파일 : " + txtFile.FullName);
                }
            }
            await GameManager.ResourceCache.PreloadResourcesAsync(audioPaths, imagePaths, SourceType.Local);
            GameManager.BeatmapRepository.AddBeatmaps(beatmaps);
        }
        catch (Exception ex)
        {
            Debug.LogError("곡 로딩 중 오류 발생: " + ex.Message);
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
            bool parsingNotes = false;


            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                // `[Notes]` 섹션인지 확인
                if (line.StartsWith("[Notes]"))
                {
                    parsingNotes = true;
                    continue;
                }

                if (parsingNotes)
                {
                    // `[Notes]` 섹션에 있는 데이터를 처리
                    string[] noteParts = line.Split(',');
                    if (noteParts.Length == 2)
                    {
                        if (float.TryParse(noteParts[0], out float xPosition) &&
                            float.TryParse(noteParts[1], out float spawnTime))
                        {
                            NoteData note = new NoteData
                            {
                                xPosition = xPosition,
                                spawnTime = spawnTime
                            };
                            beatmap.noteDataList.Add(note);
                        }
                        else
                        {
                            Debug.LogWarning($"노트 데이터를 파싱할 수 없습니다: {line}");
                        }
                    }
                    continue;
                }

                // 첫 번째 콜론 위치 찾기
                int colonIndex = line.IndexOf(':');
                if (colonIndex == -1) continue;  // 콜론이 없으면 건너뜀

                string key = line.Substring(0, colonIndex).Trim();
                string value = line.Substring(colonIndex + 1).Trim();

                switch (key)
                {
                    case "Id":
                        beatmap.id = value;
                        break;
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
                            Debug.LogWarning($"DateAdded 형식이 올바르지 않음: {value}");
                        }
                        break;

                    // 필요한 경우 추가적인 키 처리 (예: BPM, EndTime 등)
                    default:
                        break;
                }
            }

            return beatmap;
        }
        catch (Exception ex)
        {
            Debug.LogError("곡 파싱 중 오류 발생: " + ex.Message);
            return null;
        }
    }

}
