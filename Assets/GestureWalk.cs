using UnityEngine;

public class GestureWalk : MonoBehaviour
{
    private HeadLookWalkCC3 lookWalk;
    private HeadGesture gesture;

    private float cooldown = 1.0f;
    private float lastGestureTime = -10f;

    void Start()
    {
        lookWalk = GetComponent<HeadLookWalkCC3>();
        gesture = GameObject.Find("GameController").GetComponent<HeadGesture>();
    }

    void Update()
    {
        if (gesture.isMovingDown && Time.time - lastGestureTime > cooldown)
        {
            lookWalk.isWalking = !lookWalk.isWalking;
            lastGestureTime = Time.time;
        }
    }
}