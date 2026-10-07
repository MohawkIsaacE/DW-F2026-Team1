using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    bool isHeld;
    float respawnTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isHeld = false;
    }

    private void Update()
    {
        if (!isHeld)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0) gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Player>() != null 
            && !collision.gameObject.GetComponent<Player>().hasGun
            && !isHeld
            && respawnTimer <= 0)
        {
            isHeld = true;

            collision.gameObject.GetComponent<Player>().PickupGun(gameObject);

            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    public void Toss()
    {
        isHeld = false;
        respawnTimer = 2f;

        gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        rb.AddForce(new Vector2(Random.Range(-0.8f, 0.8f) * 10f, Random.Range(0.3f, 0.7f) * 10f), ForceMode2D.Impulse);
    }
}
