using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTN
{
    private CompoundTask root;
    private Adventurer adventurer;

    public HTN(Adventurer adventurer, GameManager gameManager)
    {
        this.adventurer = adventurer;
        BuildHTN(adventurer, gameManager);
    }

    //YES THIS IS UGLY, IDEALLY ONE WOULD MAKE A CUSTOM EDITOR FOR THIS, BUT LACK OF TIME
    public void BuildHTN(Adventurer adventurer, GameManager gameManager)
    {   
        //Primitive Tasks
        PrimitiveTask goTowardsMinotaur = new PrimitiveTask(adventurer.GoTowardsMinotaur);
        goTowardsMinotaur.Name = "Go Towards Minotaur";
        goTowardsMinotaur.AddPrecondition(adventurer.CanMove);
        PrimitiveTask notifyAdventurers = new PrimitiveTask(adventurer.NotifyAdventurers);
        notifyAdventurers.Name = "Notify Adventurers";
        notifyAdventurers.AddPrecondition(adventurer.CanMove);
        PrimitiveTask goTowardsTreasure = new PrimitiveTask(adventurer.GoTowardsTreasure);
        goTowardsTreasure.Name = "Go Towards Treasure";
        goTowardsTreasure.AddPrecondition(adventurer.CanMove);
        PrimitiveTask pickUpTreasure = new PrimitiveTask(adventurer.PickupTreasure);
        pickUpTreasure.Name = "Pick Up Treasure";
        pickUpTreasure.AddPrecondition(adventurer.CanMove);
        pickUpTreasure.AddPrecondition(adventurer.CanPickupTreasure);
        PrimitiveTask bringBackTreasure = new PrimitiveTask(adventurer.BringBackTreasure);
        bringBackTreasure.Name = "Bring Back Treasure";
        bringBackTreasure.AddPrecondition(adventurer.CanMove);
        bringBackTreasure.AddPrecondition(adventurer.HasTreasure);
        PrimitiveTask rest = new PrimitiveTask(adventurer.Rest);
        rest.AddPrecondition(adventurer.CanRest);
        rest.Name = "Rest";
        PrimitiveTask attackMinotaur = new PrimitiveTask(adventurer.AttackMinautor);
        attackMinotaur.Name = "Attack Minotaur";
        attackMinotaur.AddPrecondition(adventurer.CanAttack);
        PrimitiveTask goToNearestSafeArea = new PrimitiveTask(adventurer.GoToNearestSafeZone);
        goToNearestSafeArea.Name = "Go To Nearest Safe Area";
        goToNearestSafeArea.AddPrecondition(adventurer.CanMove);
        PrimitiveTask goToNearestAdventurer = new PrimitiveTask(adventurer.GoTowardsNearestAdventurer);
        goToNearestAdventurer.Name = "Go To Nearest Adventurer";
        goToNearestAdventurer.AddPrecondition(adventurer.CanMove);
        PrimitiveTask goToNearestRestArea = new PrimitiveTask(adventurer.GoToNearestRestArea);
        goToNearestRestArea.Name = "Go To Nearest Rest Area";
        goToNearestRestArea.AddPrecondition(adventurer.CanMove);

        //Compound Tasks 
        GameTask baitMinotaur = new CompoundTask(new List<GameTask>() { goTowardsMinotaur, notifyAdventurers, goToNearestSafeArea },
            new MovePotential(2, 3, 4, 5, 1));
        baitMinotaur.Name = "Bait Minotaur";
        GameTask recoverHealth = new CompoundTask(new List<GameTask>() { goToNearestRestArea, rest },
            new MovePotential(1, 1, 2, 1, 5));
        recoverHealth.Name = "Recover Health";
        GameTask defendAlly = new CompoundTask(new List<GameTask>() { goTowardsMinotaur, attackMinotaur },
            new MovePotential(4, 3, 3, 2, 1));
        defendAlly.Name = "Defend Ally";
        GameTask joinAdventurer = new CompoundTask(new List<GameTask>() { notifyAdventurers, goToNearestAdventurer },
             new MovePotential(2, 5, 3, 2, 1));
        joinAdventurer.Name = "Join Adventurer";
        GameTask hitAndRun = new CompoundTask(new List<GameTask>() { attackMinotaur, goToNearestSafeArea },
            new MovePotential(5, 4, 3, 2, 1));
        hitAndRun.Name = "Hit And Run";


        GameTask captureTreasure = new CompoundTask(new List<GameTask>() { goTowardsTreasure, pickUpTreasure, bringBackTreasure },
            new MovePotential(3, 5, 4, 2, 1));
        captureTreasure.Name = "Capture Treasure";
        GameTask attack = new CompoundTask(new List<GameTask>() { baitMinotaur, defendAlly },
            new MovePotential(5,3,2,3,1));
        attack.Name = "Attack";
        GameTask defend = new CompoundTask(new List<GameTask>() { joinAdventurer, recoverHealth, hitAndRun },
            new MovePotential(3, 4, 3, 2, 2));
        defend.Name = "Defend";
        root = new CompoundTask(new List<GameTask>() { attack, defend, captureTreasure },
            null);

        gameManager.tasks.Clear();
        gameManager.tasks.AddRange(new List<CompoundTask>() { (CompoundTask)root, (CompoundTask)attack, (CompoundTask)defend, (CompoundTask)captureTreasure,
            (CompoundTask)recoverHealth, (CompoundTask)defendAlly, (CompoundTask)joinAdventurer, (CompoundTask)hitAndRun, });
    }


    public void ExecuteHTN()
    {
        Stack<GameTask> taskStack = new Stack<GameTask>();
        taskStack.Push(root);

        while (taskStack.Count > 0)
        {
            GameTask currentTask = taskStack.Pop();

            if (currentTask.GetType() == typeof(CompoundTask))
            {
                // Select the best subtask based on the heuristic
                GameTask selectedSubtask = ((CompoundTask)currentTask).ChooseTask(adventurer);
                if (selectedSubtask != null)
                {
                    taskStack.Push(selectedSubtask);
                }
                foreach (var subtask in ((CompoundTask)currentTask).subtasks)
                {
                    if (subtask.GetType() == typeof(PrimitiveTask))
                    {
                        taskStack.Push(subtask);
                    }
                }
            }
            else
            {
                // If it's a primitive task, execute it
                adventurer.allTasks.Add(((PrimitiveTask)currentTask).Name);
                adventurer.plan.Push(((PrimitiveTask)currentTask));
            }
        }
    }

}
