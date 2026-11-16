using TMPro;
using UnityEngine;

/// <summary>
/// Scene-side owner of the PuzzleTracker. Collectibles report to it when picked up,
/// it updates the key/score UI, and it opens the gate once every key is collected.
/// Nothing is checked every frame: everything happens in response to pickups and tracker events.
/// </summary>
public class PuzzleController : MonoBehaviour
{
    [SerializeField] private int keysRequired = 3;
    [Tooltip("The object blocking the way to the flag. It is switched off when the puzzle is solved.")]
    [SerializeField] private GameObject gate;
    [SerializeField] private TMP_Text progressText;

    private PuzzleTracker tracker;

    private void Awake()
    {
        tracker = new PuzzleTracker(keysRequired);
        tracker.ProgressChanged += UpdateProgressText;
        tracker.PuzzleCompleted += OpenGate;
    }

    private void Start()
    {
        UpdateProgressText();
    }

    private void OnDestroy()
    {
        tracker.ProgressChanged -= UpdateProgressText;
        tracker.PuzzleCompleted -= OpenGate;
    }

    /// <summary>Called by a Collectible when the player picks it up.</summary>
    public void Collect(CollectibleData data, string pieceId)
    {
        tracker.AddScore(data.Points);

        if (data.CountsTowardPuzzle)
        {
            tracker.MarkComplete(pieceId);
        }
    }

    private void UpdateProgressText()
    {
        progressText.text = $"Keys: {tracker.CompletedCount}/{tracker.RequiredCount}   Score: {tracker.Score}";
    }

    private void OpenGate()
    {
        gate.SetActive(false);
    }
}
