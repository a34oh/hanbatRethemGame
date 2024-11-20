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
    public void PlayPreview(Beatmap beatmap, SourceType sourceType)
    {
        // 로컬 또는 서버의 오디오 경로를 선택
        string audioPath = sourceType == SourceType.Server ? beatmap.StorageAudioUrl : beatmap.localAudioPath;

        if (string.IsNullOrEmpty(audioPath))
        {
            Debug.LogError("오디오 URL이 비어 있습니다.");
            return;
        }

        // ResourceCache에서 오디오 클립 가져오기
        AudioClip audioClip = GameManager.ResourceCache.GetCachedAudio(audioPath, sourceType);

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
}
