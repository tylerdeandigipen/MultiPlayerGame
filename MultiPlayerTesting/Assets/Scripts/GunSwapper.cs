using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSwapper : MonoBehaviour
{
    [SerializeField]
    public GameObject[] guns;
    int WeaponNumber = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            WeaponNumber += Mathf.FloorToInt(Input.GetAxis("Mouse ScrollWheel") * 10);
            WeaponNumber = Mathf.Clamp(WeaponNumber, 0, guns.Length - 1);
            UpdateWeapon();
        }
    }

    void UpdateWeapon()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            if (i != WeaponNumber)
            {
                guns[i].gameObject.SetActive(false);
            }
            else
                guns[i].gameObject.SetActive(true);
        }
    }
}
