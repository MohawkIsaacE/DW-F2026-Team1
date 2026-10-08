using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using System;

public class Player : MonoBehaviour
{

    [SerializeField] private GameObject Spinner;
    [SerializeField] private InputActionReference ShootAction;
    [SerializeField] private GameObject GunPrefab;
    [SerializeField] private SpriteRenderer GunSprite;
    [SerializeField] private Transform body;
    private float shotCooldown = 0.2f;
    private float shotTimer;
    private float angle;
    private float shotDistance = 100f;
    private float rotateSpeed = 175f;
    private LayerMask layerMask;
    public bool hasGun;

    private PlayerMovement player;
    private Gun GunScript;

    [SerializeField] private Collider2D Player1Collider;
    [SerializeField] private Collider2D Player2Collider;

    public bool flipped_facing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shotTimer = shotCooldown;
        player = GetComponent<PlayerMovement>();
        hasGun = false;
        Spinner.SetActive(false);
        Physics2D.IgnoreCollision(Player1Collider, Player2Collider);
        GunScript = GunPrefab.GetComponent<Gun>();

        // Layer mask
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player1)
        {
            layerMask = LayerMask.GetMask("Ignore Raycast", "Player1Head");
        }
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player2)
        {
            layerMask = LayerMask.GetMask("Ignore Raycast", "Player2Head");
        }
            

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
        UpdateFacing();
    }

    void UpdateFacing()
    {
        if (body == null)
        {
            body = transform.Find("Body");
        }
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        float p1_pos_x = player1.transform.position.x;
        float p2_pos_x = player2.transform.position.x;
        bool flip_facing = false;
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player1)
        {
            if (p1_pos_x > p2_pos_x)
                {
                flip_facing = true;
                }
        }
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player2)
        {
            if (p2_pos_x > p1_pos_x)
            {
                flip_facing = true;
            }
        }


        SetFacing(flip_facing);
    }
    void SetFacing(bool facing)
    {
        body.GetComponent<SpriteRenderer>().flipX = facing;
        flipped_facing = facing;
    }
    void Shoot()
    {
        // Shoot a raycast and check if it hit their head

        RaycastHit2D hit = Physics2D.Raycast(Spinner.transform.position, Spinner.transform.up, shotDistance, ~layerMask); ;

        try
        {
            Debug.Log(hit.collider.gameObject.name);
        } 
        catch (NullReferenceException ex)
        {
            Debug.Log("Hit something without a gameobject (the sky?): " + ex);
        }
        
        // Check which player is shooting
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player1)
        {
            if (hit != false && hit.collider.gameObject.name == "Player2Head")
            {
                // Kill the Player
                hit.collider.transform.parent.gameObject.SetActive(false);

                // Play special blood effect
            }
        }
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player2)
        {
            if (hit != false && hit.collider.gameObject.name == "Player1Head")
            {
                // Kill the Player
                hit.collider.transform.parent.gameObject.SetActive(false);

                // Play special blood effect
            }
        }

        // =========== Play special effects

        // 
        if (hit.collider.gameObject.tag == "SolidObject")
        {
            // Play special miss effect
        }
        // Throw the gun after you shoot
        DropGun();
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
        GunPrefab.transform.SetParent(transform.Find("Spinner").gameObject.transform);
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player2)
        {
            GunSprite.flipY = true;
            GunSprite.color = new Color(1f, 0.5f, 0f, 1f);
        } else
        {
            GunSprite.flipY = false;
            GunSprite.color = new Color(0.1f, 1f, 0f, 1f);

        }
    }

    void DropGun()
    {
        GunSprite.transform.localPosition = Vector3.zero;
        if (GunSprite.flipY) { GunSprite.flipY = false; };
        GunSprite.color = new Color(1f, 1f, 1f, 1f);

        hasGun = false;
        angle = 0;
        // Deactivate the spinner when you lose the gun
        Spinner.SetActive(false);

        // Stop holding the gun
        GunPrefab.transform.SetParent(GameObject.Find("GunStorage").transform);

        // Throw the gun away
        GunPrefab.GetComponent<Gun>().Respawn();
    }
}
