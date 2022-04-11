using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenTower : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("towerCleanup"))
        {
            this.gameObject.transform.parent.gameObject.SetActive(false);
        }
    }
}
