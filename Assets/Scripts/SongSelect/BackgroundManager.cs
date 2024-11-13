using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager
{
    private RawImage backgroundImage;

    // Init �޼��忡�� ��� �̹����� �ʱ�ȭ���� ���� (������ ����)
    public void SetBackgroundImage(Beatmap beatmap)
    {
        if (backgroundImage == null)
        {
            GameObject bgObject = GameObject.Find("BackgroundImage");
            if (bgObject != null)
            {
                backgroundImage = bgObject.GetComponent<RawImage>();
            }
            else
            {
                Debug.LogError("BackgroundImage ������Ʈ�� ���� ���� �������� �ʽ��ϴ�.");
                return;
            }
        }

        // ResourceCache���� �̹��� �ؽ�ó ��������
        if (!string.IsNullOrEmpty(beatmap.imagePath))
        {
            Texture2D imageTexture = GameManager.ResourceCache.GetCachedImage(beatmap.imagePath);
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
}
