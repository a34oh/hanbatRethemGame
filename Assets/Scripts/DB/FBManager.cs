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

//Beatmap.cs            비트맵의 정보를 보유한 클래스 
//FBManager.cs          파이어베이스 관리
//DBManager.cs          로컬 db 관리
//GameManager.cs        각종 Manager를 Singletone을 통해 통합 관리
//BeatmapCreator.cs     비트맵을 생성하고 비트맵을 txt화한 파일, 곡, 이미지를 로컬에 저장하는 클래스. (사용자 입력 정보 : 비트맵 제목, 비트맵 아티스트, 비트맵 제작자, 비트맵 곡(mp3), 비트맵 이미지)
//BeatmapUploader.cs    BeatmapCreator에서 생성한 로컬에 저장되어 있는 비트맵을 txt화한 파일, 곡, 이미지를 파이어베이스에 업로드하는 클래스
//BeatmapParser.cs      로컬에 저장된 비트맵 정보를 읽어들이는 클래스.비트맵을 txt화한 파일들을 beatmap클래스로 파싱하고, 이미지와 곡 정보를 불러옴
//ResourceCache.cs      이미지, 곡 등 소스파일 미리 로드
//클래스이름못정함.cs   파이어베이스에 업로드 된 비트맵을 ScrollView로 보여주는 UI 클래스(가장 최근에 파이어베이스에 업로드 된 비트맵을 기준으로 최대 30개 묶음으로. 그 이상은 화살표 버튼을 통해 로드)
//클래스이름못정함.cs   로컬에 저장된 비트맵을 ScrollView로 보여주는 UI 클래스

//(서버에 업로드 된 비트맵을 ScrollView로 보여주는 UI 클래스에서 비트맵 클릭 시 로컬 db에 비트맵을 다운받는 방식) - 이건 어디에 작성하는게 좋을까
//(내부 저장소에 저장되어 있는 파일들을 로컬 db에 저장?해주는 클래스. 향후 다시 게임을 시작했을 때 바로 로딩이 되게 끔 하기 위함.))
public class FBManager
{
    public string FBurl = "https://rethemgame-default-rtdb.firebaseio.com/";  // 여기를 Firebase Console의 Database URL로 변경
    public string StorageBucketUrl = "gs://rethemgame.firebasestorage.app"; // 여기를 Firebase Console의 Storage Bucket URL로 변경

    private DatabaseReference databaseRef;
    private FirebaseStorage storage;
    private bool isOnline = true;

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
            // Firebase 앱 초기화
            FirebaseApp app = FirebaseApp.DefaultInstance;

            // Database URL과 Storage Bucket URL을 명시적으로 설정
            app.Options.DatabaseUrl = new Uri(FBurl);
            app.Options.StorageBucket = StorageBucketUrl;

            // Firebase Database와 Storage 인스턴스 초기화
            databaseRef = FirebaseDatabase.GetInstance(app, FBurl).RootReference;
            storage = FirebaseStorage.GetInstance(app, StorageBucketUrl);

            Debug.Log("Firebase가 성공적으로 초기화되었습니다.");
        }
        else
        {
            Debug.LogError("Firebase 의존성 확인 실패: " + dependencyStatus);
            isOnline = false; // Firebase 초기화 실패 시 오프라인 상태로 설정
        }
    }


    // 오프라인 상태 확인
    public bool IsOnline()
    {
        return isOnline;
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
    // Firebase Storage에 파일 업로드 메서드
    private async Task<(FileType fileType, string fileName, string downloadUrl)> UploadFileToFirebaseStorage(string localFilePath, string firebaseStoragePath, FileType fileType)
    {
        var storageReference = storage.GetReferenceFromUrl(StorageBucketUrl).Child(firebaseStoragePath);

        try
        {
            Debug.Log($"파일 업로드 중: {fileType} - {Path.GetFileName(localFilePath)}");

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
        Debug.Log($"다운로드 버튼 클릭: {beatmap.title}");

    }
}