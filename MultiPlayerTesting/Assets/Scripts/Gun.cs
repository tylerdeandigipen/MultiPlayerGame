using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class Gun : NetworkBehaviour
{
    [SerializeField]
    LayerMask EnemyLayer;
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
    Camera cam;
    PlayerMovement plMove;
    float timePressed;

    private void Start()
    {
        plMove = this.GetComponent<PlayerMovement>();
        cam = FindObjectOfType<Camera>();
    }
    void Update()
    {
        if (IsClient && IsOwner)
        {
            if (Input.GetKey(KeyCode.Mouse0) && fireRate <= timer)
            {
                timer = 0f;
                timePressed += Time.deltaTime;
                timePressed = timePressed >= maxRecoilTime ? maxRecoilTime : timePressed;
                Shoot();
            }
            else                
                timer += Time.deltaTime;
        }
        else
            timePressed = 0;
    }

    void Shoot()
    {
        RaycastHit hit;
        Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range, EnemyLayer);
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
        plMove.RecoilMath(recoilX, recoilY, timePressed, maxRecoilTime, xRecoilDir, yRecoilDir);
    }
}
