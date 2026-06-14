using UnityEngine;

public class HeadLookWalkCC3 : MonoBehaviour
{
    public float walkingSpeed = 0.7f;
    public bool isWalking = false;

    private Camera camera;
    private CharacterController controller;

    void Start()
    {
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isWalking)
        {
            Vector3 direction = camera.transform.forward;
            direction.y = 0f; // não deixa voar para cima/baixo
            direction.Normalize();

            controller.SimpleMove(direction * walkingSpeed);
        }
        else
        {
            controller.SimpleMove(Vector3.zero); // mantém gravidade sem andar
        }
    }
}