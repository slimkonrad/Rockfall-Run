using UnityEngine;

/// <summary>
/// Data for one kind of collectible (for example a Key or a Coin).
/// Each asset made from this type is a separate set of values the game reads at runtime,
/// so new collectible kinds can be added without writing new code.
/// </summary>
[CreateAssetMenu(fileName = "NewCollectible", menuName = "Rockfall Run/Collectible Data")]
public class CollectibleData : ScriptableObject
{
    [SerializeField] private string displayName = "Collectible";
    [Tooltip("Points added to the score when this is picked up.")]
    [SerializeField] private int points = 10;
    [Tooltip("Tick for items that count toward opening the gate (keys). Leave unticked for bonus items (coins).")]
    [SerializeField] private bool countsTowardPuzzle;

    public string DisplayName => displayName;
    public int Points => points;
    public bool CountsTowardPuzzle => countsTowardPuzzle;
}
