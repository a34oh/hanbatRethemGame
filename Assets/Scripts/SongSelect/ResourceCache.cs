using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public enum SourceType
{
    Local,
    Server
}

public class ResourceCache
{
    private Dictionary<string, AudioClip> localAudioCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, Texture2D> localImageCache = new Dictionary<string, Texture2D>();


    private Dictionary<string, AudioClip> serverAudioCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, Texture2D> serverImageCache = new Dictionary<string, Texture2D>();

    // 캐시 초기화
    public void ClearCache(SourceType sourceType)
    {
        if (sourceType == SourceType.Server)
        {
            serverAudioCache.Clear();
            serverImageCache.Clear();
            Debug.Log("서버 캐시 초기화 완료.");
        }
        else
        {
            localAudioCache.Clear();
            localImageCache.Clear();
            Debug.Log("로컬 캐시 초기화 완료.");
        }
    }

    public async Task PreloadResourcesAsync(List<string> audioPaths, List<string> imagePaths, SourceType sourceType)
    {
        var audioTasks = new Dictionary<string, Task<AudioClip>>();
        var imageTasks = new Dictionary<string, Task<Texture2D>>();

        // 오디오 로드 작업 생성
        foreach (string audioPath in audioPaths)
        {
            if (!GetCache(sourceType).audioCache.ContainsKey(audioPath))
            {
                audioTasks[audioPath] = LoadAudioAsync(audioPath, sourceType);
            }
        }

        // 이미지 로드 작업 생성
        foreach (string imagePath in imagePaths)
        {
            if (!GetCache(sourceType).imageCache.ContainsKey(imagePath))
            {
                imageTasks[imagePath] = LoadImageAsync(imagePath, sourceType);
            }
        }

        // 오디오와 이미지를 병렬로 처리
        var audioTask = Task.Run(async () =>
        {
            foreach (var kvp in audioTasks)
            {
                var audioResult = await kvp.Value;
                if (audioResult != null)
                {
                    GetCache(sourceType).audioCache[kvp.Key] = audioResult;
                }
            }
        });

        var imageTask = Task.Run(async () =>
        {
            foreach (var kvp in imageTasks)
            {
                var imageResult = await kvp.Value;
                if (imageResult != null)
                {
                    GetCache(sourceType).imageCache[kvp.Key] = imageResult;
                }
            }
        });

        // 두 작업을 병렬로 처리
        Debug.Log("오디오, 이미지 로드 작업 시작");
        await Task.WhenAll(audioTask, imageTask);
        Debug.Log("오디오, 이미지 로드 작업 끝");
    }

    public async Task PreloadResourcesAsync(string audioPath, string imagePath, SourceType sourceType)
    {
        var audioPaths = new List<string> { audioPath };
        var imagePaths = new List<string> { imagePath };

        await PreloadResourcesAsync(audioPaths, imagePaths, sourceType);
    }


    // 미리 로드된 오디오 클립을 반환하는 메서드
    public AudioClip GetCachedAudio(string audioPath, SourceType sourceType)
    {
        var cache = GetCache(sourceType).audioCache;
        if (cache.TryGetValue(audioPath, out AudioClip cachedClip))
        {
            return cachedClip;
        }
        else
        {
            Debug.Log("오디오 클립이 없음." + audioPath);
        }
        return null;
    }

    // 미리 로드한 이미지 텍스처를 반환하는 메서드
    public Texture2D GetCachedImage(string imagePath, SourceType sourceType)
    {
        var cache = GetCache(sourceType).imageCache;
        if (cache.TryGetValue(imagePath, out Texture2D cachedTexture))
        {
            return cachedTexture;
        }
        return null;
    }

    // 오디오 파일 로드
    private async Task<AudioClip> LoadAudioAsync(string path, SourceType sourceType)
    {
        var cache = GetCache(sourceType).audioCache;
        if (cache.TryGetValue(path, out AudioClip cachedClip))
        {
            return cachedClip; // 캐시에 존재하면 반환
        }

        string finalPath = sourceType == SourceType.Server ? path.Replace(" ", "%20") : "file://" + path;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(finalPath, AudioType.MPEG))
        {
            await www.SendWebRequestAsync();
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                cache[path] = clip; // 캐시에 저장
                return clip;
            }
            else
            {
                Debug.LogError($"오디오 로드 실패: {www.error} - {finalPath}");
                return null;
            }
        }
    }

    // 이미지 파일 로드
    private async Task<Texture2D> LoadImageAsync(string path, SourceType sourceType)
    {
        var cache = GetCache(sourceType).imageCache;
        if (cache.TryGetValue(path, out Texture2D cachedTexture))
        {
            return cachedTexture; // 캐시에 존재하면 반환
        }

        string finalPath = sourceType == SourceType.Server ? path.Replace(" ", "%20") : "file://" + path;

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(finalPath))
        {
            await www.SendWebRequestAsync();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                cache[path] = texture; // 캐시에 저장
                return texture;
            }
            else
            {
                Debug.LogError($"이미지 로드 실패: {www.error} - {finalPath}");
                return null;
            }
        }
    }

    // 캐시 선택 메서드
    private (Dictionary<string, AudioClip> audioCache, Dictionary<string, Texture2D> imageCache) GetCache(SourceType sourceType)
    {
        return sourceType == SourceType.Server
            ? (serverAudioCache, serverImageCache)
            : (localAudioCache, localImageCache);
    }


    // PreloadResourcesAsync을 메인 쓰레드로 병렬 처리 하는 코드. 위에 코드가 오류가 난다면 비상용으로 사용
    /*public async Task PreloadResourcesAsync(List<string> audioPaths, List<string> imagePaths, SourceType sourceType)
{
    var preloadTasks = new List<Task>();

    // 오디오 로드 작업 추가
    foreach (string audioPath in audioPaths)
    {
        if (!GetCache(sourceType).audioCache.ContainsKey(audioPath))
        {
            preloadTasks.Add(LoadAudioAsync(audioPath, sourceType).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    GetCache(sourceType).audioCache[audioPath] = task.Result;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext())); // 메인 스레드에서 실행 보장
        }
    }

    // 이미지 로드 작업 추가
    foreach (string imagePath in imagePaths)
    {
        if (!GetCache(sourceType).imageCache.ContainsKey(imagePath))
        {
            preloadTasks.Add(LoadImageAsync(imagePath, sourceType).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    GetCache(sourceType).imageCache[imagePath] = task.Result;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext())); // 메인 스레드에서 실행 보장
        }
    }

    // 모든 작업이 완료될 때까지 대기
    await Task.WhenAll(preloadTasks);
}*/
}
