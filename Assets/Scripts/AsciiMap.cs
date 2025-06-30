using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a parsed ASCII map for maze generation.
/// 
/// ASCII Map Characters:
/// - '#' : Hedge elements (walls/barriers)
/// - '+' : Pickup elements (collectible items)
/// - 'S' : Player start position (exactly one required)
/// - 'E' : End/finish positions (at least one required)
/// - ' ' : Empty cells (walkable space)
/// 
/// Map Requirements:
/// - Input will be automatically trimmed (leading/trailing whitespace removed from entire input and each row)
/// - All rows must have equal width after trimming
/// - No empty lines are allowed between non-empty lines
/// - Map must be bounded by hedge elements (#) on all sides
/// - Must contain exactly one start position (S)
/// - Must contain at least one end position (E)
/// - All pickups must be reachable from the start position
/// - All end goals must be reachable from the start position
/// </summary>
[System.Serializable]
public class AsciiMap
{
  public List<Vector2Int> hedgePositions;
  public List<Vector2Int> pickupPositions;
  public List<Vector2Int> endPositions;
  public Vector2Int startPosition;

  // Level definitions as strings
  private static readonly Dictionary<string, string> levelStrings = new Dictionary<string, string>
  {
    ["Level0"] = @"
########
#E     #
# #### #
# #  # #
# #  #+#
# #  # #
# #### #
#     S#
########",

    ["Level1"] = @"
##########
#S   #   #
# ## # # #
#    # # #
#### # # #
#    # # #
# #####+##
#       E#
##########",

    ["Level2"] = @"
############
#S    #    #
# ### # ## #
# # # # #  #
# # #+# # ##
# #   # #  #
# ### # ## #
#   #   #  #
### ##### ##
#        E #
############",

    ["Level3"] = @"
##############
#S  #        #
# # # ###### #
# # #      # #
# # ##### ## #
# #   +   #  #
# ####### #  #
#   #     # ##
# # # ##### ##
# #       # ##
# ####### # ##
#         # E#
##############",

    ["Level4"] = @"
################
#S   #         #
# ## # ####### #
# #  #       # #
# # ##### ## # #
# # #   # #  # #
# # # # # ## # #
# # # # #  # # #
# # # #+## # # #
# # #      # # #
# # ######## # #
# #          # #
# ############ #
#             E#
################"
  };

  // Pre-constructed AsciiMap cache
  private static readonly Dictionary<string, AsciiMap> levelMaps = new Dictionary<string, AsciiMap>();

  // Static constructor to initialize all levels
  static AsciiMap()
  {
    foreach (var kvp in levelStrings)
    {
      try
      {
        levelMaps[kvp.Key] = Parse(kvp.Value);
      }
      catch (System.ArgumentException e)
      {
        Debug.LogError($"Failed to parse level '{kvp.Key}': {e.Message}");
      }
    }
  }

  /// <summary>
  /// Gets a pre-constructed AsciiMap by name.
  /// </summary>
  /// <param name="name">The name of the level (e.g., "Level0", "Level1", etc.)</param>
  /// <returns>A pre-constructed AsciiMap instance</returns>
  /// <exception cref="System.ArgumentException">Thrown when the level name is not found</exception>
  public static AsciiMap GetByName(string name)
  {
    if (levelMaps.TryGetValue(name, out AsciiMap map))
    {
      return map;
    }
    throw new System.ArgumentException($"Level '{name}' not found. Available levels: {string.Join(", ", levelMaps.Keys)}");
  }

  /// <summary>
  /// Gets all available level names.
  /// </summary>
  /// <returns>Array of available level names</returns>
  public static string[] GetAvailableLevels()
  {
    return levelMaps.Keys.ToArray();
  }

  /// <summary>
  /// Legacy property for backward compatibility - returns Level0
  /// </summary>
  public static string Level0 => levelStrings["Level0"];

  public AsciiMap()
  {
    hedgePositions = new List<Vector2Int>();
    pickupPositions = new List<Vector2Int>();
    endPositions = new List<Vector2Int>();
  }
  /// <summary>
  /// Parses an ASCII map string and validates it, returning an AsciiMap instance.
  /// </summary>
  /// <param name="asciiMapString">The ASCII map string to parse</param>
  /// <returns>A validated AsciiMap instance</returns>
  /// <exception cref="System.ArgumentException">Thrown when the map is invalid</exception>
  public static AsciiMap Parse(string asciiMapString)
  {
    if (string.IsNullOrEmpty(asciiMapString))
    {
      throw new System.ArgumentException("ASCII map string cannot be null or empty");
    }

    // Trim leading and trailing whitespace from the entire input
    asciiMapString = asciiMapString.Trim();

    // Split the map into rows
    string[] rows = asciiMapString.Split('\n');    // Remove empty rows and trim carriage returns
    var nonEmptyRows = new List<string>();
    bool foundNonEmpty = false;
    bool foundEmptyAfterContent = false;

    foreach (string row in rows)
    {
      string trimmedRow = row.Trim(); // Remove carriage return if present
      bool hasContent = !string.IsNullOrEmpty(trimmedRow.Trim());

      if (hasContent)
      {
        if (foundEmptyAfterContent)
        {
          throw new System.ArgumentException("ASCII map cannot contain empty lines between non-empty lines");
        }
        nonEmptyRows.Add(trimmedRow);
        foundNonEmpty = true;
      }
      else if (foundNonEmpty)
      {
        foundEmptyAfterContent = true;
      }
    }

    if (nonEmptyRows.Count == 0)
    {
      throw new System.ArgumentException("ASCII map must contain at least one non-empty row");
    }    // Validate that all rows have the same width
    int mapWidth = nonEmptyRows[0].Length;
    for (int i = 1; i < nonEmptyRows.Count; i++)
    {
      if (nonEmptyRows[i].Length != mapWidth)
      {
        throw new System.ArgumentException($"All rows must have the same width. Row {i} has length {nonEmptyRows[i].Length}, expected {mapWidth}");
      }
    }

    int mapHeight = nonEmptyRows.Count;// Validate that the map is bounded by hedges
    // Check top and bottom rows
    for (int x = 0; x < mapWidth; x++)
    {
      if (nonEmptyRows[0][x] != '#' || nonEmptyRows[mapHeight - 1][x] != '#')
      {
        throw new System.ArgumentException("Map must be bounded by hedges (#) on all sides");
      }
    }

    // Check left and right columns
    for (int y = 0; y < mapHeight; y++)
    {
      if (nonEmptyRows[y][0] != '#' || nonEmptyRows[y][mapWidth - 1] != '#')
      {
        throw new System.ArgumentException("Map must be bounded by hedges (#) on all sides");
      }
    }

    AsciiMap map = new AsciiMap();
    int startCount = 0;    // Parse the map and populate the AsciiMap
    for (int y = 0; y < mapHeight; y++)
    {
      for (int x = 0; x < mapWidth; x++)
      {
        char cell = nonEmptyRows[y][x];
        Vector2Int position = new Vector2Int(x, mapHeight - 1 - y); // Flip Y to match Unity's coordinate system

        switch (cell)
        {
          case '#':
            map.hedgePositions.Add(position);
            break;
          case '+':
            map.pickupPositions.Add(position);
            break;
          case 'S':
            if (startCount > 0)
            {
              throw new System.ArgumentException("Map must contain exactly one start position (S)");
            }
            map.startPosition = position;
            startCount++;
            break;
          case 'E':
            map.endPositions.Add(position);
            break;
          case ' ':
            // Empty cell, do nothing
            break;
          default:
            throw new System.ArgumentException($"Invalid character '{cell}' at position ({x}, {y}). Valid characters are: #, +, S, E, and space");
        }
      }
    }

    // Validate that there is exactly one start position
    if (startCount != 1)
    {
      throw new System.ArgumentException("Map must contain exactly one start position (S)");
    }
    // Validate that there is at least one end position
    if (map.endPositions.Count == 0)
    {
      throw new System.ArgumentException("Map must contain at least one end position (E)");
    }

    // Validate reachability from start position
    ValidateReachability(map, mapWidth, mapHeight);

    return map;
  }

  /// <summary>
  /// Validates that all pickups and end goals are reachable from the start position.
  /// </summary>
  /// <param name="map">The map to validate</param>
  /// <param name="mapWidth">Width of the map</param>
  /// <param name="mapHeight">Height of the map</param>
  /// <exception cref="System.ArgumentException">Thrown when unreachable elements are found</exception>
  private static void ValidateReachability(AsciiMap map, int mapWidth, int mapHeight)
  {
    // Create a set of hedge positions for quick lookup
    HashSet<Vector2Int> hedgeSet = new HashSet<Vector2Int>(map.hedgePositions);

    // Perform flood fill from start position to find all reachable positions
    HashSet<Vector2Int> reachablePositions = FloodFill(map.startPosition, hedgeSet, mapWidth, mapHeight);

    // Check if all pickups are reachable
    foreach (Vector2Int pickup in map.pickupPositions)
    {
      if (!reachablePositions.Contains(pickup))
      {
        throw new System.ArgumentException($"Pickup at position ({pickup.x}, {pickup.y}) is not reachable from start position ({map.startPosition.x}, {map.startPosition.y})");
      }
    }

    // Check if all end goals are reachable
    foreach (Vector2Int endGoal in map.endPositions)
    {
      if (!reachablePositions.Contains(endGoal))
      {
        throw new System.ArgumentException($"End goal at position ({endGoal.x}, {endGoal.y}) is not reachable from start position ({map.startPosition.x}, {map.startPosition.y})");
      }
    }
  }

  /// <summary>
  /// Performs a flood fill algorithm to find all positions reachable from a starting position.
  /// </summary>
  /// <param name="startPos">The starting position</param>
  /// <param name="hedgeSet">Set of hedge positions (walls) that block movement</param>
  /// <param name="mapWidth">Width of the map</param>
  /// <param name="mapHeight">Height of the map</param>
  /// <returns>Set of all reachable positions</returns>
  private static HashSet<Vector2Int> FloodFill(Vector2Int startPos, HashSet<Vector2Int> hedgeSet, int mapWidth, int mapHeight)
  {
    HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
    Queue<Vector2Int> queue = new Queue<Vector2Int>();

    queue.Enqueue(startPos);
    visited.Add(startPos);

    // Directions for 4-way movement (up, down, left, right)
    Vector2Int[] directions = new Vector2Int[]
    {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
    };

    while (queue.Count > 0)
    {
      Vector2Int current = queue.Dequeue();

      // Check all four directions
      foreach (Vector2Int direction in directions)
      {
        Vector2Int neighbor = current + direction;

        // Check if neighbor is within bounds
        if (neighbor.x >= 0 && neighbor.x < mapWidth &&
            neighbor.y >= 0 && neighbor.y < mapHeight)
        {
          // Check if neighbor is not a hedge and not already visited
          if (!hedgeSet.Contains(neighbor) && !visited.Contains(neighbor))
          {
            visited.Add(neighbor);
            queue.Enqueue(neighbor);
          }
        }
      }
    }

    return visited;
  }
}
