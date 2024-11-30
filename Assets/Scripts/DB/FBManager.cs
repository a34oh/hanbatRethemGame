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

//Beatmap.cs            ??????? ?????? ?????? ????? 
//FBManager.cs          ???????? ????
//DBManager.cs          ???? db ????
//GameManager.cs        ???? Manager?? Singletone?? ???? ???? ????
//BeatmapCreator.cs     ??????? ??????? ??????? txt??? ????, ??, ??????? ????? ??????? ?????. (????? ??? ???? : ????? ????, ????? ??????, ????? ??????, ????? ??(mp3), ????? ?????)
//BeatmapUploader.cs    BeatmapCreator???? ?????? ????? ?????? ??? ??????? txt??? ????, ??, ??????? ?????????? ???��???? ?????
//BeatmapParser.cs      ????? ????? ????? ?????? ?��????? ?????.??????? txt??? ??????? beatmap??????? ??????, ??????? ?? ?????? ?????
//ResourceCache.cs      ?????, ?? ?? ??????? ??? ?��?
//??????????????.cs   ?????????? ???��? ?? ??????? ScrollView?? ??????? UI ?????(???? ???? ?????????? ???��? ?? ??????? ???????? ??? 30?? ????????. ?? ????? ???? ????? ???? ?��?)
//??????????????.cs   ????? ????? ??????? ScrollView?? ??????? UI ?????

//(?????? ???��? ?? ??????? ScrollView?? ??????? UI ????????? ????? ??? ?? ???? db?? ??????? ????? ???) - ??? ??? ?????��? ??????
//(???? ?????? ?????? ??? ??????? ???? db?? ?????????? ?????. ???? ??? ?????? ???????? ?? ??? ?��??? ??? ?? ??? ????.))
public class FBManager
{
    private FirebaseAuth auth; // ���̾�̽� ���� ��ü
    public string FBurl = "https://rethemgame-default-rtdb.firebaseio.com/";  // ���̾�̽� �����ͺ��̽� URL
    public string StorageBucketUrl = "gs://rethemgame.appspot.com"; // ���̾�̽� ���丮�� ��Ŷ URL

    private DatabaseReference databaseRef; // ���̾�̽� �����ͺ��̽� ����
    private FirebaseStorage storage; // ���̾�̽� ���丮�� ��ü
    private bool isOnline = true; // �¶��� ���� Ȯ��
    AuthResult authResult;
    public FirebaseUser newUser;
    enum FileType
    {
        Audio,
        Image,
        Text
    }
    // Firebase ????
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

            auth = FirebaseAuth.GetAuth(app); // FirebaseAuth ��ü �ʱ�ȭ

            Debug.Log("���̾�̽��� ���������� �ʱ�ȭ�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogError("���̾�̽� ���Ӽ� Ȯ�� ����: " + dependencyStatus);
            isOnline = false;
        }
    }

    public void login(string id, string pw)
    {
        // auth = FirebaseAuth.DefaultInstance;
        // �����Ǵ� �Լ� : �̸��ϰ� ��й�ȣ�� �α��� ���� ��
        auth.SignInWithEmailAndPasswordAsync(id, pw).ContinueWithOnMainThread(
            task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("�α��� ���");
                    return;
                }
                if (task.IsFaulted)
                {
                    foreach (var exception in task.Exception.Flatten().InnerExceptions)
                    {
                        Debug.LogError($"�α��� ���� ����: {exception.Message}");
                    }
                    return;
                }

                authResult = task.Result;
                newUser = authResult.User;

                Debug.Log($"�α��� ����: {newUser.Email}, UID: {newUser.UserId}");
                SceneManager.LoadScene("ScoreScene");
            });
        
    }
    public void register(string id, string pw) 
    {
        // auth = FirebaseAuth.DefaultInstance;
        // �����Ǵ� �Լ� : �̸��ϰ� ��й�ȣ�� ȸ������ ���� ��
        auth.CreateUserWithEmailAndPasswordAsync(id, pw).ContinueWithOnMainThread(
            task => 
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("ȸ������ ���");
                    return;
                }
                if (task.IsFaulted)
                {
                    foreach (var exception in task.Exception.Flatten().InnerExceptions)
                    {
                        Debug.LogError($"ȸ������ ���� ����: {exception.Message}");
                    }
                    return;
                }

                AuthResult register_authResult = task.Result;
                FirebaseUser register_newUser = authResult.User;

                Debug.Log($"ȸ������ ����: {register_newUser.Email}, UID: {register_newUser.UserId}");
            });
    }
    public bool isauth()
    {
        if(auth == null || FirebaseApp.DefaultInstance == null) return false;
        else return true;
    }

    // ???????? ???? ???
    public bool IsOnline()
    {
        return isOnline;
    }
    public async Task SaveResultAsync(PlayerResult result, string beatmapName)
    {
        string localPath = Path.Combine(Application.persistentDataPath, "Results", beatmapName);
        string resultFile = Path.Combine(localPath, "results.json");

        List<PlayerResult> results = new List<PlayerResult>();

        try
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(localPath))
                Directory.CreateDirectory(localPath);

            // Read existing results if the file exists
            if (File.Exists(resultFile))
            {
                string existingData = File.ReadAllText(resultFile);
                results = JsonUtility.FromJson<ListWrapper<PlayerResult>>(existingData)?.Items ?? new List<PlayerResult>();
            }

            // Add new result
            results.Add(result);

            // Write back to file
            string json = JsonUtility.ToJson(new ListWrapper<PlayerResult> { Items = results }, true);
            File.WriteAllText(resultFile, json);

            Debug.Log($"Result saved locally: {result.playerScore}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save result locally: {ex.Message}");
        }

        // If online, upload the highest result
        if (IsOnline())
        {
            await UploadHighestResultToServerAsync(beatmapName, results);
        }
    }

    private async Task UploadHighestResultToServerAsync(string beatmapName, List<PlayerResult> results)
    {
        try
        {
            // Determine highest score
            var highestResult = results.OrderByDescending(r => r.playerScore).First();

            string databasePath = $"Results/{FirebaseAuth.DefaultInstance.CurrentUser.UserId}/{beatmapName}";

            // Firebase는 Dictionary 형식을 권장
            var resultData = new Dictionary<string, object>
            {
                { "playerScore", highestResult.playerScore },
                { "playerId", highestResult.playerId },
                { "playTime", highestResult.playTime }
            };

            await databaseRef.Child(databasePath).SetValueAsync(resultData);

            Debug.Log("Highest result uploaded to server.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to upload result to server: {ex.Message}");
        }
    }
    // Wrapper for JSON serialization
    [Serializable]
    public class ListWrapper<T>
    {
        public List<T> Items;
    }

    // ????? ???��?. ????? ???? ?????? ???? ?? ???? ??? ?? ???? ???? ???? ??? ???, ???????? metaData ???��? ???? ??, stroage ???��? ?????? ??.
    public async Task UploadBeatmapToFirebase(Beatmap beatmap)
    {
        if (!IsOnline())
        {
            Debug.LogWarning("???????? ????????. Firebase?? ???��??? ?? ???????.");
            return;
        }

        string localFolderName = GetBeatmapFolderName(beatmap);

        // Firebase???? ???? ID ????????
        string uniqueId = await GetNextBeatmapIdAsync();
       
        beatmap.id = uniqueId; // Beatmap ID ???????

        // ???? ??? ????
        string folderName = GetBeatmapFolderName(beatmap);
        string newFolderPath = RenameLocalBeatmapFolder(localFolderName, folderName);

        if (string.IsNullOrEmpty(newFolderPath))
        {
            Debug.LogError("???? ??? ???? ???��? ???��? ???????.");
            return;
        }
        DirectoryInfo dirInfo = new DirectoryInfo(newFolderPath);

        // ??????? Firebase Storage?? ????? ???��?
        var uploadResults = await UploadFilesToFirebaseStorageAsync(dirInfo, beatmap, folderName);

        // ???��? ??? ??? ?? ????????? ???
        var metadata = PrepareMetadataForServer(uploadResults, beatmap);

        if (metadata == null)
        {
            Debug.LogError("???? ???��? ???��? ????????? ?????? ??????????.");
            return;
        }

        // ?????????? Firebase Realtime Database?? ???��?
        if (await UploadMetadataToDatabaseAsync(metadata, folderName))
        {
            // ID ????
            await IncrementBeatmapIdAsync(uniqueId);
            Debug.Log("ID?? ?????????? ????????????.");
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
            Debug.Log("???? ??? ???? ???.");
            return newFolderPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"???? ??? ???? ????: {ex.Message}");
            return null;
        }
    }
    private async Task<List<(FileType fileType, string fileName, string downloadUrl)>> UploadFilesToFirebaseStorageAsync(DirectoryInfo dirInfo, Beatmap beatmap, string folderName)
    {
        var uploadTasks = new List<Task<(FileType fileType, string fileName, string downloadUrl)>>();

        // ???? ???? ???��?
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
                Debug.LogError($"???? ???? ??? ?? ???? ???: {txtFile.Name}, ????: {ex.Message}");
            }
        }

        // ????? ?? ????? ???? ???��?
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
                    Debug.LogError($"???? ???��? ?? ???? ???: {file.Name}, ????: {ex.Message}");
                }
            }
        }

       /* // ??? ???? ???��?
        foreach (var skinFile in dirInfo.GetFiles("*.skin"))
        {
            try
            {
                string firebaseStoragePath = $"Songs/{beatmap.id} {beatmap.artist} - {beatmap.title}/{skinFile.Name}";
                uploadTasks.Add(UploadFileToFirebaseStorage(skinFile.FullName, firebaseStoragePath, FileType.Skin));
            }
            catch (Exception ex)
            {
                Debug.LogError($"??? ???? ??? ?? ???? ???: {skinFile.Name}, ????: {ex.Message}");
            }
        }
      */

        var results = await Task.WhenAll(uploadTasks);

        return new List<(FileType fileType, string fileName, string downloadUrl)>(results);
    }


    // ???? ??????? Id ???? ???????
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

        // ???? ???????
        await File.WriteAllLinesAsync(filePath, updatedLines);

        return;
    }

    // Firebase Database?? ????? ????????? ???��?. beatmap?? ??��?? ?? ??????? ?????? beatmap?? ??? ??????? ????? ?? ?????? ???, (audioPath, imagePath ??)
    // ???????? ??????? ??? ????????? ??? ????. (AudioStroageUrl, ImageStroageUrl ??)
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
                Debug.LogError($"???? ???��? ???? - ????: {result.fileType}, ?????: {result.fileName}");
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
            Debug.Log("?????????? Realtime Database?? ?????????? ???��????????.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"????????? ???��? ?? ???? ???: {ex.Message}");
            return false;
        }
    }
    // Firebase Storage?? ???? ???��? ?????
    private async Task<(FileType fileType, string fileName, string downloadUrl)> UploadFileToFirebaseStorage(string localFilePath, string firebaseStoragePath, FileType fileType)
    {
        var storageReference = storage.GetReferenceFromUrl(StorageBucketUrl).Child(firebaseStoragePath);

        try
        {
            Debug.Log($"???? ???��? ??: {fileType} - {Path.GetFileName(localFilePath)}");

            await storageReference.PutFileAsync(localFilePath);
            Uri uri = await storageReference.GetDownloadUrlAsync();
            Debug.Log($"???? ???��? ????: {fileType} - {Path.GetFileName(localFilePath)} - URL: {uri}");
            return (fileType, Path.GetFileName(localFilePath), uri.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError($"???? ???��? ???? - ????: {fileType}, ?????: {Path.GetFileName(localFilePath)}, ????: {ex.Message}");
            return (fileType, Path.GetFileName(localFilePath), null);
        }
    }


    // Firebase???? ???? ???? ID ????????
    private async Task<string> GetNextBeatmapIdAsync()
    {
        var idSnapshot = await databaseRef.Child("NextBeatmapId").GetValueAsync();
        int currentId = idSnapshot.Exists ? int.Parse(idSnapshot.Value.ToString()) : -1;
        Debug.Log($"currentid: {currentId}");
        return currentId.ToString();
    }

    // ???��? ???? ?? ???? ID?? ????
    private async Task IncrementBeatmapIdAsync(string currentId)
    {
        int nextId = int.Parse(currentId) + 1;
        await databaseRef.Child("NextBeatmapId").SetValueAsync(nextId);
        Debug.Log($"NextBeatmapId id???? {nextId}");
    }

    //????? ???? ???????
    public async Task<List<Beatmap>> FetchBeatmapMetadataAsync(int startIndex = 0, int limit = 10)
    {
        var beatmaps = new List<Beatmap>();
        try
        {
            // Firebase???? Songs ????? ??????? ??? ???????? ????
            var snapshot = await databaseRef
                .Child("Songs")
                .OrderByChild("DateAdded")
                .LimitToLast(startIndex + limit) // ???? ?��??? + ???? ????
                .GetValueAsync();

            if (snapshot.Exists)
            {
                var allData = snapshot.Children
                    .Select(child => child.Value as Dictionary<string, object>)
                    .Where(data => data != null)
                    .OrderByDescending(data => data["DateAdded"].ToString()) // ??? ?? ????
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
                        Debug.LogError($"????? ??????? ??? ?? ???? ???: {ex.Message}");
                    }
                }

                Debug.Log($"?? {allData.Count}???? ????? ??????? ?????????.");
            }
            else
            {
                Debug.LogWarning("????? ??????? ???????.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"????? ??????? ??????? ?? ???? ???: {ex.Message}");
        }
        return beatmaps;
    }
    private Beatmap ParseBeatmapFromMetadata(Dictionary<string, object> data)
    {
        // TextNames ???
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

        // ???? ?????? ?????? ????
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

                // ????? ???? ???��?
                if (!string.IsNullOrEmpty(audioUrl))
                {
                    downloadTasks.Add(DownloadFileFromUrlAsync(audioUrl, localFolderPath, beatmap.audioName));
                }

                // ????? ???? ???��?
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    downloadTasks.Add(DownloadFileFromUrlAsync(imageUrl, localFolderPath, beatmap.imageName));
                }

                // ???? ???? ???��?
                foreach (string textName in beatmap.textNames)
                {
                    string textUrl = snapshot.Child("StorageTextUrls")
                        .Children
                        .FirstOrDefault(child => child.Value?.ToString()?.Contains(textName) == true)
                        ?.Value?.ToString()?.Replace(" ", "%20");

                    if (!string.IsNullOrEmpty(textUrl))
                    {
                        Debug.Log($"???��??? ???? ???? URL: {textUrl}, ???? ???: {textName}");
                        downloadTasks.Add(DownloadFileFromUrlAsync(textUrl, localFolderPath, textName));
                    }
                    else
                    {
                        Debug.LogWarning($"URL?? ???????? ???? ???? ???? ????? ??? ?? ???????: {textName}");
                    }
                }

                Debug.Log($"????? {firebaseFolderName} ???��? ??...");

                await Task.WhenAll(downloadTasks);

                Debug.Log($"????? {firebaseFolderName}?? ??? ?????? ???��????????.");
            }
            else
            {
                Debug.LogWarning($"????? {firebaseFolderName}?? ?????? ??????? ???????.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"???? ???��? ?? ???? ???: {ex.Message}");
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

            Debug.Log($"???? ???��? ???: {fileName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"???? ???��? ?? ???? ??? ({fileName}): {ex.Message}");
        }
    }


}