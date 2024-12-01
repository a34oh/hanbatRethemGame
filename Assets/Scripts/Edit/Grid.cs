using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public SheetEditor sheetEditor;
    public GridGenerator gridGenerator;
    public int barNumber;

    private float lastAudioTime;
    
    // AudioSettings.dspTime�� �ؾ� ��Ȯ�� Ÿ�̹����� ����
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
            // �� ���� ���� ��� �ð� ���
            double elapsedTime = gridGenerator.a.GetElapsedTime();

            // �׸��� �̵� ���
            float targetYPosition = (float)(-elapsedTime * sheetEditor.Speed + barNumber * gridGenerator.a.BarPerSec * sheetEditor.Speed);

            // ���� ��ġ ������� �̵�
            transform.position = new Vector3(transform.position.x, targetYPosition, transform.position.z);
        }
 */