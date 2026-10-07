using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

public class Player : MonoBehaviour
{

    [SerializeField] private GameObject Spinner;
    [SerializeField] private GameObject AimLine;
    [SerializeField] private InputActionReference ShootAction;
    [SerializeField] private GameObject GunPrefab;

    private float shotCooldown = 0.2f;
    private float shotTimer;
    private float angle;
    private float shotDistance = 100f;

    public bool hasGun;

    private PlayerMovement player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shotTimer = shotCooldown;
        player = GetComponent<PlayerMovement>();
        hasGun = false;
        Spinner.SetActive(false);
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
        if (hasGun && ShootAction.action.WasPressedThisFrame() && shotTimer <= 0f)
        {
            Shoot();
            shotTimer = shotCooldown;
        }

        if (hasGun) RotateGun();
    }

    void Shoot()
    {
        // Shoot a raycast and check if it hit their head
        RaycastHit2D hit = Physics2D.Raycast(AimLine.transform.position, Spinner.transform.up, shotDistance);
        
        // Early exit if the player misses a shot
        //if (hit.collider.gameObject.GetComponent<PlayerMovement>() == null || hit == false) return;

        if (hit != false && hit.collider.gameObject.name == "Head")
        {
            Destroy(hit.collider.gameObject);
        }

        // Throw the gun after you shoot
        DropGun();
    }

    void RotateGun()
    {
        // Decide which way to spin based on player number
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player1)
        {
            angle -= 60f * Time.deltaTime;
        }
        else
        {
            angle += 60f * Time.deltaTime;
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
        GunPrefab.GetComponent<Gun>().Toss();
    }
}
