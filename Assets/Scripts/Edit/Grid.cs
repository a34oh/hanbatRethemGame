using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public SheetEditor sheetEditor;
    public GridGenerator gridGenerator;
    public int barNumber;

    private float lastAudioTime;
    
    // AudioSettings.dspTime로 해야 정확한 타이밍으로 가능
    void FixedUpdate()
    {
        if (sheetEditor.isPlay)
        {
            float currentAudioTime = gridGenerator.a.audioSource.time;
            float deltaAudioTime = currentAudioTime - lastAudioTime;

            transform.Translate(Vector3.down * deltaAudioTime * sheetEditor.Speed);

            lastAudioTime = currentAudioTime;
        }
        else
        {
            lastAudioTime = gridGenerator.a.audioSource.time;
        }
    }
}

/*
 *  if (sheetEditor.isPlay)
    {
        float currentAudioTime = gridGenerator.a.audioSource.time;
        float targetYPosition = -currentAudioTime * sheetEditor.Speed;

        transform.position = new Vector3(transform.position.x, targetYPosition + barNumber * gridGenerator.a.BarPerSec * sheetEditor.Speed, transform.position.z);
    }
 */


// https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity
/*
 *         if (sheetEditor.isPlay)
        {
            // 곡 시작 이후 경과 시간 계산
            double elapsedTime = gridGenerator.a.GetElapsedTime();

            // 그리드 이동 계산
            float targetYPosition = (float)(-elapsedTime * sheetEditor.Speed + barNumber * gridGenerator.a.BarPerSec * sheetEditor.Speed);

            // 절대 위치 기반으로 이동
            transform.position = new Vector3(transform.position.x, targetYPosition, transform.position.z);
        }
 */