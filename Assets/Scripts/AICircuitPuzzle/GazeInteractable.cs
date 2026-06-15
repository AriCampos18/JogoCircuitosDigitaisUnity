using UnityEngine;

public abstract class GazeInteractable : MonoBehaviour
{
    [Header("Feedback visual")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.cyan;
    [SerializeField] private Color activeColor = Color.green;

    private bool isActive;
    private Material runtimeMaterial;

    protected virtual void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            runtimeMaterial = targetRenderer.material;
            normalColor = runtimeMaterial.color;
        }
    }

    public virtual void OnGazeEnter(GazeSelectionController controller)
    {
        SetColor(hoverColor);
    }

    public virtual void OnGazeStay(GazeSelectionController controller, float progress)
    {
    }

    public virtual void OnGazeExit(GazeSelectionController controller)
    {
        SetColor(isActive ? activeColor : normalColor);
    }

    public void CompleteGaze(GazeSelectionController controller)
    {
        OnGazeComplete(controller);
    }

    public void SetActiveState(bool active)
    {
        isActive = active;
        SetColor(isActive ? activeColor : normalColor);
    }

    protected abstract void OnGazeComplete(GazeSelectionController controller);

    protected void SetColor(Color color)
    {
        if (runtimeMaterial != null)
        {
            runtimeMaterial.color = color;
        }
    }
}
