using UnityEngine;

/// <summary>
/// A pickup in the level (key or coin). Its values come from a CollectibleData asset.
/// When the player touches it, it reports to the PuzzleController and disappears.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [SerializeField] private CollectibleData data;
    [Tooltip("Set in the Inspector for hand-placed pickups. Spawned pickups receive it through Init().")]
    [SerializeField] private PuzzleController puzzle;

    private string pieceId;

    private void Awake()
    {
        // A fresh unique ID for every pickup, so each key counts once in the tracker's HashSet.
        pieceId = System.Guid.NewGuid().ToString();
    }

    /// <summary>Used by spawners to hand a newly created pickup its scene references.</summary>
    public void Init(PuzzleController puzzleController)
    {
        puzzle = puzzleController;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        puzzle.Collect(data, pieceId);
        gameObject.SetActive(false);
    }
}