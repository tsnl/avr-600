using UnityEngine;
using TMPro;

public class SceneManager : MonoBehaviour
{
    // Player avatar to position at start of scene:
    public GameObject playerAvatar;
    
    // Prefabs to spawn:
    public GameObject hedgeElementPrefab;
    public GameObject pickupPrefab;
    public GameObject finishPrefab;
    
    // The sound to play when picked up.
    public AudioClip pickupSound;    // The label to update with the pickup count.
    public TextMeshProUGUI pickupCounterLabel;

    // Pickup tracking
    private int pickupCount = 0;
    private int totalPickups = 0;

    /// <summary>
    /// Sets up the scene from a parsed ASCII map by instantiating prefabs and positioning the player.
    /// </summary>
    /// <param name="asciiMap">The parsed ASCII map to use for scene setup</param>
    public void SetupSceneFromAsciiMap(AsciiMap asciiMap)
    {
        if (asciiMap == null)
        {
            throw new System.ArgumentException("AsciiMap cannot be null");
        }

        // Clear any existing objects (optional - you might want to keep this for resetting scenes)
        ClearExistingMapObjects();

        // Spawn hedge elements
        foreach (Vector2Int pos in asciiMap.hedgePositions)
        {
            if (hedgeElementPrefab != null)
            {
                Vector3 worldPos = new Vector3(pos.x, 0, pos.y);
                GameObject hedge = Instantiate(hedgeElementPrefab, worldPos, Quaternion.identity);
                hedge.transform.parent = transform; // Parent to SceneManager for organization
            }
            else
            {
                Debug.LogWarning("Hedge element prefab is not assigned!");
            }
        }

        // Spawn pickup elements
        foreach (Vector2Int pos in asciiMap.pickupPositions)
        {
            if (pickupPrefab != null)
            {
                Vector3 worldPos = new Vector3(pos.x, 0.5f, pos.y); // Slightly above ground for visibility
                GameObject pickup = Instantiate(pickupPrefab, worldPos, Quaternion.identity);
                pickup.transform.parent = transform; // Parent to SceneManager for organization
            }
            else
            {
                Debug.LogWarning("Pickup prefab is not assigned!");
            }
        }

        // Spawn finish/end elements
        foreach (Vector2Int pos in asciiMap.endPositions)
        {
            if (finishPrefab != null)
            {
                Vector3 worldPos = new Vector3(pos.x, 0, pos.y);
                GameObject finish = Instantiate(finishPrefab, worldPos, Quaternion.identity);
                finish.transform.parent = transform; // Parent to SceneManager for organization
            }
            else
            {
                Debug.LogWarning("Finish prefab is not assigned!");
            }
        }        // Position the player avatar at the start position
        if (playerAvatar != null)
        {
            Vector3 playerWorldPos = new Vector3(asciiMap.startPosition.x, 0, asciiMap.startPosition.y);
            playerAvatar.transform.position = playerWorldPos;
        }
        else
        {
            Debug.LogWarning("Player avatar is not assigned!");
        }

        // Initialize pickup counting
        InitializePickupCounting(asciiMap);
    }

    /// <summary>
    /// Initializes pickup counting based on the ASCII map.
    /// </summary>
    /// <param name="asciiMap">The ASCII map to count pickups from</param>
    private void InitializePickupCounting(AsciiMap asciiMap)
    {
        pickupCount = 0;
        totalPickups = asciiMap.pickupPositions.Count;
        UpdatePickupLabel();
    }

    /// <summary>
    /// Updates the pickup counter label.
    /// </summary>
    private void UpdatePickupLabel()
    {
        if (pickupCounterLabel != null)
        {
            pickupCounterLabel.text = $"Pickups: {pickupCount}/{totalPickups}";
        }
    }

    /// <summary>
    /// Called when a pickup is collected by the player.
    /// </summary>
    /// <param name="pickupGameObject">The pickup GameObject that was collected</param>
    public void OnPickup(GameObject pickupGameObject)
    {
        Debug.Log($"Player picked up: {pickupGameObject.name}");
        
        // Play the pickup sound if it is assigned
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, pickupGameObject.transform.position);
        }
        
        // Increment pickup counter and update label
        pickupCount++;
        UpdatePickupLabel();
        
        // Destroy the pickup object to hide it from the player
        Destroy(pickupGameObject);
        
        // Check if all pickups have been collected
        if (pickupCount >= totalPickups)
        {
            Debug.Log("All pickups collected!");
            // You can add additional logic here, such as opening doors, showing messages, etc.
        }
    }

    /// <summary>
    /// Clears existing map objects that are children of this SceneManager.
    /// </summary>
    private void ClearExistingMapObjects()
    {
        // Destroy all child objects (hedges, pickups, finishes that were previously spawned)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
    
    void Start()
    {
        // Use the default Level0 map from AsciiMap
        try
        {
            AsciiMap parsedMap = AsciiMap.Parse(AsciiMap.Level0);
            SetupSceneFromAsciiMap(parsedMap);
            Debug.Log("Scene setup completed successfully using Level0!");
        }
        catch (System.ArgumentException e)
        {
            Debug.LogError($"Failed to parse ASCII map: {e.Message}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
