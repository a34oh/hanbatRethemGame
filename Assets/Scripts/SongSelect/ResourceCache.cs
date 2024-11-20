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

    // ĳ�� �ʱ�ȭ
    public void ClearCache(SourceType sourceType)
    {
        if (sourceType == SourceType.Server)
        {
            serverAudioCache.Clear();
            serverImageCache.Clear();
            Debug.Log("���� ĳ�� �ʱ�ȭ �Ϸ�.");
        }
        else
        {
            localAudioCache.Clear();
            localImageCache.Clear();
            Debug.Log("���� ĳ�� �ʱ�ȭ �Ϸ�.");
        }
    }

    public async Task PreloadResourcesAsync(List<string> audioPaths, List<string> imagePaths, SourceType sourceType)
    {
        var audioTasks = new List<Task<AudioClip>>();
        var imageTasks = new List<Task<Texture2D>>();

        // ����� �ε� �۾� ����
        foreach (string audioPath in audioPaths)
        {
            if (!GetCache(sourceType).audioCache.ContainsKey(audioPath))
            {
                audioTasks.Add(LoadAudioAsync(audioPath, sourceType));
            }
        }

        // �̹��� �ε� �۾� ����
        foreach (string imagePath in imagePaths)
        {
            if (!GetCache(sourceType).imageCache.ContainsKey(imagePath))
            {
                imageTasks.Add(LoadImageAsync(imagePath, sourceType));
            }
        }

        // ������� �̹����� ���ķ� ó��
        var audioTask = Task.Run(async () =>
        {
            var audioResults = await Task.WhenAll(audioTasks);

            for (int i = 0; i < audioPaths.Count; i++)
            {
                if (audioResults[i] != null)
                {
                    GetCache(sourceType).audioCache[audioPaths[i]] = audioResults[i];
                }
            }
        });

        var imageTask = Task.Run(async () =>
        {
            var imageResults = await Task.WhenAll(imageTasks);

            for (int i = 0; i < imagePaths.Count; i++)
            {
                if (imageResults[i] != null)
                {
                    GetCache(sourceType).imageCache[imagePaths[i]] = imageResults[i];
                }
            }
        });

        // �� �۾��� ���ķ� ó��
        Debug.Log("�����, �̹��� �ε� �۾� ����");
        await Task.WhenAll(audioTask, imageTask);
        Debug.Log("�����, �̹��� �ε� �۾� ��");
    }


    // �̸� �ε�� ����� Ŭ���� ��ȯ�ϴ� �޼���
    public AudioClip GetCachedAudio(string audioPath, SourceType sourceType)
    {
        var cache = GetCache(sourceType).audioCache;
        if (cache.TryGetValue(audioPath, out AudioClip cachedClip))
        {
            return cachedClip;
        }
        return null;
    }

    // �̸� �ε��� �̹��� �ؽ�ó�� ��ȯ�ϴ� �޼���
    public Texture2D GetCachedImage(string imagePath, SourceType sourceType)
    {
        var cache = GetCache(sourceType).imageCache;
        if (cache.TryGetValue(imagePath, out Texture2D cachedTexture))
        {
            return cachedTexture;
        }
        return null;
    }

    // ����� ���� �ε�
    private async Task<AudioClip> LoadAudioAsync(string path, SourceType sourceType)
    {
        var cache = GetCache(sourceType).audioCache;
        if (cache.TryGetValue(path, out AudioClip cachedClip))
        {
            return cachedClip; // ĳ�ÿ� �����ϸ� ��ȯ
        }

        string finalPath = sourceType == SourceType.Server ? path.Replace(" ", "%20") : "file://" + path;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(finalPath, AudioType.MPEG))
        {
            await www.SendWebRequestAsync();
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                cache[path] = clip; // ĳ�ÿ� ����
                return clip;
            }
            else
            {
                Debug.LogError($"����� �ε� ����: {www.error} - {finalPath}");
                return null;
            }
        }
    }

    // �̹��� ���� �ε�
    private async Task<Texture2D> LoadImageAsync(string path, SourceType sourceType)
    {
        var cache = GetCache(sourceType).imageCache;
        if (cache.TryGetValue(path, out Texture2D cachedTexture))
        {
            return cachedTexture; // ĳ�ÿ� �����ϸ� ��ȯ
        }

        string finalPath = sourceType == SourceType.Server ? path.Replace(" ", "%20") : "file://" + path;

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(finalPath))
        {
            await www.SendWebRequestAsync();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                cache[path] = texture; // ĳ�ÿ� ����
                return texture;
            }
            else
            {
                Debug.LogError($"�̹��� �ε� ����: {www.error} - {finalPath}");
                return null;
            }
        }
    }

    // ĳ�� ���� �޼���
    private (Dictionary<string, AudioClip> audioCache, Dictionary<string, Texture2D> imageCache) GetCache(SourceType sourceType)
    {
        return sourceType == SourceType.Server
            ? (serverAudioCache, serverImageCache)
            : (localAudioCache, localImageCache);
    }


    // PreloadResourcesAsync�� ���� ������� ���� ó�� �ϴ� �ڵ�. ���� �ڵ尡 ������ ���ٸ� �������� ���
    /*public async Task PreloadResourcesAsync(List<string> audioPaths, List<string> imagePaths, SourceType sourceType)
{
    var preloadTasks = new List<Task>();

    // ����� �ε� �۾� �߰�
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
            }, TaskScheduler.FromCurrentSynchronizationContext())); // ���� �����忡�� ���� ����
        }
    }

    // �̹��� �ε� �۾� �߰�
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
            }, TaskScheduler.FromCurrentSynchronizationContext())); // ���� �����忡�� ���� ����
        }
    }

    // ��� �۾��� �Ϸ�� ������ ���
    await Task.WhenAll(preloadTasks);
}*/
}
