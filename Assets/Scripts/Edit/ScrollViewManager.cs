using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewManager : MonoBehaviour
{
    public ScrollRect scrollView;
    public RectTransform scrollContent;
    public RectTransform snapLineParent;
    public GameObject snapLinePrefab;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI timerText;
    public Button start;
    public Scrollbar scrollbar;
    public float heightPerSecond = 100f; // 1초당 높이 (임의 설정)
    private float totalHeight;
    private int audioLength;
    private Beatmap beatmap;
    private float beatDivision; // 임시로 1/4 박자 (초)

 
    public Audio a;
    public SheetEditor sheetEditor;
    public GridGenerator gridGenerator;

    private void Start()
    {
        Beatmap beatmap = new Beatmap()
        {
            localAudioPath = "C:/Users/a34oh/AppData/LocalLow/DefaultCompany/template_2024/Songs/16f269fd87624ebeb8eac3ea98081453 vvv - pupa/song.mp3",
            audioLength = 10000
        };
        a.Init(beatmap);
        gridGenerator.Init();
    }
    public void InitializeWithBeatmap(Beatmap beatmap)
    {
        a.Init(beatmap);
       // sheetEditor.Init();
        gridGenerator.Init();
     //   noteGenerator.GenNote();
    }




}
