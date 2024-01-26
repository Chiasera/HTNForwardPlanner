# HTN Design

This section outlines the structure of the game state for the Hierarchical Task Network (HTN) forward planner.
In this game, 4 adventurers are spawned, one on each corner of the map. On the center of the map is a "minautor". Its role is the guard the treasure. The 4 adventurers job is to team up and coordinate so that one of them takes the treasure and brings it back to one corner of the map for the win. Below is a more detailed description of the implementation

## Game State Definition

The game state is a representation of all the necessary parameters at a given point in time. It should include:

### Important Parameters

- **Adventurer's Position:** The current location of the adventurer within the game world.
- **Adventurer's Health:** The health status of the adventurer, which may affect their ability to perform tasks.
- **Minotaur's State:**
  - *Position:* The location of the Minotaur.
  - *Distance from Player:* How far the Minotaur is from the player's character.
- **Treasure Location:** The fixed point where the treasure is located.
- **Nearest Adventurer to Treasure:** Identifies which adventurer is closest to the treasure, potentially to prioritize their actions.
- **Targeted Adventurer:** Specifies which adventurer the Minotaur is currently targeting.

### Strategic Planning

Key points, or "nodes," will be strategically placed across the playground. These nodes will guide adventurers in choosing their plans and prioritizing their tasks effectively.
![image](https://github.com/Chiasera/HTNForwardPlanner/assets/70693638/ef363313-ce3f-4e9b-9227-c341ba1e793b)

# Minotaur AI Behavior

## Overview

This document provides a brief overview of the AI behavior logic for the Minotaur character within the game.

## Behavior Summary

- **Idle State**: The Minotaur scouts its surroundings when idle.
- **Noise Detection**: If a player enters its detection radius, the Minotaur interprets it as "noise" and turns towards the source. If the target is confirmed, the Minotaur will jump towards it and maintain focus.
- **Priority Levels**: The Minotaur's actions are driven by a set of priority levels:
  - **LOW**: Engaged in scouting.
  - **NORMAL**: Reacts when an adventurer enters its radius or when it's attacked.
  - **IMPORTANT**: Actively pathfinds towards an adventurer who gets too close to the treasure.
  - **IMMINENT**: Focuses on an adventurer carrying the treasure, with the top priority to make them drop the treasure.

## Implementation

The following C# script outlines the basic startup logic for the Minotaur's AI in Unity:

```csharp
void Start()
{
    health = GetComponent<Health>();
    // Minotaur can't die, so no need to subscribe to OnDeath
    health.OnDamage += OnDamage;
    rb = GetComponent<Rigidbody>();
    adventurers = FindObjectsOfType<Adventurer>().ToList();
    adventurersState = new Dictionary<GameObject, ActionPriority>();
    StartCoroutine(ComputePathToTreasure());
    StartCoroutine(ScoutSurroundings());
    StartCoroutine(IdleBehaviour());
}

public enum ActionPriority
{
    IMMINENT = 4, IMPORTANT = 3, NORMAL = 2, LOW = 1
}
```
# For the Adventurers

## Overview

At the beginning of the adventure, the adventurers set off on boats while the Minotaur starts at the center of the map. The adventurers make strategic decisions based on a variety of parameters, choosing one plan over another.

## Cooperation and Decision Making

Global knowledge of the world state is leveraged by the adventurers to cooperate effectively. This shared understanding allows them to operate in concert without the need for direct communication between each other.

## AI and HTN Design

The AI and Hierarchical Task Network (HTN) planning for the adventurers is somewhat more complex. The system is designed to handle multiple factors and make intelligent choices based on the current state of the world.

### HTN for an Adventurer

The HTN for a given adventurer is detailed below, outlining the decision process and task hierarchy that guides the adventurer's behavior.

![image](https://github.com/Chiasera/HTNForwardPlanner/assets/70693638/8bc40826-2488-495b-bffb-36c149d4987e)

The above structure provides a framework for the adventurer's AI, enabling a certain level of autonomous operation within the game environment.

# Adventurers' AI and HTN Planning

## Preconditions for Actions

The Adventurers' AI is designed with specific preconditions for each action:

- **Move Towards Target**: Must be able to move (`CanMove`)
- **Notify Other Adventurers**: Must be alive
- **Move to Nearest Safe Zone**: Must be able to move (`CanMove`)
- **Approach the Minotaur**: Must be able to move (`CanMove`)
- **Attack the Minotaur**: Must be able to attack (`CanAttack`)
- **Approach Nearest Adventurer**: Must be able to move (`CanMove`)
- **Go to Nearest Rest Place**: Must be able to move (`CanMove`)
- **Rest**: Subject to a cooldown of 3 seconds
- **Pathfind to Treasure**: Must be able to move (`CanMove`)
- **Pick Up Treasure**: Must be able to pick up the treasure (subject to cooldown and proximity)
- **Deliver Treasure to Nearest Corner**: Must have the treasure (`HasTreasure`)

## Postconditions

None

## HTN Design

The Hierarchical Task Network (HTN) for the Adventurers is based on evaluating `MovePotential`, which influences the decision-making for compound tasks. Here is an overview of the design pattern and C# implementation details:

### MovePotential

Each task is associated with a `MovePotential` that defines the heuristic for the decision-making process. It encapsulates the parameters influencing whether a particular action should be taken.

### CompoundTask Class

```csharp
public class CompoundTask : GameTask
{
    public MovePotential MovePotential;

    public GameTask ChooseTask(Adventurer adventurer)
    {
        // Choose random task
        List<CompoundTask> compoundSubTasks = new List<CompoundTask>();
        foreach (var subtask in subtasks)
        {
            if (subtask.GetType() == typeof(CompoundTask))
            {
                compoundSubTasks.Add((CompoundTask)subtask);
            }
        }
        
        if (compoundSubTasks.Count > 0)
        {
            return GetBestMatchingTask(adventurer, compoundSubTasks);
        }
        else
        {
            return null;
        }
    }
}

public class MovePotential
{
    public float Attack;
    public float Move;
    public float Flee;
    public float Bait;
    public float Idle;

    public float GetAverage()
    {
        return (Attack + Move + Flee + Bait + Idle) / 5;
    }

    public MovePotential(float attack, float move, float flee, float bait, float idle)
    {
        Attack = attack;
        Move = move;
        Flee = flee;
        Bait = bait;
        Idle = idle;
    }
}
```

# Action Planning and Execution

Actions are appended to a plan and tracked for later execution. This involves passing an asynchronous delegate into our plan to enable awaiting upon execution. Below is the code snippet handling the action executions. The `WorldState` class, attached to the game manager, plays a pivotal role by providing a vector for the adventurer and facilitating action-target-source relationships.

## Registering Actions

Each action related to a task is first registered. Here is an example of how a primitive task is registered:

```csharp
// Primitive Tasks
PrimitiveTask goTowardsMinotaur = new PrimitiveTask(adventurer.GoTowardsMinotaur);
goTowardsMinotaur.Name = "Go Towards Minotaur";
goTowardsMinotaur.AddPrecondition(adventurer.CanMove);
```
## Executing Actions in Orde
Actions are then added to the plan and executed asynchronously in the specified order:
``` csharp
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

public async Task GoTowardsMinotaur()
{
    WorldState.AssassinAdventurer = this;
    if(WorldState.Minotaur == null)
    {
        WorldState.Minotaur = FindObjectOfType<Minotaur>();
        BoatAgent.Target = WorldState.Minotaur.transform;
        await WorldState.RegisterWorldEvent(EventType.TargetReached, this, WorldState.Minotaur.gameObject, cts);
    }
}
```
Each action waits for the completion of the previous one by using asynchronous programming patterns, ensuring the plan is executed in the correct sequence.

# Action Termination and Plan Visualization

Once actions are initiated, it is crucial to await their completion before proceeding. Below is an example of how to implement a wait for an event to terminate in the world state system.

```csharp
public async static Task RegisterWorldEvent(EventType type, Adventurer adventurer, GameObject target, CancellationTokenSource cts)
{
    // Implementation details...
}
```

## Editor Visualization
In Unity's Editor, one can observe the planned actions and current action of a character. It's important to note that the actions are displayed in a stack, meaning the most recent action is shown at the top.
![image](https://github.com/Chiasera/HTNForwardPlanner/assets/70693638/875e524e-8983-4494-b7b3-40c885306e60)


## Final Considerations
Important: The current simulation model has limitations. The computations for similarity and probability distributions are not accurately representing the decision-making process an agent should ideally perform. Consequently, agents in the simulation tend to uniformly attack the Minotaur, which may not be the desired behavior.

Please take this into account when evaluating the system's performance and consider it when planning further improvements or iterations of the AI logic.
