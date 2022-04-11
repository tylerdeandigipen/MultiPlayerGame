using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTower : MonoBehaviour
{
    public EnemyHealth towerHealth;
    [SerializeField]
    GameObject Tower;
    [SerializeField]
    GameObject groundedTower;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (towerHealth.currentHealth < 0)
        {
            groundedTower.SetActive(true);
            Tower.SetActive(false);
            Destroy(this);
        }
    }
}
