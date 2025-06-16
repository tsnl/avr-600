using UnityEngine;

public class Pickup : MonoBehaviour
{
    // Speed of rotation in degrees per second
    public float rotationSpeed = 90f;

    // The sound to play when picked up
    public AudioClip pickupSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
            
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            Destroy(gameObject);
        }
    }
}
