using UnityEngine;

public class HeadGesture : MonoBehaviour
{
    public bool isFacingDown = false;
    public bool isMovingDown = false;

    private Camera camera;
    private float previousCameraAngle;
    private float sweepRate = 1.0f;

    void Start()
    {
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        previousCameraAngle = CameraAngleFromGround();
    }

    void Update()
    {
        isFacingDown = DetectFacingDown();
        isMovingDown = DetectMovingDown();
    }

    private bool DetectFacingDown()
    {
        return CameraAngleFromGround() < 60.0f;
    }

    private bool DetectMovingDown()
    {
        float angle = CameraAngleFromGround();
        float deltaAngle = previousCameraAngle - angle;
        float rate = deltaAngle / Time.deltaTime;
        previousCameraAngle = angle;

        return rate >= sweepRate;
    }

    private float CameraAngleFromGround()
    {
        return Vector3.Angle(Vector3.up, camera.transform.forward);
    }
}