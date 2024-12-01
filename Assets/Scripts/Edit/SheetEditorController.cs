using UnityEngine;

public class SheetEditorController : MonoBehaviour
{
    public Camera mainCam;
    public GameObject cursurObj;

    public RaycastHit mRay;
    public Vector3 CursurEffectPos { get; set; }
    public int ScrollDir { get; set; }

    void Update()
    {
        OnTouchInput();
    }

    void LateUpdate()
    {
        OnTouchRay();
        OnCursurEffect();
    }

    void OnTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // ��ġ ����
                Debug.Log("��ġ ����: " + touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                // ��ġ �̵�
                Debug.Log("��ġ �̵�: " + touch.deltaPosition);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                // ��ġ ����
                Debug.Log("��ġ ����");
            }
        }
    }

    void OnTouchRay()
    {
        if (Input.touchCount > 0)
        {
            Vector3 touchPos = Input.GetTouch(0).position;
            touchPos.z = mainCam.farClipPlane;

            Vector3 dir = mainCam.ScreenToWorldPoint(touchPos);
            if (Physics.Raycast(mainCam.transform.position, dir, out mRay))
            {
                // ��ġ�� ������Ʈ ó��
                //Debug.Log("��ġ�� ������Ʈ: " + mRay.transform.name);
            }
        }
    }

    void OnCursurEffect()
    {
        if (cursurObj != null)
        {
            cursurObj.transform.position = CursurEffectPos;
        }
    }
}
