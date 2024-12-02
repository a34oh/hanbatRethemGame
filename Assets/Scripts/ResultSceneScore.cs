using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultSceneScore : MonoBehaviour
{
    void Start()
    {
        int perfectCount = PlayerPrefs.GetInt("PerfectCount");
        int greatCount = PlayerPrefs.GetInt("GreatCount");
        int goodCount = PlayerPrefs.GetInt("GoodCount");
        int badCount = PlayerPrefs.GetInt("BadCount");
        int missCount = PlayerPrefs.GetInt("MissCount");
        int maxCombo = PlayerPrefs.GetInt("MaxCombo");
    }
}
