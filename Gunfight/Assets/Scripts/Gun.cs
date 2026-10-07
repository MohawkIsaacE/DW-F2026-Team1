using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour
{
    //[SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform SpawnPointParent;
    private Transform[] SpawnPoints;
    bool isHeld;
    float respawnTimer;
    float angle;

    private void Start()
    {
        //rb = GetComponent<Rigidbody2D>();
        isHeld = false;
        respawnTimer = 0f;
        SpawnPoints = new Transform[SpawnPointParent.childCount];

        // Assign all spawn points
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
            transform.rotation = Quaternion.identity;
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

            collider.gameObject.GetComponent<Player>().PickupGun(gameObject);
        }
        else
        {
            Debug.Log("Not a player");
        }
    }


    public void Respawn()
    {
        isHeld = false;
        respawnTimer = 2f;
        angle = 0;

        int randomSpawn = (int)Random.Range(0, SpawnPoints.Length);
        transform.position = SpawnPoints[randomSpawn].position;
    }
}
