using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public SheetEditor sheetEditor;
    public Beatmap beatmap;
    public AudioSource audioSource;
    private AudioClip audioClip;

    public event Action OnAudioSetting;

    private float bpm;
    public float Offset { get; set; }

    // 1 마디
    public float BarPerSec { get; set; }
    // 1 박자
    public float BeatPerSec { get; set; }
    // 32 비트
    public float BeatPerSec32rd { get; set; }

    public float audioLength;

    private void Start()
    {
        audioSource = GameManager.AudioManager.audioSource;
        Debug.Log("오디오 적용");
    }


    public void Init(Beatmap beatmap)
    {
        Stop();
        this.beatmap = beatmap;
        // bpm = beatmap.bpm;
        bpm = 120f;  //임의로 설정
        //Offset = beatmap.offset;
        Offset = 2f; //임의로 설정

        //audioLength = audioClip.length;
        audioClip = GameManager.ResourceCache.GetCachedAudio(beatmap.localAudioPath, SourceType.Local);
       
        audioLength = audioClip.length;
        
        GameManager.AudioManager.SetAudioClip(audioClip);
        InitializeAudioSource(); // 초기화


        BarPerSec = 240f / bpm; // 4/4기준 = 60*4, 3/4 = 60*3

        BeatPerSec = 60f / bpm;

        BeatPerSec32rd = BeatPerSec / 8f;

        Offset *= BarPerSec;

        //Debug.Log("1마디 : " + BarPerSec);
        //Debug.Log("32비트: " + BeatPerSec32rd);
        //Debug.Log("오프셋 : " + Offset);
        OnAudioSetting.Invoke();
    }

    // 이걸 해줘야 곡을 재생시키지 않아도 스크롤 바로 이동 가능
    public void InitializeAudioSource()
    {
        if (!audioSource.isPlaying && audioSource.time == 0f)
        {
            audioSource.Play();
            audioSource.Pause();
            audioSource.time = 0f;
        }
    }
    public void PlayorPause()
    {
        //Debug.Log(audioSource.clip);
        //Debug.Log("현재 타임샘플 포지션 : " + audioSource.timeSamples);
        //Debug.Log("타임샘플 전체 : " + audioClip.samples);
        //Debug.Log("클립 주파수 : " + audioClip.frequency);

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
            audioSource.Stop();
            audioSource.timeSamples = 0;
        }

        sheetEditor.isPlay = false;
    }


    public void ChangePos(float time)
    {
        float currentTime = audioSource.time;

        currentTime += time;
        currentTime = Mathf.Clamp(currentTime, 0f, audioClip.length - 0.0001f); // 클립 길이에 딱 맞게 자르면 오류가 발생하여 끄트머리 조금 싹뚝

        audioSource.time = currentTime; //Debug.Log("현재 음악 위치 " + audioSource.time);
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
