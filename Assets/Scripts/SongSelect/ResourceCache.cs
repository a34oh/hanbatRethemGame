using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceCache
{
    private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();

    // ����ϰ� �� �̸� �ε� �޼���
    public async Task PreloadBeatmapResourcesAsync(List<string> audioPaths, List<string> imagePaths)
    {
        foreach (string audioPath in audioPaths)
        {
            if (!audioCache.ContainsKey(audioPath))
            {
                AudioClip clip = await GetAudioClipAsync(audioPath);
                if (clip != null)
                {
                    audioCache[audioPath] = clip;
                }
            }
        }

        foreach (string imagePath in imagePaths)
        {
            if (!imageCache.ContainsKey(imagePath))
            {
                Texture2D texture = await GetImageTextureAsync(imagePath);
                if (texture != null)
                {
                    imageCache[imagePath] = texture;
                }
            }
        }
    }


    // �̸� �ε�� ����� Ŭ���� ��ȯ�ϴ� �޼���
    public AudioClip GetCachedAudio(string audioPath)
    {
        audioCache.TryGetValue(audioPath, out AudioClip cachedClip);
        return cachedClip;
    }

    // �̸� �ε��� �̹��� �ؽ�ó�� ��ȯ�ϴ� �޼���
    public Texture2D GetCachedImage(string imagePath)
    {
        imageCache.TryGetValue(imagePath, out Texture2D cachedTexture);
        return cachedTexture;
    }

    // ����� ���� �ε�
    public async Task<AudioClip> GetAudioClipAsync(string audioPath)
    {
        if (audioCache.TryGetValue(audioPath, out AudioClip cachedClip))
        {
            return cachedClip; // ĳ�ÿ� �����ϸ� ��ȯ
        }

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.MPEG))
        {
            await www.SendWebRequestAsync();
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                audioCache[audioPath] = clip; // ĳ�ÿ� ����
                return clip;
            }
            else
            {
                Debug.LogError("����� �ε� ����: " + www.error);
                return null;
            }
        }
    }

    // �̹��� ���� �ε�
    public async Task<Texture2D> GetImageTextureAsync(string imagePath)
    {
        if (imageCache.TryGetValue(imagePath, out Texture2D cachedTexture))
        {
            return cachedTexture; // ĳ�ÿ� �����ϸ� ��ȯ
        }

        using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture("file://" + imagePath))
        {
            await uwr.SendWebRequestAsync();
            if (uwr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
                imageCache[imagePath] = texture; // ĳ�ÿ� ����
                return texture;
            }
            else
            {
                Debug.LogError("�̹��� �ε� ����: " + uwr.error);
                return null;
            }
        }
    }
}
