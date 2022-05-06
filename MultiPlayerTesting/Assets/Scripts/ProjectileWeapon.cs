using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
public class ProjectileWeapon : MonoBehaviour
{
    [Header("General Stats")]
    [SerializeField]
    bool isAuto = true;
    [SerializeField]
    LayerMask IgnoreLayer;
    [SerializeField]
    float fireRate = 3f;
    [SerializeField]
    float adsZoom = 30;
    public float adsSensitivity;

    [Header("Bullet Stats")]
    [SerializeField]
    int maxAmmo;
    [SerializeField]
    float reloadSpeed = 2f;
    [SerializeField]
    float projectileSpeed = 10f;
    [SerializeField]
    float damage = 10f;
    [SerializeField]
    private GameObject projectileSpawnPoint;
    [SerializeField]
    GameObject projectile;

    [Header("Recoil")]
    [SerializeField]
    float maxRecoilTime;
    [SerializeField]
    float recoilY;
    [SerializeField]
    float recoilX;
    [SerializeField]
    float yRecoilDir = 1;
    [SerializeField]
    float xRecoilDir = -1;



    float timer = 0f;
    float oldFOV;
    float oldSens;
    bool isADSing = false;
    private TextMeshProUGUI ammoCounter;
    Camera cam;
    RBPlayerMovement plMove;
    float timePressed;
    int currentAmmo;
    float timer2 = 0f;
    bool isReloading;
    float currentDammage = 0;
    private void Start()
    {
        currentAmmo = maxAmmo;
        plMove = GetComponentInParent<RBPlayerMovement>();
        cam = FindObjectOfType<Camera>();
        ammoCounter = GameObject.Find("AmmoCounter").GetComponent<TextMeshProUGUI>();
        ammoCounter.gameObject.SetActive(true);
        oldFOV = cam.fieldOfView;
        oldSens = plMove.sensitivity;
    }
    void Update()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            ammoCounter.text = $"{currentAmmo}/{maxAmmo}";
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                ADS();
            }
            if (Input.GetKeyUp(KeyCode.Mouse1) && isADSing)
            {
                unADS();
            }
            if (isAuto == true)
            {
                if (Input.GetKey(KeyCode.Mouse0) && fireRate <= timer && currentAmmo != 0 && isReloading == false)
                {
                    timer = 0f;
                    timePressed += Time.deltaTime;
                    timePressed = timePressed >= maxRecoilTime ? maxRecoilTime : timePressed;
                    Shoot();
                }
                else
                {
                    timer += Time.deltaTime;
                    timePressed = 0;
                }
            }
            else if (isAuto == false)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && fireRate <= timer && currentAmmo != 0 && isReloading == false)
                {
                    timer = 0f;
                    timePressed += Time.deltaTime;
                    timePressed = timePressed >= maxRecoilTime ? maxRecoilTime : timePressed;
                    Shoot();
                }
                else
                {
                    timer += Time.deltaTime;
                    timePressed = 0;
                }
            }
            if (Input.GetKeyDown(KeyCode.R) && currentAmmo != maxAmmo && isReloading != true)
            {
                isReloading = true;
            }
            if (isReloading == true)
            {
                if (timer2 >= reloadSpeed)
                {
                    currentAmmo = maxAmmo;
                    timer2 = 0;
                    isReloading = false;
                }
                else
                    timer2 += Time.deltaTime;
            }
        }

    }

    void Shoot()
    {
        currentAmmo -= 1;
        Instantiate(projectile, projectileSpawnPoint.transform.position, Quaternion.LookRotation(cam.transform.forward));
    }

    public void ADS()
    {
        isADSing = true;
        oldSens = plMove.sensitivity;
        plMove.sensitivity = adsSensitivity;
        oldFOV = cam.fieldOfView;
        cam.fieldOfView = adsZoom;
    }
    public void unADS()
    {
        isADSing = false;
        plMove.sensitivity = oldSens;
        cam.fieldOfView = oldFOV;
    }   
}