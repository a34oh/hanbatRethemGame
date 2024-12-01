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
                // 터치 시작
                Debug.Log("터치 시작: " + touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                // 터치 이동
                Debug.Log("터치 이동: " + touch.deltaPosition);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                // 터치 종료
                Debug.Log("터치 종료");
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
                // 터치한 오브젝트 처리
                //Debug.Log("터치한 오브젝트: " + mRay.transform.name);
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
