using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : Weapon
{
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            DealDamage(other.gameObject);
        }
    }
}
