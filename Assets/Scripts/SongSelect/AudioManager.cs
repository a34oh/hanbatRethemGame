using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager
{
    //�뷡 ��� ������ҽ�.   ȿ�����̳� ��Ÿ �Ҹ��� ���� ����� �ҽ� ���
    public AudioSource audioSource { get;  private set; }

    public void Init()
    {
        GameObject audioManagerObject = new GameObject { name = "AudioManager" };
        audioSource = audioManagerObject.AddComponent<AudioSource>();
        Debug.Log("audioSource���");
        Object.DontDestroyOnLoad(audioManagerObject);
    }

    public void SetAudioClip(AudioClip audioclip)
    {
        audioSource.clip = audioclip;
        if (audioclip == null)
            Debug.Log("audioClip is null");
        else 
            Debug.Log("audioClip ���� �Ϸ�");
    }
    public void PlayorStop()
    {   
        if (audioSource.isPlaying)
        {
            audioSource.Pause(); // ��� ���̸� �Ͻ�����
        }
        else
        {
            audioSource.Play();
        }
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

    public float GetAudioTime()
    {
        return audioSource.time;
    }

    public void ClearAudio()
    {
        audioSource.clip = null;
    }

}
