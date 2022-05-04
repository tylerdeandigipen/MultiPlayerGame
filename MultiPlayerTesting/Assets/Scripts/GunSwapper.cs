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
            int previousWeaponNumber = WeaponNumber;
            WeaponNumber += Mathf.FloorToInt(Input.GetAxis("Mouse ScrollWheel") * 10);            
            WeaponNumber = Mathf.Clamp(WeaponNumber, 0, guns.Length - 1);
            if(previousWeaponNumber != WeaponNumber)
                UpdateWeapon();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            WeaponNumber = 0;
            UpdateWeapon();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            WeaponNumber = 1;
            UpdateWeapon();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            WeaponNumber = 2;
            UpdateWeapon();
        }
    }

    void UpdateWeapon()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            if (i != WeaponNumber)
            {

                Gun gunscript = guns[i].gameObject.GetComponent<Gun>();
                if (gunscript != null)
                {
                    gunscript.unADS();
                    guns[i].gameObject.SetActive(false);
                }
                else
                {
                    guns[i].gameObject.GetComponent<ShotGun>().unADS();
                    guns[i].gameObject.SetActive(false);
                }
            }
            else
                guns[i].gameObject.SetActive(true);
        }
    }
}
