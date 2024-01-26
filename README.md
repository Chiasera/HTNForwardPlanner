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


