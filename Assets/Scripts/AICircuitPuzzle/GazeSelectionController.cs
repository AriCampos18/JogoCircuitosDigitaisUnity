using UnityEngine;
using UnityEngine.UI;

public class GazeSelectionController : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera gazeCamera;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private float maxDistance = 12f;

    [Header("Tempo de olhar")]
    [SerializeField] private float gazeDuration = 2f;
    [SerializeField] private bool allowMouseOrTouchClick = true;

    [Header("UI opcional")]
    [SerializeField] private Text gazeProgressText = null;

    private GazeInteractable currentTarget;
    private float gazeTimer;

    private void Start()
    {
        if (gazeCamera == null)
        {
            gazeCamera = Camera.main;
        }

        UpdateProgressText(0f);
    }

    private void Update()
    {
        // O raycast sai do centro da camera, que e o ponto natural de mira no Cardboard.
        GazeInteractable target = FindGazeTarget();

        if (target != currentTarget)
        {
            ChangeTarget(target);
        }

        if (currentTarget == null)
        {
            UpdateProgressText(0f);
            return;
        }

        gazeTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(gazeTimer / gazeDuration);
        currentTarget.OnGazeStay(this, progress);
        UpdateProgressText(progress);

        if (progress >= 1f || ClickRequested())
        {
            // Ao completar o tempo de olhar, o objeto recebe a acao principal.
            currentTarget.CompleteGaze(this);
            gazeTimer = 0f;
            UpdateProgressText(0f);
        }
    }

    private GazeInteractable FindGazeTarget()
    {
        if (gazeCamera == null)
        {
            return null;
        }

        Ray ray = new Ray(gazeCamera.transform.position, gazeCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, interactableLayers))
        {
            // Permite que o Collider esteja em um filho do objeto interativo.
            return hit.collider.GetComponentInParent<GazeInteractable>();
        }

        return null;
    }

    private void ChangeTarget(GazeInteractable nextTarget)
    {
        if (currentTarget != null)
        {
            currentTarget.OnGazeExit(this);
        }

        currentTarget = nextTarget;
        gazeTimer = 0f;

        if (currentTarget != null)
        {
            currentTarget.OnGazeEnter(this);
        }
    }

    private bool ClickRequested()
    {
        if (!allowMouseOrTouchClick)
        {
            return false;
        }

        bool mouseClicked = Input.GetMouseButtonDown(0);
        bool touched = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        return mouseClicked || touched;
    }

    private void UpdateProgressText(float progress)
    {
        if (gazeProgressText != null)
        {
            int percent = Mathf.RoundToInt(progress * 100f);
            gazeProgressText.text = percent > 0 ? percent + "%" : "";
        }
    }
}
