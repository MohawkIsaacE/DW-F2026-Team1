using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

public class Player : MonoBehaviour
{

    [SerializeField] private GameObject Gun;
    [SerializeField] private GameObject GunPrefab;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private InputActionReference ShootAction;
    private GameObject Spinner;
    private float shotCooldown = 0.2f;
    private float shotTimer;
    private float rotateSpeed = 100f;
    private float angle;
    private float shotDistance = 10f;
    public bool hasGun = false;
    private LayerMask layerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shotTimer = shotCooldown;
        Spinner = GameObject.Find("Spinner");
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
        // Decide which way to spin based on player number
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player1)
        {
            angle -= rotateSpeed * Time.deltaTime;
        }
        else
        {
            angle += rotateSpeed * Time.deltaTime;
        }
        Spinner.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void PickupGun(GameObject gunReference)
    {
        hasGun = true;

        // Activate the spinner so the player can aim
        Spinner.SetActive(true);
        angle = 0f;

        // Hold the gun
        GunPrefab = gunReference;
        GunPrefab.transform.SetParent(transform);
    }

    void DropGun()
    {
        hasGun = false;

        // Deactivate the spinner when you lose the gun
        Spinner.SetActive(false);

        // Stop holding the gun
        GunPrefab.transform.SetParent(GameObject.Find("GunStorage").transform);

        // Throw the gun away
        GunPrefab.GetComponent<Gun>().Respawn();
    }
}
