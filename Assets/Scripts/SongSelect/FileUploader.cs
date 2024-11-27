using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System;

// ���� ���� �������̽�
public interface IFileBrowser
{
    Task<string> OpenFilePanelAsync(string title, string fileTypes);
}

// PC�� ���� ���ñ� ����
public class PCFileBrowser : IFileBrowser
{
    public Task<string> OpenFilePanelAsync(string title, string fileTypes)
    {
        // �����ϴ� ���� Ȯ���� ���� ����
        string[] extensions = fileTypes.Split(',');
        string filters = string.Join(",", extensions);
        string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters(title, "", new string[] { "Files", filters });

        return Task.FromResult(string.IsNullOrEmpty(path) ? null : path);
    }
}



// ���� ���ε� Ŭ����
public class FileUploader
{
    private IFileBrowser fileBrowser;

    public FileUploader(IFileBrowser fileBrowser)
    {
        this.fileBrowser = fileBrowser;
    }

    // ���� ���� ���ε�
    public async Task<string> UploadMusicFileAsync()
    {
        try
        {
            // mp3 ���� ����
            string path = await fileBrowser.OpenFilePanelAsync("Select MP3 file", "mp3");

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                Debug.Log("���� ������ ���������� ���õǾ����ϴ�: " + path);
                return path;
            }
            else
            {
                Debug.Log("���� ������ �������� �ʾҰų� ������ �������� �ʽ��ϴ�.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("���� ���� ���ε� �� ���� �߻�: " + ex.Message);
            return null;
        }
    }

    // �̹��� ���� ���ε� (PNG, JPG ����)
    public async Task<string> UploadImageFileAsync()
    {
        try
        {
            // �̹��� ���� ����
            string path = await fileBrowser.OpenFilePanelAsync("Select Image file", "png,jpg,jpeg");

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                Debug.Log("�̹��� ������ ���������� ���õǾ����ϴ�: " + path);
                return path;
            }
            else
            {
                Debug.Log("�̹��� ������ �������� �ʾҰų� ������ �������� �ʽ��ϴ�.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("�̹��� ���� ���ε� �� ���� �߻�: " + ex.Message);
            return null;
        }
    }
}
