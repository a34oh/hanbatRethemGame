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


public class FBManager
{
    private FirebaseAuth auth; // 파이어베이스 인증 객체
    public string FBurl = "https://rethemgame-default-rtdb.firebaseio.com/";  // 파이어베이스 데이터베이스 URL
    public string StorageBucketUrl = "gs://rethemgame.firebasestorage.app"; // 파이어베이스 스토리지 버킷 URL

    private DatabaseReference databaseRef; // 파이어베이스 데이터베이스 참조
    private FirebaseStorage storage; // 파이어베이스 스토리지 객체
    private bool isOnline = true; // 온라인 상태 확인
    AuthResult authResult;
    public FirebaseUser newUser;
    enum FileType
    {
        Audio,
        Image,
        Text
    }
    // Firebase 초기화
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
                SceneManager.LoadScene(SceneType.SongSelectScene.ToString());
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
    // 비트맵 업로드. 예기치 못한 이유로 실패 시 폴더 이름 및 텍스트 파일 변경 취소 추가, 파이어베이스 metaData 업로드 실패 시, stroage 업로드도 취소해야 함.
    public async Task UploadBeatmapToFirebase(Beatmap beatmap)
    {
        if (!IsOnline())
        {
            Debug.LogWarning("오프라인 상태입니다. Firebase에 업로드할 수 없습니다.");
            return;
        }

        string localFolderName = GetBeatmapFolderName(beatmap);

        // Firebase에서 고유 ID 가져오기
        string uniqueId = await GetNextBeatmapIdAsync();

        beatmap.id = uniqueId; // Beatmap ID 업데이트

        // 폴더 이름 생성
        string folderName = GetBeatmapFolderName(beatmap);
        string newFolderPath = RenameLocalBeatmapFolder(localFolderName, folderName);

        if (string.IsNullOrEmpty(newFolderPath))
        {
            Debug.LogError("폴더 이름 변경 실패로 업로드를 중단합니다.");
            return;
        }
        DirectoryInfo dirInfo = new DirectoryInfo(newFolderPath);

        // 파일들을 Firebase Storage에 병렬로 업로드
        var uploadResults = await UploadFilesToFirebaseStorageAsync(dirInfo, beatmap, folderName);

        // 업로드 결과 처리 및 메타데이터 준비
        var metadata = PrepareMetadataForServer(uploadResults, beatmap);

        if (metadata == null)
        {
            Debug.LogError("파일 업로드 실패로 메타데이터 생성이 중단되었습니다.");
            return;
        }

        // 메타데이터를 Firebase Realtime Database에 업로드
        if (await UploadMetadataToDatabaseAsync(metadata, folderName))
        {
            // ID 증가
            await IncrementBeatmapIdAsync(uniqueId);
            Debug.Log("ID가 성공적으로 증가되었습니다.");
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
            Debug.Log("폴더 이름 변경 완료.");
            return newFolderPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"폴더 이름 변경 실패: {ex.Message}");
            return null;
        }
    }
    private async Task<List<(FileType fileType, string fileName, string downloadUrl)>> UploadFilesToFirebaseStorageAsync(DirectoryInfo dirInfo, Beatmap beatmap, string folderName)
    {
        var uploadTasks = new List<Task<(FileType fileType, string fileName, string downloadUrl)>>();

        // 텍스트 파일 업로드
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
                Debug.LogError($"텍스트 파일 처리 중 오류 발생: {txtFile.Name}, 오류: {ex.Message}");
            }
        }

        // 오디오 및 이미지 파일 업로드
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
                    Debug.LogError($"파일 업로드 중 오류 발생: {file.Name}, 오류: {ex.Message}");
                }
            }
        }

        /* // 스킨 파일 업로드
         foreach (var skinFile in dirInfo.GetFiles("*.skin"))
         {
             try
             {
                 string firebaseStoragePath = $"Songs/{beatmap.id} {beatmap.artist} - {beatmap.title}/{skinFile.Name}";
                 uploadTasks.Add(UploadFileToFirebaseStorage(skinFile.FullName, firebaseStoragePath, FileType.Skin));
             }
             catch (Exception ex)
             {
                 Debug.LogError($"스킨 파일 처리 중 오류 발생: {skinFile.Name}, 오류: {ex.Message}");
             }
         }
       */

        var results = await Task.WhenAll(uploadTasks);

        return new List<(FileType fileType, string fileName, string downloadUrl)>(results);
    }


    // 텍스트 파일에서 Id 값을 업데이트
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

        // 파일 업데이트
        await File.WriteAllLinesAsync(filePath, updatedLines);

        return;
    }

    // Firebase Database에 비트맵 메타데이터 업로드. beatmap을 통째로 안 올ㄹ리는 이유는 beatmap의 모든 데이터가 필요할 것 같지는 않고, (audioPath, imagePath 등)
    // 서버에만 존재해야 하는 데이터들이 있기 때문. (AudioStroageUrl, ImageStroageUrl 등)
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
                Debug.LogError($"파일 업로드 실패 - 유형: {result.fileType}, 파일명: {result.fileName}");
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
            Debug.Log("메타데이터가 Realtime Database에 성공적으로 업로드되었습니다.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"메타데이터 업로드 중 오류 발생: {ex.Message}");
            return false;
        }
    }
    /*
    // Firebase Storage에 파일 업로드 메서드
    private async Task<(FileType fileType, string fileName, string downloadUrl)> UploadFileToFirebaseStorage(string localFilePath, string firebaseStoragePath, FileType fileType)
    {
        var storageReference = storage.GetReferenceFromUrl(StorageBucketUrl).Child(firebaseStoragePath);
        try
        {
            var reference = storage.GetReferenceFromUrl(StorageBucketUrl);
            Debug.Log("Firebase Storage Reference 생성 성공!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase Storage Reference 생성 실패: {ex.Message}");
        }
        try
        {
            Debug.Log($"StorageBucketUrl: {StorageBucketUrl}");
            // Firebase Storage 경로 확인 로그 추가
            Debug.Log($"업로드 대상 Firebase 경로: {firebaseStoragePath}");


            // 파일명 디코딩
            string fileName = Uri.UnescapeDataString(Path.GetFileName(localFilePath));
            Debug.Log($"디코딩된 파일명: {fileName}");

            Debug.Log($"파일 업로드 중: {fileType} - {Path.GetFileName(localFilePath)}");
            Debug.Log($"localFilePath : {localFilePath}");

            var uriObject = GetContentUriFromFilePath(localFilePath);
            string uriString = uriObject.Call<string>("toString");
            uriString.Replace("%20", " ");
            Debug.Log($"uriString : {uriString}");

            await storageReference.PutFileAsync(localFilePath);
            Uri uri = await storageReference.GetDownloadUrlAsync();
            Debug.Log($"파일 업로드 성공: {fileType} - {Path.GetFileName(localFilePath)} - URL: {uri}");
            return (fileType, Path.GetFileName(localFilePath), uri.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError($"파일 업로드 실패 - 유형: {fileType}, 파일명: {Path.GetFileName(localFilePath)}, 오류: {ex.Message}");
            return (fileType, Path.GetFileName(localFilePath), null);
        }
    }*/

    // Firebase Storage에 파일 업로드 메서드
    private async Task<(FileType fileType, string fileName, string downloadUrl)> UploadFileToFirebaseStorage(string localFilePath, string firebaseStoragePath, FileType fileType)
    {
        var storageReference = storage.GetReferenceFromUrl(StorageBucketUrl).Child(firebaseStoragePath);

        try
        {
            Debug.Log($"파일 업로드 중: {fileType} - {Path.GetFileName(localFilePath)}");

            //모바일로 돌릴 때
            /*var uriObject = GetContentUriFromFilePath(localFilePath);
            string uriString = uriObject.Call<string>("toString").Replace("%20", " ");
            Debug.Log($"uriString : {uriString}");
        
            await storageReference.PutFileAsync(uriString);*/
            //PC 에디터로 돌릴 때
            await storageReference.PutFileAsync(localFilePath);
            Uri uri = await storageReference.GetDownloadUrlAsync();
            Debug.Log($"파일 업로드 성공: {fileType} - {Path.GetFileName(localFilePath)} - URL: {uri}");
            return (fileType, Path.GetFileName(localFilePath), uri.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError($"파일 업로드 실패 - 유형: {fileType}, 파일명: {Path.GetFileName(localFilePath)}, 오류: {ex.Message}");
            return (fileType, Path.GetFileName(localFilePath), null);
        }
    }




    private AndroidJavaObject GetContentUriFromFilePath(string filePath)
    {
        // Create a Java File object
        using (var file = new AndroidJavaObject("java.io.File", filePath))
        {
            // Get the Uri from android.net.Uri.fromFile(File)
            var uri = new AndroidJavaClass("android.net.Uri")
                .CallStatic<AndroidJavaObject>("fromFile", file);
            return uri;
        }
    }


    // Firebase에서 다음 고유 ID 가져오기
    private async Task<string> GetNextBeatmapIdAsync()
    {
        var idSnapshot = await databaseRef.Child("NextBeatmapId").GetValueAsync();
        int currentId = idSnapshot.Exists ? int.Parse(idSnapshot.Value.ToString()) : -1;
        Debug.Log($"currentid: {currentId}");
        return currentId.ToString();
    }

    // 업로드 성공 시 고유 ID를 증가
    private async Task IncrementBeatmapIdAsync(string currentId)
    {
        int nextId = int.Parse(currentId) + 1;
        await databaseRef.Child("NextBeatmapId").SetValueAsync(nextId);
        Debug.Log($"NextBeatmapId id증가 {nextId}");
    }

    //비트맵 정보 불러오기
    public async Task<List<Beatmap>> FetchBeatmapMetadataAsync(int startIndex = 0, int limit = 10)
    {
        if (databaseRef != null)
            Debug.Log($"databaseRef : {databaseRef }");
        else
        {
            Debug.Log("databaseRef is null");
        }
        var beatmaps = new List<Beatmap>();
        try
        {
            // Firebase에서 Songs 노드의 데이터를 날짜 기준으로 정렬
            var snapshot = await databaseRef
                .Child("Songs")
                .OrderByChild("DateAdded")
                .LimitToLast(startIndex + limit) // 시작 인덱스 + 제한 갯수
                .GetValueAsync();

            if (snapshot.Exists)
            {
                var allData = snapshot.Children
                    .Select(child => child.Value as Dictionary<string, object>)
                    .Where(data => data != null)
                    .OrderByDescending(data => data["DateAdded"].ToString()) // 최신 순 정렬
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
                        Debug.LogError($"비트맵 데이터를 변환 중 오류 발생: {ex.Message}");
                    }
                }

                Debug.Log($"총 {allData.Count}개의 비트맵 데이터를 불러왔습니다.");
            }
            else
            {
                Debug.LogWarning("비트맵 데이터가 없습니다.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"비트맵 데이터를 불러오는 중 오류 발생: {ex.Message}");
        }
        return beatmaps;
    }
    private Beatmap ParseBeatmapFromMetadata(Dictionary<string, object> data)
    {
        // TextNames 파싱
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

        // 로컬 폴더가 없으면 생성
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

                // 오디오 파일 다운로드
                if (!string.IsNullOrEmpty(audioUrl))
                {
                    downloadTasks.Add(DownloadFileFromUrlAsync(audioUrl, localFolderPath, beatmap.audioName));
                }

                // 이미지 파일 다운로드
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    downloadTasks.Add(DownloadFileFromUrlAsync(imageUrl, localFolderPath, beatmap.imageName));
                }

                // 텍스트 파일 다운로드
                foreach (string textName in beatmap.textNames)
                {
                    string textUrl = snapshot.Child("StorageTextUrls")
                        .Children
                        .FirstOrDefault(child => child.Value?.ToString()?.Contains(textName) == true)
                        ?.Value?.ToString()?.Replace(" ", "%20");

                    if (!string.IsNullOrEmpty(textUrl))
                    {
                        Debug.Log($"다운로드할 텍스트 파일 URL: {textUrl}, 파일 이름: {textName}");
                        downloadTasks.Add(DownloadFileFromUrlAsync(textUrl, localFolderPath, textName));
                    }
                    else
                    {
                        Debug.LogWarning($"URL이 존재하지 않거나 텍스트 파일 이름을 찾을 수 없습니다: {textName}");
                    }
                }

                Debug.Log($"비트맵 {firebaseFolderName} 다운로드 중...");

                await Task.WhenAll(downloadTasks);

                Debug.Log($"비트맵 {firebaseFolderName}의 모든 파일이 다운로드되었습니다.");
                await GameManager.BeatmapParser.ParserAllBeatmapsAsync();
            }
            else
            {
                Debug.LogWarning($"비트맵 {firebaseFolderName}에 해당하는 데이터가 없습니다.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"파일 다운로드 중 오류 발생: {ex.Message}");
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

            Debug.Log($"파일 다운로드 완료: {fileName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"파일 다운로드 중 오류 발생 ({fileName}): {ex.Message}");
        }
    }
    public async Task<PlayerResult> GetHighestScoreAsync(string beatmapId)
    {
        try
        {
            // 로그인한 사용자의 UserId 가져오기
            string userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("No user is currently logged in.");
                return null;
            }

            // Firebase 경로 설정
            string databasePath = $"Results/{userId}/{beatmapId}";
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child(databasePath);

            // Firebase에서 데이터 가져오기
            var snapshot = await reference.GetValueAsync();

            if (snapshot.Exists)
            {
                // JSON 데이터를 PlayerResult 객체로 변환
                string json = snapshot.GetRawJsonValue();
                PlayerResult highestScore = JsonUtility.FromJson<PlayerResult>(json);

                Debug.Log($"Highest Score: {highestScore.playerScore}, Player: {highestScore.playerId}");
                return highestScore;
            }
            else
            {
                Debug.LogWarning($"No data found for beatmapId: {beatmapId}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to fetch highest score: {ex.Message}");
            return null;
        }
    }
    public async void high()
    {
        string beatmapId = "beatmapid";
        PlayerResult highestScore = await GetHighestScoreAsync(beatmapId);

        if (highestScore != null)
        {
            Debug.Log($"Highest Score: {highestScore.playerScore}, Player: {highestScore.playerId}");
        }
        else
        {
            Debug.Log("No score found for this beatmap.");
        }
    }
}