using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyAfterTime : MonoBehaviour
{
    float timer = 0f;
    [SerializeField]
    float timetodestroy = 0f;
    void Update()
    {
        if (timer > timetodestroy)
            Destroy(this.gameObject);
        else
            timer += Time.deltaTime;
    }
}
