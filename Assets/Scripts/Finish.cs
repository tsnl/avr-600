using UnityEngine;

public class Finish : MonoBehaviour
{
    // The sound to play when the player reaches the finish
    public AudioClip finishSound;

    // Optional particle effect to play at the finish line
    public ParticleSystem finishEffect;

    // Reference to the SceneManager (found by tag)
    private SceneManager sceneManager;    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        // Ensure this object has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning($"Finish object {gameObject.name} should have a Collider component set as trigger!");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Called when another collider enters the trigger collider attached to this object
    void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player (by tag)
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player reached finish: {gameObject.name}");

            // Play the finish sound if it is assigned
            if (finishSound != null)
            {
                AudioSource.PlayClipAtPoint(finishSound, transform.position);
            }

            // Play particle effect if assigned
            if (finishEffect != null)
            {
                finishEffect.Play();
            }

            // You can add additional win condition logic here
            // For example: load next level, show victory screen, etc.
            OnPlayerFinish();
        }
    }
    /// <summary>
    /// Called when the player reaches this finish point. Override this method for custom behavior.
    /// </summary>
    protected virtual void OnPlayerFinish()
    {
        Debug.Log("Congratulations! You've reached the finish!");

        // Load the next level via SceneManager
        if (sceneManager != null)
        {
            sceneManager.LoadNextLevel();
        }
        else
        {
            Debug.LogError("SceneManager reference is null! Cannot load next level.");
        }
    }
}