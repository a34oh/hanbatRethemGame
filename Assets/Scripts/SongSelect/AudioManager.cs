using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (!string.IsNullOrEmpty(beatmap.audioPath))
        {
            AudioClip audioClip = GameManager.ResourceCache.GetCachedAudio(beatmap.audioPath);

            if (audioClip != null)
            {
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
}
