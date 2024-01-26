using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("FusionWater/_Examples/BoatController")]
public class BoatController : MonoBehaviour
{
    private float initialMoveSpeed;
    public float moveSpeed = 10f;
    public float turnSpeed = 10f;

    [Range(0.5f, 5)]
    public float accelerationTime = 3f;
    public float baseAcceleration = 0.3f;

    public AnimationCurve accelerationCurve;

    public Transform boatMotor;

    private Rigidbody rb;

    private void Awake()
    {
        initialMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
    }

    public void StopBoat()
    {
        moveSpeed = 0;
    }

    public void RestartBoat()
    {
        moveSpeed = initialMoveSpeed;
    }

    public void OrientateBoat(Vector3 direction)
    {
        //Below is to rotate the boat rudder according to the target
        Vector3 directionToTarget = direction - boatMotor.position;
        Vector3 rudderOrientation = Vector3.Reflect(directionToTarget, new Vector3(transform.right.x, 0, transform.right.z));
        Debug.DrawRay(boatMotor.position, rudderOrientation, Color.white);
        Debug.DrawRay(boatMotor.position, rudderOrientation, Color.yellow);
        Vector3 normalizedRudderForward = new Vector3(boatMotor.forward.x, 0, boatMotor.forward.z);
        float angle = Vector3.Angle(normalizedRudderForward, rudderOrientation);
        float t = Mathf.InverseLerp(180, 0, angle);
        rudderOrientation.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(rudderOrientation);
        boatMotor.rotation = Quaternion.Slerp(targetRotation, boatMotor.rotation, t * Time.fixedDeltaTime * turnSpeed);

        //Below is to clamp the rotation of the boat rudder to 75 degrees (somewhat realistic)
        float localYRotation = NormalizeAngle(boatMotor.localRotation.eulerAngles.y);
        localYRotation = Mathf.Clamp(localYRotation, -75f, 75f);
        boatMotor.localEulerAngles = new Vector3(boatMotor.localEulerAngles.x, localYRotation, boatMotor.localEulerAngles.z);
    }

    public void MoveTowards(Vector3 target)
    {
        float angle = Vector3.Angle(transform.forward, target);
        //caped between 0 and 180
        float t = Mathf.InverseLerp(0, 180, angle);
        float accelerationFactor = 1 / accelerationTime;
        float acceleration = accelerationCurve.Evaluate((t + baseAcceleration) * accelerationFactor);
        //Add Force at Boat Motors Position
        rb.AddForceAtPosition(acceleration * moveSpeed * boatMotor.forward, boatMotor.position);
    }

    public float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }
}
