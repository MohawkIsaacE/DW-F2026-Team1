using UnityEngine;

public class Triangle : MonoBehaviour
{
    public GameObject triangle;
    float angle;

    public GameObject bullet;
    float bulletSpawnTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bulletSpawnTimer = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        angle += Time.deltaTime * 60;
        triangle.transform.rotation = Quaternion.Euler(0, 0, angle);

        bulletSpawnTimer -= Time.deltaTime;
        if (bulletSpawnTimer <= 0)
        {
            bulletSpawnTimer = 1f;
            Instantiate(bullet, transform.position, Quaternion.Euler(0, 0, angle));
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up);

    }
}
