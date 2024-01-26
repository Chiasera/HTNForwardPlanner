using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldState))]
public class GameManager : MonoBehaviour
{
    public List<CompoundTask> tasks = new List<CompoundTask>();
}
