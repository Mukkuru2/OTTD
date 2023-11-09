using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class LevelState : MonoBehaviour
{
    public EnemyManager enemyManager;
    public TowerManager towerManager;
    
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI healthText;

    public GameObject introUI;


    public enum GameState
    {
        Running,
        Placing,
        GameOver,
        Won,
        Start
    }

    public static GameState State = GameState.Start;
    public static UnityEvent StateChanged = new();

    public void Start()
    {
        SetState(GameState.Start);
        moneyText.text = $"Money: {Economy.Money}";
        healthText.text = $"Health: {Economy.Health}";
    }

    public static void SetState(GameState state)
    {
        State = state;
        StateChanged.Invoke();
    }

    public void StartGame()
    {
        introUI.SetActive(false);
        SetState(GameState.Placing);
    }

    public void Update()
    {
        switch (State)
        {
            case GameState.Running:
                towerManager.UpdateTowers();
                UpdateEconomy();
                break;
            case GameState.Placing:
                towerManager.ShowPreview();
                CheckCancel();
                break;
            case GameState.GameOver:
                break;
            case GameState.Won:
                break;
            case GameState.Start:
                break;
        }
    }

    private void CheckCancel()
    {
        // If right mouse button or esc is pressed, cancel the tower placement
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            SetState(GameState.Running);
        }
    }

    private void UpdateEconomy()
    {
        // Call the Economy.Update function if one second has passed since the last time that happened

        moneyText.text = $"Money: {Economy.Money}";
        healthText.text = $"Health: {Economy.Health}";
        
    }

    public void FixedUpdate()
    {
        switch (State)
        {
            case GameState.Running:
                enemyManager.FixedUpdateEnemies();
                break;
            case GameState.Placing:
                break;
            case GameState.GameOver:
                break;
            case GameState.Won:
                break;
        }
    }
}