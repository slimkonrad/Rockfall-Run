using System;
using System.Collections.Generic;

/// <summary>
/// Pure C# class (not a MonoBehaviour) that holds the puzzle and scoring logic.
/// It records which puzzle pieces have been collected in a HashSet, keeps the score,
/// and raises events when progress changes or when every piece is complete.
/// </summary>
public class PuzzleTracker
{
    private readonly HashSet<string> completedPieces = new HashSet<string>();

    public int RequiredCount { get; }
    public int Score { get; private set; }
    public int CompletedCount => completedPieces.Count;
    public bool IsComplete => completedPieces.Count >= RequiredCount;

    /// <summary>Raised whenever a piece is collected or the score changes.</summary>
    public event Action ProgressChanged;

    /// <summary>Raised once, the moment the last required piece is collected.</summary>
    public event Action PuzzleCompleted;

    public PuzzleTracker(int requiredCount)
    {
        RequiredCount = requiredCount;
    }

    /// <summary>
    /// Records a puzzle piece by its unique ID. Collecting the same piece twice is ignored,
    /// because a HashSet only stores each ID once.
    /// </summary>
    public void MarkComplete(string pieceId)
    {
        if (IsComplete || !completedPieces.Add(pieceId))
        {
            return;
        }

        ProgressChanged?.Invoke();

        if (IsComplete)
        {
            PuzzleCompleted?.Invoke();
        }
    }

    public void AddScore(int points)
    {
        Score += points;
        ProgressChanged?.Invoke();
    }
}
