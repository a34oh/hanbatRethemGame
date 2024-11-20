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
            Debug.LogError("BackgroundImage 오브젝트가 현재 씬에 존재하지 않습니다.");
            return;
        }
        string imagePath = sourceType == SourceType.Server ? beatmap.StorageImageUrl : beatmap.localImagePath;

        if (string.IsNullOrEmpty(imagePath))
        {
            Debug.LogError("이미지 URL이 비어 있습니다.");
            return;
        }

        Texture2D imageTexture = GameManager.ResourceCache.GetCachedImage(imagePath, sourceType);
        if (imageTexture != null)
        {
            backgroundImage.texture = imageTexture;
        }
        else
        {
            Debug.LogError("이미지 텍스처가 캐시에 존재하지 않습니다.");
        }
    }
}
