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
    GameObject camera_;
    private void Start()
    {
        camera_ = FindObjectOfType<Camera>().gameObject;
    }
    void Update()
    {
        //make so that it adds the cameras down vector * (how far down * scalar of how far camera is)

        //make so the size is controlled by a function and not liniarly 
        this.transform.position = new Vector3 ((this.transform.position.x + (easeNumber(timer) * 2)), this.transform.position.y, this.transform.position.z) ;

        float size = (Camera.main.transform.position - transform.position).magnitude;
        size = size / sizeOfNumber;
        transform.localScale = new Vector3(size, size, size);
        this.transform.LookAt(camera_.transform);
        if (timer > timetodestroy)
            Destroy(this.gameObject);
        else
            timer += Time.deltaTime;
    }

    float easeNumber(float x)
    {
        return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
    }
}
