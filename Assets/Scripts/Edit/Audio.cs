using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public SheetEditor sheetEditor;
    public Beatmap beatmap;
    public AudioSource audioSource;
    public AudioClip audioClip;

    public event Action OnAudioSetting;

    private float bpm;
    public float Offset { get; set; }

    // 1 ����
    public float BarPerSec { get; set; }
    // 1 ����
    public float BeatPerSec { get; set; }
    // 32 ��Ʈ
    public float BeatPerSec32rd { get; set; }

    public float audioLength;

    private void Start()
    {
        audioSource = GameManager.AudioManager.audioSource;
        Debug.Log("����� ����");
    }


    public void Init(Beatmap beatmap)
    {
        Stop();
        this.beatmap = beatmap;
        // bpm = beatmap.bpm;
        bpm = 120f;  //���Ƿ� ����
        //Offset = beatmap.offset;
        Offset = 2f; //���Ƿ� ����

        audioLength = audioClip.length;
        //audioClip = GameManager.ResourceCache.GetCachedAudio(beatmap.localAudioPath, SourceType.Local);
        
        GameManager.AudioManager.SetAudioClip(audioClip);



        BarPerSec = 240f / bpm; // 4/4���� = 60*4, 3/4 = 60*3

        BeatPerSec = 60f / bpm;

        BeatPerSec32rd = BeatPerSec / 8f;

        Offset *= BarPerSec;

        //Debug.Log("1���� : " + BarPerSec);
        //Debug.Log("32��Ʈ: " + BeatPerSec32rd);
        //Debug.Log("������ : " + Offset);
        OnAudioSetting.Invoke();
    }

    public void PlayorPause()
    {
        //Debug.Log(audioSource.clip);
        //Debug.Log("���� Ÿ�ӻ��� ������ : " + audioSource.timeSamples);
        //Debug.Log("Ÿ�ӻ��� ��ü : " + audioClip.samples);
        //Debug.Log("Ŭ�� ���ļ� : " + audioClip.frequency);

        //if (audioClip == null)
        //    Init();
        if (sheetEditor.isPlay)
        {
            audioSource.Pause();
            sheetEditor.isPlay = false;
        }
        else
        {
            audioSource.Play();
            sheetEditor.isPlay = true;
        }
    }

    public void Stop()
    {
        if (audioClip != null)
        {
            audioSource.timeSamples = 0;
            audioSource.Stop();
        }

        sheetEditor.isPlay = false;
    }


    public void ChangePos(float time)
    {
        float currentTime = audioSource.time;

        currentTime += time;
        currentTime = Mathf.Clamp(currentTime, 0f, audioClip.length - 0.0001f); // Ŭ�� ���̿� �� �°� �ڸ��� ������ �߻��Ͽ� ��Ʈ�Ӹ� ���� �϶�

        audioSource.time = currentTime; //Debug.Log("���� ���� ��ġ " + audioSource.time);
    }

    public void ChangePosByProgressBar(float pos)
    {
        float time = audioClip.length * pos;

        audioSource.time = time;
    }

    public string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int sec = Mathf.FloorToInt(seconds % 60);
        int millis = Mathf.FloorToInt((seconds * 1000) % 1000);
        return $"{minutes:D2}:{sec:D2}:{millis:D3}";
    }
}
