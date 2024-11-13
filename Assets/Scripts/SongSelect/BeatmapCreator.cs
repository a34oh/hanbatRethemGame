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

    private string uploadedMusicPath;
    private string uploadedImagePath;


    void Awake()
    {
        fileUploader = new FileUploader(new PCFileBrowser());

    }

    void Start()
    {
        // ��ư Ŭ�� �̺�Ʈ ���
        uploadBeatmapButton.onClick.AddListener(() => OnCreateBeatmap());
        backButton.onClick.AddListener(OnCloseCreateBeatmapCanvas);

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

        uploadedMusicPath = task.Result;
        if (!string.IsNullOrEmpty(uploadedMusicPath))
        {
            debugText.text = "���� ������ ���ε�Ǿ����ϴ�: " + uploadedMusicPath;
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
        string title = titleInput.text;
        string artist = artistInput.text;
        string creator = creatorInput.text;
        string level = versionInput.text;

        // �Է°� ����
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(creator) || string.IsNullOrEmpty(level))
        {
            debugText.text = "��� �ʵ带 ä���ּ���.";
            return;
        }

        if (string.IsNullOrEmpty(uploadedMusicPath) || string.IsNullOrEmpty(uploadedImagePath))
        {
            debugText.text = "���ǰ� �̹����� ���ε����ּ���.";
            return;
        }

        // �ߺ����� �ʴ� ū ���� ���� ID ����
        string uniqueId = GenerateUniqueID();

        var createTask = CreateBeatmapFolderAsync(uniqueId, title, artist, creator, level, uploadedMusicPath, uploadedImagePath);
        await createTask;

        if (createTask.Exception == null)
        {
            // �� ���� ���� �� Firebase ���ε�
            await UploadBeatmapToFirebase(uniqueId);
            gameObject.SetActive(false);
        }
        else
        {
            debugText.text = "�� ���� �� ���� �߻�: " + createTask.Exception.Message;
        }
    }

    // Firebase�� ��Ʈ�� ���ε�
    private async Task UploadBeatmapToFirebase(string uniqueId)
    {
        string localFolderPath = Path.Combine(Application.persistentDataPath, "Songs", $"{uniqueId} {artistInput.text} - {titleInput.text}").Replace("/", "\\");
        try
        {
            await GameManager.FBManager.UploadBeatmapAsync(localFolderPath);
            debugText.text = "���� Firebase�� ���������� ���ε�Ǿ����ϴ�!";
        }
        catch (Exception ex)
        {
            debugText.text = "Firebase ���ε� �� ���� �߻�: " + ex.Message;
        }
    }

    public async Task CreateBeatmapFolderAsync(string uniqueId, string title, string artist, string creator, string level, string mp3Path, string imagePath)
    {
        try
        {
            // ���� �̸��� ����� �� ���� ���� ����
            string sanitizedTitle = SanitizeFileName(title);
            string sanitizedArtist = SanitizeFileName(artist);
            string sanitizedCreater = SanitizeFileName(creator);

            // �� ���� �̸� ����
            string folderName = $"{uniqueId} {sanitizedArtist} - {sanitizedTitle}";
            string folderPath = Path.Combine(Application.persistentDataPath, "Songs", folderName);
            Debug.Log($"folderPath : {folderPath}");
            // ���� ���� ���� Ȯ��
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"������ �����Ǿ����ϴ�: {folderPath}");
            }

            // ������ ���� ���� ���� ���� Ȯ��
            string levelFileName = $"{sanitizedArtist} {sanitizedTitle} ({sanitizedCreater}) [{level}].txt";
            string levelFilePath = Path.Combine(folderPath, levelFileName);

            if (File.Exists(levelFilePath))
            {
                Debug.LogError("���� ������ ������ �̹� �����մϴ�.");
                return;
            }

            // ���� ���� ����
            string audioExtension = Path.GetExtension(mp3Path);
            string audioName = $"song{audioExtension}";
            string audioPath = Path.Combine(folderPath, audioName);
            await CopyFileAsync(mp3Path, audioPath);
            Debug.Log($"���� ������ song{audioExtension}�� ����Ǿ����ϴ�: {audioPath}");

            // �̹��� ���� ����
            string imageExtension = Path.GetExtension(imagePath);
            string imageName = $"image{imageExtension}";
            string imageDestinationPath = Path.Combine(folderPath, imageName);
            await CopyFileAsync(imagePath, imageDestinationPath);
            Debug.Log($"�̹��� ������ image{imageExtension}�� ����Ǿ����ϴ�: {imageDestinationPath}");

            // ���� ���� ����
            await CreateLevelFileAsync(uniqueId, folderPath, sanitizedTitle, sanitizedArtist, sanitizedCreater, level, audioPath, audioName, imageName);

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
    private async Task CreateLevelFileAsync(string uniqueId, string folderPath, string title, string artist, string creator, string version, string audioPath, string audioName, string imageName)
    {
        try
        {
            string levelFileName = $"{artist} - {title} ({creator}) {version}.txt";
            string levelFilePath = Path.Combine(folderPath, levelFileName);

            string mp3FileName = Path.GetFileName(audioPath);

            // ����� ���� ��������
            int audioLength = await GetAudioLengthAsync(audioPath);
            int previewTime = UnityEngine.Random.Range(0, audioLength);

            // ���� �ð� (dateAdded)
            DateTime dateAdded = DateTime.Now;

            // ���� ���� �ۼ�
            string fileContent =
                $@"Id:{uniqueId }
Title:{title}
Artist:{artist}
Creator:{creator}
Version:{version}
AudioFilename:{audioName}
ImageFilename:{imageName}
PreviewTime:{previewTime}
DateAdded:{dateAdded.ToString("yyyy/MM/dd HH:mm:ss")}"
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

    // ����� ���� ��������
    private async Task<int> GetAudioLengthAsync(string audioPath)
    {
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.MPEG))
        {
            await www.SendWebRequestAsync();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                return (int)(clip.length * 1000); // �и��� ������ ��ȯ
            }
            else
            {
                Debug.LogError("����� ���� �������� ����: " + www.error);
                return 120000; // �⺻������ 2�� ����
            }
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
    void OnCloseCreateBeatmapCanvas()
    {
        gameObject.SetActive(false);
    }

}
