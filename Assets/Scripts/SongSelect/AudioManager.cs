using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager
{
    //노래 재생 오디오소스.   효과음이나 기타 소리는 별도 오디오 소스 사용
    public AudioSource audioSource { get;  private set; }

    public void Init()
    {
        GameObject audioManagerObject = new GameObject { name = "AudioManager" };
        audioSource = audioManagerObject.AddComponent<AudioSource>();
        Debug.Log("audioSource등록");
        Object.DontDestroyOnLoad(audioManagerObject);
    }

    public void SetAudioClip(AudioClip audioclip)
    {
        audioSource.clip = audioclip;
        if (audioclip == null)
            Debug.Log("audioClip is null");
        else 
            Debug.Log("audioClip 저장 완료");
        audioSource.Stop();
    }
    public void PlayorStop()
    {   
        if (audioSource.isPlaying)
        {
            audioSource.Pause(); // 재생 중이면 일시정지
        }
        else
        {
            audioSource.Play();
        }
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

    public float GetAudioTime()
    {
        return audioSource.time;
    }

    public void ClearAudio()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }

}
