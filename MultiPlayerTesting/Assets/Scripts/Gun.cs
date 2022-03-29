using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
public class Gun : NetworkBehaviour
{
    [SerializeField]
    bool isAuto = true;
    [SerializeField]
    LayerMask EnemyLayer;
    [SerializeField]
    int maxAmmo;
    [SerializeField]
    float reloadSpeed = 2f;
    [SerializeField]
    float damage = 10f;
    [SerializeField]
    float range = 100f;
    [SerializeField]
    float fireRate = 3f;
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
    [SerializeField]
    ParticleSystem hitEffects;
    [SerializeField]
    GameObject bulletDecal;
    [SerializeField]
    GameObject bloodSplatter;
    private TextMeshProUGUI ammoCounter;
    [SerializeField]
    private TrailRenderer trailRenderer;
    [SerializeField]
    private GameObject bulletSpawnPoint;
    Camera cam;
    PlayerMovement plMove;
    float timePressed;
    int currentAmmo;
    float timer2 = 0f;
    bool isReloading;
    private void Start()
    {
        currentAmmo = maxAmmo;
        plMove = this.GetComponent<PlayerMovement>();
        cam = FindObjectOfType<Camera>();
        ammoCounter = GameObject.Find("AmmoCounter").GetComponent<TextMeshProUGUI>();
        ammoCounter.gameObject.SetActive(true);
    }
    void Update()
    {
        if (IsClient && IsOwner)
        {
            ammoCounter.text = $"{currentAmmo}/{maxAmmo}";
            if (isAuto == true)
            {
                if (Input.GetKey(KeyCode.Mouse0) && fireRate <= timer && currentAmmo != 0)
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
                if (Input.GetKeyDown(KeyCode.Mouse0) && fireRate <= timer && currentAmmo != 0)
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
        RaycastHit hit;
        Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range, EnemyLayer);
        plMove.RecoilMath(recoilX, recoilY, timePressed, maxRecoilTime, xRecoilDir, yRecoilDir);        
        if (hit.collider != null)
        {
            TrailRenderer trail = Instantiate(trailRenderer, bulletSpawnPoint.transform.position, Quaternion.identity);
            if (hit.collider.gameObject.layer == 6)
            {
                StartCoroutine(SpawnTrail(trail, hit));
                EnemyHealth enemyHealthScript = hit.collider.gameObject.GetComponent<EnemyHealth>();
                enemyHealthScript.takeDamage(damage);
            }
            else 
            {
                StartCoroutine(SpawnTrail(trail, hit));
                //Instantiate(hitEffects, hit.point, Quaternion.LookRotation(hit.normal));
                //Instantiate(bulletDecal, hit.point + new Vector3(hit.normal.x * .01f, hit.normal.y * .01f, hit.normal.z * .01f), Quaternion.LookRotation(-hit.normal));
            }
        }        
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit Hit)
    {
        float time = 0;
        Vector3 startPosition = trail.transform.position;

        while (time < .5)
        {
            trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }
        trail.transform.position = Hit.point;
        if (Hit.collider.gameObject.layer == 6)
        {
            Instantiate(bloodSplatter, Hit.point, Quaternion.LookRotation(Hit.normal));
        }
        else
        {
            Instantiate(hitEffects, Hit.point, Quaternion.LookRotation(Hit.normal));
            Instantiate(bulletDecal, Hit.point + new Vector3(Hit.normal.x * .01f, Hit.normal.y * .01f, Hit.normal.z * .01f), Quaternion.LookRotation(-Hit.normal));
        }

        Destroy(trail.gameObject, trail.time);
    }
}
