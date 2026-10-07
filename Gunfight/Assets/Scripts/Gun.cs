using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    bool isHeld;
    float respawnTimer;
    float angle;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isHeld = false;
        respawnTimer = 0f;
    }

    private void Update()
    {
        if (!isHeld)
        {
            respawnTimer -= Time.deltaTime;
            // Spin when thrown
            angle += rb.linearVelocityX * Time.deltaTime * 70f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (isHeld)
        {
            transform.position = transform.parent.position;
            transform.rotation = Quaternion.identity;
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
        }
    }

    public void Toss()
    {
        isHeld = false;
        respawnTimer = 2f;
        angle = 0;

        transform.position = new Vector2(transform.position.x, transform.position.y + 2);
        rb.linearVelocityY = 0f;
        int randomAngle = (int)Random.Range(-1, 2);
        rb.AddForce(new Vector2(randomAngle * 5f, 1 * 5f), ForceMode2D.Impulse);
    }
}
