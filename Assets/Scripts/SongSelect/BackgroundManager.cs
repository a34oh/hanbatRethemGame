using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
public class BackgroundManager
{
    private RawImage backgroundImage;

    public void SetBackgroundImage(Beatmap beatmap, RawImage go, SourceType sourceType)
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
        string imagePath = sourceType == SourceType.Server ? beatmap.StorageImageUrl : beatmap.localImagePath;

        if (string.IsNullOrEmpty(imagePath))
        {
            Debug.LogError("�̹��� URL�� ��� �ֽ��ϴ�.");
            return;
        }

        Texture2D imageTexture = GameManager.ResourceCache.GetCachedImage(imagePath, sourceType);
        if (imageTexture != null)
        {
            backgroundImage.texture = imageTexture;
        }
        else
        {
            Debug.LogError("�̹��� �ؽ�ó�� ĳ�ÿ� �������� �ʽ��ϴ�.");
        }
    }
}
