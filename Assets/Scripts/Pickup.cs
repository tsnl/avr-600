using UnityEngine;

public class Pickup : MonoBehaviour
{
    // Speed of rotation in degrees per second.
    public float rotationSpeed = 90f;

    // Reference to the SceneManager (found by tag)
    private SceneManager sceneManager;

    void Start()
    {
        // Find the SceneManager by tag
        GameObject sceneManagerObject = GameObject.FindGameObjectWithTag("SceneManager");
        if (sceneManagerObject != null)
        {
            sceneManager = sceneManagerObject.GetComponent<SceneManager>();
            if (sceneManager == null)
            {
                Debug.LogError("GameObject with 'SceneManager' tag does not have a SceneManager component!");
            }
        }
        else
        {
            Debug.LogError("No GameObject with 'SceneManager' tag found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Spin around the Y axis at a constant velocity
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }

    // Called when another collider enters the trigger collider attached to this object
    void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player (by tag)
        if (other.CompareTag("Player"))
        {
            // Call the SceneManager's OnPickup method to handle the pickup logic
            if (sceneManager != null)
            {
                sceneManager.OnPickup(gameObject);
            }
            else
            {
                Debug.LogError("SceneManager reference is null! Cannot process pickup.");
                // Fallback: destroy the pickup anyway to prevent it from being stuck
                Destroy(gameObject);
            }
        }
    }
}
