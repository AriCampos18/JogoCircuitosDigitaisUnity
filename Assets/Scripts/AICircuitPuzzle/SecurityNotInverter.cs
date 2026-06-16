using UnityEngine;

public class SecurityNotInverter : GazeInteractable
{
    public SecurityPuzzleManager manager;
    public TextMesh gateTextMesh;
    public TextMesh stateTextMesh;

    public bool Enabled { get; private set; }

    public void SetInverterEnabled(bool enabled)
    {
        Enabled = enabled;
        SetActiveState(Enabled);

        if (stateTextMesh != null)
        {
            stateTextMesh.text = Enabled ? "INVERSOR\nLIGADO" : "INVERSOR\nDESLIGADO";
        }
    }

    protected override void OnGazeComplete(GazeSelectionController controller)
    {
        if (manager != null)
        {
            manager.ToggleInverter(this);
        }
    }
}
