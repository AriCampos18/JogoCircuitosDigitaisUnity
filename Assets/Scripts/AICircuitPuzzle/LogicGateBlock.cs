using UnityEngine;

public class LogicGateBlock : GazeInteractable
{
    [Header("Porta logica")]
    public LogicGateType gateType = LogicGateType.OR;
    public CircuitPuzzleManager puzzleManager;

    [Tooltip("Prefab opcional usado quando a porta for encaixada no slot. Se vazio, o proprio bloco sera clonado.")]
    public GameObject placedPrefab;

    protected override void OnGazeComplete(GazeSelectionController controller)
    {
        if (puzzleManager == null)
        {
            puzzleManager = FindObjectOfType<CircuitPuzzleManager>();
        }

        if (puzzleManager != null)
        {
            puzzleManager.SelectGate(this);
        }
    }
}
