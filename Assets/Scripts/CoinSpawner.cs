using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Procedurally places bonus coins on top of the level's ground tiles using Perlin noise.
/// Every walkable surface tile is tested: where the noise value is above the threshold, a coin
/// spawns one tile above it. The seed shifts the noise, so each seed gives a different but
/// repeatable layout. Coins are optional, so the level is always beatable whatever the layout.
/// </summary>
public class CoinSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collectible coinPrefab;
    [SerializeField] private PuzzleController puzzle;
    [SerializeField] private Tilemap groundTilemap;
    [Tooltip("Coins never spawn where this tilemap has a tile (so none appear inside the gate).")]
    [SerializeField] private Tilemap gateTilemap;

    [Header("Generation")]
    [Tooltip("Same seed = same coin layout every time.")]
    [SerializeField] private int seed = 386;
    [Tooltip("Tick to pick a new random seed every time the level loads.")]
    [SerializeField] private bool randomizeSeed;
    [Tooltip("Smaller values make bigger clusters of coins; larger values scatter them.")]
    [SerializeField] private float noiseScale = 0.35f;
    [Tooltip("Noise must be above this (0 to 1) for a coin to spawn. Higher = fewer coins.")]
    [Range(0f, 1f)]
    [SerializeField] private float threshold = 0.55f;

    private void Start()
    {
        if (randomizeSeed)
        {
            seed = Random.Range(0, 100000);
        }

        GenerateCoins();
    }

    private void GenerateCoins()
    {
        // The seed decides where in the infinite Perlin noise field we sample from.
        System.Random random = new System.Random(seed);
        float offsetX = random.Next(0, 10000);
        float offsetY = random.Next(0, 10000);

        Camera cam = Camera.main;
        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            Vector3Int above = cell + Vector3Int.up;

            if (!IsOpenSurface(cell, above))
            {
                continue;
            }

            Vector3 spawnPosition = groundTilemap.GetCellCenterWorld(above);
            if (!IsOnScreen(cam, spawnPosition))
            {
                continue;
            }

            float noise = Mathf.PerlinNoise(cell.x * noiseScale + offsetX, cell.y * noiseScale + offsetY);
            if (noise > threshold)
            {
                Collectible coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity, transform);
                coin.Init(puzzle);
            }
        }
    }

    /// <summary>A ground tile with empty space above it (not inside the gate) is somewhere a player can stand.</summary>
    private bool IsOpenSurface(Vector3Int cell, Vector3Int above)
    {
        return groundTilemap.HasTile(cell)
            && !groundTilemap.HasTile(above)
            && !gateTilemap.HasTile(above);
    }

    /// <summary>Skips wall tops and anything else outside the camera's view.</summary>
    private bool IsOnScreen(Camera cam, Vector3 worldPosition)
    {
        Vector3 viewport = cam.WorldToViewportPoint(worldPosition);
        return viewport.x > 0f && viewport.x < 1f && viewport.y > 0f && viewport.y < 1f;
    }
}
