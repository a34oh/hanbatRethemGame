using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine;

//Beatmap.cs            ��Ʈ���� ������ ������ Ŭ���� 
//FBManager.cs          ���̾�̽� ����
//DBManager.cs          ���� db ����
//GameManager.cs        ���� Manager�� Singletone�� ���� ���� ����
//BeatmapCreator.cs     ��Ʈ���� �����ϰ� ��Ʈ���� txtȭ�� ����, ��, �̹����� ���ÿ� �����ϴ� Ŭ����. (����� �Է� ���� : ��Ʈ�� ����, ��Ʈ�� ��Ƽ��Ʈ, ��Ʈ�� ������, ��Ʈ�� ��(mp3), ��Ʈ�� �̹���)
//BeatmapUploader.cs    BeatmapCreator���� ������ ���ÿ� ����Ǿ� �ִ� ��Ʈ���� txtȭ�� ����, ��, �̹����� ���̾�̽��� ���ε��ϴ� Ŭ����
//BeatmapParser.cs      ���ÿ� ����� ��Ʈ�� ������ �о���̴� Ŭ����.��Ʈ���� txtȭ�� ���ϵ��� beatmapŬ������ �Ľ��ϰ�, �̹����� �� ������ �ҷ���
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

    // ��Ʈ�� ���ε�
    public async Task UploadBeatmapAsync(string localFolderPath)
    {
        if (!IsOnline())
        {
            Debug.LogWarning("�������� �����Դϴ�. Firebase�� ���ε��� �� �����ϴ�.");
            return;
        }

        // Firebase���� ���� ID ��������
        string uniqueId = await GetNextBeatmapIdAsync();

        // ���� ���� �̸��� ���� ID�� ����
        DirectoryInfo dirInfo = new DirectoryInfo(localFolderPath);



        // ���� ID ����: ���� �̸����� ó�� ������ ���� ���� �ؽ�Ʈ�� ���
        string existingFolderName = dirInfo.Name;
        string folderNameWithoutId = existingFolderName.Contains(" ") ? existingFolderName.Substring(existingFolderName.IndexOf(" ") + 1) : existingFolderName;

        // ���ο� ID�� ���� �̸����� ID�� ������ ���� �̸� ����
        string newFolderName = $"{uniqueId} {folderNameWithoutId}";
        string newFolderPath = Path.Combine(dirInfo.Parent.FullName, newFolderName);

        try
        {
            Directory.Move(localFolderPath, newFolderPath);
            Debug.Log("���� �̸� ���� �Ϸ�.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to move folder: {ex.Message}");
        }


        // �� ���� ��ο� ���� DirectoryInfo �缳��
        dirInfo = new DirectoryInfo(newFolderPath);
        FileInfo[] txtFiles = dirInfo.GetFiles("*.txt");

        string audioFilename = null;
        string imageFilename = null;
        bool uploadSuccess = true; // ���ε� ���� ���� Ȯ���� ���� ����

        try
        {
            // �ؽ�Ʈ ���Ͽ��� AudioFilename, ImageFilename ���� �� Id ������Ʈ
            foreach (FileInfo txtFile in txtFiles)
            {
                // �ؽ�Ʈ ���� ���� ���� �� ���ε�
                var updateResult = await UpdateIdInTextFile(txtFile.FullName, uniqueId);
                Debug.Log($"�ؽ�Ʈ ���� {txtFile.Name}�� Id�� ������Ʈ�Ǿ����ϴ�.");

                audioFilename = updateResult.audioFilename;
                imageFilename = updateResult.imageFilename;


                // txt ���� Firebase Storage�� ���ε�
                string firebaseStoragePath = $"Songs/{newFolderName}/{txtFile.Name}";
                await UploadFileToFirebaseStorage(txtFile.FullName, firebaseStoragePath);
                Debug.Log($"���̵� ���� {txtFile.Name}�� Firebase Storage�� ���ε�Ǿ����ϴ�.");
            }

            // AudioFilename�� ImageFilename�� �ش��ϴ� ���ϸ� ���ε�
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                if (file.Name == audioFilename || file.Name == imageFilename)
                {
                    string firebaseStoragePath = $"Songs/{newFolderName}/{file.Name}";
                    await UploadFileToFirebaseStorage(file.FullName, firebaseStoragePath);
                    Debug.Log($"{file.Name} ������ Firebase Storage�� ���ε�Ǿ����ϴ�.");
                }
            }
        }
        catch (Exception ex)
        {
            uploadSuccess = false;
            Debug.LogError($"���� ���ε� �� ���� �߻�: {ex.Message}");
        }

        // ���ε� ���� �ÿ��� ID ����
        if (uploadSuccess)
        {
            await IncrementBeatmapIdAsync(uniqueId);
        }
        else
        {
            Debug.LogWarning($"���ε尡 �����Ͽ� ID ������ �ߴܵǾ����ϴ�: {uniqueId}");
        }
    }
    // �ؽ�Ʈ ���Ͽ��� Id ���� ������Ʈ�ϰ� ������ ������ �����ϴ� �޼���
    private async Task<(string audioFilename, string imageFilename)> UpdateIdInTextFile(string filePath, string newId)
    {
        var updatedLines = new List<string>();
        string audioFilename = null;
        string imageFilename = null;

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
                case "AudioFilename":
                    audioFilename = value;
                    updatedLines.Add(line);
                    break;
                case "ImageFilename":
                    imageFilename = value;
                    updatedLines.Add(line);
                    break;
                default:
                    updatedLines.Add(line);
                    break;
            }
        }

        // ���� ������Ʈ
        await File.WriteAllLinesAsync(filePath, updatedLines);

        return (audioFilename, imageFilename);
    }
    // Firebase Storage�� ���� ���ε� �޼���
    private async Task UploadFileToFirebaseStorage(string localFilePath, string firebaseStoragePath)
    {
        // Firebase Storage�� Ư�� URL ���� ��������
        var storageReference = storage.GetReferenceFromUrl(StorageBucketUrl).Child(firebaseStoragePath);
        var uploadTask = storageReference.PutFileAsync(localFilePath);

        await uploadTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"���� ���ε� ����: {localFilePath}");
            }
            else
            {
                Debug.Log($"���� ���ε� ����: {localFilePath}");
            }
        });
    }

    // Firebase���� ���� ���� ID ��������
    private async Task<string> GetNextBeatmapIdAsync()
    {
        var idSnapshot = await databaseRef.Child("NextBeatmapId").GetValueAsync();
        int currentId = idSnapshot.Exists ? int.Parse(idSnapshot.Value.ToString()) : -1;
        Debug.Log($"currentid: {currentId}");
        return currentId.ToString();
    }

    // ���ε� ���� �� ���� ID�� ������Ű�� �޼���
    private async Task IncrementBeatmapIdAsync(string currentId)
    {
        int nextId = int.Parse(currentId) + 1;
        await databaseRef.Child("NextBeatmapId").SetValueAsync(nextId);
        Debug.Log($"NextBeatmapId id���� {nextId}");
    }
}