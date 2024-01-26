using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public enum ActionPriority
{
    IMMINENT = 4, IMPORTANT = 3, NORMAL = 2, LOW = 1
}

public class Minotaur : MonoBehaviour
{
    public Treasure treasure;
    public Transform target;
    [SerializeField]
    [Range(1, 100)]
    private int scoutNumRays = 15;
    public Transform rayCastRoot;
    public Transform head;
    private Rigidbody rb;
    [SerializeField]
    [Range(1, 10)]
    private float diveForce = 5f;
    public float searchFrustrumAngle = 45;
    private const float rayCastScoutPositionY = 0.5f;
    private Health health;
    [SerializeField]
    [Range(1, 50)]
    private float movementSpeed = 5f;
    public ActionPriority currentActionPriority = ActionPriority.LOW;
    public ActionPriority CurrentActionPriority { get => currentActionPriority; set => currentActionPriority = value; }
    [Range(1, 10f)]
    public float turnSpeed = 5f;
    private NavMeshPath path;
    public bool mustReachTreasure = false;
    private List<Adventurer> adventurers;
    private float noEmemyTimer = 0f;
    private float idleTurnSpeed = 100f;
    private bool canAttack = true;

    private Dictionary<GameObject, ActionPriority> adventurersState;
    // Start is called before the first frame update
    void Start()
    {
        health = GetComponent<Health>();
        //Can't die so no need to subscribe to OnDeath
        health.OnDamage += OnDamage;
        rb = GetComponent<Rigidbody>();
        adventurers = FindObjectsOfType<Adventurer>().ToList();
        adventurersState = new Dictionary<GameObject, ActionPriority>();
        StartCoroutine(ComputePathToTreasure());
        StartCoroutine(ScoutSurroundings());
        StartCoroutine(IdleBehaviour());
    }

    // Update is called once per frame
    void Update()
    {
        rayCastRoot.position = new Vector3(transform.position.x, rayCastScoutPositionY, transform.position.z);
    }

    private IEnumerator IdleBehaviour()
    {
        if (adventurersState.Count() == 0)
        {
            while (noEmemyTimer < 10f)
            {
                yield return TurnArround();
            }
            //recheck the state of the game
            if (noEmemyTimer >= 10f && adventurersState.Count() == 0)
            {
                DiveTowardsTarget(treasure.transform);
                RejoinTreasure();
            }
        }
        else
        {
            noEmemyTimer = 0f;
        }
    }

    public IEnumerator TurnArround()
    {
        float totalAngle = Random.Range(60f, 260f);
        float currentTurnAngle = 0;
        int randomSign = Random.Range(0, 2) * 2 - 1;
        float t;
        while (currentTurnAngle < totalAngle)
        {
            t = (1 - currentTurnAngle / totalAngle);
            float angle = idleTurnSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, randomSign * angle * t);
            currentTurnAngle += angle;
            noEmemyTimer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));
    }

    public void OnDamage()
    {
        ActionPriority newPriority = ActionPriority.NORMAL;
        if (newPriority >= currentActionPriority)
        {
            currentActionPriority = newPriority;
            this.target = health.GetLastDamadeDealer().transform;
            AppendCandidate(health.DamageDealer, newPriority);
            DiveTowardsTarget(health.DamageDealer.transform);
        }
    }

    public void AppendCandidate(GameObject adventurer, ActionPriority priority)
    {
        if (adventurersState.ContainsKey(adventurer))
        {
            if (adventurersState[adventurer] < priority)
            {
                adventurersState[adventurer] = priority;
            }
        }
        else
        {
            adventurersState.Add(adventurer, priority);
        }
    }

    public void RemoveCandidate(GameObject adventurer)
    {
        if (adventurersState.ContainsKey(adventurer))
        {
            adventurersState.Remove(adventurer);
        }
    }

    //Idle behavior
    IEnumerator ScoutSurroundings()
    {
        float step = 2 * searchFrustrumAngle / scoutNumRays;
        while (isActiveAndEnabled)
        {
            Vector3 leftScoutBoundary = Quaternion.AngleAxis(-searchFrustrumAngle, Vector3.up) * rayCastRoot.forward;
            Debug.DrawRay(rayCastRoot.position, leftScoutBoundary, Color.red);
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < scoutNumRays; i++)
            {
                Vector3 nextBoundary = Quaternion.AngleAxis(step * i, Vector3.up) * leftScoutBoundary;
                RaycastHit hit;
                if (Physics.Raycast(rayCastRoot.position, nextBoundary, out hit, 5f))
                {
                    Debug.DrawRay(rayCastRoot.position, nextBoundary * hit.distance, Color.magenta);

                    if (hit.transform.CompareTag("Adventurer"))
                    {
                        ActionPriority newPriority = ActionPriority.LOW;
                        if (newPriority >= currentActionPriority)
                        {
                            //if see a player
                            AppendCandidate(hit.transform.gameObject, newPriority);
                            target = hit.transform;
                        }
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (target != null && !mustReachTreasure)
        {
            RotateTowardsTarget(target.position);
            ChaseTarget();
        }
        else
        {
            ProtectTreasure();
        }
        //Debug.Log(adventurersState.Count());
    }

    private IEnumerator ComputePathToTreasure()
    {
        while (isActiveAndEnabled)
        {
            yield return new WaitForSeconds(1f);
            if (mustReachTreasure)
            {
                path = FindPathToTarget();
            }
        }
    }

    private void RejoinTreasure()
    {
        target = treasure.transform;
        FindPathToTarget();
    }
    private void ProtectTreasure()
    {
        if (path != null)
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.green);
            }
        }
        if (path != null && path.corners != null && path.corners.Length > 1)
        {
            Vector3 closestCorner = path.corners[1];
            Vector3 direction = closestCorner - transform.position;
            RotateTowardsTarget(target.position);
            rb.AddForce(direction.normalized * movementSpeed * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    }

    public void OnTreasureZoneEntered()
    {
        ActionPriority newPriority = ActionPriority.IMPORTANT;
        //If a player gets too close to the treasure, automatically dive towards him, -> highest priority
        FindClosestPlayerToTreasure();
        AppendCandidate(target.gameObject, newPriority);
        currentActionPriority = newPriority;
        DiveTowardsTarget(treasure.transform);
        mustReachTreasure = true;
    }

    public void OnTreasurePickedUp(GameObject adventurer)
    {
        ActionPriority newPriority = ActionPriority.IMMINENT;
        //If the treasure is picked up, dive towards the player that picked it up -> imminent priority
        //Do not look for the closest player, because the player that picked up the treasure is the closest
        target = adventurer.transform;
        AppendCandidate(target.gameObject, newPriority);
        currentActionPriority = newPriority;
        DiveTowardsTarget(target.transform);
        mustReachTreasure = true;
    }

    public void OnTreasureDropped(GameObject adventurer)
    {
        RemoveCandidate(adventurer);
        OnTreasureZoneEntered();
    }

    public void FindClosestPlayerToTreasure()
    {
        float minDistance = float.MaxValue;
        foreach (Adventurer adventurer in adventurers)
        {
            float distanceToTreasure = Vector3.Distance(adventurer.transform.position, treasure.transform.position);
            if (distanceToTreasure < minDistance)
            {
                minDistance = distanceToTreasure;
                target = adventurer.transform;
            }
        }
    }

    private void DiveTowardsTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction = direction.normalized + Vector3.up;
        rb.AddForce(direction.normalized * diveForce, ForceMode.Impulse);
    }

    public async void CheckEnemyVisible(float duration, Transform target)
    {
        while (duration > 0)
        {
            RotateTowardsTarget(target.position);
            await Task.Yield();
            duration -= Time.deltaTime;
        }
        if (target != null)
        {
            DiveTowardsTarget(target);
        }
    }

    NavMeshPath FindPathToTarget()
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.SamplePosition(transform.position, out NavMeshHit hitA, 10f, NavMesh.AllAreas);
        NavMesh.SamplePosition(target.position, out NavMeshHit hitB, 10f, NavMesh.AllAreas);
        bool foundPath = NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, path);
        return path;
    }

    private void ChaseTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction = direction.normalized;
        rb.AddForce(direction.normalized * movementSpeed * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    private void RotateTowardsTarget(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0;
        direction = direction.normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * turnSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Adventurer")
        {
            ActionPriority newPriority = ActionPriority.LOW;
            if (newPriority >= currentActionPriority)
            {
                currentActionPriority = newPriority;
                AppendCandidate(other.gameObject, newPriority);
                CheckEnemyVisible(1.0f, other.transform);
            }
        }
    }

    public void SelectNextCandidate()
    {
        if (adventurersState.Count() == 0)
        {
            return;
        }
        foreach (var state in adventurersState)
        {
            if (state.Value >= currentActionPriority)
            {
                currentActionPriority = state.Value;
                target = state.Key.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Adventurer")
        {
            //If manages to escape the  minautor's sight, remove it from the list of candidates
            RemoveCandidate(other.gameObject);
            SelectNextCandidate();
        }
    }

    private async void OnAttackCooldown(GameObject target)
    {
        await Task.Delay(1000);
        canAttack = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Treasure"))
        {
            mustReachTreasure = false;
            if (target.gameObject == treasure)
            {
                target = null;
                currentActionPriority = ActionPriority.LOW;
            }
        }

        if (collision.gameObject.CompareTag("Adventurer"))
        {
            target.GetComponentInChildren<Health>().TakeDamage(1, this.gameObject);
            canAttack = false;
            OnAttackCooldown(collision.gameObject);

        }
    }
}
