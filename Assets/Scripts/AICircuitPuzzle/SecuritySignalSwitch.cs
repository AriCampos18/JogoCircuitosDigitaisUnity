using UnityEngine;

public class SecuritySignalSwitch : GazeInteractable
{
    public SecurityPuzzleManager manager;
    public string signalName = "A";
    public TextMesh valueTextMesh;

    public bool Value { get; private set; }

    public void SetSignalValue(bool value)
    {
        Value = value;
        SetActiveState(Value);

        if (valueTextMesh != null)
        {
            valueTextMesh.text = Value ? "LIGADA" : "DESLIGADA";
        }
    }

    protected override void OnGazeComplete(GazeSelectionController controller)
    {
        if (manager != null)
        {
            manager.ToggleSignal(this);
        }
    }
}
