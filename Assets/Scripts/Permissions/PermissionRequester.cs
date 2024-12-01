using UnityEngine;
using UnityEngine.Android;

public class PermissionRequester : MonoBehaviour
{
    void Start()
    {
        // 권한 요청 상태 확인
        if (!PlayerPrefs.HasKey("PermissionsGranted"))
        {
            RequestPermissions();
            PlayerPrefs.SetInt("PermissionsGranted", 1); // 권한 요청 완료 표시
        }
    }

    void RequestPermissions()
    {
        // 외부 저장소 읽기 권한 요청
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }

        // 외부 저장소 쓰기 권한 요청
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        // Android 11(API Level 30) 이상에서 전체 파일 관리 권한 요청
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android && AndroidVersionCheck() >= 30)
        {
            // 권한 상태를 확인
            if (!IsManageExternalStorageGranted())
            {
                using (var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var currentActivity = activity.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaObject permissionIntent = new AndroidJavaObject(
                        "android.content.Intent",
                        "android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION"
                    );
                    currentActivity.Call("startActivity", permissionIntent);
                }
            }
        }
#endif
    }

    // 안드로이드 API Level 확인
    private int AndroidVersionCheck()
    {
        using (var versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return versionClass.GetStatic<int>("SDK_INT");
        }
    }

    // Android 11 이상에서 전체 파일 관리 권한 부여 상태 확인
    private bool IsManageExternalStorageGranted()
    {
        bool isGranted = false;

#if UNITY_ANDROID
        if (AndroidVersionCheck() >= 30)
        {
            using (var environment = new AndroidJavaClass("android.os.Environment"))
            {
                isGranted = environment.CallStatic<bool>("isExternalStorageManager");
            }
        }
#endif

        return isGranted;
    }
}
