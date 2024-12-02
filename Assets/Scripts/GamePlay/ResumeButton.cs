using UnityEngine;
using UnityEngine.UI;

public class ResumeButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnResumeButtonClicked);
        }
    }

    private void OnResumeButtonClicked()
    {
        GamePlayManager.Instance.TogglePause();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnResumeButtonClicked);
        }
    }
}