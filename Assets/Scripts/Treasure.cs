using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    private Minotaur minautor;
    private bool isPickedUp;
    public bool IsPickedUp { get => isPickedUp; set => isPickedUp = value; }

    private void Awake()
    {
        minautor = FindObjectOfType<Minotaur>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Adventurer")
        {
            minautor.OnTreasureZoneEntered();
        }
    }

    public void PickUp(Adventurer adventurer)
    {
        isPickedUp = true;
        minautor.OnTreasurePickedUp(adventurer.gameObject);
    }

    public void Drop(GameObject adventurer)
    {
        isPickedUp = false;
        minautor.OnTreasureDropped(adventurer.gameObject);
    }
}
