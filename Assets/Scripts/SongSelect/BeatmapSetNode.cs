using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class BeatmapSetNode
{
    public BeatmapSet beatmapSet;
    public int beatmapIndex = -1; // Ȯ����� ���� ���¿����� -1
    public int index = 0; // ����Ʈ �������� �ε���
    public BeatmapSetNode prev;
    public BeatmapSetNode next;

    // UI ���
    public GameObject nodeUI;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI artistText;
    private TextMeshProUGUI versionText;
    private RawImage image;
    private Button button;

    // Ŭ�� �̺�Ʈ
    public event Action<BeatmapSetNode> OnClick;

    public BeatmapSetNode(BeatmapSet beatmapSet)
    {
        this.beatmapSet = beatmapSet;
    }

    // ���õ� Beatmap ��ȯ
    public Beatmap GetSelectedBeatmap()
    {
        if (beatmapIndex < 0 || beatmapIndex >= beatmapSet.Count)
            return null;
        return beatmapSet.Get(beatmapIndex);
    }

    // UI �ʱ�ȭ
    public void InitializeUI(GameObject prefab, Transform parent)
    {
        nodeUI = GameObject.Instantiate(prefab, parent);

        // �ʿ��� UI ������Ʈ ���� ��������
        titleText = nodeUI.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        artistText = nodeUI.transform.Find("Artist").GetComponent<TextMeshProUGUI>();
        versionText = nodeUI.transform.Find("Version").GetComponent<TextMeshProUGUI>();
        image = nodeUI.transform.Find("Image").GetComponent<RawImage>();
        button = nodeUI.GetComponent<Button>();

        // Beatmap ������ UI ������Ʈ
        UpdateUI();

        // Ŭ�� �̺�Ʈ ���
        button.onClick.AddListener(() => OnClick?.Invoke(this));
    }

    // UI ������Ʈ
    public void UpdateUI()
    {
        Beatmap beatmap = GetSelectedBeatmap() ?? beatmapSet.Get(0);

        titleText.text = beatmap.title;
        artistText.text = beatmap.artist;
        versionText.text = beatmap.version;

        if (!string.IsNullOrEmpty(beatmap.imagePath))
        {
            Texture2D imageTexture = GameManager.ResourceCache.GetCachedImage(beatmap.imagePath);
            if (imageTexture != null)
            {
                image.texture = imageTexture;
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

    // ��Ŀ�� ���� ����
    public void SetFocus(bool focus)
    {
        // ��Ŀ�� ���¿� ���� UI ������Ʈ (��: ���� ����)
        if (nodeUI != null)
        {
            Image background = nodeUI.GetComponent<Image>();
            if (focus)
            {
                background.color = Color.cyan;
            }
            else
            {
                background.color = Color.white;
            }
        }
    }

    // ���ڿ� ǥ�� ��ȯ
    public override string ToString()
    {
        if (beatmapIndex == -1)
            return beatmapSet.ToString();
        else
            return beatmapSet.Get(beatmapIndex).ToString();
    }
}
