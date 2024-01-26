using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum EventType
{
    TreasurePickedUp,
    TreasureCaptured,
    AdventurerDied,
    TargetReached,
    AttackTarget,
}   

public class WorldState : MonoBehaviour
{
    //Implement the GameState class here
    private static List<GameArea> gameAreas;
    private static List<Adventurer> adventurers;
    private static Minotaur minotaur;
    private static Treasure treasure;
    private static bool isTreasurePickedUp;
    private static bool isTreasureCaptured;
    private static List<MapCorner> mapCorners;
    public static Adventurer onTreasureQuestAdventurer;
    public  static Adventurer closestPlayerToMinotaur;
    public static Adventurer closestPlayerToTreasure;
    public static Adventurer mostCriticalHealthAdventurer;
    public static float MaxCoveredDistance = 20.0f; //scale is [0, 20]
    public static float MaxHealth = 10f; //scale is [0, 10]
    public static float AverageHealth = 3f; //scale is [0, 10]
    public static float AverageDistanceToMinotaur = 10f; //scale is [0, 20]
    public static float AverageDistanceToTreasure = 10f; //scale is [0, 20]
    public static Adventurer AssassinAdventurer;
    public static Adventurer BaitAdventurer;
    public static Adventurer TreasurePickerAdventurer;

    public static List<MapCorner> MapCorners { get => mapCorners;}
    public static List<GameArea> GameAreas { get => gameAreas;}
    public static List<Adventurer> Adventurers { get => adventurers; set => adventurers = value; }
    public static Minotaur Minotaur { get => minotaur; set => minotaur = value; }
    public static Treasure Treasure { get => treasure; set => treasure = value; }
    public static bool IsTreasurePickedUp { get => isTreasurePickedUp; set => isTreasureCaptured = value; }
    public static bool IsTreasureCaptured { get => isTreasureCaptured; set => isTreasureCaptured = value; }


    public static void ClearTreasureQuest()
    {
        onTreasureQuestAdventurer = null;
    }

    private IEnumerator UpdateWorldState()
    {
        while (true)
        {
            closestPlayerToMinotaur = GetClosestAdventurerToMinotaur();
            closestPlayerToTreasure = GetClosestAdventurerToTreasure();
            mostCriticalHealthAdventurer = GetMostCriticalHealthPlayer();
            AverageHealth = adventurers.Average(a => a.Health.HealthValue);
            AverageDistanceToMinotaur = adventurers.Average(a => Vector3.Distance(a.transform.position, minotaur.transform.position));
            AverageDistanceToTreasure = adventurers.Average(a => Vector3.Distance(a.transform.position, treasure.transform.position));
            yield return null;
            //Debug.Log(TreasurePickerAdventurer);
        }
    }

    public Adventurer GetMostCriticalHealthPlayer()
    {
        Adventurer mostCriticalHealthAdventurer = null;
        float lowestHealth = float.MaxValue;
        foreach (var adventurer in adventurers)
        {
            if (adventurer.Health.HealthValue < lowestHealth)
            {
                lowestHealth = adventurer.Health.HealthValue;
                mostCriticalHealthAdventurer = adventurer;
            }
        }
        return mostCriticalHealthAdventurer;
    }

    private Adventurer GetClosestAdventurerToTreasure()
    {
        Adventurer closestAdventurer = null;
        float closestDistance = float.MaxValue;
        foreach (var adventurer in adventurers)
        {
            float distance = Vector3.Distance(adventurer.transform.position, treasure.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestAdventurer = adventurer;
            }
        }
        return closestAdventurer;
    }   

    private Adventurer GetClosestAdventurerToMinotaur()
    {
        Adventurer closestAdventurer = null;
        float closestDistance = float.MaxValue;
        foreach (var adventurer in adventurers)
        {
            float distance = Vector3.Distance(adventurer.transform.position, minotaur.transform.position);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestAdventurer = adventurer;
            }
        }
        return closestAdventurer;
    }

    private void Awake()
    {
        mapCorners = FindObjectsOfType<MapCorner>().ToList();
        gameAreas = FindObjectsOfType<GameArea>().ToList();
        adventurers = FindObjectsOfType<Adventurer>().ToList();
        minotaur = FindObjectOfType<Minotaur>();
        treasure = FindObjectOfType<Treasure>();
        isTreasurePickedUp = false;
        StartCoroutine(UpdateWorldState());

    }

    public async static Task RegisterWorldEvent(EventType type, Adventurer adventurer, GameObject target, CancellationTokenSource cts)
    {
        switch (type)
        {
            case EventType.TreasurePickedUp:
                {
                    while(!adventurer.HasTreasure() && !cts.IsCancellationRequested)
                    {
                        await Task.Yield();
                    }
                    isTreasurePickedUp = true;
                    return;
                }
            case EventType.TargetReached:
                {        
                    while (Vector3.Distance(adventurer.transform.position, target.transform.position) > 2.0f
                       && !cts.IsCancellationRequested)
                    {
                        await Task.Yield();
                    }
                    return;                  
                }
            case  EventType.AttackTarget:
                {
                    while(!adventurer.CanAttack() && !cts.IsCancellationRequested)
                    {
                        await Task.Yield();
                    }
                    adventurer.AttackTarget(target);        
                    return;
                }
        }
    }
}
