using UnityEngine;

public class Canon : MonoBehaviour
{
    public Transform projectileSpawnPoint; // Where the projectile will be spawned
    public GameObject projectilePrefab; // The projectile prefab
    public Transform canonTip;
    public Transform canonTube;

    public float launchSpeed = 30.0f; // Initial speed of the projectile
    public Vector3 launchDirection;
    [Range(0.1f, 10f)]
    public float rotationSpeed = 1f;

    // Function to launch the projectile
    public void ShootFrom(GameObject sender)
    {
        CanonBullet projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity)
            .GetComponent<CanonBullet>();
        projectile.SetYielder(sender);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        // Rotate the vector around the X-axis
        launchDirection = canonTip.transform.forward;
        Debug.DrawRay(transform.position, launchDirection, Color.yellow);
        rb.AddForce(launchDirection * launchSpeed, ForceMode.Impulse);
    }

    public void AimAtTarget(Vector3 target)
    {
        Vector3 directionToTarget = target - canonTube.position;
        //Debug.DrawRay(canonTip.position, targetVerticalAimPosition, Color.red);
        Quaternion verticalAimRotation = Quaternion.LookRotation(directionToTarget);
        canonTube.rotation = Quaternion.Slerp(canonTube.rotation, verticalAimRotation, Time.deltaTime * rotationSpeed);
        Vector3 rotateDirection = target - transform.position;
        rotateDirection.y = 0;
        Quaternion horizontalAimRotation = Quaternion.LookRotation(rotateDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, horizontalAimRotation, Time.deltaTime * rotationSpeed);
    }

    private void Update()
    {
        launchDirection = canonTip.transform.forward;
        Debug.DrawRay(transform.position, launchDirection, Color.yellow);
    }
}