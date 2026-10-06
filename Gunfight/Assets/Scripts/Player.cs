using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

public class Player : MonoBehaviour
{

    [SerializeField] private GameObject Gun;
    [SerializeField] private GameObject AimLine;
    [SerializeField] private InputActionReference ShootAction;
    private float shotCooldown = 0.2f;
    private float shotTimer;
    private float angle;
    private float shotDistance = 100f;
    private LayerMask layerMask;

    private PlayerMovement player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shotTimer = shotCooldown;
        player = GetComponent<PlayerMovement>();
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
        if (ShootAction.action.WasPressedThisFrame() && shotTimer <= 0f)
        {
            Shoot();
            shotTimer = shotCooldown;
        }

        RotateGun();
    }

    void Shoot()
    {
        // Shoot a raycast and check if it hit their head
        RaycastHit2D hit = Physics2D.Raycast(AimLine.transform.position, Gun.transform.up, shotDistance);

        // Check if the hit target is a player (kill), if not then exit
        if (hit == false) return;
        if (hit.collider.gameObject.GetComponent<PlayerMovement>() == null) return;
        if (hit.collider.gameObject.GetComponent<PlayerMovement>().GetPlayerId() != player.GetPlayerId())
        {
            Destroy(hit.collider.gameObject);
        }
    }

    void RotateGun()
    {
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player1)
        {
            angle -= 60f * Time.deltaTime;
        }
        else
        {
            angle += 60f * Time.deltaTime;
        }
        Gun.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
