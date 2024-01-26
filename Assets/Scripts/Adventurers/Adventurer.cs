using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum AgentPriority
{
    Low, Medium, High
}
public enum AdventurerType
{
    Ranged, Melee
}
public class Adventurer : MonoBehaviour
{
    [SerializeField]
    private Transform pickupLocation;
    public Transform TreasurePickupLocation { get => pickupLocation; }
    protected Health health;
    public Health Health { get => health; }
    protected Minotaur minautor;
    protected AdventurerType adventurerType;
    public AdventurerType AdventurerType { get => adventurerType; }
    private bool isInSafeZone;
    private bool isInRestZone;
    private BoatAgent boatAgent;
    public BoatAgent BoatAgent { get => boatAgent; }
    private AgentPriority priority = AgentPriority.Low;
    private bool canPickupTreasure;
    private bool hasTreasure;
    protected bool canAttack = true;
    private bool canRest;

    public List<string> allTasks;
    public Stack<PrimitiveTask> plan;
    private HTN htn;
    public string currentAction;

    private CancellationTokenSource cts;

    private float normalizedDistanceToAdventurer;
    private float normalizedDistanceToTreasure;
    private float normalizedDistanceToMinotaur;
    private float healthRatio;

    public MovePotential MovePotential;

    protected virtual void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += OnDeath;
        health.OnDamage += OnDamage;
        minautor = FindObjectOfType<Minotaur>();
        boatAgent = GetComponentInParent<BoatAgent>();
        allTasks = new List<string>();
        plan = new Stack<PrimitiveTask>();
        htn = new HTN(this, FindObjectOfType<GameManager>());
        onWaitExecute();
        UpdateCurrentState();
    }

    private async void onWaitExecute()
    {
        await Task.Delay(3000);
        htn.ExecuteHTN();
        for (int i = 0; i < plan.Count; i++)
        {
            Debug.Log(plan.ElementAt(i));
        }
        await Task.Yield();
        await ExecuteActions();
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AttackTarget(minautor.gameObject);
        }
    }

    private async void UpdateCurrentState()
    {
        while (true)
        {
            await Task.Delay(1000);
            healthRatio = Mathf.Clamp(health.HealthValue / WorldState.AverageHealth, 0f, 1f);

            float distanceToTreasure = Vector3.Distance(transform.position, WorldState.Treasure.transform.position);
            float distanceToMinotaur = Vector3.Distance(transform.position, WorldState.Minotaur.transform.position);
            Adventurer closestAdventurer = WorldState.Adventurers.OrderBy(a => Vector3.Distance(a.transform.position, transform.position)).First();
            float distanceToClosestAdventurer = Vector3.Distance(transform.position, closestAdventurer.transform.position);

            // Normalize distances (assuming maxDistance is the furthest possible distance in your game world)
            normalizedDistanceToAdventurer = Mathf.Clamp(distanceToClosestAdventurer / WorldState.MaxCoveredDistance, 0f, 1f);
            normalizedDistanceToTreasure = Mathf.Clamp(distanceToTreasure / WorldState.MaxCoveredDistance, 0f, 1f);
            normalizedDistanceToMinotaur = Mathf.Clamp(distanceToMinotaur / WorldState.MaxCoveredDistance, 0f, 1f);

            MovePotential = new MovePotential(GetAttackPotential(), GetMovePotential(), GetFleePotential(), GetBaitPotential(), GetIdlePotential());
        }      
    }

    private float GetMovePotential()
    {
        // Weights
        const float healthWeight = 1.5f;
        const float adventurerWeight = 1.0f;
        const float treasureWeight = 1.0f;
        const float minotaurWeight = 1.5f;

        // Scores
        float healthScore = healthRatio * 5 * healthWeight;
        float adventurerScore = normalizedDistanceToAdventurer * 5 * adventurerWeight;
        float treasureScore = normalizedDistanceToTreasure * 5 * treasureWeight;
        AdjustTreasureScore(ref treasureScore, treasureWeight);
        float minotaurScore = normalizedDistanceToMinotaur * 5 * minotaurWeight;

        // Calculate total score with weights
        float totalWeight = healthWeight + adventurerWeight + treasureWeight + minotaurWeight;
        return CalculateWeightedAverage(healthScore, adventurerScore, treasureScore, minotaurScore, totalWeight);
    }

    private float GetFleePotential()
    {
        // Weights
        const float healthWeight = 2.0f;
        const float adventurerWeight = 1.0f;
        const float treasureWeight = 0.5f;
        const float minotaurWeight = 2.5f;

        // Scores
        float healthScore = (1 - healthRatio) * 5 * healthWeight;
        float adventurerScore = normalizedDistanceToAdventurer * 5 * adventurerWeight;
        float treasureScore = (1 - normalizedDistanceToTreasure) * 5 * treasureWeight;
        AdjustTreasureScore(ref treasureScore, treasureWeight);
        float minotaurScore = (1 - normalizedDistanceToMinotaur) * 5 * minotaurWeight;

        // Calculate total score with weights
        float totalWeight = healthWeight + adventurerWeight + treasureWeight + minotaurWeight;
        return CalculateWeightedAverage(healthScore, adventurerScore, treasureScore, minotaurScore, totalWeight);
    }

    private float GetAttackPotential()
    {
        // Weights
        const float healthWeight = 2.0f;
        const float adventurerWeight = 2.0f;
        const float treasureWeight = 1.0f;
        const float minotaurWeight = 1.5f;

        // Scores
        float healthScore = healthRatio * 5 * healthWeight;
        float adventurerScore = (1 - normalizedDistanceToAdventurer) * 5 * adventurerWeight;
        float treasureScore = normalizedDistanceToTreasure * 5 * treasureWeight;
        AdjustTreasureScore(ref treasureScore, treasureWeight);
        float minotaurScore = (1 - normalizedDistanceToMinotaur) * 5 * minotaurWeight;

        // Calculate total score with weights
        float totalWeight = healthWeight + adventurerWeight + treasureWeight + minotaurWeight;
        return CalculateWeightedAverage(healthScore, adventurerScore, treasureScore, minotaurScore, totalWeight);
    }

    private float GetBaitPotential()
    {
        // Weights
        const float healthWeight = 1.5f;
        const float adventurerWeight = 1.0f;
        const float treasureWeight = 2.0f;
        const float minotaurWeight = 2.0f;

        // Scores
        float healthScore = healthRatio * 5 * healthWeight;
        float adventurerScore = normalizedDistanceToAdventurer * 5 * adventurerWeight;
        float treasureScore = normalizedDistanceToTreasure * 5 * treasureWeight;
        AdjustTreasureScore(ref treasureScore, treasureWeight);
        float minotaurScore = (1 - normalizedDistanceToMinotaur) * 5 * minotaurWeight;

        // Calculate total score with weights
        float totalWeight = healthWeight + adventurerWeight + treasureWeight + minotaurWeight;
        return CalculateWeightedAverage(healthScore, adventurerScore, treasureScore, minotaurScore, totalWeight);
    }

    private float GetIdlePotential()
    {
        // Weights
        const float healthWeight = 2.0f;
        const float adventurerWeight = 1.0f;
        const float treasureWeight = 1.0f;
        const float minotaurWeight = 1.5f;

        // Scores
        float healthScore = healthRatio * 5 * healthWeight;
        float adventurerScore = (1 - normalizedDistanceToAdventurer) * 5 * adventurerWeight;
        float treasureScore = (1 - normalizedDistanceToTreasure) * 5 * treasureWeight;
        AdjustTreasureScore(ref treasureScore, treasureWeight);
        float minotaurScore = normalizedDistanceToMinotaur * 5 * minotaurWeight;

        // Calculate total score with weights
        float totalWeight = healthWeight + adventurerWeight + treasureWeight + minotaurWeight;
        return CalculateWeightedAverage(healthScore, adventurerScore, treasureScore, minotaurScore, totalWeight);
    }

    private void AdjustTreasureScore(ref float treasureScore, float weight)
    {
        if (WorldState.TreasurePickerAdventurer == this)
        {
            treasureScore += 0.5f * weight;
        }
        else if(WorldState.TreasurePickerAdventurer == null)
        {
            treasureScore = 1 * weight;
        } else
        {
            treasureScore -= 0.5f * weight;
        }
    }

    private float CalculateWeightedAverage(float healthScore, float adventurerScore, float treasureScore, float minotaurScore, float totalWeight)
    {
        return (healthScore + adventurerScore + treasureScore + minotaurScore) / totalWeight;
    }

    public void OnDamage()
    {
        if (hasTreasure)
        {
            DropTreasure();
        }
    }

    public void OnDeath()
    {
        Debug.Log("Adventurer died");
        Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameArea"))
        {
            GameArea gameArea = other.GetComponent<GameArea>();
            if (gameArea.AreaType == GameAreaType.Safe)
            {
                isInSafeZone = true;
            }
            else if (gameArea.AreaType == GameAreaType.Rest)
            {
                isInRestZone = true;
            }
        }

        if (other.CompareTag("TreasurePickupZone"))
        {
            canPickupTreasure = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GameArea"))
        {
            GameArea gameArea = other.GetComponent<GameArea>();
            if (gameArea.AreaType == GameAreaType.Safe)
            {
                isInSafeZone = false;
            }
            else if (gameArea.AreaType == GameAreaType.Rest)
            {
                isInRestZone = false;
            }
        }
        if (other.CompareTag("TreasurePickupZone"))
        {
            canPickupTreasure = false;
        }
    }

    public async void ChangePlan()
    {
        Debug.Log("CHANGING PLAN");
        plan.Clear();
        allTasks.Clear();
        cts.Cancel();
        if(WorldState.onTreasureQuestAdventurer == this)
        {
            WorldState.ClearTreasureQuest();
        }
        htn.ExecuteHTN();
        //wait for next frame
        await Task.Yield();
        await ExecuteActions();
    }

    private async Task ExecuteActions()
    {
        cts = new CancellationTokenSource();
        foreach (var task in plan)
        {
            currentAction = task.Name;
            await task.TryExecuteAction();
        }
        ChangePlan();
    }

    //==========================Guards==========================
    public bool CanAttack()
    {
        return canAttack;
    }

    public bool CanRest()
    {
        return canRest;
    }

    public bool IsCriticalHealth()
    {
        return Health.HealthValue <= 2f;
    }

    public bool IsBeingChased()
    {
        return WorldState.Minotaur.target.gameObject == gameObject;
    }

    public bool CanMove()
    {
        return Health.HealthValue > 0f;
    }

    public bool IsInRestZone()
    {
        return isInRestZone;
    }

    public bool IsInSafeZone()
    {
        return isInRestZone;
    }

    public bool CanPickupTreasure()
    {
        return canPickupTreasure;
    }

    public bool HasTreasure()
    {
        return hasTreasure;
    }


    //===============================Actions===============================

    public async Task GoTowardsNearestAdventurer()
    {
        Adventurer nearestAdventurer = null;
        float minDistance = float.MaxValue;
        foreach (var adventurer in WorldState.Adventurers)
        {
            if (adventurer != this)
            {
                float distance = Vector3.Distance(adventurer.transform.position, transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestAdventurer = adventurer;
                }
            }
        }
        if (nearestAdventurer != null)
        {
            await WorldState.RegisterWorldEvent(EventType.TargetReached, this, nearestAdventurer.gameObject, cts);
            BoatAgent.Target = nearestAdventurer.transform;
        }
    }
    public async Task GoTowardsTreasure()
    {
        WorldState.TreasurePickerAdventurer = this;
        Debug.Log(BoatAgent);
        if(WorldState.Treasure == null)
        {
            WorldState.Treasure = FindObjectOfType<Treasure>();
        }
        BoatAgent.Target = WorldState.Treasure.transform;
        await WorldState.RegisterWorldEvent(EventType.TargetReached, this, WorldState.Treasure.gameObject, cts);
        BoatAgent.StopBoat();
    }

    public async Task GoTowardsMinotaur()
    {
        WorldState.AssassinAdventurer = this;
        if(WorldState.Minotaur == null)
        {
            WorldState.Minotaur = FindObjectOfType<Minotaur>();
        }
        BoatAgent.Target = WorldState.Minotaur.transform;
        await WorldState.RegisterWorldEvent(EventType.TargetReached, this, WorldState.Minotaur.gameObject, cts);
    }


    public async Task GoToNearestSafeZone()
    {
        WorldState.BaitAdventurer = this;
        GameArea nearestSafeZone = null;
        float minDistance = float.MaxValue;
        foreach (var gameArea in WorldState.GameAreas)
        {
            if (gameArea.AreaType == GameAreaType.Safe)
            {
                float distance = Vector3.Distance(gameArea.transform.position, transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestSafeZone = gameArea;
                }
            }
        }
        if (nearestSafeZone != null)
        {
            BoatAgent.Target = nearestSafeZone.transform;
            await WorldState.RegisterWorldEvent(EventType.TargetReached, this, nearestSafeZone.gameObject, cts);
        }
    }

    public async Task GoToNearestRestArea()
    {
        GameArea nearestRestArea = null;
        float minDistance = float.MaxValue;
        foreach (var gameArea in WorldState.GameAreas)
        {
            if (gameArea.AreaType == GameAreaType.Rest)
            {
                float distance = Vector3.Distance(gameArea.transform.position, transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestRestArea = gameArea;
                }
            }
        }
        if (nearestRestArea != null)
        {
            BoatAgent.Target = nearestRestArea.transform;
            await WorldState.RegisterWorldEvent(EventType.TargetReached, this, nearestRestArea.gameObject, cts);
        }
    }

    public virtual void AttackTarget(GameObject target) { }

    public async Task AttackMinautor()
    {
        WorldState.AssassinAdventurer = this;
        AttackTarget(WorldState.Minotaur.gameObject);
        await WorldState.RegisterWorldEvent(EventType.AttackTarget, this, WorldState.Minotaur.gameObject, cts);
        await Task.Yield();
    }


    public async Task NotifyAdventurers()
    {
       //Notifyingis donethrough the world state
    }

    public async Task BringBackTreasure()
    {
        MapCorner nearestCorner = null;
        float minDistance = float.MaxValue;
        foreach (var corner in WorldState.MapCorners)
        {
            float distance = Vector3.Distance(corner.transform.position, transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCorner = corner;
            }
        }
        if(nearestCorner != null)
        {
            BoatAgent.Target = nearestCorner.transform;
            await WorldState.RegisterWorldEvent(EventType.TargetReached, this, nearestCorner.gameObject, cts);
        }
    }

    public async Task Rest()
    {
        Debug.Log("RESTING");
        BoatAgent.StopBoat();
        float timer = 0;
        while (CanRest())
        {
            timer += Time.deltaTime;
            if (timer > 5.0f)
            {
                Health.HealthValue = Mathf.Max(Health.MaxHealth, Health.HealthValue + 1);
            }
            await Task.Yield();
        }
    }

    public async Task PickupTreasure()
    {
        Debug.Log("PICKING UP TREASURE !!!");
        float currentHealth = Health.HealthValue;
        await Task.Delay(3000);
        float newHealth = Health.HealthValue;
        //If the treasure is picked up and the health is not changed (no damage taken), the treasure is picked up
        if (!WorldState.IsTreasurePickedUp && currentHealth == newHealth)
        {
            WorldState.IsTreasurePickedUp = true;
            WorldState.Treasure.transform.position = TreasurePickupLocation.position;
            WorldState.Treasure.transform.SetParent(TreasurePickupLocation);
            hasTreasure = true;
        }
    }

    private void DropTreasure()
    {
        WorldState.Treasure.transform.SetParent(null);
        WorldState.Treasure.transform.position = transform.position;
        WorldState.IsTreasurePickedUp = false;
        hasTreasure = false;
    }
}
