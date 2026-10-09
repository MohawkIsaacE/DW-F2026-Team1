using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Player : MonoBehaviour
{

    [SerializeField] private GameObject Spinner;
    [SerializeField] private InputActionReference ShootAction;
    [SerializeField] private GameObject GunPrefab;
    [SerializeField] private SpriteRenderer GunSprite;
    [SerializeField] private Transform body;
    [SerializeField] private GameObject GameMaster;
    [SerializeField] private AudioClip shoot;
    [SerializeField] private AudioClip pickUp;

    private float shotCooldown = 0.2f;
    private float shotTimer;
    private float angle;
    private float shotDistance = 100f;
    private float handOffset = 0.4f;   // how far to the side the gun is held
    private float lineOffset = 0.5f;   // shifts the aim line onto the barrel
    //private float rotateSpeed = 180f;
    private LayerMask layerMask;
    public bool hasGun;

    private PlayerMovement player;
    private GameMaster GameMasterScript;
    private Gun GunScript;


    [SerializeField] private Collider2D Player1Collider;
    [SerializeField] private Collider2D Player2Collider;

    public bool flipped_facing = false;
    [Header("Particle Effects")]
    private GameObject smokeParticle;
    private GameObject sparkParticle;
    private GameObject bloodParticle;
    private GameObject flashParticle;

    void Awake()
    {
        player = GetComponent<PlayerMovement>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shotTimer = shotCooldown;
        hasGun = false;
        Spinner.SetActive(false);
        Physics2D.IgnoreCollision(Player1Collider, Player2Collider);
        GunScript = GunPrefab.GetComponent<Gun>();
        GameMasterScript = GameMaster.GetComponent<GameMaster>();

        // Layer mask
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player1)
        {
            layerMask = LayerMask.GetMask("Ignore Raycast", "Player1Head");
        }
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player2)
        {
            layerMask = LayerMask.GetMask("Ignore Raycast", "Player2Head");
        }

        // Set up the particles for use
        sparkParticle = GameObject.Find("HitParticle");
        smokeParticle = GameObject.Find("ShootParticle");
        bloodParticle = GameObject.Find("BloodParticle");
        flashParticle = GameObject.Find("MuzzleFlash");
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
        if (hasGun && player.ShootPressed() && shotTimer <= 0f)
        {
            Shoot();
            shotTimer = shotCooldown;
        }

        if (hasGun) RotateGun();
        UpdateFacing();
    }
    Vector2 GetAimDirection()
    {
        if (player != null)
        {
            return player.rightStickDirection;
        }
        else
        {
            return Vector2.zero;
        }
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
        // Running overrides facing
        float moveInput = player.GetMoveInput();
        if (player.IsGrounded())
            if (moveInput > 0.1f)
            {
                flip_facing = false;
            }
            else if (moveInput < -0.1f)
            {
                flip_facing = true;
            }

        SetFacing(flip_facing);
    }
    void SetFacing(bool facing)
    {
        body.GetComponent<SpriteRenderer>().flipX = facing;
        flipped_facing = facing;

        // Hold the gun in the hand on the side we're facing
        Vector3 handPos = Spinner.transform.localPosition;
        handPos.x = facing ? -handOffset : handOffset;
        Spinner.transform.localPosition = handPos;
    }

    void Shoot()
    {

        AudioManager.Instance.PlaySfx(shoot, 0.1f);

        // Play shoot particle at the gun position
        smokeParticle.transform.position = GunPrefab.transform.position;
        smokeParticle.GetComponent<ParticleSystem>().Play();
        // Shoot a raycast and check if it hit their head

        RaycastHit2D hit = Physics2D.Raycast(Spinner.transform.position, Spinner.transform.up, shotDistance, ~layerMask); ;

        try
        {
            //Debug.Log(hit.collider.gameObject.name);
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
                GameMasterScript.KillSequence(gameObject);

                // Play special blood effect
                bloodParticle.transform.position = hit.collider.transform.position;
                bloodParticle.GetComponent<ParticleSystem>().Play();
            }
        }
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player2)
        {
            if (hit != false && hit.collider.gameObject.name == "Player1Head")
            {
                // Kill the Player
                GameMasterScript.KillSequence(gameObject);

                // Play special blood effect
                bloodParticle.transform.position = hit.collider.transform.position;
                bloodParticle.GetComponent<ParticleSystem>().Play();
            }
        }

        // Play muzzle flash to where the shot landed
        ParticleSystem flash = flashParticle.GetComponent<ParticleSystem>();
        Vector2 gunPos = GunPrefab.transform.position;

        float length = 30f;   // length if nothing is reached
        if (hit)
        {
            length = Vector2.Distance(gunPos, hit.point);
        }

        var main = flash.main;
        main.startSizeY = length;

        flashParticle.transform.position = gunPos + (Vector2)Spinner.transform.up * (length / 2f);
        flashParticle.transform.rotation = Spinner.transform.rotation;
        flash.Play();


        // =========== Play special effects

        // 
        if (hit && hit.collider.gameObject.tag == "SolidObject")
        {
            // Play special miss effect
            sparkParticle.transform.position = hit.point;
            sparkParticle.GetComponent<ParticleSystem>().Play();
        }
        // Throw the gun after you shoot
        DropGun();
    }

    void RotateGun()
    {
        // Turn right stick directions into angle and rotate gun by it
        Vector2 direction = player.rightStickDirection;
        if (direction.magnitude > 0.1f)
        {
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        }
        Spinner.transform.rotation = Quaternion.Euler(0, 0, angle);

        // so it's never upside down
        bool pointingLeft = Spinner.transform.up.x < 0f;
        Vector3 scale = GunPrefab.transform.localScale;
        scale.y = pointingLeft ? -Mathf.Abs(scale.y) : Mathf.Abs(scale.y);
        GunPrefab.transform.localScale = scale;
        // The barrel switches sides when the gun is mirrored, so move the aim line with it
        Transform shootLine = Spinner.transform.Find("ShootLine");
        Vector3 linePos = shootLine.localPosition;
        linePos.x = pointingLeft ? lineOffset : -lineOffset;
        shootLine.localPosition = linePos;
    }

    public void PickupGun(GameObject gunReference)
    {
        hasGun = true;
        AudioManager.Instance.PlaySfx(pickUp, 0.1f);
        // Activate the spinner so the player can aim
        Spinner.SetActive(true);
        // Start pointing the way the player is facing
        angle = flipped_facing ? 90f : -90f;

        // Hold the gun
        GunPrefab = gunReference;
        GunPrefab.transform.SetParent(transform.Find("Spinner").gameObject.transform);

        // Colour the gun by player (flipping is handled in RotateGun now)
        if (player.GetPlayerId() == PlayerMovement.PlayerId.Player2)
        {
            GunSprite.color = new Color(1f, 0.5f, 0f, 1f);
        }
        else
        {
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

        Vector3 scale = GunPrefab.transform.localScale;
        scale.y = Mathf.Abs(scale.y);
        GunPrefab.transform.localScale = scale;
        // Throw the gun away
        GunPrefab.GetComponent<Gun>().Respawn();
    }
}
