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
using System.Net.Http;
using Firebase.Auth;
using UnityEngine.SceneManagement;

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
    private FirebaseAuth auth; // 파이어베이스 인증 객체
    public string FBurl = "https://rethemgame-default-rtdb.firebaseio.com/";  // 파이어베이스 데이터베이스 URL
    public string StorageBucketUrl = "gs://rethemgame.appspot.com"; // 파이어베이스 스토리지 버킷 URL

    private DatabaseReference databaseRef; // 파이어베이스 데이터베이스 참조
    private FirebaseStorage storage; // 파이어베이스 스토리지 객체
    private bool isOnline = true; // 온라인 상태 확인
    AuthResult authResult;
    FirebaseUser newUser;
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
            FirebaseApp app = FirebaseApp.DefaultInstance;
            app.Options.DatabaseUrl = new Uri(FBurl);
            app.Options.StorageBucket = StorageBucketUrl;

            databaseRef = FirebaseDatabase.GetInstance(app, FBurl).RootReference;
            storage = FirebaseStorage.GetInstance(app, StorageBucketUrl);

            auth = FirebaseAuth.GetAuth(app); // FirebaseAuth 객체 초기화

            Debug.Log("파이어베이스가 성공적으로 초기화되었습니다.");
        }
        else
        {
            Debug.LogError("파이어베이스 종속성 확인 실패: " + dependencyStatus);
            isOnline = false;
        }
    }

    public void login(string id, string pw)
    {
        // auth = FirebaseAuth.DefaultInstance;
        // 제공되는 함수 : 이메일과 비밀번호로 로그인 시켜 줌
        auth.SignInWithEmailAndPasswordAsync(id, pw).ContinueWithOnMainThread(
            task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("로그인 취소");
                    return;
                }
                if (task.IsFaulted)
                {
                    foreach (var exception in task.Exception.Flatten().InnerExceptions)
                    {
                        Debug.LogError($"로그인 실패 이유: {exception.Message}");
                    }
                    return;
                }

                authResult = task.Result;
                newUser = authResult.User;

                Debug.Log($"로그인 성공: {newUser.Email}, UID: {newUser.UserId}");
                SceneManager.LoadScene("SongSelectScene");
            });
        
    }
    public void register(string id, string pw) 
    {
        // auth = FirebaseAuth.DefaultInstance;
        // 제공되는 함수 : 이메일과 비밀번호로 회원가입 시켜 줌
        auth.CreateUserWithEmailAndPasswordAsync(id, pw).ContinueWithOnMainThread(
            task => 
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("회원가입 취소");
                    return;
                }
                if (task.IsFaulted)
                {
                    foreach (var exception in task.Exception.Flatten().InnerExceptions)
                    {
                        Debug.LogError($"회원가입 실패 이유: {exception.Message}");
                    }
                    return;
                }

                AuthResult register_authResult = task.Result;
                FirebaseUser register_newUser = authResult.User;

                Debug.Log($"회원가입 성공: {register_newUser.Email}, UID: {register_newUser.UserId}");
            });
    }
    public bool isauth()
    {
        if(auth == null || FirebaseApp.DefaultInstance == null) return false;
        else return true;
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
        {"AudioName", beatmap.audioName },
        {"ImageName", beatmap.imageName },
         { "TextNames", new List<string>() },
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
                    ((List<string>)metadata["TextNames"]).Add(result.fileName);
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
        // TextNames �Ľ�
        var textNames = data.ContainsKey("TextNames")
            ? (data["TextNames"] as List<object>)?.Select(o => o.ToString()).ToList()
            : new List<string>();

        Beatmap beatmap = new Beatmap
        {
            id = data.ContainsKey("Id") ? data["Id"].ToString() : "",
            title = data.ContainsKey("Title") ? data["Title"].ToString() : "",
            artist = data.ContainsKey("Artist") ? data["Artist"].ToString() : "",
            creator = data.ContainsKey("Creator") ? data["Creator"].ToString() : "",
            version = data.ContainsKey("Version") ? data["Version"].ToString() : "",
            audioName = data.ContainsKey("AudioName") ? data["AudioName"].ToString() : "",
            imageName = data.ContainsKey("ImageName") ? data["ImageName"].ToString() : "",
            StorageAudioUrl = data.ContainsKey("StorageAudioUrl") ? data["StorageAudioUrl"].ToString() : "",
            StorageImageUrl = data.ContainsKey("StorageImageUrl") ? data["StorageImageUrl"].ToString() : "",
            previewTime = data.ContainsKey("PreviewTime") ? int.Parse(data["PreviewTime"].ToString()) : 0,
            dateAdded = data.ContainsKey("DateAdded") ? DateTime.Parse(data["DateAdded"].ToString()) : DateTime.MinValue,
            textNames = textNames
        };
        return beatmap;
    }

    public async Task DownloadBeatmapAsync(Beatmap beatmap)
    {
        string firebaseFolderName = GetBeatmapFolderName(beatmap);
        var firebaseFolderPath = storage.GetReference("Songs").Child(firebaseFolderName);

        string localFolderPath = Path.Combine(Application.persistentDataPath, "Songs", firebaseFolderName).Replace("\\", "/");

        // ���� ������ ������ ����
        if (!Directory.Exists(localFolderPath))
        {
            Directory.CreateDirectory(localFolderPath);
        }

        try
        {
            var snapshot = await databaseRef.Child("Songs").Child(firebaseFolderName).GetValueAsync();
            if (snapshot.Exists)
            {
                string audioUrl = snapshot.Child("StorageAudioUrl").Value?.ToString().Replace(" ", "%20");
                string imageUrl = snapshot.Child("StorageImageUrl").Value?.ToString().Replace(" ", "%20");
                
                imageUrl.Replace(" ", "%20");
                Debug.Log($"audioUrl : {audioUrl}");
                Debug.Log($"imageUrl : {imageUrl}");
                var downloadTasks = new List<Task>();

                // ����� ���� �ٿ�ε�
                if (!string.IsNullOrEmpty(audioUrl))
                {
                    downloadTasks.Add(DownloadFileFromUrlAsync(audioUrl, localFolderPath, beatmap.audioName));
                }

                // �̹��� ���� �ٿ�ε�
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    downloadTasks.Add(DownloadFileFromUrlAsync(imageUrl, localFolderPath, beatmap.imageName));
                }

                // �ؽ�Ʈ ���� �ٿ�ε�
                foreach (string textName in beatmap.textNames)
                {
                    string textUrl = snapshot.Child("StorageTextUrls")
                        .Children
                        .FirstOrDefault(child => child.Value?.ToString()?.Contains(textName) == true)
                        ?.Value?.ToString()?.Replace(" ", "%20");

                    if (!string.IsNullOrEmpty(textUrl))
                    {
                        Debug.Log($"�ٿ�ε��� �ؽ�Ʈ ���� URL: {textUrl}, ���� �̸�: {textName}");
                        downloadTasks.Add(DownloadFileFromUrlAsync(textUrl, localFolderPath, textName));
                    }
                    else
                    {
                        Debug.LogWarning($"URL�� �������� �ʰų� �ؽ�Ʈ ���� �̸��� ã�� �� �����ϴ�: {textName}");
                    }
                }

                Debug.Log($"��Ʈ�� {firebaseFolderName} �ٿ�ε� ��...");

                await Task.WhenAll(downloadTasks);

                Debug.Log($"��Ʈ�� {firebaseFolderName}�� ��� ������ �ٿ�ε�Ǿ����ϴ�.");
            }
            else
            {
                Debug.LogWarning($"��Ʈ�� {firebaseFolderName}�� �ش��ϴ� �����Ͱ� �����ϴ�.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� �ٿ�ε� �� ���� �߻�: {ex.Message}");
        }
    }
    private async Task DownloadFileFromUrlAsync(string url, string localFolderPath, string fileName)
    {
        try
        {
            Debug.Log($"localFolderPath : {localFolderPath}");
            Debug.Log($"fileName : {fileName}");

            string localFilePath = Path.Combine(localFolderPath, fileName);

            using (HttpClient client = new HttpClient())
            {
                var fileBytes = await client.GetByteArrayAsync(url); 
                await File.WriteAllBytesAsync(localFilePath, fileBytes);
            }

            Debug.Log($"���� �ٿ�ε� �Ϸ�: {fileName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� �ٿ�ε� �� ���� �߻� ({fileName}): {ex.Message}");
        }
    }


}