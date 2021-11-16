using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int lives = 1;
    [SerializeField] Transform barrel;
    [SerializeField] private float bulletPerSec = .3f;
    [SerializeField] GameObject ammo;
    [SerializeField] float shotSpeed = 1500f;
    private GameObject PlayerObject;
    [SerializeField] private float enemyRotationSpeed = 1f;
    private Vector3 direction;
    [SerializeField] private float enemyVision = 13f;
    private float distance;
    float timeBetwShots = 1/0.3f;
    float timeLapse;
    // Start is called before the first frame update
    void Start()
    {
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(PlayerObject.transform.position, transform.position);
        CheckLives();
        CheckVision();
    }


    private void CheckVision()
    {
        if(distance <= enemyVision)
        {
            LookAt();
            timeLapse += Time.fixedDeltaTime;
            if (timeLapse >= timeBetwShots)
            {
                FireWeapon();
                timeLapse = 0;
            }

        }
    }

    private void CheckLives()
    {
        if(lives <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, enemyVision);
    }

    private void LookAt()
    {
        direction = (PlayerObject.transform.position - transform.position);
        Quaternion newRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, enemyRotationSpeed * Time.deltaTime);
    }


    void FireWeapon()
    {
        float timeBetwShots = 1 / bulletPerSec;

            GameObject newAmmo;

            newAmmo = Instantiate(ammo, barrel.position, barrel.rotation);

            newAmmo.GetComponent<Rigidbody>().AddForce(barrel.forward * shotSpeed);
            
            Destroy(newAmmo, 4f);
    }


}
