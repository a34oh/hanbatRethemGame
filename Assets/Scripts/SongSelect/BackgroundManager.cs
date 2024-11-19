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
                Debug.LogError("BackgroundImage ������Ʈ�� ���� ���� �������� �ʽ��ϴ�.");
                return;
            }
        }

        // ResourceCache���� �̹��� �ؽ�ó ��������
        if (!string.IsNullOrEmpty(beatmap.localImagePath))
        {
            Texture2D imageTexture = GameManager.ResourceCache.GetCachedImage(beatmap.localImagePath);
            if (imageTexture != null)
            {
                backgroundImage.texture = imageTexture;
            }
            else
            {
                Debug.LogError("�̹��� �ؽ�ó�� ĳ�ÿ� �������� �ʽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogError("�̹��� ��ΰ� ��� �ֽ��ϴ�.");
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
            Debug.LogError("BackgroundImage ������Ʈ�� ���� ���� �������� �ʽ��ϴ�.");
            return;
        }


        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("�̹��� URL�� ��� �ֽ��ϴ�.");
            return;
        }

        Texture2D texture = await LoadImageFromFirebaseAsync(imageUrl);

        if (texture != null)
        {
            backgroundImage.texture = texture;
            Debug.Log("��� �̹����� ���������� �����Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogError("��� �̹��� �ε忡 �����߽��ϴ�.");
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
                Debug.Log("�̹����� Firebase���� ���������� �ε�Ǿ����ϴ�.");
                return DownloadHandlerTexture.GetContent(www);
            }
            else
            {
                Debug.LogError($"�̹��� �ε� ����: {www.error}");
                return null;
            }
        }
    }
}
