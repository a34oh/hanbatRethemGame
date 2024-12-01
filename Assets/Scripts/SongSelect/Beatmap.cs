using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 곡 데이터 클래스
[System.Serializable]
public class Beatmap
{
    public string id;
    public string title;        
    public string artist;
    public string creator;
    public string version;           //난이도
    public string audioName;         //오디오 이름
    public string imageName;         //이미지 이름
    public List<string> textNames;   // 난이도 파일 이름
    public string localAudioPath;   // 로컬 오디오 주소
    public string localImagePath;   // 로컬 이미지 주소
    public string StorageAudioUrl;  // 서버 오디오 주소
    public string StorageImageUrl;  // 서버 이미지 주소
    public int audioLength;
    public int previewTime;

    public List<NoteData> noteDataList = new List<NoteData>(); // 노트 데이터 추가

    public string tags;
 
    public bool favorite;

    // 추가된 속성들
    public int bpm;
    public int endTime;
    public DateTime dateAdded;
    public int playCount;
    public DateTime lastPlayed;
    public double starRating = -1;

    // 생성자
    public Beatmap()
    {
   //     dateAdded = DateTime.Now;
        playCount = 0;
        lastPlayed = DateTime.MinValue;
    }

    public void Initialize()
    {

    }
    // 플레이 카운터 증가
    public void IncrementPlayCounter()
    {
        playCount++;
        lastPlayed = DateTime.Now;
    }

    // 즐겨찾기 토글
    public void ToggleFavorite()
    {
        favorite = !favorite;
    }
}

//   public AudioClip audioClip;     //음악
//public Texture2D imageTexture;  //이미지
