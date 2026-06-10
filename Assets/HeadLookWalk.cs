using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadLookWalk : MonoBehaviour
{

    public float walkingSpeed = 0.7f;
    GameObject olhos;
    Camera camera;
    private CharacterController characterController;
    // Start is called before the first frame update
    void Start()
    {
        olhos = GameObject.FindWithTag("MeusOlhos");
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vectror3 moveDirection = camera.transform.foward;
        moveDirection = moveDirection*walkingSpeed*Time.deltaTime;
        moveDirection.y = -1f; // Mantém o personagem no chão
        olhos.transform.position = olhos.transform.position +moveDirection;
        characterController.Move(moveDirection);
    }
}
