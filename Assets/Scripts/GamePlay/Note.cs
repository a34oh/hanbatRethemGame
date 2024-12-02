using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed;
    public KeyCode keyToPress;
    public float spawnTime;
    public float targetTime;
    public float xPosition;
    public bool isHit = false;
    public GameObject hitEffectPrefab;

    void Start()
    {
        spawnTime = Time.time;
        speed = GamePlayManager.Instance.GetNoteSpeed();
    }

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
        float timeDifference = Time.time - targetTime;
        if (transform.position.y < -5f)
        {
            Destroy(gameObject);
        }
    }

}