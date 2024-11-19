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

    private int currentPageItemCount = 0; // 한 페이지의 아이템 갯수
    private int currentPage = 1;
    private Beatmap currentBeatmap; // 현재 클릭 된 곡
    private const int itemsPerPage = 10;

    private async void Start()
    {
        await LoadBeatmapPage(currentPage);

        
        // 화살표 버튼 이벤트 등록
        nextPageButton.onClick.AddListener(() => ChangePage(1));
        prevPageButton.onClick.AddListener(() => ChangePage(-1));

        backButton.onClick.AddListener(OnCloseBeatmapBrowserCanvas);
        downloadButton.onClick.AddListener(OnDownloadButtonClick);

        downloadButton.interactable = false;
        // 버튼 초기 상태 설정
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
            Debug.LogWarning("해당 페이지에 비트맵 데이터가 없습니다.");
        }
    }

    private void UpdateScrollView(List<Beatmap> beatmaps)
    {
        // 기존에 생성된 곡 아이템 제거 (갱신 시 중복 방지)
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
            // 곡 아이템 클릭시 이벤트 등록.. 
            songItem.GetComponent<Button>().onClick.AddListener(() => OnBeatmapItemClick(beatmap));

        }
    }

    // 곡 아이템 클릭 시 처리
    private void OnBeatmapItemClick(Beatmap beatmap)
    {
        Debug.Log($"{beatmap.title} 곡 클릭");

        // 동일한 곡인지 확인
        if (currentBeatmap == beatmap)
        {
            return;
        }

        Debug.Log($"beatmap.StorageImageUrl : {beatmap.StorageImageUrl}");
        Debug.Log($"beatmap.StorageAudioUrl : {beatmap.StorageAudioUrl}");
        // Firebase Storage에서 오디오 및 이미지 로드
        GameManager.AudioManager.PlayPreviewFromFirebase(beatmap);
        GameManager.BackgroundManager.SetBackgroundImageFromFirebase(beatmap.StorageImageUrl, backgroundImage);

        // 현재 클릭 된 곡 업데이트
        currentBeatmap = beatmap;

        // 다운로드 버튼 활성화
        downloadButton.interactable = true;
    }

    private void UpdateNavigationButtons()
    {
        // 이전 페이지 버튼
        prevPageButton.gameObject.SetActive(currentPage > 1);

        // 다음 페이지 버튼
        nextPageButton.gameObject.SetActive(currentPageItemCount == itemsPerPage);
    }

    private void OnDownloadButtonClick()
    {
        if (currentBeatmap == null)
        {
            Debug.LogWarning("다운로드할 곡이 선택되지 않았습니다.");
            return;
        }


        // 다운로드 작업 요청
        GameManager.FBManager.DownloadBeatmap(currentBeatmap);
    }

    void OnCloseBeatmapBrowserCanvas()
    {
        gameObject.SetActive(false);
    }
}
