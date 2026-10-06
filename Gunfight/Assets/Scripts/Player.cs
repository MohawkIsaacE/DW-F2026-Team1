using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] private GameObject Gun;
    [SerializeField] private InputActionReference ShootAction;
    private float shotCooldown = 0.2f;
    private float shotTimer;
    private float angle;
    private float shotDistance = 10f;
    private LayerMask layerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shotTimer = shotCooldown;
    }

    // Update is called once per frame
    void Update()
    {

        // Reduce coolddown on shot
        if (shotTimer > 0)
        {
            shotTimer -= Time.deltaTime;
        }
        // Check if shooting can happen
        if (ShootAction.action.IsPressed() && shotTimer <= 0f)
        {
            Shoot();
            shotTimer = shotCooldown;
        }

        RotateGun();
    }

    void Shoot()
    {

        // Calculate shoot direction from angle

        // Shoot a raycast and check if it hit their head
        RaycastHit2D hit = Physics2D.Raycast(Gun.transform.position, Gun.transform.up, shotDistance);
        Debug.Log(hit.collider.gameObject.name);

    }

    void RotateGun()
    {
        angle += 60f * Time.deltaTime;
        Gun.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
