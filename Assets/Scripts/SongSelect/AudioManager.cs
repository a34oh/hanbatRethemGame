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
        // ResourceCache에서 오디오 클립 가져오기
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
                Debug.LogError("오디오 클립이 캐시에 존재하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("오디오 경로가 비어 있습니다.");
        }
    }

    public async void PlayPreviewFromFirebase(Beatmap beatmap)
    {
        if (string.IsNullOrEmpty(beatmap.StorageAudioUrl))
        {
            Debug.LogError("오디오 URL이 비어 있습니다.");
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
            Debug.LogError("오디오 클립 로드에 실패했습니다.");
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
                Debug.Log("오디오 클립이 Firebase에서 성공적으로 로드되었습니다.");
                return DownloadHandlerAudioClip.GetContent(www);
            }
            else
            {
                Debug.LogError($"오디오 로드 실패: {www.error}");
                return null;
            }
        }
    }
}
