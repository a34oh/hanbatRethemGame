using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine;
using System.Linq;

//Beatmap.cs            ��Ʈ���� ������ ������ Ŭ���� 
//FBManager.cs          ���̾�̽� ����
//DBManager.cs          ���� db ����
//GameManager.cs        ���� Manager�� Singletone�� ���� ���� ����
//BeatmapCreator.cs     ��Ʈ���� �����ϰ� ��Ʈ���� txtȭ�� ����, ��, �̹����� ���ÿ� �����ϴ� Ŭ����. (����� �Է� ���� : ��Ʈ�� ����, ��Ʈ�� ��Ƽ��Ʈ, ��Ʈ�� ������, ��Ʈ�� ��(mp3), ��Ʈ�� �̹���)
//BeatmapUploader.cs    BeatmapCreator���� ������ ���ÿ� ����Ǿ� �ִ� ��Ʈ���� txtȭ�� ����, ��, �̹����� ���̾�̽��� ���ε��ϴ� Ŭ����
//BeatmapParser.cs      ���ÿ� ����� ��Ʈ�� ������ �о���̴� Ŭ����.��Ʈ���� txtȭ�� ���ϵ��� beatmapŬ������ �Ľ��ϰ�, �̹����� �� ������ �ҷ���
//ResourceCache.cs      �̹���, �� �� �ҽ����� �̸� �ε�
//Ŭ�����̸�������.cs   ���̾�̽��� ���ε� �� ��Ʈ���� ScrollView�� �����ִ� UI Ŭ����(���� �ֱٿ� ���̾�̽��� ���ε� �� ��Ʈ���� �������� �ִ� 30�� ��������. �� �̻��� ȭ��ǥ ��ư�� ���� �ε�)
//Ŭ�����̸�������.cs   ���ÿ� ����� ��Ʈ���� ScrollView�� �����ִ� UI Ŭ����

//(������ ���ε� �� ��Ʈ���� ScrollView�� �����ִ� UI Ŭ�������� ��Ʈ�� Ŭ�� �� ���� db�� ��Ʈ���� �ٿ�޴� ���) - �̰� ��� �ۼ��ϴ°� ������
//(���� ����ҿ� ����Ǿ� �ִ� ���ϵ��� ���� db�� ����?���ִ� Ŭ����. ���� �ٽ� ������ �������� �� �ٷ� �ε��� �ǰ� �� �ϱ� ����.))
public class FBManager
{
    public string FBurl = "https://rethemgame-default-rtdb.firebaseio.com/";  // ���⸦ Firebase Console�� Database URL�� ����
    public string StorageBucketUrl = "gs://rethemgame.firebasestorage.app"; // ���⸦ Firebase Console�� Storage Bucket URL�� ����

    private DatabaseReference databaseRef;
    private FirebaseStorage storage;
    private bool isOnline = true;

    enum FileType
    {
        Audio,
        Image,
        Text
    }
    // Firebase �ʱ�ȭ
    public async Task InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            // Firebase �� �ʱ�ȭ
            FirebaseApp app = FirebaseApp.DefaultInstance;

            // Database URL�� Storage Bucket URL�� ��������� ����
            app.Options.DatabaseUrl = new Uri(FBurl);
            app.Options.StorageBucket = StorageBucketUrl;

            // Firebase Database�� Storage �ν��Ͻ� �ʱ�ȭ
            databaseRef = FirebaseDatabase.GetInstance(app, FBurl).RootReference;
            storage = FirebaseStorage.GetInstance(app, StorageBucketUrl);

            Debug.Log("Firebase�� ���������� �ʱ�ȭ�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogError("Firebase ������ Ȯ�� ����: " + dependencyStatus);
            isOnline = false; // Firebase �ʱ�ȭ ���� �� �������� ���·� ����
        }
    }


    // �������� ���� Ȯ��
    public bool IsOnline()
    {
        return isOnline;
    }

    // ��Ʈ�� ���ε�. ����ġ ���� ������ ���� �� ���� �̸� �� �ؽ�Ʈ ���� ���� ��� �߰�, ���̾�̽� metaData ���ε� ���� ��, stroage ���ε嵵 ����ؾ� ��.
    public async Task UploadBeatmapToFirebase(Beatmap beatmap)
    {
        if (!IsOnline())
        {
            Debug.LogWarning("�������� �����Դϴ�. Firebase�� ���ε��� �� �����ϴ�.");
            return;
        }

        string localFolderName = GetBeatmapFolderName(beatmap);

        // Firebase���� ���� ID ��������
        string uniqueId = await GetNextBeatmapIdAsync();
       
        beatmap.id = uniqueId; // Beatmap ID ������Ʈ

        // ���� �̸� ����
        string folderName = GetBeatmapFolderName(beatmap);
        string newFolderPath = RenameLocalBeatmapFolder(localFolderName, folderName);

        if (string.IsNullOrEmpty(newFolderPath))
        {
            Debug.LogError("���� �̸� ���� ���з� ���ε带 �ߴ��մϴ�.");
            return;
        }
        DirectoryInfo dirInfo = new DirectoryInfo(newFolderPath);

        // ���ϵ��� Firebase Storage�� ���ķ� ���ε�
        var uploadResults = await UploadFilesToFirebaseStorageAsync(dirInfo, beatmap, folderName);

        // ���ε� ��� ó�� �� ��Ÿ������ �غ�
        var metadata = PrepareMetadataForServer(uploadResults, beatmap);

        if (metadata == null)
        {
            Debug.LogError("���� ���ε� ���з� ��Ÿ������ ������ �ߴܵǾ����ϴ�.");
            return;
        }

        // ��Ÿ�����͸� Firebase Realtime Database�� ���ε�
        if (await UploadMetadataToDatabaseAsync(metadata, folderName))
        {
            // ID ����
            await IncrementBeatmapIdAsync(uniqueId);
            Debug.Log("ID�� ���������� �����Ǿ����ϴ�.");
        }
    }
    private string GetBeatmapFolderName(Beatmap beatmap)
    {
        return $"{beatmap.id} {beatmap.artist} - {beatmap.title}";
    }

    private string RenameLocalBeatmapFolder(string localFolderName, string folderName)
    {
        string localFolderPath = Path.Combine(Application.persistentDataPath, "Songs", localFolderName).Replace("\\", "/");
        string newFolderPath = Path.Combine(Application.persistentDataPath, "Songs", folderName).Replace("\\", "/");

        try
        {
            Directory.Move(localFolderPath, newFolderPath);
            Debug.Log("���� �̸� ���� �Ϸ�.");
            return newFolderPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� �̸� ���� ����: {ex.Message}");
            return null;
        }
    }
    private async Task<List<(FileType fileType, string fileName, string downloadUrl)>> UploadFilesToFirebaseStorageAsync(DirectoryInfo dirInfo, Beatmap beatmap, string folderName)
    {
        var uploadTasks = new List<Task<(FileType fileType, string fileName, string downloadUrl)>>();

        // �ؽ�Ʈ ���� ���ε�
        foreach (var txtFile in dirInfo.GetFiles("*.txt"))
        {
            try
            {
                await UpdateIdInTextFile(txtFile.FullName, beatmap.id);
                string firebaseStoragePath = $"Songs/{folderName}/{txtFile.Name}";
                uploadTasks.Add(UploadFileToFirebaseStorage(txtFile.FullName, firebaseStoragePath, FileType.Text));
            }
            catch (Exception ex)
            {
                Debug.LogError($"�ؽ�Ʈ ���� ó�� �� ���� �߻�: {txtFile.Name}, ����: {ex.Message}");
            }
        }

        // ����� �� �̹��� ���� ���ε�
        foreach (var file in dirInfo.GetFiles())
        {
            if (file.Name == beatmap.audioName || file.Name == beatmap.imageName)
            {
                try
                {
                    FileType fileType = file.Name == beatmap.audioName ? FileType.Audio : FileType.Image;
                    string firebaseStoragePath = $"Songs/{folderName}/{file.Name}";
                    uploadTasks.Add(UploadFileToFirebaseStorage(file.FullName, firebaseStoragePath, fileType));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"���� ���ε� �� ���� �߻�: {file.Name}, ����: {ex.Message}");
                }
            }
        }

       /* // ��Ų ���� ���ε�
        foreach (var skinFile in dirInfo.GetFiles("*.skin"))
        {
            try
            {
                string firebaseStoragePath = $"Songs/{beatmap.id} {beatmap.artist} - {beatmap.title}/{skinFile.Name}";
                uploadTasks.Add(UploadFileToFirebaseStorage(skinFile.FullName, firebaseStoragePath, FileType.Skin));
            }
            catch (Exception ex)
            {
                Debug.LogError($"��Ų ���� ó�� �� ���� �߻�: {skinFile.Name}, ����: {ex.Message}");
            }
        }
      */

        var results = await Task.WhenAll(uploadTasks);

        return new List<(FileType fileType, string fileName, string downloadUrl)>(results);
    }


    // �ؽ�Ʈ ���Ͽ��� Id ���� ������Ʈ
    private async Task UpdateIdInTextFile(string filePath, string newId)
    {
        var updatedLines = new List<string>();

        string[] lines = await File.ReadAllLinesAsync(filePath);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
            {
                updatedLines.Add(line);
                continue;
            }

            int colonIndex = line.IndexOf(':');
            if (colonIndex == -1)
            {
                updatedLines.Add(line);
                continue;
            }

            string key = line.Substring(0, colonIndex).Trim();
            string value = line.Substring(colonIndex + 1).Trim();

            switch (key)
            {
                case "Id":
                    updatedLines.Add($"Id:{newId}");
                    break;
                default:
                    updatedLines.Add(line);
                    break;
            }
        }

        // ���� ������Ʈ
        await File.WriteAllLinesAsync(filePath, updatedLines);

        return;
    }

    // Firebase Database�� ��Ʈ�� ��Ÿ������ ���ε�. beatmap�� ��°�� �� �ä����� ������ beatmap�� ��� �����Ͱ� �ʿ��� �� ������ �ʰ�, (audioPath, imagePath ��)
    // �������� �����ؾ� �ϴ� �����͵��� �ֱ� ����. (AudioStroageUrl, ImageStroageUrl ��)
    private Dictionary<string, object> PrepareMetadataForServer(
    IEnumerable<(FileType fileType, string fileName, string downloadUrl)> uploadResults,
    Beatmap beatmap)
    {
        var metadata = new Dictionary<string, object>
    {
        { "Id", beatmap.id },
        { "Title", beatmap.title },
        { "Artist", beatmap.artist },
        { "Creator", beatmap.creator },
        { "Version", beatmap.version },
        { "PreviewTime", beatmap.previewTime },
        { "DateAdded", beatmap.dateAdded.ToString("yyyy/MM/dd HH:mm:ss") },
        { "StorageTextUrls", new List<string>() },
      //  { "StorageSkinUrls", new List<string>() }
    };

        foreach (var result in uploadResults)
        {
            if (result.downloadUrl == null)
            {
                Debug.LogError($"���� ���ε� ���� - ����: {result.fileType}, ���ϸ�: {result.fileName}");
                return null;
            }

            switch (result.fileType)
            {
                case FileType.Audio:
                    metadata["StorageAudioUrl"] = result.downloadUrl;
                    break;
                case FileType.Image:
                    metadata["StorageImageUrl"] = result.downloadUrl;
                    break;
                case FileType.Text:
                    ((List<string>)metadata["StorageTextUrls"]).Add(result.downloadUrl);
                    break;
   //             case "Skin":
   //                 ((List<string>)metadata["StorageSkinUrls"]).Add(result.downloadUrl);
   //                 break;
            }
        }

        return metadata;
    }
    private async Task<bool> UploadMetadataToDatabaseAsync(Dictionary<string, object> metadata, string folderName)
    {
        try
        {
            await databaseRef.Child("Songs").Child(folderName).SetValueAsync(metadata);
            Debug.Log("��Ÿ�����Ͱ� Realtime Database�� ���������� ���ε�Ǿ����ϴ�.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"��Ÿ������ ���ε� �� ���� �߻�: {ex.Message}");
            return false;
        }
    }
    // Firebase Storage�� ���� ���ε� �޼���
    private async Task<(FileType fileType, string fileName, string downloadUrl)> UploadFileToFirebaseStorage(string localFilePath, string firebaseStoragePath, FileType fileType)
    {
        var storageReference = storage.GetReferenceFromUrl(StorageBucketUrl).Child(firebaseStoragePath);

        try
        {
            Debug.Log($"���� ���ε� ��: {fileType} - {Path.GetFileName(localFilePath)}");

            await storageReference.PutFileAsync(localFilePath);
            Uri uri = await storageReference.GetDownloadUrlAsync();
            Debug.Log($"���� ���ε� ����: {fileType} - {Path.GetFileName(localFilePath)} - URL: {uri}");
            return (fileType, Path.GetFileName(localFilePath), uri.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� ���ε� ���� - ����: {fileType}, ���ϸ�: {Path.GetFileName(localFilePath)}, ����: {ex.Message}");
            return (fileType, Path.GetFileName(localFilePath), null);
        }
    }


    // Firebase���� ���� ���� ID ��������
    private async Task<string> GetNextBeatmapIdAsync()
    {
        var idSnapshot = await databaseRef.Child("NextBeatmapId").GetValueAsync();
        int currentId = idSnapshot.Exists ? int.Parse(idSnapshot.Value.ToString()) : -1;
        Debug.Log($"currentid: {currentId}");
        return currentId.ToString();
    }

    // ���ε� ���� �� ���� ID�� ����
    private async Task IncrementBeatmapIdAsync(string currentId)
    {
        int nextId = int.Parse(currentId) + 1;
        await databaseRef.Child("NextBeatmapId").SetValueAsync(nextId);
        Debug.Log($"NextBeatmapId id���� {nextId}");
    }

    //��Ʈ�� ���� �ҷ�����
    public async Task<List<Beatmap>> FetchBeatmapMetadataAsync(int startIndex = 0, int limit = 10)
    {
        var beatmaps = new List<Beatmap>();
        try
        {
            // Firebase���� Songs ����� �����͸� ��¥ �������� ����
            var snapshot = await databaseRef
                .Child("Songs")
                .OrderByChild("DateAdded")
                .LimitToLast(startIndex + limit) // ���� �ε��� + ���� ����
                .GetValueAsync();

            if (snapshot.Exists)
            {
                var allData = snapshot.Children
                    .Select(child => child.Value as Dictionary<string, object>)
                    .Where(data => data != null)
                    .OrderByDescending(data => data["DateAdded"].ToString()) // �ֽ� �� ����
                    .Skip(startIndex)
                    .Take(limit)
                    .ToList();

                foreach (var data in allData)
                {
                    try
                    {
                        Beatmap beatmap = ParseBeatmapFromMetadata(data);
                        beatmaps.Add(beatmap);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"��Ʈ�� �����͸� ��ȯ �� ���� �߻�: {ex.Message}");
                    }
                }

                Debug.Log($"�� {allData.Count}���� ��Ʈ�� �����͸� �ҷ��Խ��ϴ�.");
            }
            else
            {
                Debug.LogWarning("��Ʈ�� �����Ͱ� �����ϴ�.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"��Ʈ�� �����͸� �ҷ����� �� ���� �߻�: {ex.Message}");
        }
        return beatmaps;
    }
    private Beatmap ParseBeatmapFromMetadata(Dictionary<string, object> data)
    {
        return new Beatmap
        {
            id = data.ContainsKey("Id") ? data["Id"].ToString() : "",
            title = data.ContainsKey("Title") ? data["Title"].ToString() : "",
            artist = data.ContainsKey("Artist") ? data["Artist"].ToString() : "",
            creator = data.ContainsKey("Creator") ? data["Creator"].ToString() : "",
            version = data.ContainsKey("Version") ? data["Version"].ToString() : "",
            StorageAudioUrl = data.ContainsKey("StorageAudioUrl") ? data["StorageAudioUrl"].ToString() : "",
            StorageImageUrl = data.ContainsKey("StorageImageUrl") ? data["StorageImageUrl"].ToString() : "",
            previewTime = data.ContainsKey("PreviewTime") ? int.Parse(data["PreviewTime"].ToString()) : 0,
            dateAdded = data.ContainsKey("DateAdded") ? DateTime.Parse(data["DateAdded"].ToString()) : DateTime.MinValue
        };
    }

    public void DownloadBeatmap(Beatmap beatmap)
    {
        Debug.Log($"�ٿ�ε� ��ư Ŭ��: {beatmap.title}");

    }
}