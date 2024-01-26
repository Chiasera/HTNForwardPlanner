using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CanonBullet : Weapon
{
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            DealDamage(other.gameObject);
        }
    }
}
