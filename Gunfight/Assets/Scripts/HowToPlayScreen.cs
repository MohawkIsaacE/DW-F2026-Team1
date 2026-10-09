using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Lives in the "How To Play" scene. Closes the screen when a player presses A (gamepad),
// Start, Enter, Space or clicks. The artwork is just an Image in the scene, so it can be
// swapped or restyled without touching this script.
public class HowToPlayScreen : MonoBehaviour
{
    [SerializeField] private float minShowTime = 0.3f; // ignores the click that opened the level

    private InputAction dismissAction;
    private float shownAt;
    private bool closing;

    void OnEnable()
    {
        shownAt = Time.unscaledTime;
        dismissAction = new InputAction("Dismiss", InputActionType.Button);
        dismissAction.AddBinding("<Gamepad>/buttonSouth");
        dismissAction.AddBinding("<Gamepad>/start");
        dismissAction.AddBinding("<Keyboard>/enter");
        dismissAction.AddBinding("<Keyboard>/space");
        dismissAction.AddBinding("<Mouse>/leftButton");
        dismissAction.Enable();
    }

    void OnDisable()
    {
        dismissAction.Disable();
        dismissAction.Dispose();
    }

    void Update()
    {
        if (closing || Time.unscaledTime - shownAt < minShowTime) return;
        if (dismissAction.WasPressedThisFrame()) Close();
    }

    private void Close()
    {
        closing = true;
        HowToPlayLoader.Release();
        // Only unload when we are layered on top of a level (not when this scene is opened alone)
        if (SceneManager.sceneCount > 1) SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}
