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
    float timer = 0f;
    [SerializeField]
    ParticleSystem hitEffects;
    Camera cam;

    private void Start()
    {
        cam = FindObjectOfType<Camera>();
    }
    void Update()
    {
        if (IsClient && IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && fireRate <= timer)
            {
                timer = 0f;
                Shoot();
            }
            else
                timer += Time.deltaTime;
        }
    }

    void Shoot()
    {
        RaycastHit hit;
        Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range, EnemyLayer);
        if (hit.collider != null)
        {
            Instantiate(hitEffects, hit.point, Quaternion.LookRotation(hit.normal));
            if (hit.collider.gameObject.layer == 6)
            {
                Debug.Log("hit");
                EnemyHealth enemyHealthScript = hit.collider.gameObject.GetComponent<EnemyHealth>();
                enemyHealthScript.takeDamage(damage);
            }
        }
    }
}
