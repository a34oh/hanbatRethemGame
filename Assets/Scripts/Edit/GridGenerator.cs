using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public SheetEditor sheetEditor;
    public GameObject grid_Beatbar;
    public Audio a;

    public List<GameObject> grids = new List<GameObject>();

    public int maxBeatCnt = 32;

    float scrollSpeed = 4f; // �׸��� �������� �ӵ�

    float snapAmount = 8f; // ���콺 ��ũ�ѽ� �׸��� �����̴� �� 1 (32��Ʈ), 2 (16��Ʈ), 4(8��Ʈ), 8(4��Ʈ), 16(2��Ʈ), 32(1��Ʈ=1�׸���) 

    public float ScrollSnapAmount
    {
        get { return snapAmount; }
        set { snapAmount = Mathf.Clamp(value, 1f, 32f); }
    }
    void Start()
    {
        scrollSpeed = sheetEditor.Speed;
    }
    public void Init()
    {
        Destroy();
        Create();
        InitPos();
        ChangeSnap();
    }

    // �׸��� ����
    void Create()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(grid_Beatbar, new Vector3(0f, i * 6f * scrollSpeed, 0f), Quaternion.identity);
            Grid grid = obj.GetComponent<Grid>();
            grid.barNumber = i;

            grids.Add(obj);
            obj.SetActive(false);
        }
    }
    // �׸��� ��ü �ı�
    void Destroy()
    {
        for (int i = 0; i < grids.Count; i++)
        {
            if (grids[i] != null)
            {
                GameObject obj = grids[i];
                Destroy(obj);
            }
        }
        grids.Clear();
    }

    // �׸��� ��ǥ �ʱ�ȭ
    void InitPos()
    {
        for (int i = 0; i < grids.Count; i++)
        {
            GameObject obj = grids[i];
            obj.transform.position = new Vector3(0f, /*a.Offset +*/ a.BarPerSec * i * scrollSpeed, 0f);
          
            BoxCollider coll = obj.GetComponent<BoxCollider>();
            coll.size = new Vector3(10f, a.BarPerSec * scrollSpeed, 0.1f);
            coll.center = new Vector3(0f, a.BarPerSec * scrollSpeed * 0.5f, 0f);

            Process32rd(obj);
            obj.SetActive(true);
        }
    }
    // �� �׸������ 32���� ��Ʈ�� �������� ó���Ѵ�.
    void Process32rd(GameObject grid)
    {
        for (int i = 0; i < maxBeatCnt; i++)
        {
            GameObject obj = grid.transform.GetChild(i).gameObject;
            obj.transform.localPosition = new Vector3(0f, a.BeatPerSec32rd * i * scrollSpeed, -0.1f);
        }
    }
    // ���� ��ġ�κ��� ���� �Ʒ��� ���ϴ¸�ŭ �����δ�.
    public void ChangePos(float dir)
    {
        // �̵��� �Ÿ� ���
        float moveAmount = dir * a.BeatPerSec32rd * scrollSpeed * snapAmount;

        // �Ѱ� �˻�: ù ��° �׸��尡 ȭ�� ���� ����� �ʵ��� ����
        if (grids[0].transform.position.y + moveAmount > 0f && dir > 0f)
        {
            moveAmount = -grids[0].transform.position.y; // ù ��° �׸��尡 0�� ���������� ����
        }

        // �Ѱ� �˻�: ������ �׸��尡 ȭ�� �Ʒ��� ����� �ʵ��� ����
        if (grids[grids.Count - 1].transform.position.y + moveAmount < 0f && dir < 0f)
        {
            moveAmount = -grids[grids.Count - 1].transform.position.y; // ������ �׸��尡 0�� ���������� ����
        }

        // ��� �׸��� �̵�
        for (int i = 0; i < grids.Count; i++)
        {
            GameObject obj = grids[i];
            obj.transform.Translate(new Vector3(0f, moveAmount, 0f));
        }
    }

    // ������ ��ġ�� �����Ѵ�. �ڵ� ���� �ʿ�. ���ϴ� ��ġ�� �Ű����� �� ��ġ�� �°� grids�� �̵��ؾ� ��.
    public void ChangeFixedPos(float pos)
    {
        for (int i = 0; i < grids.Count; i++)
        {
            GameObject obj = grids[i];

            obj.transform.position = new Vector3(0f, pos + i * a.BarPerSec * scrollSpeed, 0f);

        }
    }

    public void ChangeSnap()
    {
        int maxSnapAmount = maxBeatCnt / (int)snapAmount;
        int index = 0;

        for (int i = 0; i < grids.Count; i++)
        {
            GameObject obj = grids[i];
            for (int j = 0; j < maxBeatCnt; j++) // ���� �����ٰ�
            {
                GameObject child = obj.transform.GetChild(j).gameObject;
                child.SetActive(false);
            }
            for (int j = 0; j < maxSnapAmount; j++) // �ʿ��� �κи� �׷��ش�
            {
                index = j * (int)snapAmount;

                GameObject child = obj.transform.GetChild(index).gameObject;
                child.SetActive(true);
            }
        }
    }
}
