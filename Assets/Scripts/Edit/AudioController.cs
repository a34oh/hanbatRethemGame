using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    private void Start()
    {
        a.OnAudioSetting += SetEndTime;

        playorPauseButton.onClick.AddListener(a.PlayorPause);
    }
    private void Update()
    {
        MoveProgressBarPos();
        UpdateProgressUI();
    }

    public void Scroll(int scrollDir)
    {
        float movePos;
        movePos = a.BeatPerSec32rd * gridGenerator.ScrollSnapAmount;

        if (scrollDir == 1)
        {
            Debug.Log(movePos + " �� �ڷ�");
            a.ChangePos(movePos);
        }
        else if (scrollDir == -1)
        {
            Debug.Log(movePos + " �� ������");
            a.ChangePos(-movePos);
        }
    }

    public void MoveProgressBarPos() // �������࿡ ����
    {
        if (a.audioSource.clip != null)
            scrollbar.value = GameManager.AudioManager.GetAudioTime() / a.audioSource.clip.length;
    }

    public void ControlProgressBarPos() // ����� ���ۿ� ����
    {
        float pos = scrollbar.value;
        ChangePosByProgressBar(pos);
        CalculatePos(pos);
    }

    public void ChangePosByProgressBar(float pos)
    {
        float time = a.audioSource.clip.length * pos;

        a.audioSource.time = time;
    }
    void CalculatePos(float pos)
    {
        float value = a.audioSource.clip.length * pos;
        gridGenerator.ChangeFixedPos(-value);
    }


    void UpdateProgressUI()
    {
        float progress = a.audioSource.time / a.audioLength;
        progressText.text = $"{progress * 100:F1}%";
        startTimerText.text = a.FormatTime(a.audioSource.time);
    }

    void SetEndTime()
    {
        endTimerText.text = a.FormatTime(a.audioLength);
    }
}
