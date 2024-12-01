using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// 노트 데이터를 저장할 구조체
[System.Serializable]
public class NoteData
{
    public float xPosition;
    public float spawnTime;
}

public class SheetEditor : MonoBehaviour
{
    public bool isPlay = false;
    public SheetEditorController sheetController;
    public BeatmapCreator beatmapCreator;
    public float InterpolValue { get; private set; }

    public float Speed { get; set; } = 4;
    float divSpeed;

    public GameObject prevNotePrefab;
    public GameObject actualNotePrefab;
    private GameObject selectedNote;    // 선택된 실제 노트



    public GridGenerator gridGenerator;
    public Audio a;

    private List<GameObject> previewNotes = new List<GameObject>(); // 미리보기 노트 리스트
    GameObject seletedObject; // 배치 단계에서 선택된 오브젝트
    int currentSelectedLine;

    Vector3 snapPos;



    private List<NoteData> noteDataList = new List<NoteData>(); // 저장할 노트 데이터 리스트


    public void Init()
    {
        divSpeed = 1 / Speed;
        InterpolValue = a.BeatPerSec32rd * 0.5f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition);
        }
    }

    // 터치 처리
    private void HandleTouch(Vector3 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1.0f); // 레이 시각화


        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObject = hit.transform.gameObject;
            Debug.Log($"hitObject : {hitObject.name}");

            if (hitObject.layer == (int)LayerTypes.PreviewNote)
            {
                // 이미 있는 미리보기 노트를 제거
                RemovePreviewNote(hitObject);
            }
            else if (hitObject.layer == (int)LayerTypes.Grid)
            {
                // 그리드에 새 미리보기 노트를 배치
                PlacePreviewNote(hit.point, hitObject, hit);
            }
            else if (hitObject.layer == (int)LayerTypes.ActualNote)
            {
                // 실제 노트 선택
                SelectActualNote(hitObject);
            }
        }
    }

    // 미리보기 노트 배치
    private void PlacePreviewNote(Vector3 hitPoint, GameObject gridObject, RaycastHit hit)
    {
        GameObject noteContainer = gridObject.transform.GetChild(32).gameObject;

        // ProcessSnapPos를 사용하여 스냅 좌표 계산
        Vector3 hitToGrid = hitPoint - gridObject.transform.position;
        ProcessSnapPos(hitToGrid, gridObject, hit);

        // 미리 보기 노트가 이미 존재할 경우 생성 x
        foreach (var note in previewNotes)
        {
            if (Mathf.Approximately(note.transform.position.x, snapPos.x) &&
                Mathf.Approximately(note.transform.position.y, snapPos.y))
            {
                Debug.Log("이미 해당 위치에 미리보기 노트가 있습니다.");
                return; // 이미 존재하면 함수 종료
            }
        }
        // 미리보기 노트를 생성
        GameObject previewNote = Instantiate(prevNotePrefab, snapPos, Quaternion.identity, noteContainer.transform);
        previewNotes.Add(previewNote);
    }

    // 미리보기 노트 제거
    private void RemovePreviewNote(GameObject previewNote)
    {
        previewNotes.Remove(previewNote);
        Destroy(previewNote);
    }

    // 실제 노트 생성
    public void GenerateActualNotes()
    {
        foreach (var previewNote in previewNotes)
        {
            Transform noteContainer = previewNote.transform.parent;
            Vector3 position = previewNote.transform.position;

            // 실제 노트 생성
            GameObject actualNote = Instantiate(actualNotePrefab, position, Quaternion.identity, noteContainer);

            //시간 계산
            int currentBarNumber = previewNote.transform.root.GetComponent<Grid>().barNumber;
            Debug.Log($"actualNote.transform.localPosition.y : {actualNote.transform.localPosition.y}") ;
            Debug.Log($" (currentBarNumber * a.BarPerSec * Speed) : { (currentBarNumber * a.BarPerSec * Speed)}");
            Debug.Log($"actualNote.transform.localPosition.y + (currentBarNumber * a.BarPerSec * Speed : {actualNote.transform.localPosition.y + (currentBarNumber * a.BarPerSec * Speed)}");
            Debug.Log($"divSpeed : {divSpeed}");
            float spawnTime = (actualNote.transform.localPosition.y + (currentBarNumber * a.BarPerSec * Speed)) * 1000f * divSpeed;

            // 노트 데이터 추가
            var newNote = new NoteData
            {
                xPosition = actualNote.transform.position.x,
                spawnTime = spawnTime
            };
            noteDataList.Add(newNote);

            Debug.Log($"노트 생성: X={actualNote.transform.position.x}, 시간={newNote.spawnTime}");
        }

        // 미리보기 노트 제거
        foreach (var previewNote in previewNotes)
        {
            Destroy(previewNote);
        }
        previewNotes.Clear();
    }

    // 실제 노트 선택
    private void SelectActualNote(GameObject note)
    {
        selectedNote = note;
        Debug.Log($"노트 선택: {note.name}");
    }

    // 실제 노트 삭제
    public void DeleteSelectedNote()
    {
        if (selectedNote != null)
        {
            // 선택된 노트의 위치와 시간 계산
            Vector3 notePosition = selectedNote.transform.position;
            int currentBarNumber = selectedNote.transform.root.GetComponent<Grid>().barNumber;
            float noteTime = selectedNote.transform.localPosition.y + (currentBarNumber * a.BarPerSec * Speed) * 1000f * divSpeed;

            // noteDataList에서 해당 노트 데이터 제거
            var noteToRemove = noteDataList.Find(note =>
                Mathf.Approximately(note.xPosition, notePosition.x) &&
                Mathf.Approximately(note.spawnTime, noteTime));

            if (noteToRemove != null)
            {
                noteDataList.Remove(noteToRemove);
                Debug.Log($"노트 데이터 제거: X={noteToRemove.xPosition}, 시간={noteToRemove.spawnTime}");
            }
            else
            {
                Debug.LogWarning("노트 데이터를 찾지 못했습니다. 삭제되지 않았습니다.");
            }

            // 선택된 노트 GameObject 삭제
            Destroy(selectedNote);
            selectedNote = null;
        }
        else
        {
            Debug.LogWarning("삭제할 노트가 선택되지 않았습니다.");
        }
    }


    void ProcessSnapPos(Vector3 hitToGrid, GameObject gridObject, RaycastHit hit)
    {
        // 현재 스냅양에 따라 스냅될 위치를 계산한다. (x값)
        float snapPosX = 0f;
        if (hit.point.x > -5f && hit.point.x < -2.5f)
        {
            snapPosX = -3.75f;
            currentSelectedLine = 1;
        }
        else if (hit.point.x > -2.5f && hit.point.x < 0f)
        {
            snapPosX = -1.25f;
            currentSelectedLine = 2;
        }
        else if (hit.point.x > 0f && hit.point.x < 2.5f)
        {
            snapPosX = 1.25f;
            currentSelectedLine = 3;
        }
        else if (hit.point.x > 2.5f && hit.point.x < 5f)
        {
            snapPosX = 3.75f;
            currentSelectedLine = 4;
        }

        // 현재 스냅양에 따라 스냅될 위치를 계산한다. (y값)
        float snapAmount = gridGenerator.ScrollSnapAmount * a.BeatPerSec32rd * Speed;
        float halfSnapAmount = snapAmount / 2;

        float snapPosY = hitToGrid.y;
        for (int i = 0; i < 32 / gridGenerator.ScrollSnapAmount; i++)
        {
            if (snapPosY >= (snapAmount * i) - halfSnapAmount && snapPosY <= (snapAmount * i) + halfSnapAmount)
            {
                //Debug.Log("최소 : " + ((snapAmount * i) - halfSnapAmount) + " 최대 : " + ((snapAmount * i) + halfSnapAmount));
                //Debug.Log("걸린 곳 : " + i);
                snapPos = new Vector3(snapPosX, gridObject.transform.position.y + i * snapAmount, -0.1f);

                break;
            }
        }
    }

    // 버튼 클릭으로 노트 저장 호출
    public void OnSaveNotesButtonClick()
    {
        SaveNotes();
    }

    // 노트 데이터를 텍스트 파일로 저장
    public async void SaveNotes()
    {
        a.beatmap.noteDataList = noteDataList;
        await beatmapCreator.AppendNoteDataToLevelFileAsync(a.beatmap);
    }

   

}
