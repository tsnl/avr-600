using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneManager : MonoBehaviour
{
    // Time allotted for this level in seconds:
    public int timeLimit;

    // Next level to load when this one is completed:
    public string nextLevelName;

    // Next level to load if the player fails:
    public string failLevelName;

    // Player avatar to position at start of scene:
    public GameObject playerAvatar;

    // Prefabs to spawn:
    public GameObject hedgeElementPrefab;
    public GameObject pickupPrefab;
    public GameObject finishPrefab;

    // The sound to play when picked up.
    public AudioClip pickupSound;    // The label to update with the pickup count.
    public TextMeshProUGUI pickupCounterLabel;
    public TextMeshProUGUI timerLabel;    // Pickup tracking
    private int pickupCount = 0;
    private int totalPickups = 0;

    // Timer tracking
    private float remainingTime;
    private bool timerActive = false;

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
        }        // Initialize pickup counting
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
    /// Initializes and starts the countdown timer.
    /// </summary>
    private void InitializeTimer()
    {
        remainingTime = timeLimit;
        timerActive = true;
        UpdateTimerLabel();
    }

    /// <summary>
    /// Updates the timer label display.
    /// </summary>
    private void UpdateTimerLabel()
    {
        if (timerLabel != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerLabel.text = $"Time: {minutes:00}:{seconds:00}";
        }
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
            InitializeTimer(); // Start the countdown timer
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
        // Handle countdown timer
        if (timerActive && remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerLabel();

            // Check if time has run out
            if (remainingTime <= 0)
            {
                remainingTime = 0;
                timerActive = false;
                UpdateTimerLabel();
                LoadFailLevel();
            }
        }
    }    /// <summary>
         /// Loads the fail level when time runs out.
         /// </summary>
    private void LoadFailLevel()
    {
        Debug.Log("Time's up! Loading fail level.");
        if (!string.IsNullOrEmpty(failLevelName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(failLevelName);
        }
        else
        {
            Debug.LogError("Fail level name is not set!");
        }
    }

    /// <summary>
    /// Called when the player successfully completes the level.
    /// </summary>
    public void LoadNextLevel()
    {
        Debug.Log("Level completed! Loading next level.");
        timerActive = false; // Stop the timer
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.LogError("Next level name is not set!");
        }
    }
}
