using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager
{
    private AudioSource audioSource;

    public void Init()
    {
        GameObject audioManagerObject = new GameObject { name = "AudioManager" };
        audioSource = audioManagerObject.AddComponent<AudioSource>();

        Object.DontDestroyOnLoad(audioManagerObject);
    }
    public void PlayPreview(Beatmap beatmap)
    {
        // ResourceCache���� ����� Ŭ�� ��������
        if (!string.IsNullOrEmpty(beatmap.localAudioPath))
        {
            AudioClip audioClip = GameManager.ResourceCache.GetCachedAudio(beatmap.localAudioPath);

            if (audioClip != null)
            {
                audioSource.loop = true;
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                audioSource.clip = audioClip;
                audioSource.time = beatmap.previewTime / 1000f;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("����� Ŭ���� ĳ�ÿ� �������� �ʽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogError("����� ��ΰ� ��� �ֽ��ϴ�.");
        }
    }

    public async void PlayPreviewFromFirebase(Beatmap beatmap)
    {
        if (string.IsNullOrEmpty(beatmap.StorageAudioUrl))
        {
            Debug.LogError("����� URL�� ��� �ֽ��ϴ�.");
            return;
        }

        AudioClip audioClip = await LoadAudioFromFirebaseAsync(beatmap.StorageAudioUrl);

        if (audioClip != null)
        {
            audioSource.loop = true;
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.clip = audioClip;
            audioSource.time = beatmap.previewTime / 1000f;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("����� Ŭ�� �ε忡 �����߽��ϴ�.");
        }
    }

    private async Task<AudioClip> LoadAudioFromFirebaseAsync(string audioUrl)
    {
        string fixedUrl = audioUrl.Replace(" ", "%20");
        Debug.Log($"Encoded Audio URL: {fixedUrl}");


        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fixedUrl, AudioType.MPEG))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("����� Ŭ���� Firebase���� ���������� �ε�Ǿ����ϴ�.");
                return DownloadHandlerAudioClip.GetContent(www);
            }
            else
            {
                Debug.LogError($"����� �ε� ����: {www.error}");
                return null;
            }
        }
    }
}
