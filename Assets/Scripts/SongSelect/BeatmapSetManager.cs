using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;

public class BeatmapSetManager : MonoBehaviour
{
  //  public GameObject beatmapSetNodePrefab;
    public Transform scrollViewContent;
    public TextMeshProUGUI messageText;
    public GameObject beatmapPrefab;
    public RawImage backgroundImage;

 //   private List<BeatmapSetNode> beatmapSetNodes;
    private BeatmapParser beatmapParser;

  //  private int currentFocusIndex = 0;
    private Beatmap currentBeatmap; // ���� ��� ���� ��

    private void Start()
    {
        beatmapParser = new BeatmapParser();
        InitializeBeatmapSetsAsync().ConfigureAwait(false);
        //������ �ʱ�ȭ�� GameManager���� �����ϰ� ����, BeatmapSetList�� ���� ��Ʈ�� ������ ����.
    }

    // �� �ε�
    private async Task InitializeBeatmapSetsAsync()
    {
        // BeatmapParser�� ���� Beatmap�� �ε��ϰ�, BeatmapSetList�� �߰�
        try
        {
            // BeatmapParser�� ���� ��Ʈ�� ������ �ε�
            List<Beatmap> beatmaps = await beatmapParser.ParserAllBeatmapsAsync();

            if (beatmaps.Count == 0)
            {
                messageText.text = "���� �߰��� ���� �����ϴ�.";
                return;
            }

            messageText.text = "";

            // ScrollView�� ��Ʈ�� �����͸� ä��
            UpdateScrollView(beatmaps);
        }
        catch (Exception ex)
        {
            Debug.LogError($"��Ʈ�� �ʱ�ȭ �� ���� �߻�: {ex.Message}");
        }

        /*
        Debug.Log($"beatmaps count : {beatmaps.Count}");
        // BeatmapSetList�� Beatmap �߰�
        // ���⼭�� �ܼ��� ��� Beatmap�� �ϳ��� BeatmapSet���� �߰�������,
        // �����δ� ��Ƽ��Ʈ�� ������ �������� �׷�ȭ�ؾ� �մϴ�.
        //GameManager.BeatmapSetList.AddSongGroup(beatmaps);

        // Beatmap���� ��Ƽ��Ʈ�� ������ �������� �׷�ȭ
        Dictionary<string, List<Beatmap>> beatmapGroups = new Dictionary<string, List<Beatmap>>();

        foreach (Beatmap beatmap in beatmaps)
        {
            string key = $"{beatmap.artist}-{beatmap.title}";
            if (!beatmapGroups.ContainsKey(key))
            {
                beatmapGroups[key] = new List<Beatmap>();
            }
            beatmapGroups[key].Add(beatmap);
        }

        // BeatmapSetList�� �׷�ȭ�� BeatmapSet �߰�
        foreach (var group in beatmapGroups)
        {
            GameManager.BeatmapSetList.AddSongGroup(group.Value);
        }

        // ����Ʈ�� �籸���ϱ� ���� Reset ȣ��
         GameManager.BeatmapSetList.Reset();

        // ���� �� �ʱ�ȭ
        GameManager.BeatmapSetList.Init();

        // ��� ��� ��������
        beatmapSetNodes = new List<BeatmapSetNode>();
        Debug.Log($"GameManager.BeatmapSetList.Size() : {GameManager.BeatmapSetList.Size()}");

        for (int i = 0; i < GameManager.BeatmapSetList.Size(); i++)
        {
            BeatmapSetNode node = GameManager.BeatmapSetList.GetBaseNode(i);
            node.InitializeUI(beatmapSetNodePrefab, scrollViewContent);
            beatmapSetNodes.Add(node);

            // Ŭ�� �̺�Ʈ ���
            node.OnClick += OnBeatmapSetNodeClick;
        }

        // ù ��° ��忡 ��Ŀ�� ����
        SetFocusNode(0);
    }
    else
    {
        Debug.LogError("�� �ε� �� ���� �߻�: " + task.Exception.Message);
    }
       */
    }
    
    // ��ũ�Ѻ信 �� ��� ǥ��
    private void UpdateScrollView(List<Beatmap> beatmaps)
    {
        // ������ ������ �� ������ ���� (���� �� �ߺ� ����)
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        foreach (Beatmap beatmap in beatmaps)
        {

            GameObject songItem = Instantiate(beatmapPrefab, scrollViewContent);
            songItem.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = beatmap.title;
            songItem.transform.Find("Artist").GetComponent<TextMeshProUGUI>().text = beatmap.artist;
            songItem.transform.Find("Version").GetComponent<TextMeshProUGUI>().text = beatmap.version;

            songItem.transform.Find("Image").GetComponent<RawImage>().texture = GameManager.ResourceCache.GetCachedImage(beatmap.localImagePath, SourceType.Local);
            // �� ������ Ŭ�� �̺�Ʈ ���
            songItem.GetComponent<Button>().onClick.AddListener(() => OnSongItemClick(beatmap));

        }
    }
    // �� ������ Ŭ�� �� ó��
    private void OnSongItemClick(Beatmap beatmap)
    {
        Debug.Log($"{beatmap.title} �� Ŭ��");

        // ������ ������ Ȯ��
        if (currentBeatmap == beatmap)
        {
            // ������ ���̸� �ƹ� ���۵� ���� ����
            return;
        }

        GameManager.AudioManager.PlayPreview(beatmap, SourceType.Local);
        GameManager.BackgroundManager.SetBackgroundImage(beatmap, backgroundImage, SourceType.Local);

        // ���� ��� ���� �� ������Ʈ
        currentBeatmap = beatmap;
    }

    /*
    // ��Ŀ���� ��� ����
    public void SetFocusNode(int index)
    {
        if (index < 0 || index >= beatmapSetNodes.Count)
            return;
        Debug.Log($"SetFocusNode index : {index}, currentFocusIndex : {currentFocusIndex}");
        // ���� ��Ŀ���� ����� ��Ŀ�� ����
        beatmapSetNodes[currentFocusIndex].SetFocus(false);

        // ���ο� ��忡 ��Ŀ�� ����
        currentFocusIndex = index;
        beatmapSetNodes[currentFocusIndex].SetFocus(true);
    }

    // BeatmapSetNode Ŭ�� �� ó��
    private void OnBeatmapSetNodeClick(BeatmapSetNode node)
    {
        Debug.Log("OnBeatmapSetNodeClick");
        // ��Ŀ���� ��� ����
        int index = beatmapSetNodes.IndexOf(node);
        SetFocusNode(index);

        // ��� Ȯ��
        GameManager.BeatmapSetList.Expand(index);

        // UI ����
        RefreshUI();

        // ����� �� ��� �̹��� ����
        Beatmap selectedBeatmap = node.GetSelectedBeatmap();
        if (selectedBeatmap != null)
        {
            GameManager.AudioManager.PlayPreview(selectedBeatmap);
            GameManager.BackgroundManager.SetBackgroundImage(selectedBeatmap);
        }
    }

    // UI ���� �޼��� (Ȯ��/��� �� ȣ��)
    private void RefreshUI()
    {
        // ���� UI ����
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        // ��� ��� ����
        beatmapSetNodes = new List<BeatmapSetNode>();
        Debug.Log($" GameManager.BeatmapSetList.Size() : { GameManager.BeatmapSetList.Size()}");
        for (int i = 0; i < GameManager.BeatmapSetList.Size(); i++)
        {
            BeatmapSetNode node = GameManager.BeatmapSetList.GetBaseNode(i) ?? GameManager.BeatmapSetList.GetNode(i);
            Debug.Log("��Ʈ ����");
            node.InitializeUI(beatmapSetNodePrefab, scrollViewContent);
            beatmapSetNodes.Add(node);

            // Ŭ�� �̺�Ʈ ���
            node.OnClick += OnBeatmapSetNodeClick;
        }
    }
    */
}
