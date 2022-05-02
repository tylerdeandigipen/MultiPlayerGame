using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
public class ShotGun : MonoBehaviour
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

    [Header("Bullet Stats")]
    [SerializeField]
    int maxAmmo;
    [SerializeField]
    int ammountOfPellets;
    [SerializeField]
    float maxBulletSpread;
    [SerializeField]
    float reloadSpeed = 2f;
    [SerializeField]
    float bulletSpeed = 1;
    [SerializeField]
    float damage = 10f;

    [Header("Range / Falloff")]
    [SerializeField]
    float range = 100f;
    [SerializeField]
    float dammageFalloffRange = 50;
    [SerializeField]
    private float falloffStrength = 2;

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

    [Header("Effects")]
    [SerializeField]
    ParticleSystem hitEffects;
    [SerializeField]
    GameObject bulletDecal;
    [SerializeField]
    GameObject bloodSplatter;
    [SerializeField]
    private TrailRenderer trailRenderer;
    [SerializeField]
    private GameObject bulletSpawnPoint;
    [SerializeField]
    private GameObject dammageNumber;


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
        for (int i = 0; i < ammountOfPellets; i++)
        {
            RaycastHit hit;
            TrailRenderer trail = Instantiate(trailRenderer, bulletSpawnPoint.transform.position, Quaternion.identity);
            Vector3 temp = new Vector3(cam.transform.forward.x + Random.Range(-maxBulletSpread, maxBulletSpread), cam.transform.forward.y + Random.Range(-maxBulletSpread, maxBulletSpread), cam.transform.forward.z + Random.Range(-maxBulletSpread, maxBulletSpread));
            Debug.Log("Forward: " + cam.transform.forward);
            Debug.Log("Temp: " + temp);
            Physics.Raycast(cam.transform.position, temp, out hit, range, IgnoreLayer);
            plMove.RecoilMath(recoilX, recoilY, timePressed, maxRecoilTime, xRecoilDir, yRecoilDir);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.layer == 6)
                {
                    StartCoroutine(SpawnTrail(trail, hit, new Vector3(0, 0, 0)));
                    EnemyHealth enemyHealthScript = hit.collider.gameObject.GetComponent<EnemyHealth>();
                    if (hit.distance < dammageFalloffRange)
                        currentDammage = damage;
                    else
                        currentDammage = Mathf.Round(damage * easeNumber(1 - (Mathf.Clamp(((hit.distance - dammageFalloffRange) / 15), 0f, 1f))));
                    enemyHealthScript.takeDamage(currentDammage);
                    spawnDammageNumber(hit);
                }
                else
                {
                    StartCoroutine(SpawnTrail(trail, hit, new Vector3 (0,0,0)));
                }
            }
            else
                StartCoroutine(SpawnTrail(trail, hit, temp));
        }

    }

    void spawnDammageNumber(RaycastHit Hit)
    {
        if (currentDammage >= 1)
        {
            GameObject number = Instantiate(dammageNumber, Hit.point, new Quaternion(0, 0, 0, 0));
            number.GetComponentInChildren<TextMeshProUGUI>().text = $"{currentDammage}";
        }
    }

    float easeNumber(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
    }
    public void ADS()
    {
        isADSing = true;
        oldSens = plMove.sensitivity;
        oldFOV = cam.fieldOfView;
        cam.fieldOfView = adsZoom;
    }
    public void unADS()
    {
        isADSing = false;
        plMove.sensitivity = oldSens;
        cam.fieldOfView = oldFOV;
    }
    private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit Hit, Vector3 direction)
    {
        float time = 0;
        Vector3 startPosition = trail.transform.position;
        Vector3 defaultPos = cam.transform.position + cam.transform.forward * range;
        float timeToTarget;
        if (Hit.collider != null)
            timeToTarget = Hit.distance / bulletSpeed;
        else
            timeToTarget = range / bulletSpeed;
        while (time < .5)
        {
            if (Hit.collider != null)
            {
                trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
                time += Time.deltaTime / trail.time;
            }
            else
            {
                trail.transform.position = Vector3.Lerp(startPosition, startPosition + (trail.transform.forward * 100), time);
                time += Time.deltaTime / trail.time;
            }
            yield return null;
        }
        if (Hit.collider == null)
        {
            Instantiate(hitEffects, defaultPos, new Quaternion(0, 0, 0, 0));
        }
        else if (Hit.collider.gameObject.layer == 6)
        {
            trail.transform.position = Hit.point;
            Instantiate(bloodSplatter, Hit.point, Quaternion.LookRotation(Hit.normal));
        }
        else
        {
            trail.transform.position = Hit.point;
            Instantiate(hitEffects, Hit.point, Quaternion.LookRotation(Hit.normal));
            Instantiate(bulletDecal, Hit.point + new Vector3(Hit.normal.x * .01f, Hit.normal.y * .01f, Hit.normal.z * .01f), Quaternion.LookRotation(-Hit.normal));
        }
        Destroy(trail.gameObject, trail.time);
    }
}
