using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System;

// 파일 선택 인터페이스
public interface IFileBrowser
{
    Task<string> OpenFilePanelAsync(string title, string fileTypes);

}

/*
// PC용 파일 선택기 구현
public class PCFileBrowser : IFileBrowser
{
    public Task<string> OpenFilePanelAsync(string title, string fileTypes)
    {
        // 지원하는 파일 확장자 필터 생성
        string[] extensions = fileTypes.Split(',');
        string filters = string.Join(",", extensions);
        string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters(title, "", new string[] { "Files", filters });

        return Task.FromResult(string.IsNullOrEmpty(path) ? null : path);
    }
}
*/

// 안드로이드용 파일 선택기 구현
public class MobileFileBrowser : IFileBrowser
{
    private static string selectedFilePath;
    private TaskCompletionSource<string> taskCompletionSource;

    public MobileFileBrowser()
    {
        
    }
    public static void SetSelectedFile(string path)
    {
        selectedFilePath = path;
    }

    public Task<string> OpenFilePanelAsync(string title, string fileTypes)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        taskCompletionSource = new TaskCompletionSource<string>();

        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // Intent 생성
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.GET_CONTENT");
            intent.Call<AndroidJavaObject>("setType", ConvertFileTypeToMimeType(fileTypes));
            intent.Call<AndroidJavaObject>("addCategory", "android.intent.category.OPENABLE");

            // UnityPlayerActivity에 결과 처리 등록
            currentActivity.Call("startActivityForResult", intent, 1); // requestCode = 1
        }
        catch (Exception e)
        {
            Debug.LogError("MobileFileBrowser: 파일 선택 중 오류 발생 - " + e.Message);
            taskCompletionSource.SetResult(null);
        }
        return taskCompletionSource.Task;
#else
        Debug.LogError("MobileFileBrowser는 안드로이드에서만 동작합니다.");
        return Task.FromResult<string>(null);
#endif
    }

    // 파일 선택 결과 처리
    public void HandleFileSelection(string path)
    {
        if (taskCompletionSource != null)
        {
            Debug.Log($"파일 경로 설정됨: {path}");
            taskCompletionSource.SetResult(path);
            taskCompletionSource = null;
        }
        else
        {
            Debug.LogError("TaskCompletionSource가 설정되지 않았습니다.");
        }
    }

    private string ConvertFileTypeToMimeType(string fileTypes)
    {
        string[] extensions = fileTypes.Split(',');
        if (extensions.Length == 1)
        {
            switch (extensions[0].ToLower())
            {
                case "mp3": return "audio/*";
                case "png":
                case "jpg":
                case "jpeg": return "image/*";
                default: return "*/*";
            }
        }
        return "*/*";
    }
}


// 파일 업로드 클래스
public class FileUploader
{
    private IFileBrowser fileBrowser;

    public FileUploader(IFileBrowser fileBrowser)
    {
        this.fileBrowser = fileBrowser;
    }

    // 음악 파일 업로드
    public async Task<string> UploadMusicFileAsync()
    {
        try
        {
            // mp3 파일 선택
            string path = await fileBrowser.OpenFilePanelAsync("Select MP3 file", "mp3");

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                Debug.Log("음악 파일이 성공적으로 선택되었습니다: " + path);
                return path;
            }
            else
            {
                Debug.Log("음악 파일을 선택하지 않았거나 파일이 존재하지 않습니다.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("음악 파일 업로드 중 오류 발생: " + ex.Message);
            return null;
        }
    }

    // 이미지 파일 업로드 (PNG, JPG 지원)
    public async Task<string> UploadImageFileAsync()
    {
        try
        {
            // 이미지 파일 선택
            string path = await fileBrowser.OpenFilePanelAsync("Select Image file", "png,jpg,jpeg");

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                Debug.Log("이미지 파일이 성공적으로 선택되었습니다: " + path);
                return path;
            }
            else
            {
                Debug.Log("이미지 파일을 선택하지 않았거나 파일이 존재하지 않습니다.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("이미지 파일 업로드 중 오류 발생: " + ex.Message);
            return null;
        }
    }
}
