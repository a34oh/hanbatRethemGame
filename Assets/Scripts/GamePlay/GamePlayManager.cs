using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance { get; private set; }
    public bool IsInitialized { get; private set; }

    [Header("UI References")]
    public GameObject gameplayUI;
    public GameObject settingsUI;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI songTimeText;

    [Header("Audio")]
    public AudioSource musicSource;

    [Header("Settings")]
    public Slider syncSpeedSlider;
    public Slider volumeSlider;
    public Slider noteSpeedSlider;
    public TextMeshProUGUI syncSpeedText;
    public TextMeshProUGUI volumeText;
    public TextMeshProUGUI noteSpeedText;

    [Header("Judgement Images")]
    public Sprite perfectSprite;
    public Sprite greatSprite;
    public Sprite goodSprite;
    public Sprite badSprite;
    public Sprite missSprite;

    [Header("Restart Confirmation UI")]
    public GameObject restartConfirmationUI;
    public Button yesButton;
    public Button noButton;

    [Header("Judge Count UI")]
    public TextMeshProUGUI perfectCountText;
    public TextMeshProUGUI greatCountText;
    public TextMeshProUGUI goodCountText;
    public TextMeshProUGUI badCountText;
    public TextMeshProUGUI missCountText;

    private float initialNoteSpeed;
    private bool isPaused = false;
    private float syncSpeed = 0f;
    private float volume = 1f;
    private float noteSpeed = 22f;
    private int score = 0;
    private int combo = 0;
    private Dictionary<float, Image> judgementImages = new Dictionary<float, Image>();
    private Dictionary<float, Coroutine> judgementCoroutines = new Dictionary<float, Coroutine>();
    private int perfectCount = 0;
    private int greatCount = 0;
    private int goodCount = 0;
    private int badCount = 0;
    private int missCount = 0;
    private int maxCombo = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            IsInitialized = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Beatmap selectedBeatmap = BeatmapSetManager.GameData.SelectedBeatmap;
        if (selectedBeatmap != null)
        {
            musicSource.GetComponent<AudioSource>().clip = GameManager.ResourceCache.GetCachedAudio(selectedBeatmap.localAudioPath, SourceType.Local);
        }

        if (settingsUI != null)
        {
            settingsUI.SetActive(false);
        }

        if (PlayerPrefs.HasKey("NoteSpeed"))
        {
            noteSpeed = PlayerPrefs.GetFloat("NoteSpeed", 22f);
        }
        else
        { 
            noteSpeed = 22f;
            PlayerPrefs.SetFloat("NoteSpeed", 22f);
            PlayerPrefs.Save();
        }
        syncSpeed = PlayerPrefs.GetFloat("SyncSpeed", 0f);
        volume = PlayerPrefs.GetFloat("Volume", 1f);
        
        initialNoteSpeed = noteSpeed;
        ApplyNoteSpeed(noteSpeed);

        

        if (syncSpeedSlider != null)
        {
            syncSpeedSlider.onValueChanged.AddListener(OnSyncSpeedChanged);
            syncSpeedSlider.minValue = -50f;
            syncSpeedSlider.maxValue = 50f;
            syncSpeedSlider.value = syncSpeed * 10f;
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            volumeSlider.value = volume;
        }

        if (noteSpeedSlider != null)
        {
            noteSpeedSlider.onValueChanged.RemoveAllListeners();
            noteSpeedSlider.minValue = 4f;
            noteSpeedSlider.maxValue = 30f;
            noteSpeedSlider.value = noteSpeed;
            noteSpeedSlider.onValueChanged.AddListener(OnNoteSpeedChanged);
        }

        UpdateSyncSpeedText(syncSpeed);
        UpdateVolumeText(volume);
        UpdateNoteSpeedText(noteSpeed);

        float[] lanePositions = { -3.75f, -1.25f, 1.25f, 3.75f };
        foreach (float xPos in lanePositions)
        {
            judgementImages[xPos] = CreateJudgementImage(xPos);
            judgementImages[xPos].enabled = false;
        }

        if (yesButton != null) yesButton.onClick.AddListener(OnRestartConfirmed);
        if (noButton != null) noButton.onClick.AddListener(OnRestartDeclined);

        if (restartConfirmationUI != null) restartConfirmationUI.SetActive(false);

       
        StartCoroutine(StartGameWithDelay());
    }


    void Update()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            float currentTime = musicSource.time;
            float totalTime = musicSource.clip.length;
            songTimeText.text = $"{currentTime:F1} / {totalTime:F1}";

            if (!musicSource.isPlaying && currentTime >= totalTime)
            {
                StartCoroutine(LoadResultScene());
            }
        }
    }

    private Image CreateJudgementImage(float xPos)
    {
        GameObject imageObj = new GameObject($"JudgementImage_{xPos}");
        Canvas canvas = GameObject.Find("GameplayCanvas").GetComponent<Canvas>();
        imageObj.transform.SetParent(canvas.transform, false);

        Image image = imageObj.AddComponent<Image>();
        image.raycastTarget = false;

        RectTransform rect = image.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250, 120);

        if (xPos == -3.75f) rect.anchoredPosition = new Vector2(-430, -450);
        else if (xPos == -1.25f) rect.anchoredPosition = new Vector2(-140, -450);
        else if (xPos == 1.25f) rect.anchoredPosition = new Vector2(140, -450);
        else if (xPos == 3.75f) rect.anchoredPosition = new Vector2(430, -450);

        image.transform.SetAsLastSibling();
        return image;
    }

    public void ShowJudgement(string judgement, Vector3 position)
    {
        float xPos = position.x;

        if (!judgementImages.ContainsKey(xPos))
        {
            judgementImages[xPos] = CreateJudgementImage(xPos);
        }

        Image judgementImage = judgementImages[xPos];
        judgementImage.enabled = true;

        switch (judgement)
        {
            case "Perfect":
                judgementImage.sprite = perfectSprite;
                break;
            case "Great":
                judgementImage.sprite = greatSprite;
                break;
            case "Good":
                judgementImage.sprite = goodSprite;
                break;
            case "Bad":
                judgementImage.sprite = badSprite;
                break;
            case "Miss":
                judgementImage.sprite = missSprite;
                break;
        }

        if (judgementCoroutines.ContainsKey(xPos))
        {
            StopCoroutine(judgementCoroutines[xPos]);
        }

        judgementCoroutines[xPos] = StartCoroutine(HideJudgementAfterDelay(judgementImage));
    }

    private IEnumerator HideJudgementAfterDelay(Image image)
    {
        yield return new WaitForSeconds(0.5f);
        image.enabled = false;
    }

    public void ProcessNoteHit(string judgement)
    {
        switch (judgement)
        {
            case "Perfect":
                score += 100;
                combo++;
                perfectCount++;
                break;
            case "Great":
                score += 80;
                combo++;
                greatCount++;
                break;
            case "Good":
                score += 50;
                combo++;
                goodCount++;
                break;
            case "Bad":
                score += 20;
                combo = 0;
                badCount++;
                break;
            case "Miss":
                combo = 0;
                missCount++;
                break;
        }

        if (combo > maxCombo)
        {
            maxCombo = combo;
        }
        UpdateUI();
        UpdateJudgeCountUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (comboText != null) comboText.text = "Combo: " + combo;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        musicSource.Pause();
        gameplayUI.SetActive(false);
        settingsUI.SetActive(true);
    }

    private void ResumeGame()
    {
        if (noteSpeed != initialNoteSpeed)
        {
            ShowRestartConfirmation(); 
        }
        else
        {
            settingsUI.SetActive(false);
            Time.timeScale = 1f;
            musicSource.Play();
            gameplayUI.SetActive(true);
        }
    }

    private void ShowRestartConfirmation()
    {
        settingsUI.SetActive(false);
        restartConfirmationUI.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnRestartConfirmed()
    {
        restartConfirmationUI.SetActive(false);
        RestartGame();
    }

    private void OnRestartDeclined()
    {
        restartConfirmationUI.SetActive(false);
        initialNoteSpeed = noteSpeed;
        settingsUI.SetActive(false);
        Time.timeScale = 1f;
        musicSource.Play();
        gameplayUI.SetActive(true);
    }


    private void RestartGame()
    {
        PlayerPrefs.SetFloat("NoteSpeed", noteSpeed);
        PlayerPrefs.Save();


        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnSyncSpeedChanged(float value)
    {
        syncSpeed = value / 10f;
        UpdateSyncSpeedText(syncSpeed);
        PlayerPrefs.SetFloat("SyncSpeed", syncSpeed);
        PlayerPrefs.Save();
        ApplySyncSpeed(syncSpeed);
    }

    private void OnVolumeChanged(float value)
    {
        volume = value;
        UpdateVolumeText(volume);
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
    }

    private void OnNoteSpeedChanged(float value)
    {
        noteSpeed = value;
        UpdateNoteSpeedText(noteSpeed);
        PlayerPrefs.SetFloat("NoteSpeed", noteSpeed);
        PlayerPrefs.Save();
    }

    private void UpdateSyncSpeedText(float speed)
    {
        syncSpeedText.text = $"Sync: {speed:F1} s";
    }

    private void UpdateVolumeText(float vol)
    {
        volumeText.text = $"Volume: {vol:P0}";
    }

    private void UpdateNoteSpeedText(float speed)
    {
        noteSpeedText.text = $"Note Speed: {speed:F1}";
    }

    private void ApplySyncSpeed(float speed)
    {
        musicSource.time += speed;
    }

    private void ApplyNoteSpeed(float speed)
    {
        noteSpeed = speed;
        PlayerPrefs.SetFloat("NoteSpeed", speed);
        PlayerPrefs.Save();
    }



    public float GetNoteSpeed()
    {
        return noteSpeed;
    }

    private IEnumerator StartGameWithDelay()
    {
        if (musicSource != null)
        {
            musicSource.Play();
            musicSource.Pause();
            musicSource.time = 0;
            yield return new WaitForSeconds(2f);
            musicSource.Play();
        }
    }

    private void OnDestroy()
    {
        if (yesButton != null) yesButton.onClick.RemoveListener(OnRestartConfirmed);
        if (noButton != null) noButton.onClick.RemoveListener(OnRestartDeclined);
    }

    private void UpdateJudgeCountUI()
    {
        if (perfectCountText != null) perfectCountText.text = $"Perfect: {perfectCount}";
        if (greatCountText != null) greatCountText.text = $"Great: {greatCount}";
        if (goodCountText != null) goodCountText.text = $"Good: {goodCount}";
        if (badCountText != null) badCountText.text = $"Bad: {badCount}";
        if (missCountText != null) missCountText.text = $"Miss: {missCount}";
    }

    private IEnumerator LoadResultScene()
    {
        yield return new WaitForSeconds(2f);

        PlayerPrefs.SetInt("PerfectCount", perfectCount);
        PlayerPrefs.SetInt("GreatCount", greatCount);
        PlayerPrefs.SetInt("GoodCount", goodCount);
        PlayerPrefs.SetInt("BadCount", badCount);
        PlayerPrefs.SetInt("MissCount", missCount);
        PlayerPrefs.SetInt("MaxCombo", maxCombo);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Scenes/ScoreScene"); // 결과 씬 없어서 메인메뉴로 대체해놓음
    }
}
