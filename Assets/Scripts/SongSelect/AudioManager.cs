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
        // ���� �Ǵ� ������ ����� ��θ� ����
        string audioPath = sourceType == SourceType.Server ? beatmap.StorageAudioUrl : beatmap.localAudioPath;

        if (string.IsNullOrEmpty(audioPath))
        {
            Debug.LogError("����� URL�� ��� �ֽ��ϴ�.");
            return;
        }

        // ResourceCache���� ����� Ŭ�� ��������
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
            Debug.LogError("����� Ŭ�� �ε忡 �����߽��ϴ�.");
        }
    }
}
