using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected GameObject yielder;
    public void DealDamage(GameObject target)
    {
        //Send sender reference to the victim 
        target.GetComponent<Health>().TakeDamage(1, yielder);
    }

    public void SetYielder(GameObject weaponYielder)
    {
        this.yielder = weaponYielder;
    }
}
