using Unity.VisualScripting;
using UnityEngine;

public class CameraHandle : MonoBehaviour
{
    public float left_limit = -9.645f;
    public float right_limit = 9.645f;
    public float top_limit = 5.5f;
    public float bottom_limit = -5.2f;
    public float maxZoom = 5.3f;
    public float minZoom = 4.5f;
    [SerializeField] public float dist_ratio = 3f;

    // How fast the camera will follow the players
    public float cam_speed = 8f;
    public Transform player1;
    public Transform player2;
    private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        // Middle point between the players
        float targetX = (player1.position.x + player2.position.x) / 2f;
        float targetY = (player1.position.y + player2.position.y) / 2f;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Pan away if players stray too far from camera
        Vector3 p1_pos = (player1.position);
        Vector3 p2_pos = (player2.position);
        Vector2 distance = (p2_pos - p1_pos);
        float ratio = distance.magnitude / dist_ratio;
        float zoom_target = Mathf.Lerp(cam.orthographicSize, 1 * ratio, 0.005f);
        cam.orthographicSize = zoom_target;
        if (cam.orthographicSize > maxZoom)
        {
            cam.orthographicSize = maxZoom;
        }
        else if (cam.orthographicSize < minZoom)
        {
            cam.orthographicSize = minZoom;
        }
        halfHeight = cam.orthographicSize;

        float minX = left_limit + halfWidth;
        float maxX = right_limit - halfWidth;

        float minY = bottom_limit + halfHeight;
        float maxY = top_limit - halfHeight;

        // Keeps within the walls and floor
        targetX = Mathf.Clamp(targetX, minX, maxX);
        targetY = Mathf.Clamp(targetY, minY, maxY);

        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, cam_speed * Time.deltaTime);
        pos.y = Mathf.Lerp(pos.y, targetY, cam_speed * Time.deltaTime);
        pos.y = Mathf.Min(pos.y, 0.02f);
        transform.position = pos;

    }
}
