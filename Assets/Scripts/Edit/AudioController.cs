using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    public SheetEditorController sheetController;
    public GridGenerator gridGenerator;
    public Audio a;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI startTimerText;
    public TextMeshProUGUI endTimerText;
    public Button playorPauseButton;
    public Scrollbar scrollbar;
    private bool isUserInteracting = false;

    private void Start()
    {
        a.OnAudioSetting += SetEndTime;

        playorPauseButton.onClick.AddListener(a.PlayorPause);
        ScrollbarSetEventTrigger();
        scrollbar.onValueChanged.AddListener(OnScrollBarValueChanged);
    }
    private void Update()
    {
        MoveProgressBarPos();
        UpdateProgressUI();
    }

    private void ScrollbarSetEventTrigger()
    {
        EventTrigger eventTrigger = scrollbar.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDownEntry.callback.AddListener((data) => { OnBeginUserInteraction(); });
        eventTrigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUpEntry.callback.AddListener((data) => { OnEndUserInteraction(); });
        eventTrigger.triggers.Add(pointerUpEntry);

    }
    public void OnBeginUserInteraction()
    {
        isUserInteracting = true;
    }

    public void OnEndUserInteraction()
    {
        isUserInteracting = false;
    }


    public void Scroll(int scrollDir)
    {
        float movePos;
        movePos = a.BeatPerSec32rd * gridGenerator.ScrollSnapAmount;

        if (scrollDir == 1)
        {
            Debug.Log(movePos + " 초 뒤로");
            a.ChangePos(movePos);
        }
        else if (scrollDir == -1)
        {
            Debug.Log(movePos + " 초 앞으로");
            a.ChangePos(-movePos);
        }
    }

    public void MoveProgressBarPos() // 음악진행에 의한
    {
        if (a.audioSource.clip != null)
            scrollbar.value = GameManager.AudioManager.GetAudioTime() / a.audioSource.clip.length;
    }

    public void OnScrollBarValueChanged(float value) // 사용자 조작에 의한
    {
        float pos = scrollbar.value;
        if (isUserInteracting)
        {
            ChangeAudioSourceTime(pos);
            CalculatePos(pos);
        }
    }

    public void ChangeAudioSourceTime(float pos)
    {
        float time = a.audioSource.clip.length * pos;

        a.audioSource.time = time;
    }
    private void CalculatePos(float pos)
    {
        float value = a.audioSource.clip.length * pos;
        gridGenerator.ChangeFixedPos(-value);
    }


    private void UpdateProgressUI()
    {
        float progress = a.audioSource.time / a.audioLength;
        progressText.text = $"{progress * 100:F1}%";
        startTimerText.text = a.FormatTime(a.audioSource.time);
    }

    private void SetEndTime()
    {
        endTimerText.text = a.FormatTime(a.audioLength);
    }
}
