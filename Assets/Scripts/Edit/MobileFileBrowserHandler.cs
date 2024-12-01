using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MobileFileBrowserHandler : MonoBehaviour
{
    private MobileFileBrowser mobileFileBrowser;

    // 초기화 시 MobileFileBrowser를 설정
    public void Initialize(MobileFileBrowser fileBrowser)
    {
        mobileFileBrowser = fileBrowser;
    }

    // 파일 선택 후 Unity에서 호출될 메서드
    public void OnFileSelected(string path)
    {
        Debug.Log($"파일 경로 받음: {path}");
        mobileFileBrowser?.HandleFileSelection(path); // MobileFileBrowser에 전달
    }
}
