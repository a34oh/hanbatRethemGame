using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class JudgementLine : MonoBehaviour
{
    private readonly Dictionary<string, float> judgementWindows = new Dictionary<string, float>
    {
        { "Perfect", 0.4f },
        { "Great", 0.8f },
        { "Good", 1.2f },
        { "Bad", 2.0f }
    };

    private readonly Dictionary<KeyCode, float> keyPositions = new Dictionary<KeyCode, float>
    {
        { KeyCode.S, -3.75f },
        { KeyCode.D, -1.25f },
        { KeyCode.L, 1.25f },
        { KeyCode.Semicolon, 3.75f }
    };

    [Header("Mobile UI")]
    public Button leftButton1;    
    public Button leftButton2;    
    public Button rightButton1;   
    public Button rightButton2;

    private void Start()
    {
        leftButton1.onClick.AddListener(() => CheckNoteHit(-3.75f));
        leftButton2.onClick.AddListener(() => CheckNoteHit(-1.25f));
        rightButton1.onClick.AddListener(() => CheckNoteHit(1.25f));
        rightButton2.onClick.AddListener(() => CheckNoteHit(3.75f));
    }

    void Update()
    {
        #if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.S))
            {
                CheckNoteHit(-3.75f);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                CheckNoteHit(-1.25f);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                CheckNoteHit(1.25f);
            }
            if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                CheckNoteHit(3.75f);
            }
        #endif
        CheckMissedNotes();
    }
    private void CheckMissedNotes()
    {
        Note[] notes = FindObjectsOfType<Note>();
        foreach (Note note in notes)
        {
            if (!note.isHit && note.transform.position.y < -2f)
            {
                note.isHit = true;
                GamePlayManager.Instance.ProcessNoteHit("Miss");
                Destroy(note.gameObject);
            }
        }
    }

    private void CheckNoteHit(float xPosition)
    {
        Note[] notes = FindObjectsOfType<Note>();
        List<Note> validNotes = new List<Note>();


        foreach (Note note in notes)
        {
            if (note.isHit) continue;

            float yDistance = Mathf.Abs(note.transform.position.y);
            float xDistance = Mathf.Abs(note.transform.position.x - xPosition);

            if (xDistance < 0.2f && yDistance < 2f) 
            {
                validNotes.Add(note);
            }
        }

        if (validNotes.Count > 0)
        {
            Note closestNote = validNotes.OrderBy(n => Mathf.Abs(n.transform.position.y)).First();
            float closestDistance = Mathf.Abs(closestNote.transform.position.y);


            closestNote.isHit = true;
            string judgement = GetJudgement(closestDistance);
            Vector3 judgementPosition = new Vector3(closestNote.transform.position.x, 0f, 0f);
            GamePlayManager.Instance.ShowJudgement(judgement, judgementPosition);
            GamePlayManager.Instance.ProcessNoteHit(judgement);
            Destroy(closestNote.gameObject);
        }
    }

    private string GetJudgement(float distance)
    {
        
        if (distance <= judgementWindows["Perfect"]) return "Perfect";
        if (distance <= judgementWindows["Great"]) return "Great";
        if (distance <= judgementWindows["Good"]) return "Good";
        if (distance <= judgementWindows["Bad"]) return "Bad";
        return "Miss";
    }
}
