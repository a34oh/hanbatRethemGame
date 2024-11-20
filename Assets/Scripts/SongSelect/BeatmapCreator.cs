using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System;

// 곡 입력 처리 클래스
public class BeatmapCreator : MonoBehaviour
{
    public TMP_InputField titleInput;
    public TMP_InputField artistInput;
    public TMP_InputField creatorInput;
    public TMP_InputField versionInput;
    public Button uploadMusicButton;
    public Button uploadImageButton;
    public Button uploadBeatmapButton;
    public Button backButton;
    public TextMeshProUGUI debugText;

    private FileUploader fileUploader;

    private string uploadedAudioPath;
    private string uploadedImagePath;


    void Awake()
    {
        fileUploader = new FileUploader(new PCFileBrowser());

    }

    void Start()
    {
        // 버튼 클릭 이벤트 등록
        uploadBeatmapButton.onClick.AddListener(() => OnCreateBeatmap());
        backButton.onClick.AddListener(OnCloseBeatmapCreateCanvas);

        uploadMusicButton.onClick.AddListener(() => StartCoroutine(OnUploadMusic()));
        uploadImageButton.onClick.AddListener(() => StartCoroutine(OnUploadImage()));

    }



    // 음악 파일 업로드
    IEnumerator OnUploadMusic()
    {
        var task = fileUploader.UploadMusicFileAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        uploadedAudioPath = task.Result;
        if (!string.IsNullOrEmpty(uploadedAudioPath))
        {
            debugText.text = "음악 파일이 업로드되었습니다: " + uploadedAudioPath;
        }
        else
        {
            debugText.text = "음악 파일 업로드 실패.";
        }
    }

    // 이미지 파일 업로드
    IEnumerator OnUploadImage()
    {
        var task = fileUploader.UploadImageFileAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        uploadedImagePath = task.Result;
        if (!string.IsNullOrEmpty(uploadedImagePath))
        {
            debugText.text = "이미지 파일이 업로드되었습니다: " + uploadedImagePath;
        }
        else
        {
            debugText.text = "이미지 파일 업로드 실패.";
        }
    }

    // 곡 생성
    private async void OnCreateBeatmap()
    {
        string title = SanitizeFileName(titleInput.text);
        string artist = SanitizeFileName(artistInput.text);
        string creator = SanitizeFileName(creatorInput.text);
        string level = SanitizeFileName(versionInput.text);


        // 입력값 검증
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(creator) || string.IsNullOrEmpty(level))
        {
            debugText.text = "모든 필드를 채워주세요.";
            return;
        }

        if (string.IsNullOrEmpty(uploadedAudioPath) || string.IsNullOrEmpty(uploadedImagePath))
        {
            debugText.text = "음악과 이미지를 업로드해주세요.";
            return;
        }

        // 중복되지 않는 큰 값의 랜덤 ID 생성
        string uniqueId = GenerateUniqueID();

        // 곡 폴더 이름 생성
        string folderName = $"{uniqueId} {artist} - {title}";
        string folderPath = Path.Combine(Application.persistentDataPath, "Songs", folderName).Replace("\\", "/");
        Debug.Log($"folderPath : {folderPath}");


        try
        {
            var beatmap = await CreateBeatmapObjectAndFolderAsync(uniqueId, title, artist, creator, level, uploadedAudioPath, uploadedImagePath, folderPath);

            // 곡 생성 성공 시 Firebase 업로드
            await UploadBeatmapToFirebase(beatmap);
            debugText.text = "곡 생성이 완료되었습니다!";
            gameObject.SetActive(false);
        }
        catch (Exception ex)
        {
            debugText.text = $"곡 생성 중 오류 발생: {ex.Message}";
        }
    }

    // Beatmap 객체 생성 및 폴더 생성
    private async Task<Beatmap> CreateBeatmapObjectAndFolderAsync(string uniqueId, string title, string artist, string creator, string version, string uploadedAudioPath, string uploadedImagePath, string folderPath)
    {
        DateTime dateAdded = DateTime.Now;

        //오디오 이름 가져오기
        string audioExtension = Path.GetExtension(uploadedAudioPath);
        string audioName = $"song{audioExtension}";
        string audioPath = Path.Combine(folderPath, audioName).Replace("\\", "/");
        Debug.Log($"audioPath : {audioPath}");


        //이미지 이름 가져오기
        string imageExtension = Path.GetExtension(uploadedImagePath);
        string imageName = $"image{imageExtension}";
        string imagePath = Path.Combine(folderPath, imageName).Replace("\\", "/");



        Beatmap beatmap = new Beatmap
        {
            id = uniqueId,
            title = title,
            artist = artist,
            creator = creator,
            version = version,
            audioName = audioName,
            imageName = imageName,
            localAudioPath = audioPath,
            localImagePath = imagePath,
            audioLength = 0,
            previewTime = 0,
            dateAdded = dateAdded
            //기타 bpm, 검색지원 tag, favorite.. 채보 등
        };
        // 폴더 및 파일 생성
        await CreateBeatmapFolderAsync(beatmap, folderPath, uploadedAudioPath, uploadedImagePath);

        // 오디오 길이 가져오기
        await AssignPreviewTimeAsync(beatmap);

        // 레벨 파일 생성
        await CreateLevelFileAsync(beatmap, folderPath);


        return beatmap;
    }
    // Firebase에 비트맵 업로드
    private async Task UploadBeatmapToFirebase(Beatmap beatmap)
    {
        try
        {
            await GameManager.FBManager.UploadBeatmapToFirebase(beatmap);
            //            await GameManager.FBManager.UploadMetadataToDatabase(beatmap);


            debugText.text = "곡이 Firebase에 성공적으로 업로드되었습니다!";
        }
        catch (Exception ex)
        {
            debugText.text = "Firebase 업로드 중 오류 발생: " + ex.Message;
        }
    }

    public async Task CreateBeatmapFolderAsync(Beatmap beatmap, string folderPath, string uploadedAudioPath, string uploadedImagePath)
    {
        try
        {
            // 폴더 생성 여부 확인
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"폴더가 생성되었습니다: {folderPath}");
            }

            // 동일한 레벨 파일 존재 여부 확인
            string levelFileName = $"{beatmap.artist} {beatmap.title} ({beatmap.creator}) [{beatmap.version}].txt";
            string levelFilePath = Path.Combine(folderPath, levelFileName).Replace("\\", "/");

            if (File.Exists(levelFilePath))
            {
                Debug.LogError("같은 레벨의 파일이 이미 존재합니다.");
                return;
            }

            // 음악 파일 복사
            await CopyFileAsync(uploadedAudioPath, beatmap.localAudioPath);
            Debug.Log($"음악 파일이 {beatmap.audioName}로 저장되었습니다: {beatmap.localAudioPath}");

            // 이미지 파일 복사
            await CopyFileAsync(uploadedImagePath, beatmap.localImagePath);
            Debug.Log($"이미지 파일이 {beatmap.imageName}로 저장되었습니다: {beatmap.localImagePath}");

            // 레벨 파일 생성
        //    await CreateLevelFileAsync(beatmap, folderPath);

        }
        catch (Exception ex)
        {
            Debug.LogError("비트맵 생성 중 오류 발생: " + ex.Message);
        }
    }

    // 파일 복사 비동기 처리
    private async Task CopyFileAsync(string sourcePath, string destinationPath)
    {
        try
        {
            using (FileStream sourceStream = File.Open(sourcePath, FileMode.Open))
            using (FileStream destinationStream = File.Create(destinationPath))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("파일 복사 중 오류 발생: " + ex.Message);
            throw;
        }
    }

    // 레벨 파일 생성
    private async Task CreateLevelFileAsync(Beatmap beatmap, string folderPath)
    {
        try
        {
            string levelFileName = $"{beatmap.artist} - {beatmap.title} ({beatmap.creator}) {beatmap.version}.txt";
            string levelFilePath = Path.Combine(folderPath, levelFileName).Replace("\\", "/");


            string fileContent =
$@"Id:{beatmap.id }
Title:{beatmap.title}
Artist:{beatmap.artist}
Creator:{beatmap.creator}
Version:{beatmap.version}
Audioname:{beatmap.audioName}
Imagename:{beatmap.imageName}
PreviewTime:{beatmap.previewTime}"
;

            await File.WriteAllTextAsync(levelFilePath, fileContent);
            Debug.Log($"레벨 파일이 저장되었습니다: {levelFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError("레벨 파일 생성 중 오류 발생: " + ex.Message);
            throw;
        }
    }

    private async Task AssignPreviewTimeAsync(Beatmap beatmap)
    {
        if (string.IsNullOrEmpty(beatmap.localAudioPath))
        {
            Debug.LogError("오디오 파일 경로가 비어있습니다.");
            return;
        }

        int audioLength = await GetAudioLengthAsync(beatmap.localAudioPath);
        beatmap.previewTime = UnityEngine.Random.Range(0, audioLength);
        Debug.Log($"PreviewTime이 {beatmap.previewTime}으로 설정되었습니다.");
    }


    // 오디오 길이 가져오기
    private async Task<int> GetAudioLengthAsync(string audioPath)
    {
        string uriPath = "file://" + audioPath;

        try
        {
            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(uriPath, AudioType.MPEG))
            {
                await www.SendWebRequestAsync();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                    return (int)(clip.length * 1000); // 밀리초 단위로 변환
                }
                else
                {
                    Debug.LogError($"오디오 길이 가져오기 실패: {www.error} - URL: {uriPath}");
                    return 120000; // 기본값으로 2분 설정
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"예외 발생: {ex.Message}");
            return 120000; // 기본값으로 반환
        }
    }

    // 고유 ID 생성
    private string GenerateUniqueID()
    {
        return Guid.NewGuid().ToString("N");
    }

    // 파일 이름 정규화 (특수 문자 제거)
    private string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }
    // 곡 생성 화면 닫기
    void OnCloseBeatmapCreateCanvas()
    {
        gameObject.SetActive(false);
    }

}
