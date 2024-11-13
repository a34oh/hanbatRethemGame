using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BeatmapSetManager : MonoBehaviour
{
    public GameObject beatmapSetNodePrefab;
    public Transform scrollViewContent;
    public TextMeshProUGUI messageText;


    private List<BeatmapSetNode> beatmapSetNodes;
    private BeatmapParser beatmapParser;

    private int currentFocusIndex = 0;

    private void Start()
    {
        StartCoroutine(InitializeBeatmapSets());
    }

    // �� �ε�
    private IEnumerator InitializeBeatmapSets()
    {
        // BeatmapParser�� ���� Beatmap�� �ε��ϰ�, BeatmapSetList�� �߰�
        beatmapParser = new BeatmapParser();
        var task = beatmapParser.ParserAllBeatmapsAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Exception == null)
        {
            List<Beatmap> beatmaps = task.Result;

            if (beatmaps.Count == 0)
            {
                messageText.text = "���� �߰��� ���� �����ϴ�.";
                yield break;
            }
            else
            {
                messageText.text = "";
            }
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
    }

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
}
