using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
public class BackgroundManager
{
    private RawImage backgroundImage;

    public void SetBackgroundImage(Beatmap beatmap, RawImage go = null)
    {
        if (backgroundImage == null)
        {
            if (go == null)
                backgroundImage = GameObject.Find("BackgroundImage")?.GetComponent<RawImage>();
            else
                backgroundImage = go;

            if(backgroundImage == null)
            {
                Debug.LogError("BackgroundImage 오브젝트가 현재 씬에 존재하지 않습니다.");
                return;
            }
        }

        // ResourceCache에서 이미지 텍스처 가져오기
        if (!string.IsNullOrEmpty(beatmap.localImagePath))
        {
            Texture2D imageTexture = GameManager.ResourceCache.GetCachedImage(beatmap.localImagePath);
            if (imageTexture != null)
            {
                backgroundImage.texture = imageTexture;
            }
            else
            {
                Debug.LogError("이미지 텍스처가 캐시에 존재하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("이미지 경로가 비어 있습니다.");
        }
    }

    public async void SetBackgroundImageFromFirebase(string imageUrl, RawImage go)
    {
        if (go == null)
        {
            Debug.LogWarning("RawImage is null");
            return;
        }
        else if (backgroundImage != go)
            backgroundImage = go;

        if (backgroundImage == null)
        {
            Debug.LogError("BackgroundImage 오브젝트가 현재 씬에 존재하지 않습니다.");
            return;
        }


        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("이미지 URL이 비어 있습니다.");
            return;
        }

        Texture2D texture = await LoadImageFromFirebaseAsync(imageUrl);

        if (texture != null)
        {
            backgroundImage.texture = texture;
            Debug.Log("배경 이미지가 성공적으로 설정되었습니다.");
        }
        else
        {
            Debug.LogError("배경 이미지 로드에 실패했습니다.");
        }
    }

    private async Task<Texture2D> LoadImageFromFirebaseAsync(string imageUrl)
    {
        string fixedUrl = imageUrl.Replace(" ", "%20");

        Debug.Log($"Encoded Image URL: {fixedUrl}");
        Debug.Log($"imageUrl : {imageUrl}");
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(fixedUrl))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("이미지가 Firebase에서 성공적으로 로드되었습니다.");
                return DownloadHandlerTexture.GetContent(www);
            }
            else
            {
                Debug.LogError($"이미지 로드 실패: {www.error}");
                return null;
            }
        }
    }
}
