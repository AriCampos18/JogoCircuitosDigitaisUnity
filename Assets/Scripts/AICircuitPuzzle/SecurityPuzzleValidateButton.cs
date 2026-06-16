using UnityEngine;

public class SecurityPuzzleValidateButton : GazeInteractable
{
    public SecurityPuzzleManager manager;

    protected override void OnGazeComplete(GazeSelectionController controller)
    {
        if (manager != null)
        {
            manager.ValidateSignals();
        }
    }
}
