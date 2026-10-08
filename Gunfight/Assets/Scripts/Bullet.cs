using UnityEngine;

public class Bullet : MonoBehaviour
{
    float lifeTimer = 2;
    float speed = 3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0) Destroy(gameObject);
    }
}
