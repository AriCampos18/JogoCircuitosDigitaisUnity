using UnityEngine;

public class LogicGateSlot : GazeInteractable
{
    [Header("Slot")]
    public CircuitPuzzleManager puzzleManager;
    public Transform attachPoint;
    public int slotOrder;
    public bool replacePlacedGate = true;

    private bool hasGate;
    private LogicGateType placedGateType;
    private GameObject currentVisual;

    public bool HasGate
    {
        get { return hasGate; }
    }

    public LogicGateType PlacedGateType
    {
        get { return placedGateType; }
    }

    protected override void OnGazeComplete(GazeSelectionController controller)
    {
        if (puzzleManager == null)
        {
            puzzleManager = FindObjectOfType<CircuitPuzzleManager>();
        }

        if (puzzleManager != null)
        {
            puzzleManager.PlaceSelectedGate(this);
        }
    }

    public bool PlaceGate(LogicGateBlock gateBlock)
    {
        if (gateBlock == null)
        {
            return false;
        }

        if (hasGate && !replacePlacedGate)
        {
            return false;
        }

        ClearSlot();

        hasGate = true;
        placedGateType = gateBlock.gateType;

        GameObject visualSource = gateBlock.placedPrefab != null ? gateBlock.placedPrefab : gateBlock.gameObject;
        Transform parent = attachPoint != null ? attachPoint : transform;
        currentVisual = Instantiate(visualSource, parent.position, parent.rotation, parent);

        // O clone encaixado e apenas visual; ele nao deve continuar recebendo gaze.
        RemoveInteractiveComponents(currentVisual);
        SetActiveState(true);

        return true;
    }

    public void ClearSlot()
    {
        if (currentVisual != null)
        {
            Destroy(currentVisual);
            currentVisual = null;
        }

        hasGate = false;
        SetActiveState(false);
    }

    private void RemoveInteractiveComponents(GameObject visual)
    {
        LogicGateBlock[] blocks = visual.GetComponentsInChildren<LogicGateBlock>();
        for (int i = 0; i < blocks.Length; i++)
        {
            Destroy(blocks[i]);
        }

        GazeInteractable[] interactables = visual.GetComponentsInChildren<GazeInteractable>();
        for (int i = 0; i < interactables.Length; i++)
        {
            if (interactables[i] is LogicGateBlock)
            {
                continue;
            }

            Destroy(interactables[i]);
        }

        Collider[] colliders = visual.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Destroy(colliders[i]);
        }
    }
}
