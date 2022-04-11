using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerCleanup : MonoBehaviour
{
    [SerializeField]
    float waveSpeed = 1;
    [SerializeField]
    float finalSize = 1;
    public bool cleanTowers = false;
    Vector3 waveSpawn;
    float timer;
    // Start is called before the first frame update
    void Start()
    {
        waveSpawn = this.gameObject.transform.position;
        this.GetComponent<MeshCollider>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (cleanTowers == true)
        {
            this.GetComponent<MeshCollider>().enabled = true;
            cleanBrokenTowers();
        }
    }

    private void cleanBrokenTowers()
    {
        this.gameObject.transform.position = waveSpawn;
        timer += Time.deltaTime * waveSpeed;
        this.transform.localScale = new Vector3(timer, this.transform.localScale.y, timer);
        if (this.transform.localScale.x >= finalSize)
        {
            cleanTowers = false;
            this.transform.localScale = new Vector3(1, this.transform.localScale.y, 1);
            this.transform.position = new Vector3(this.transform.position.x, -100, this.transform.position.x);
            this.GetComponent<MeshCollider>().enabled = false;
            timer = 0;
        }
        else
        {
            this.GetComponent<MeshCollider>().enabled = true;
        }
    }
}
