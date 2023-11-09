using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Tower : MonoBehaviour
{
    // The tower is a game object that can be placed on the field.
    // It has children, and the more total children it has, the more damage it does, and the range also slightly increases.
    // Adding children does get more expensive though.
    // Also, if this tower gets killed, all children die.
    
    private float _health = 100;
    public float MaxHealth { get; private set; } = 100;
    private int _damage = 5;
    public float damageModifier = 1;
    private float _range = 1;
    public float Range => _range;

    public float Health
    {
        get => _health;
        set => _health = value;
    }

    // Gun cooldown
    private float _cooldown = 0.3f;
    public float cooldownTimer = 0f;

    public List<Tower> neighbours = new();
    public SpriteRenderer spriteRenderer;
    public List<SpriteRenderer> connections = new();

    public Vector2Int coordinates;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        float percentage = (float) _health / MaxHealth;
        Color color;
        color = percentage < 0.5f ? Color.Lerp(Color.red, Color.yellow, percentage) : Color.Lerp(Color.yellow, Color.green, (percentage - 0.5f));
        spriteRenderer.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        cooldownTimer -= Time.deltaTime;
    }

    public void SetDetails(int health, int damage, float range, float shootCooldown)
    {
        _health = health;
        MaxHealth = health;
        _damage = damage;
        _range = range;
        _cooldown = shootCooldown;
    }
    
    public void SetDetails((float, int, float, float) getDetails)
    {
        _health = getDetails.Item1;
        MaxHealth = getDetails.Item1;
        _damage = getDetails.Item2;
        _range = getDetails.Item3;
        _cooldown = getDetails.Item4;
    }
    
    public (float, int, float, float) GetDetails()
    {
        return (_health, _damage, _range, _cooldown);
    }

    public void UpdateTower()
    {
        // First, check if an enemy is within range.
        // If so, attack it.
        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        float distance;
        foreach (Enemy enemy in EnemyManager.Enemies)
        {
            distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;

                // Rotate towards the enemy
                Vector3 direction = enemy.transform.position - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        
        if (nearestDistance <= _range && cooldownTimer <= 0f)
        {
            distance = Vector2.Distance(transform.position, nearestEnemy!.transform.position);
            
            // Enable the laserbeam
            Transform laserBeam = transform.GetChild(0).transform.GetChild(0);
            laserBeam.gameObject.SetActive(true);
            laserBeam.localScale = new Vector3(distance, 1, 1);
            laserBeam.localScale /= transform.localScale.x;
                
            StartCoroutine(DisableLaserBeam(laserBeam));
            nearestEnemy!.TakeDamage((int)(_damage * damageModifier));
            cooldownTimer = _cooldown;
        }
    }

    private IEnumerator DisableLaserBeam(Transform laserBeam)
    {
        yield return new WaitForSeconds(0.1f);
        laserBeam.gameObject.SetActive(false);
    }

    public void TakeDamage(float f)
    {
        _health -= f / Mathf.Pow(neighbours.Count + 1, 0.2f);
        
        // Change color from green, to yellow, to orange to red
        float percentage = (float) _health / MaxHealth;
        Color color;
        color = percentage < 0.5f ? Color.Lerp(Color.red, Color.yellow, percentage) : Color.Lerp(Color.yellow, Color.green, (percentage - 0.5f));
        spriteRenderer.color = color;
        
        // Update all neighbours health to equal the percentage of health this tower has
        foreach (Tower neighbour in neighbours)
        {
            neighbour._health = (int) (neighbour.MaxHealth * percentage);
            neighbour.spriteRenderer.color = color;
        }
        
        // Update all the connection colours to go from green to red
        foreach (SpriteRenderer connection in connections)
        {
            connection.color = color;
        }
        
        if (_health <= 0)
        {
            TowerManager.DestoryTower(this);
        }
    }

}