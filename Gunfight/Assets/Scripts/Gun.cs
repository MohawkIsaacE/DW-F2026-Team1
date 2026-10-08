using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour
{
    bool isHeld;
    float respawnTimer;
    float angle;
    public Vector3 gunSpriteOffset = new Vector3(0.9f, 0.2f, 0f); 

    [SerializeField] private Transform SpawnPointParent;
    private Transform[] SpawnPoints;

    private void Start()
    {
        isHeld = false;
        respawnTimer = 0f;

        // Set Spawn Points
        SpawnPoints = new Transform[SpawnPointParent.childCount];
        for (int i = 0; i < SpawnPoints.Length; i++)
        {
            SpawnPoints[i] = SpawnPointParent.GetChild(i);
        }

    }

    private void Update()
    {
        if (!isHeld)
        {
            respawnTimer -= Time.deltaTime;
            // Spin when thrown
            angle += Time.deltaTime * 70f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (isHeld)
        {
            transform.position = transform.parent.position;
            //transform.rotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.GetComponent<Player>() != null 
            && !collider.gameObject.GetComponent<Player>().hasGun
            && !isHeld
            && respawnTimer <= 0)
        {
            isHeld = true;
            transform.Find("Gun Sprite").transform.localPosition = gunSpriteOffset;


            // Hold the gun in spinning direction
            Debug.Log(transform.parent.name);
            Debug.Log(transform.parent.rotation);



            collider.gameObject.GetComponent<Player>().PickupGun(gameObject);
            Debug.Log(transform.parent.name);
            Debug.Log(transform.parent.rotation);

            transform.rotation = transform.parent.rotation * Quaternion.Euler(0, 0, 90f);


        }
    }

    public void Respawn()
    {
        isHeld = false;
        respawnTimer = 2f;
        angle = 0;

        int randomSpawn = (int)Random.Range(0, SpawnPoints.Length);
        transform.rotation = Quaternion.identity;
        transform.position = SpawnPoints[randomSpawn].transform.position;

    }
}
