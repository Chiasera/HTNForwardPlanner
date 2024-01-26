using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class MovePotential
{
    public float Attack;
    public float Move;
    public float Flee;
    public float Bait;
    public float Idle;

    public float GetAverage()
    {
        return (Attack + Move + Flee + Bait + Idle)/5;
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

[System.Serializable]
public abstract class GameTask
{
    public string name;
    public string Name { get => name; set => name = value; }
    public abstract bool IsPrimitive();
}

public delegate Task Operator();

public class PrimitiveTask : GameTask
{
    private List<Func<bool>> preconditions;
    private Operator action;

    // Method to add a precondition
    public void AddPrecondition(Func<bool> precondition)
    {
        preconditions.Add(precondition);
    }

    public PrimitiveTask(Operator action)
    {
        preconditions = new List<Func<bool>>();
        this.action = action;
    }

    private bool ArePreconditionsMet()
    {
        foreach (var precondition in preconditions)
        {
            if (!precondition())
            {
                return false;
            }
        }
        return true;
    }
    public override bool IsPrimitive()
    {
        return true;
    }

    public async Task<bool> TryExecuteAction()
    {
        if (ArePreconditionsMet())
        {
            await action();
            return true;
        } else
        {
            return false;
        }
    }
}

[System.Serializable]
public class CompoundTask : GameTask
{
    public MovePotential MovePotential;

    public GameTask ChooseTask(Adventurer adventurer)
    {
        //choose random task
        List<CompoundTask> compoundSubTasks = new List<CompoundTask>();
        foreach (var subtask in subtasks)
        {
            if (!subtask.IsPrimitive())
            {
                CompoundTask task = (CompoundTask)subtask;
                compoundSubTasks.Add(task);
                //if no one is on the treasure objective, go for it
                if(WorldState.onTreasureQuestAdventurer == null)
                {
                    WorldState.onTreasureQuestAdventurer = adventurer;
                    return task;
                }
            }
        }
        if(compoundSubTasks.Count > 0)
        {
            return GetBestMatchingTask(adventurer, compoundSubTasks);
        } else
        {
            return null;
        }
    }

    public float CalculateSimilarityScore(MovePotential mp1, MovePotential mp2, Adventurer adventurer)
    {
        // Optional: Weights for each attribute based on their importance
        float attackWeight = 1.0f;
        float moveWeight = 1.0f;       
        float fleeWeight = 1.0f;
        float baitWeight = 1.0f;
        float idleWeight = 1.0f;
        if (WorldState.TreasurePickerAdventurer == null)
        {
            moveWeight = 2.0f;
            attackWeight = 0.5f;
        } else if (WorldState.AssassinAdventurer == null)
        {
            attackWeight = 2.0f;
            moveWeight = 0.5f;
        } else if (WorldState.AssassinAdventurer != null && WorldState.AssassinAdventurer != adventurer)
        {
            attackWeight = 0.5f;
            moveWeight = 1.5f;
        } else if(WorldState.AssassinAdventurer == adventurer)
        {
            attackWeight = 1.5f;
            moveWeight = 0.5f;
        } else if(WorldState.TreasurePickerAdventurer != null && WorldState.TreasurePickerAdventurer != adventurer)
        {
            moveWeight = 0.5f;
            attackWeight = 1.5f;
        } else if(WorldState.TreasurePickerAdventurer == adventurer)
        {
            moveWeight = 1.5f;
            attackWeight = 0.5f;
        }
        // Calculate the difference for each attribute
        float attackDiff = Mathf.Abs(mp1.Attack - mp2.Attack) * attackWeight;
        float moveDiff = Mathf.Abs(mp1.Move - mp2.Move) * moveWeight;
        float fleeDiff = Mathf.Abs(mp1.Flee - mp2.Flee) * fleeWeight;
        float baitDiff = Mathf.Abs(mp1.Bait - mp2.Bait) * baitWeight;
        float idleDiff = Mathf.Abs(mp1.Idle - mp2.Idle) * idleWeight;

        // Sum the differences
        float totalDiff = attackDiff + moveDiff + fleeDiff + baitDiff + idleDiff;

        // Calculate similarity (higher score means more similar)
        float similarityScore = 1 / (1 + totalDiff); // Ensure the score is always positive

        return similarityScore;
    }

    private CompoundTask GetBestMatchingTask(Adventurer adventurer, List<CompoundTask> compoundSubTasks)
    {
        CompoundTask candidate = null;
        float bestSimilarity = 0;
        foreach (var subtask in compoundSubTasks)
        {
            float similarity = CalculateSimilarityScore(adventurer.MovePotential, subtask.MovePotential, adventurer);
            if (similarity > bestSimilarity)
            {
                bestSimilarity = similarity;
                candidate = subtask;
            }
        }
        return candidate;
    }

    public List<GameTask> subtasks;
    public override bool IsPrimitive()
    {
        return false;
    }
    public CompoundTask(List<GameTask> subtasks, MovePotential properties)
    {
        this.MovePotential = properties;
        this.subtasks = subtasks;
    }
}

