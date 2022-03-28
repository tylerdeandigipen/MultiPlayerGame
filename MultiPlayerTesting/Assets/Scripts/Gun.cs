using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
public class Gun : NetworkBehaviour
{
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
    [SerializeField]
    private TextMeshProUGUI ammoCounter;
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
            if (hit.collider.gameObject.layer == 6)
            {
                Debug.Log("hit");
                Instantiate(bloodSplatter, hit.point, Quaternion.LookRotation(hit.normal));
                EnemyHealth enemyHealthScript = hit.collider.gameObject.GetComponent<EnemyHealth>();
                enemyHealthScript.takeDamage(damage);
            }
            else 
            {
                Instantiate(hitEffects, hit.point, Quaternion.LookRotation(hit.normal));
                Instantiate(bulletDecal, hit.point + new Vector3(hit.normal.x * .01f, hit.normal.y * .01f, hit.normal.z * .01f), Quaternion.LookRotation(-hit.normal));
            }
        }        
    }
}
