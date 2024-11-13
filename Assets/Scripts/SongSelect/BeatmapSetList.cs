using System;
using System.Collections.Generic;

public class BeatmapSetList
{
    // ��ü BeatmapSetNode ���
    public List<BeatmapSetNode> parsedNodes { get; private set; }

    // ���� �׷쿡 ���� ��� ���
    private List<BeatmapSetNode> groupNodes;

    // �˻� ����� ���� ��� ���
    private List<BeatmapSetNode> nodes;

    // ��ü Beatmap ��
    private int mapCount = 0;

    // ���� Ȯ��� ����� �ε��� (-1�̸� Ȯ�� ����)
    private int expandedIndex = -1;

    // Ȯ��� ����� ���۰� ��
    private BeatmapSetNode expandedStartNode;
    private BeatmapSetNode expandedEndNode;

    // ������ �˻���
    private string lastQuery;


    public BeatmapSetList()
    {
        parsedNodes = new List<BeatmapSetNode>();
        Reset();
    }

    // ����Ʈ �ʱ�ȭ
    public void Reset()
    {
        groupNodes = BeatmapGroupExtensions.Current.Filter(parsedNodes);
        nodes = new List<BeatmapSetNode>(groupNodes);
        expandedIndex = -1;
        expandedStartNode = null;
        expandedEndNode = null;
        lastQuery = null;
    }

    // ��� �߰�
    public void AddSongGroup(List<Beatmap> beatmaps)
    {
        BeatmapSet beatmapSet = new BeatmapSet(beatmaps);
        BeatmapSetNode node = new BeatmapSetNode(beatmapSet);
        parsedNodes.Add(node);
        mapCount += beatmaps.Count;
    }

    // ��� ���� ��ȯ
    public int Size()
    {
        return nodes.Count;
    }

    // ���� �ʱ�ȭ
    public void Init()
    {
        if (Size() < 1)
            return;

        // ����
        nodes.Sort(BeatmapSortOrderExtensions.Current.GetComparator());
        expandedIndex = -1;
        expandedStartNode = null;
        expandedEndNode = null;

        // ��ũ�� ����Ʈ ����
        BeatmapSetNode lastNode = nodes[0];
        lastNode.index = 0;
        lastNode.prev = null;
        for (int i = 1; i < Size(); i++)
        {
            BeatmapSetNode node = nodes[i];
            lastNode.next = node;
            node.index = i;
            node.prev = lastNode;

            lastNode = node;
        }
        lastNode.next = null;
    }

    // ��� Ȯ��
    public void Expand(int index)
    {
        Unexpand();

        BeatmapSetNode node = GetBaseNode(index);
        if (node == null)
            return;

        expandedStartNode = null;
        expandedEndNode = null;

        BeatmapSet beatmapSet = node.beatmapSet;
        BeatmapSetNode prevNode = node.prev;
        BeatmapSetNode nextNode = node.next;

        for (int i = 0; i < beatmapSet.Count; i++)
        {
            BeatmapSetNode newNode = new BeatmapSetNode(beatmapSet)
            {
                index = index,
                beatmapIndex = i,
                prev = i == 0 ? prevNode : nodes[nodes.Count - 1]
            };

            if (i == 0)
            {
                expandedStartNode = newNode;
                if (prevNode != null)
                    prevNode.next = newNode;
            }
            else
            {
                nodes[nodes.Count - 1].next = newNode;
            }

            nodes.Insert(index + i, newNode);
        }

        if (nextNode != null)
        {
            nodes[nodes.Count - 1].next = nextNode;
            nextNode.prev = nodes[nodes.Count - 1];
        }

        expandedEndNode = nodes[nodes.Count - 1];
        expandedIndex = index;
    }

    // ��� ���
    public void Unexpand()
    {
        if (expandedIndex < 0 || expandedIndex >= nodes.Count)
            return;

        int startIndex = expandedIndex;
        int count = expandedEndNode.index - expandedStartNode.index + 1;

        // ��ũ�� ����Ʈ �籸��
        BeatmapSetNode prevNode = expandedStartNode.prev;
        BeatmapSetNode nextNode = expandedEndNode.next;

        if (prevNode != null)
            prevNode.next = nextNode;
        if (nextNode != null)
            nextNode.prev = prevNode;

        nodes.RemoveRange(startIndex, count);

        expandedIndex = -1;
        expandedStartNode = null;
        expandedEndNode = null;
    }

    // �⺻ ��� ��ȯ (Ȯ�� ��� �� ��)
    public BeatmapSetNode GetBaseNode(int index)
    {
        if (index < 0 || index >= nodes.Count)
            return null;

        BeatmapSetNode node = nodes[index];
        // Ȯ��� ��尡 �ƴ� �⺻ ��带 ��ȯ
        return node.beatmapIndex == -1 ? node : null;
    }

    // �˻� ���
    public void Search(string query)
    {
        if (query == null)
            return;

        query = query.Trim().ToLower();
        if (lastQuery != null && query.Equals(lastQuery))
            return;

        lastQuery = query;
        List<string> terms = new List<string>(query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

        if (query.Length == 0 || terms.Count == 0)
        {
            nodes = new List<BeatmapSetNode>(groupNodes);
            return;
        }

        nodes = new List<BeatmapSetNode>();

        foreach (BeatmapSetNode node in groupNodes)
        {
            bool matches = true;
            foreach (string term in terms)
            {
                if (!node.beatmapSet.Matches(term))
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
                nodes.Add(node);
        }
    } 
   
    public BeatmapSetNode GetNode(int index)
    {
        if (index < 0 || index >= nodes.Count)
            return null;
        return nodes[index];
    }
}
