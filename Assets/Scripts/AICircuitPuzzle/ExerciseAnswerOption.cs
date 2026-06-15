using UnityEngine;

public class ExerciseAnswerOption : GazeInteractable
{
    [Header("Resposta")]
    public CircuitPuzzleManager puzzleManager;
    public bool answerValue;

    protected override void OnGazeComplete(GazeSelectionController controller)
    {
        if (puzzleManager == null)
        {
            puzzleManager = FindObjectOfType<CircuitPuzzleManager>();
        }

        if (puzzleManager != null)
        {
            puzzleManager.SelectExerciseAnswer(answerValue);
        }
    }
}
