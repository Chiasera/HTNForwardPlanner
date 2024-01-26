using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameAreaType
{
    Safe,
    Dangerous,
    Rest,
    Strategic,
}
public class GameArea : MonoBehaviour
{
    public static float safeDistance = 4.0f;
    public static float restDistance = 8.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float radius = 2.0f;
    [SerializeField]
    private GameAreaType areaType;
    public GameAreaType AreaType { get => areaType; }
    
    private void OnDrawGizmos()
    {
       switch(areaType)
        {
            case GameAreaType.Safe:
                Gizmos.color = Color.green;
                break;
            case GameAreaType.Dangerous:
                Gizmos.color = Color.red;
                break;
            case GameAreaType.Rest:
                Gizmos.color = Color.blue;
                break;
            case GameAreaType.Strategic:
                Gizmos.color = Color.yellow;
                break;
        }
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void Update()
    {
        UpdateAreaType();
    }

    public void UpdateAreaType()
    {
        //Area types should be updated and sorted according to their distance to either the minautor, the treasure or the adventurers
        float distanceToTreasure = Vector3.Distance(WorldState.Treasure.transform.position, transform.position);
        float distanceToMinotaur = Vector3.Distance(WorldState.Minotaur.transform.position, transform.position);

        //If the area is safe and the treasure is in the area, it is strategic
        if(distanceToTreasure < radius && distanceToMinotaur > safeDistance)
        {
            areaType = GameAreaType.Strategic;
        }
        //If there is a minautor in the area, it is not safe
        if(distanceToMinotaur < safeDistance)
        {
            areaType = GameAreaType.Dangerous;
        }
        //Being near the treausre is not safe as the minautor will target the adventurer
        if (distanceToMinotaur > safeDistance && distanceToTreasure > safeDistance)
        {
            areaType = GameAreaType.Safe;
        }
        if (distanceToMinotaur > restDistance && distanceToTreasure > safeDistance)
        {
            areaType = GameAreaType.Rest;
        }
    }
}
