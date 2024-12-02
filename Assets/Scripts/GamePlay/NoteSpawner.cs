using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NoteSpawner : MonoBehaviour
{
    private bool isInitialized = false;
    public GameObject notePrefab;
    private List<NoteData> notes = new List<NoteData>();
    Beatmap selectedBeatmap = BeatmapSetManager.GameData.SelectedBeatmap;
    private int currentNoteIndex = 0;
    private float gameStartTime;


    void Start()
    {
        notes = selectedBeatmap.noteDataList;
        StartCoroutine(InitializeWithDelay());
    }

    private IEnumerator InitializeWithDelay()
    {
        yield return new WaitUntil(() => GamePlayManager.Instance != null);
        yield return new WaitForSeconds(0.1f);

        gameStartTime = Time.time;
        isInitialized = true;
    }
    
    

    void Update()
    {
        if (!isInitialized) return;
        if (Time.time < gameStartTime) return;

        float currentTime = (Time.time - gameStartTime) * 1000;

        while (currentNoteIndex < notes.Count)
        {
            float noteTime = notes[currentNoteIndex].spawnTime;

            if (currentTime >= noteTime)
            {
                SpawnNote(notes[currentNoteIndex]);
                currentNoteIndex++;
            }
            else
            {
                break;
            }
        }
    }


    void SpawnNote(NoteData noteInfo)
    {
        if (notePrefab == null || GamePlayManager.Instance == null) return;

        GameObject note = Instantiate(notePrefab);
        note.transform.SetParent(null);
        Note noteComponent = note.GetComponent<Note>();

        if (noteComponent != null)
        {
            float speed = GamePlayManager.Instance.GetNoteSpeed();
            float timeToJudgement = 2f;
            float startingY = speed * timeToJudgement;

            note.transform.position = new Vector3(noteInfo.xPosition, startingY, 0);

            noteComponent.speed = speed;
            noteComponent.keyToPress = GetKeyForPosition(noteInfo.xPosition);
            noteComponent.targetTime = (noteInfo.spawnTime / 1000f) + gameStartTime;
            noteComponent.xPosition = noteInfo.xPosition;
            noteComponent.spawnTime = Time.time;
        }
    }

    private KeyCode GetKeyForPosition(float xPos)
    {
        if (xPos == -3.75f) return KeyCode.S;
        if (xPos == -1.25f) return KeyCode.D;
        if (xPos == 1.25f) return KeyCode.L;
        if (xPos == 3.75f) return KeyCode.Semicolon;
        return KeyCode.None;
    }
}
