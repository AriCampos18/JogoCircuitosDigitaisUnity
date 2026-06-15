using UnityEngine;

public class HeadLookWalkCC3 : MonoBehaviour
{
    public float walkingSpeed = 0.7f;
    public bool isWalking = false;
    public float fallLimit = -2f;
    public bool swimMode = true;
    public float swimHeight = 0.2f;

    private Camera playerCamera;
    private CharacterController controller;
    private Vector3 startPosition;

    void Start()
    {
        playerCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        controller = GetComponent<CharacterController>();
        startPosition = transform.position;
        swimHeight = startPosition.y;
    }

    void Update()
    {
        if (transform.position.y < fallLimit)
        {
            RespawnAtStart();
            return;
        }

        if (swimMode)
        {
            UpdateSwimMovement();
            return;
        }

        if (isWalking)
        {
            Vector3 direction = playerCamera.transform.forward;
            direction.y = 0f; // não deixa voar para cima/baixo
            direction.Normalize();

            controller.SimpleMove(direction * walkingSpeed);
        }
        else
        {
            controller.SimpleMove(Vector3.zero); // mantém gravidade sem andar
        }
    }

    private void UpdateSwimMovement()
    {
        Vector3 direction = Vector3.zero;

        if (isWalking)
        {
            direction = playerCamera.transform.forward;
            direction.y = 0f;
            direction.Normalize();
        }

        if (controller != null && controller.enabled)
        {
            controller.Move(direction * walkingSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += direction * walkingSpeed * Time.deltaTime;
        }

        Vector3 fixedPosition = transform.position;
        fixedPosition.y = swimHeight;
        transform.position = fixedPosition;
    }

    private void RespawnAtStart()
    {
        isWalking = false;

        if (controller != null)
        {
            controller.enabled = false;
        }

        transform.position = startPosition;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}
