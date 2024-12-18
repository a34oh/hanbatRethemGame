using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
[Serializable]
public class PlayerResult
{
    public double playerScore;
    public string playerId;
    public string playTime; // DateTime을 string으로 변경

    public PlayerResult(double score, string name)
    {
        playerScore = score;
        playerId = name;
        playTime = DateTime.Now.ToString("o"); // ISO 8601 형식으로 저장
    }
}
public class ScoreCalc : MonoBehaviour
{
    FBManager fBManager;
    double final_score;
    public List<GameObject> rank_count;
    public List<GameObject> record_image;
    float touch_able_count = 5.0f;
    
    // Start is called before the first frame update
    async void Start()
    {
        fBManager = new FBManager();
        double finalScore = Score(PlayerPrefs.GetInt("PerfectCount"),
                                  PlayerPrefs.GetInt("GreatCount"),
                                  PlayerPrefs.GetInt("GoodCount"),
                                  PlayerPrefs.GetInt("BadCount"),
                                  PlayerPrefs.GetInt("MissCount"),
                                  PlayerPrefs.GetInt("MaxCombo"));
        string beatmapid = PlayerPrefs.GetString("BeatmapId");
        Rank(finalScore);
        PlayerResult playerResult = new PlayerResult(finalScore, GameManager.FBManager.newUser.UserId);
        await GameManager.FBManager.SaveResultAsync(playerResult,"beatmapid");
        // // 결과를 로컬에 저장
        // ResultManager resultManager = new ResultManager();
        // resultManager.SaveResultToLocal(finalScore, "id", "beatmapid");
        // resultManager.UploadResultToFirebase(finalScore, "id", "beatmapid");

        // // 서버에 연결된 경우 최고 기록만 업로드
        // // ServerManager serverManager = new ServerManager();
        // // serverManager.UploadBestResultToServer("id", "beatmapId");
    }

    // Update is called once per frame
    void Update()
    {
        touch_able_count -= Time.deltaTime;
        if(touch_able_count < 0 && Input.anyKeyDown)
        {
            SceneManager.LoadScene("SongSelectScene");
        }
    }
    double Score(float perfect_count, float  cool_count, float good_count,float bad_count, float miss_count , float max_combo)
    {
        List <float> count = new List <float>{perfect_count,cool_count,good_count,bad_count,miss_count};
        float total = count.Sum();
        float perfect_score = (900000/total) * perfect_count;
        float cool_score = (600000/total)* cool_count;
        float great_score = (300000/total)*good_count;
        float bad_score = (100000/total) * bad_count;
        
        float score = (perfect_score + cool_score + great_score + bad_score) + (max_combo / total)*100000;
        for(int i =0;i < count.Count;i++)
        {
            TextMeshProUGUI temp =  rank_count[i].GetComponent<TextMeshProUGUI>();
            temp.text = $"{count[i]}";
        }
        Double player_score =  Math.Round(score,1);
        return player_score;
    }
    void Rank(double score)
    {
        switch (score)
        {
            case var _ when score > 950000:
                record_image[0].SetActive(true);
                Debug.Log("Rank: SSS");
                break;
            case var _ when score > 900000:
                record_image[1].SetActive(true);
                Debug.Log("Rank: SS");
                break;
            case var _ when score > 850000:
                Debug.Log("Rank: S");
                break;
            case var _ when score > 700000:
                Debug.Log("Rank: A");
                break;
            case var _ when score > 500000:
                Debug.Log("Rank: B");
                break;
            default:
                Debug.Log("Rank: C");
                break;
        }
    }
}
