using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BaseBeatmapSelectController : MonoBehaviour
{
    public Button beatmapCreateButton;
    public Button beatmapBrowserButton;

    public GameObject beatmapCreateCanvas;    
    public GameObject beatmapBrowserCanvas;
    void Start()
    {
        beatmapCreateButton.onClick.AddListener(OnOpenBeatmapCreateCanvas);
        beatmapBrowserButton.onClick.AddListener(OnOpenBeatmapBrowserCanvas);
    }


    void OnOpenBeatmapCreateCanvas()
    {
        GameManager.AudioManager.ClearAudio();
        SceneManager.LoadScene(SceneType.EditorScene.ToString());
//        beatmapCreateCanvas.SetActive(true);
    }
    void OnOpenBeatmapBrowserCanvas()
    {
        GameManager.AudioManager.ClearAudio();
        beatmapBrowserCanvas.SetActive(true);
    }

}
