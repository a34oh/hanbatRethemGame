using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.NetworkInformation;
public class OpenUI : MonoBehaviour
{
    public Button CreateAccountSetting;
    public Button CreateAccount;
    public Button Login;
    public Button OpenSettingButton;
    public Button CloseSettingButton;
    public TMP_InputField Input_id;
    public TMP_InputField Input_pw;
    public TMP_InputField Input_new_id;
    public TMP_InputField Input_new_pw;
    public GameObject OpenSettingCanvas;
    public GameObject AccountSettingCanvas;
    

    void Start()
    {
        Debug.Log(IsDeviceOnline());
        OpenSettingButton.onClick.AddListener(OnOpenSettingCanvas);
        CloseSettingButton.onClick.AddListener(OnCloseSettingCanvas);
        CreateAccountSetting.onClick.AddListener(OnOpenAccountCanvas);
        CreateAccount.onClick.AddListener(OnCloseAccountCanvas);
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("인터넷에 연결되어 있지 않습니다.");
        }
    }
    public bool IsDeviceOnline()
    {
        try
        {
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            PingReply reply = ping.Send("google.com", 1000);  // 인터넷 연결이 가능한지 확인
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
    void OnOpenSettingCanvas()
    {
        OpenSettingCanvas.SetActive(true);
    }

    void OnCloseSettingCanvas()
    {
        OpenSettingCanvas.SetActive(false);
    }
    void OnOpenAccountCanvas()
    {
        Debug.Log(2);
        AccountSettingCanvas.SetActive(true);
    }
    void OnCloseAccountCanvas()
    {
        AccountSettingCanvas.SetActive(false);
    }

    public void LogIn_Button()
    {
        if (!GameManager.FBManager.isauth())
        {
            Debug.LogError("Firebase 인증이 초기화되지 않았습니다.");
            return;
        }
        string player_id = Input_id.text;
        string player_pw = Input_pw.text;
        GameManager.FBManager.login(player_id, player_pw);
    }

    public void Create_Account()
    {
        if (!GameManager.FBManager.isauth())
        {
            Debug.LogError("Firebase 인증이 초기화되지 않았습니다.");
            return;
        }
        string player_id = Input_new_id.text;
        string player_pw = Input_new_pw.text;
        GameManager.FBManager.register(player_id,player_pw);
    }
}
