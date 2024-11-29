using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ScoreCalc : MonoBehaviour
{
    
    
    double final_score;
    public List<GameObject> rank_count;
    public List<GameObject> record_image;
    // Start is called before the first frame update
    void Start()
    {
        final_score = Score(1331,200,0,0,0,1531);
        Rank(final_score);
    }

    // Update is called once per frame

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
        PlayerResult player_result = new PlayerResult(player_score,"id");
        
        Debug.Log(player_score);
        return player_score;
    }

    
    
    void Rank(double score)
    {
        switch (score)
        {
            case var _ when score > 900000:
                record_image[0].SetActive(true);
                Debug.Log("Rank: SSS");
                break;
            case var _ when score > 800000:
                Debug.Log("Rank: SS");
                break;
            case var _ when score > 700000:
                Debug.Log("Rank: S");
                break;
            case var _ when score > 600000:
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
