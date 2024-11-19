using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BeatmapBrowser : MonoBehaviour
{
    public GameObject beatmapBrowserPrefab;
    public Transform scrollViewContent;
    public Button nextPageButton;
    public Button prevPageButton;
    public Button backButton;
    public Button downloadButton;
    public RawImage backgroundImage;

    private int currentPageItemCount = 0; // �� �������� ������ ����
    private int currentPage = 1;
    private Beatmap currentBeatmap; // ���� Ŭ�� �� ��
    private const int itemsPerPage = 10;

    private async void Start()
    {
        await LoadBeatmapPage(currentPage);

        
        // ȭ��ǥ ��ư �̺�Ʈ ���
        nextPageButton.onClick.AddListener(() => ChangePage(1));
        prevPageButton.onClick.AddListener(() => ChangePage(-1));

        backButton.onClick.AddListener(OnCloseBeatmapBrowserCanvas);
        downloadButton.onClick.AddListener(OnDownloadButtonClick);

        downloadButton.interactable = false;
        // ��ư �ʱ� ���� ����
        UpdateNavigationButtons();
    }

    private async void ChangePage(int direction)
    {
        currentPage += direction;
        currentPage = Mathf.Max(1, currentPage);

        await LoadBeatmapPage(currentPage);

        UpdateNavigationButtons();
    }

    private async Task LoadBeatmapPage(int pageIndex)
    {
        int startIndex = (pageIndex - 1) * itemsPerPage;

        var beatmapData = await GameManager.FBManager.FetchBeatmapMetadataAsync(startIndex, itemsPerPage);

        currentPageItemCount = beatmapData.Count;
        if (beatmapData != null && beatmapData.Count > 0)
        {
            UpdateScrollView(beatmapData);
        }
        else
        {
            Debug.LogWarning("�ش� �������� ��Ʈ�� �����Ͱ� �����ϴ�.");
        }
    }

    private void UpdateScrollView(List<Beatmap> beatmaps)
    {
        // ������ ������ �� ������ ���� (���� �� �ߺ� ����)
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        foreach (Beatmap beatmap in beatmaps)
        {

            GameObject songItem = Instantiate(beatmapBrowserPrefab, scrollViewContent);
            songItem.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = beatmap.title;
            songItem.transform.Find("Artist").GetComponent<TextMeshProUGUI>().text = beatmap.artist;
            songItem.transform.Find("Version").GetComponent<TextMeshProUGUI>().text = beatmap.version;

            songItem.transform.Find("Image").GetComponent<RawImage>().texture = GameManager.ResourceCache.GetCachedImage(beatmap.StorageImageUrl);
            // �� ������ Ŭ���� �̺�Ʈ ���.. 
            songItem.GetComponent<Button>().onClick.AddListener(() => OnBeatmapItemClick(beatmap));

        }
    }

    // �� ������ Ŭ�� �� ó��
    private void OnBeatmapItemClick(Beatmap beatmap)
    {
        Debug.Log($"{beatmap.title} �� Ŭ��");

        // ������ ������ Ȯ��
        if (currentBeatmap == beatmap)
        {
            return;
        }

        Debug.Log($"beatmap.StorageImageUrl : {beatmap.StorageImageUrl}");
        Debug.Log($"beatmap.StorageAudioUrl : {beatmap.StorageAudioUrl}");
        // Firebase Storage���� ����� �� �̹��� �ε�
        GameManager.AudioManager.PlayPreviewFromFirebase(beatmap);
        GameManager.BackgroundManager.SetBackgroundImageFromFirebase(beatmap.StorageImageUrl, backgroundImage);

        // ���� Ŭ�� �� �� ������Ʈ
        currentBeatmap = beatmap;

        // �ٿ�ε� ��ư Ȱ��ȭ
        downloadButton.interactable = true;
    }

    private void UpdateNavigationButtons()
    {
        // ���� ������ ��ư
        prevPageButton.gameObject.SetActive(currentPage > 1);

        // ���� ������ ��ư
        nextPageButton.gameObject.SetActive(currentPageItemCount == itemsPerPage);
    }

    private void OnDownloadButtonClick()
    {
        if (currentBeatmap == null)
        {
            Debug.LogWarning("�ٿ�ε��� ���� ���õ��� �ʾҽ��ϴ�.");
            return;
        }


        // �ٿ�ε� �۾� ��û
        GameManager.FBManager.DownloadBeatmap(currentBeatmap);
    }

    void OnCloseBeatmapBrowserCanvas()
    {
        gameObject.SetActive(false);
    }
}
