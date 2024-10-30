using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerTank playerTank;
    public AITank aiTank;
    public enum GameState { PlayerMove, PlayerAim, PlayerFire, AIMove, AIAim, AIFire, GameOver }
    private GameState currentState;
    public GameState CurrentState {
        get => currentState;
        set
        {
            if (currentState != value)
            {
                Debug.Log("Changing CurrentState from "+ currentState + " to "+ value);
            }
            currentState = value;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else
        {
            DestroyImmediate(Instance);
        }
    }

    private void Start()
    {
        currentState = GameState.PlayerMove;
        StartCoroutine(TurnLoop());
    }

    private IEnumerator TurnLoop()
    {
        while (currentState != GameState.GameOver)
        {
            switch (currentState)
            {
                case GameState.PlayerMove:
                    playerTank.ActivateMovement();
                    yield return StartCoroutine(WaitForPlayerToReachDestination());
                    CurrentState = GameState.PlayerAim;
                    break;

                case GameState.PlayerAim:
                    playerTank.EnableAiming(); // Placeholder for player aiming logic
                    yield return new WaitUntil(() => playerTank.HasAimed());
                    CurrentState = GameState.PlayerFire;
                    break;

                case GameState.PlayerFire:
                    playerTank.Fire();
                    yield return new WaitUntil(() => playerTank.ShotComplete());
                    CurrentState = GameState.AIMove;
                    break;

                case GameState.AIMove:
                    aiTank.MoveToRandomPosition();
                    //yield return new WaitUntil(() => aiTank.ReachedDestination());
                    CurrentState = GameState.AIAim;
                    break;

                case GameState.AIAim:
                    //aiTank.AimAtPlayer(); // Placeholder for AI aiming logic
                    yield return new WaitForSeconds(1f); // Simulate AI aiming time
                    CurrentState = GameState.AIFire;
                    break;

                case GameState.AIFire:
                    //aiTank.Fire();
                    //yield return new WaitUntil(() => aiTank.ShotComplete());
                    CurrentState = GameState.PlayerMove;
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator WaitForPlayerToReachDestination()
    {
        yield return new WaitUntil(() => playerTank.ReachedDestination());
        //playerTank.enabled = false;
    }
}