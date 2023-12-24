using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void DamageToEnemy(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Death();
    }

    private void Death()
    {
        Destroy(gameObject);
    }
}
