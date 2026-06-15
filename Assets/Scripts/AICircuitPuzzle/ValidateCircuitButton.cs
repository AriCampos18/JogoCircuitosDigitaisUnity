using UnityEngine;

public class ValidateCircuitButton : GazeInteractable
{
    [Header("Puzzle")]
    public CircuitPuzzleManager puzzleManager;

    protected override void OnGazeComplete(GazeSelectionController controller)
    {
        if (puzzleManager == null)
        {
            puzzleManager = FindObjectOfType<CircuitPuzzleManager>();
        }

        if (puzzleManager != null)
        {
            puzzleManager.ValidateCircuit();
        }
    }
}
