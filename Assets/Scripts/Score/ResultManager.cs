using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

[System.Serializable]
public class PlayerResult
{
    public double playerScore;
    public string playerName;
    public DateTime playTime;

    public PlayerResult(double score, string name)
    {
        playerScore = score;
        playerName = name;
        playTime = DateTime.Now;
    }
}

public class ResultManager : MonoBehaviour
{
    private string resultsDirectory = "Results";

    // 결과를 로컬에 저장 (최고점수가 아닌 모든 기록 저장)
    public void SaveResultToLocal(double score, string playerName, string beatmapId)
    {
        string beatmapDirectory = Path.Combine(resultsDirectory, beatmapId);
        if (!Directory.Exists(beatmapDirectory))
        {
            Directory.CreateDirectory(beatmapDirectory);
        }

        // 결과 객체 생성
        PlayerResult result = new PlayerResult(score, playerName);

        // 결과를 JSON으로 직렬화
        string jsonResult = JsonUtility.ToJson(result);

        // 파일 경로 생성 (결과 파일 이름은 타임스탬프나 고유 ID로 설정)
        string resultFilePath = Path.Combine(beatmapDirectory, $"{DateTime.Now.Ticks}.json");

        // 결과 파일로 저장
        File.WriteAllText(resultFilePath, jsonResult);
    }

    // 로컬에 저장된 최고 기록을 불러오는 함수
    public PlayerResult GetBestResult(string beatmapId)
    {
        string beatmapDirectory = Path.Combine(resultsDirectory, beatmapId);

        if (Directory.Exists(beatmapDirectory))
        {
            // 결과 파일들을 읽고 최고 점수 찾기
            var resultFiles = Directory.GetFiles(beatmapDirectory, "*.json");
            PlayerResult bestResult = null;

            foreach (var file in resultFiles)
            {
                string json = File.ReadAllText(file);
                PlayerResult result = JsonUtility.FromJson<PlayerResult>(json);

                if (bestResult == null || result.playerScore > bestResult.playerScore)
                {
                    bestResult = result;
                }
            }

            return bestResult;
        }

        return null;
    }

    // 결과 파일을 암호화해서 저장하는 함수 (위변조 방지용)
    
}
