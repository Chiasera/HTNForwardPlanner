using System;
using System.ComponentModel;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    [Range(0.1f, float.PositiveInfinity)]
    private float maxHealth = 1f;
    public float MaxHealth { get => maxHealth; }
    public float health;
    public float HealthValue { get => health; set => health=value; }
    public Action OnDeath;
    public Action OnDamage;
    private GameObject damageDealer;
    public GameObject DamageDealer { get => damageDealer; }


    private void Awake()
    {
        health = maxHealth;
    }
    public void TakeDamage(float damage, GameObject sender)
    {
        if(health != float.PositiveInfinity)
        {
            health -= damage;
        }
        if (health <= 0)
        {
            OnDeath?.Invoke();
        }
        else
        {
            damageDealer = sender;
            OnDamage?.Invoke();
        }
    } 
    
    public GameObject GetLastDamadeDealer()
    {
        return damageDealer;
    }

    public void Heal(float heal)
    {
        health += heal;
    }
}
