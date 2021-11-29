using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] int lives = 1;
    [SerializeField] private float bulletPerSec = .3f; //cantidad máxima de balas q dispara el enemigo por segundo
    [SerializeField] Transform barrel; //obtengo la posicion de la punta del arma
    [SerializeField] GameObject ammo; //para conseguir el prefab de la bala
    [SerializeField] float shotSpeed = 1500f;
    private GameObject PlayerObject;
    [SerializeField] private float enemyRotationSpeed = 1f;
    private Vector3 direction;
    [SerializeField] private float enemyVision = 13f; // Rango de vision (esférico)
    private float distance; //Distancia entre jugador y enemigo
    float timeBetwShots;
    float timeLapse;
    // Start is called before the first frame update
    void Start()
    {
        timeBetwShots = 1 / bulletPerSec;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(PlayerObject.transform.position, transform.position);
        CheckLives(); // chequear si está vivo o muerto
        CheckVision(); // chequear si está el jugador en rango de visión
    }


    private void CheckVision()
    {
        if(distance <= enemyVision)
        {
            LookAt(); // mirar al objetivo
            timeLapse += Time.fixedDeltaTime; // time lapse se usa para contar el tiempo entre disparos
            if (timeLapse >= timeBetwShots)
            {
                FireWeapon(); // disparar
                timeLapse = 0; // cuando dispara, resetear el timeLapse
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
        //lerp para rotacion suave y mas "realista"
    }


    void FireWeapon()
    {
            GameObject newAmmo;

            newAmmo = Instantiate(ammo, barrel.position, barrel.rotation); // creo una bala

            newAmmo.GetComponent<Rigidbody>().AddForce(barrel.forward * shotSpeed); // le doy fuerza para que actue como tal
            
            Destroy(newAmmo, 4f); // destruir tras 4 segundos, si no choca con nada
    }

    public void MinusLives()
    {
        lives--;
    }


}
