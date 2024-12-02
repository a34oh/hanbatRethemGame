using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuGameManager : MonoBehaviour
{
    public Button startButton;
    private void Start()
    {
        float syncSpeed = PlayerPrefs.GetFloat("SyncSpeed", 0f);
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        float noteSpeed = PlayerPrefs.GetFloat("NoteSpeed", 0f);

        PlayerPrefs.SetFloat("SyncSpeed", syncSpeed);
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat("NoteSpeed", noteSpeed);
        PlayerPrefs.Save();

        startButton.onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Scenes/GamePlay");
    }
}