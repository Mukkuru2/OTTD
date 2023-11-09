using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public float damage = 10f;
    public float attackSpeed = 1f;
    public float attackRange = 1f;
    public int reward = 100;
    public float rewardPercentage = 1f;
    
    private EnemyState _state;
    private bool[,] _visitedTiles;
   
    private Vector2Int _currentTile;
    private Vector2Int _targetTile;
    
    private Vector2Int TargetTile
    {
        set
        {
            _targetTile = value;
            _targetPosition = FieldGenerator.Field[_targetTile.x, _targetTile.y].transform.position;
        }
    }
    
    private Vector2 _targetPosition;
    private Vector2 _direction;
    private float _baseSpeed = 1f;

    private SpriteRenderer[] sprites;
    private float cooldownTimer = 0f;

    public enum EnemyState
    {
        Moving,
        Attacking,
        Paused
    }
    
    public void Create(Vector2Int startTile)
    {
        _currentTile = startTile;
        _visitedTiles = new bool[FieldGenerator.Field.GetLength(0), FieldGenerator.Field.GetLength(1)];
        _visitedTiles[ startTile.x, startTile.y] = true;
        TargetTile = FindTarget();
    }
    
    void Update()
    {
        cooldownTimer -= Time.deltaTime;
        
        // Reduce the reward percentage by 50% every 30 seconds
        rewardPercentage -= Time.deltaTime / 30f * 0.5f;
        if (rewardPercentage < 0f) rewardPercentage = 0f;
    }
    
    public void Start()
    {
        _state = EnemyState.Moving;
        sprites = GetComponentsInChildren<SpriteRenderer>();
    }

    public void UpdateEnemy()
    {
        switch (_state)
        {
            case EnemyState.Moving:
                Move(_baseSpeed);
                CheckNextGameState();
                break;
            case EnemyState.Attacking:
                Move(_baseSpeed * 0.2f);
                Attack();
                break;
            case EnemyState.Paused:
                break;
        }
    }

    private void Attack()
    {
        if (!CheckIfTowerInRange())
        {
            _state = EnemyState.Moving;
            transform.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, 0);
            return;
        }
        Aim();
    }

    private void Aim()
    {
        GameObject tower = GetTowerInRange();
        
        // Make barrel aim towards tower
        Vector2 direction = tower.transform.position - transform.position;
        direction.Normalize();
        
        // Get barrel object
        GameObject barrel = transform.GetChild(1).gameObject;
        barrel.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    public void TakeDamage(int damageTaken)
    {
        health -= damageTaken;
        
                
        // Check if the enemy is dead
        if (health <= 0)
        {
            // Destroy the enemy
            EnemyManager.RemoveEnemy(this);
            return;
        }
        
        // Flash all sprites red for a split second
        StopCoroutine(FlashSpritesRed());
        StartCoroutine(FlashSpritesRed());

    }

    private IEnumerator FlashSpritesRed()
    {
        const float flashTime = 0.05f;
        const float nFlashFrames = 2f;
        for (int i = 0; i <= nFlashFrames; i++)
        {
            foreach (SpriteRenderer sprite in sprites)
            {
                sprite.color = new Color(1, 0.5f + 0.5f * i / nFlashFrames, 0.5f + 0.5f * i / nFlashFrames);
            }
            yield return new WaitForSeconds(flashTime / nFlashFrames);
        }
    }

    private void Move(float speed)
    {
        // Check if the enemy has reached the target
        if (Vector2.Distance(transform.position, _targetPosition) < 0.01f)
        {
            // Check if the enemy has reached the end
            if (FieldGenerator.Field[_targetTile.x, _targetTile.y].CompareTag("End"))
            {
                // Destroy the enemy
                EnemyManager.EnemyFinished(this);
                return;
            }
            
            // Set the new position to correct for errors
            transform.position = _targetPosition;
            
            // Update the current tile
            _currentTile = _targetTile;
            
            // Set the current tile to visited
            _visitedTiles[_currentTile.x, _currentTile.y] = true;
            
            // Find a new target
            TargetTile = FindTarget();
            
            // Calculate the direction
            _direction = _targetPosition - (Vector2) transform.position;
            _direction.Normalize();
        }
        
        // Move the enemy
        transform.position = Vector2.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);
    }

    private void CheckNextGameState()
    {
        // Check if the enemy is in range of a tower
        if (CheckIfTowerInRange())
        {
            _state = EnemyState.Attacking;
        }
    }

    private bool CheckIfTowerInRange()
    {
        // Check if there is a tower in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.CompareTag("Tower"))
            {
                return true;
            }
        }
        
        return false;
    }

    private GameObject GetTowerInRange()
    {
        // First, check if a tower is within range.
        // If so, attack it.
        Tower nearestTower = null;
        float nearestDistance = float.MaxValue;
        foreach (Tower tower in TowerManager.towers)
        {
            float distance = Vector2.Distance(transform.position, tower.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTower = tower;

                // Rotate towards the enemy
                Vector3 direction = tower.transform.position - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        if (nearestDistance <= attackRange && cooldownTimer <= 0f)
        {
            nearestTower!.TakeDamage(damage);
            cooldownTimer = attackSpeed;
            
            transform.GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(true);
            StartCoroutine(DisableShot());
        }
        
        return nearestTower.gameObject;
    }

    private IEnumerator DisableShot()
    {
        yield return new WaitForSeconds(0.1f);
        transform.GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(false);
    }

    private Vector2Int FindTarget()
    {
        // Find an adjacent path tile that is not visited
        
        // Check the adjacent tiles
        Vector2Int[] adjacentTiles = new Vector2Int[4];
        adjacentTiles[0] = new Vector2Int(_currentTile.x + 1, _currentTile.y);
        adjacentTiles[1] = new Vector2Int(_currentTile.x - 1, _currentTile.y);
        adjacentTiles[2] = new Vector2Int(_currentTile.x, _currentTile.y + 1);
        adjacentTiles[3] = new Vector2Int(_currentTile.x, _currentTile.y - 1);
        
        // Loop through the adjacent tiles
        foreach (Vector2Int adjacentTile in adjacentTiles)
        {
            // Check if the tile is in the field
            if (adjacentTile.x < 0 || adjacentTile.x >= FieldGenerator.Field.GetLength(0) ||
                adjacentTile.y < 0 || adjacentTile.y >= FieldGenerator.Field.GetLength(1))
            {
                continue;
            }
            
            GameObject fieldTile = FieldGenerator.Field[adjacentTile.x, adjacentTile.y];
            
            // Check if the tile is a path tile or the end tile
            if (fieldTile.CompareTag("Path") || fieldTile.CompareTag("End"))
            {
                // Check if the tile is visited
                if (!_visitedTiles[adjacentTile.x, adjacentTile.y])
                {
                    // Calculate the direction
                    _direction = FieldGenerator.Field[adjacentTile.x, adjacentTile.y].transform.position - transform.position;
                    _direction.Normalize();
                    
                    // Rotate according to direction
                    transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg);
                    
                    // Return the adjacent tile
                    return adjacentTile;
                }
            }
        }
        
        return Vector2Int.zero;
    }

    public void SetState(EnemyState enemystate)
    {
        _state = enemystate;
    }
}