using UnityEngine;
using TMPro;

public class Pickup : MonoBehaviour
{
    // Speed of rotation in degrees per second.
    // This should be the same across all Prefab instances.
    public float rotationSpeed = 90f;

    // The sound to play when picked up.
    // This should be the same across all Prefab instances.
    public AudioClip pickupSound;

    // The label to update with the pickup count.
    // This should be the same across all Prefab instances.
    public TextMeshProUGUI pickupCounterLabel;

    // Static lock and flag for one-time initialization
    private static readonly object labelInitLock = new object();
    private static bool labelInitialized = false;
    private static TextMeshProUGUI staticPickupCounterLabelRef;
    private static int pickupCount = 0;
    private static int totalPickups = 1;
    
    // Helper to update the label
    private static void UpdatePickupLabel()
    {
        if (staticPickupCounterLabelRef != null)
        {
            staticPickupCounterLabelRef.text = $"Pickups: {pickupCount}/{totalPickups}";
        }
    }

    void Start()
    {
        // One-time label initialization
        lock (labelInitLock)
        {
            if (!labelInitialized && pickupCounterLabel != null)
            {
                staticPickupCounterLabelRef = pickupCounterLabel;
                pickupCount = 0;
                // Count all GameObjects with the "Pickup" tag
                totalPickups = GameObject.FindGameObjectsWithTag("Pickup").Length;
                UpdatePickupLabel();
                labelInitialized = true;
            }
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
            Debug.Log($"Player picked up: {gameObject.name}");
            
            // Play the pickup sound if it is assigned:
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Increment pickup counter and update label
            pickupCount++;
            UpdatePickupLabel();
            
            // Destroy the pickup object to hide it from the player.
            Destroy(gameObject);
        }
    }
}
