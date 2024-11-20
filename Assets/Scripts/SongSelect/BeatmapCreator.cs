using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System;

// �� �Է� ó�� Ŭ����
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
        // ��ư Ŭ�� �̺�Ʈ ���
        uploadBeatmapButton.onClick.AddListener(() => OnCreateBeatmap());
        backButton.onClick.AddListener(OnCloseBeatmapCreateCanvas);

        uploadMusicButton.onClick.AddListener(() => StartCoroutine(OnUploadMusic()));
        uploadImageButton.onClick.AddListener(() => StartCoroutine(OnUploadImage()));

    }



    // ���� ���� ���ε�
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
            debugText.text = "���� ������ ���ε�Ǿ����ϴ�: " + uploadedAudioPath;
        }
        else
        {
            debugText.text = "���� ���� ���ε� ����.";
        }
    }

    // �̹��� ���� ���ε�
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
            debugText.text = "�̹��� ������ ���ε�Ǿ����ϴ�: " + uploadedImagePath;
        }
        else
        {
            debugText.text = "�̹��� ���� ���ε� ����.";
        }
    }

    // �� ����
    private async void OnCreateBeatmap()
    {
        string title = SanitizeFileName(titleInput.text);
        string artist = SanitizeFileName(artistInput.text);
        string creator = SanitizeFileName(creatorInput.text);
        string level = SanitizeFileName(versionInput.text);


        // �Է°� ����
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(creator) || string.IsNullOrEmpty(level))
        {
            debugText.text = "��� �ʵ带 ä���ּ���.";
            return;
        }

        if (string.IsNullOrEmpty(uploadedAudioPath) || string.IsNullOrEmpty(uploadedImagePath))
        {
            debugText.text = "���ǰ� �̹����� ���ε����ּ���.";
            return;
        }

        // �ߺ����� �ʴ� ū ���� ���� ID ����
        string uniqueId = GenerateUniqueID();

        // �� ���� �̸� ����
        string folderName = $"{uniqueId} {artist} - {title}";
        string folderPath = Path.Combine(Application.persistentDataPath, "Songs", folderName).Replace("\\", "/");
        Debug.Log($"folderPath : {folderPath}");


        try
        {
            var beatmap = await CreateBeatmapObjectAndFolderAsync(uniqueId, title, artist, creator, level, uploadedAudioPath, uploadedImagePath, folderPath);

            // �� ���� ���� �� Firebase ���ε�
            await UploadBeatmapToFirebase(beatmap);
            debugText.text = "�� ������ �Ϸ�Ǿ����ϴ�!";
            gameObject.SetActive(false);
        }
        catch (Exception ex)
        {
            debugText.text = $"�� ���� �� ���� �߻�: {ex.Message}";
        }
    }

    // Beatmap ��ü ���� �� ���� ����
    private async Task<Beatmap> CreateBeatmapObjectAndFolderAsync(string uniqueId, string title, string artist, string creator, string version, string uploadedAudioPath, string uploadedImagePath, string folderPath)
    {
        DateTime dateAdded = DateTime.Now;

        //����� �̸� ��������
        string audioExtension = Path.GetExtension(uploadedAudioPath);
        string audioName = $"song{audioExtension}";
        string audioPath = Path.Combine(folderPath, audioName).Replace("\\", "/");
        Debug.Log($"audioPath : {audioPath}");


        //�̹��� �̸� ��������
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
            //��Ÿ bpm, �˻����� tag, favorite.. ä�� ��
        };
        // ���� �� ���� ����
        await CreateBeatmapFolderAsync(beatmap, folderPath, uploadedAudioPath, uploadedImagePath);

        // ����� ���� ��������
        await AssignPreviewTimeAsync(beatmap);

        // ���� ���� ����
        await CreateLevelFileAsync(beatmap, folderPath);


        return beatmap;
    }
    // Firebase�� ��Ʈ�� ���ε�
    private async Task UploadBeatmapToFirebase(Beatmap beatmap)
    {
        try
        {
            await GameManager.FBManager.UploadBeatmapToFirebase(beatmap);
            //            await GameManager.FBManager.UploadMetadataToDatabase(beatmap);


            debugText.text = "���� Firebase�� ���������� ���ε�Ǿ����ϴ�!";
        }
        catch (Exception ex)
        {
            debugText.text = "Firebase ���ε� �� ���� �߻�: " + ex.Message;
        }
    }

    public async Task CreateBeatmapFolderAsync(Beatmap beatmap, string folderPath, string uploadedAudioPath, string uploadedImagePath)
    {
        try
        {
            // ���� ���� ���� Ȯ��
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"������ �����Ǿ����ϴ�: {folderPath}");
            }

            // ������ ���� ���� ���� ���� Ȯ��
            string levelFileName = $"{beatmap.artist} {beatmap.title} ({beatmap.creator}) [{beatmap.version}].txt";
            string levelFilePath = Path.Combine(folderPath, levelFileName).Replace("\\", "/");

            if (File.Exists(levelFilePath))
            {
                Debug.LogError("���� ������ ������ �̹� �����մϴ�.");
                return;
            }

            // ���� ���� ����
            await CopyFileAsync(uploadedAudioPath, beatmap.localAudioPath);
            Debug.Log($"���� ������ {beatmap.audioName}�� ����Ǿ����ϴ�: {beatmap.localAudioPath}");

            // �̹��� ���� ����
            await CopyFileAsync(uploadedImagePath, beatmap.localImagePath);
            Debug.Log($"�̹��� ������ {beatmap.imageName}�� ����Ǿ����ϴ�: {beatmap.localImagePath}");

            // ���� ���� ����
        //    await CreateLevelFileAsync(beatmap, folderPath);

        }
        catch (Exception ex)
        {
            Debug.LogError("��Ʈ�� ���� �� ���� �߻�: " + ex.Message);
        }
    }

    // ���� ���� �񵿱� ó��
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
            Debug.LogError("���� ���� �� ���� �߻�: " + ex.Message);
            throw;
        }
    }

    // ���� ���� ����
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
            Debug.Log($"���� ������ ����Ǿ����ϴ�: {levelFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError("���� ���� ���� �� ���� �߻�: " + ex.Message);
            throw;
        }
    }

    private async Task AssignPreviewTimeAsync(Beatmap beatmap)
    {
        if (string.IsNullOrEmpty(beatmap.localAudioPath))
        {
            Debug.LogError("����� ���� ��ΰ� ����ֽ��ϴ�.");
            return;
        }

        int audioLength = await GetAudioLengthAsync(beatmap.localAudioPath);
        beatmap.previewTime = UnityEngine.Random.Range(0, audioLength);
        Debug.Log($"PreviewTime�� {beatmap.previewTime}���� �����Ǿ����ϴ�.");
    }


    // ����� ���� ��������
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
                    return (int)(clip.length * 1000); // �и��� ������ ��ȯ
                }
                else
                {
                    Debug.LogError($"����� ���� �������� ����: {www.error} - URL: {uriPath}");
                    return 120000; // �⺻������ 2�� ����
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� �߻�: {ex.Message}");
            return 120000; // �⺻������ ��ȯ
        }
    }

    // ���� ID ����
    private string GenerateUniqueID()
    {
        return Guid.NewGuid().ToString("N");
    }

    // ���� �̸� ����ȭ (Ư�� ���� ����)
    private string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }
    // �� ���� ȭ�� �ݱ�
    void OnCloseBeatmapCreateCanvas()
    {
        gameObject.SetActive(false);
    }

}
