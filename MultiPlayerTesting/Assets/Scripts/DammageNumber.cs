using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DammageNumber : MonoBehaviour
{
    float timer = 0f;
    [SerializeField]
    float timetodestroy = 0f;
    [SerializeField]
    float sizeOfNumber = .5f;
    [SerializeField]
    float moveStrength = 2f;
    GameObject camera_;
    private void Start()
    {
        camera_ = FindObjectOfType<Camera>().gameObject;
    }
    void Update()
    {
        //make so that it adds the cameras down vector * (how far down * scalar of how far camera is)

        //make so the size is controlled by a function and not liniarly 

        float size = (Camera.main.transform.position - transform.position).magnitude;
        size = size / sizeOfNumber;
        transform.localScale = new Vector3(size, size, size);
        this.transform.LookAt(camera_.transform);
        if (timer > timetodestroy)
            Destroy(this.gameObject);
        else
            timer += Time.deltaTime;
        this.transform.Translate(new Vector3((easeNumber(timer) * moveStrength) * Time.deltaTime, 0, 0));

    }

    float easeNumber(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3);
    }
}
