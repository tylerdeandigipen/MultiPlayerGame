using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAtCamera : MonoBehaviour
{
    GameObject camera_;
    // Start is called before the first frame update
    void Start()
    {
        camera_ = FindObjectOfType<Camera>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(camera_.transform);
    }
}
