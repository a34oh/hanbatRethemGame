using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseScene : MonoBehaviour
{
    public SceneType SceneType { get; protected set; } = SceneType.None;

    void Start()
    {
        Init();

    }
    protected virtual void Init()
    { }
}
