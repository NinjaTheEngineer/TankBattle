using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Elements")]
    [SerializeField] private PlayerTank playerTank;
    [SerializeField] private AITank aiTank;

    [Header("UI Elements")]
    [SerializeField] private GameObject uiStart;
    [SerializeField] private GameObject uiEnd;
    [SerializeField] private TextMeshProUGUI guideText;
    [SerializeField] private TextMeshProUGUI endText;

    public enum GameState { WaitingForStart, PlayerPickPosition, AIPickPosition, PlayerMove, PlayerAim, PlayerFire, AIMove, AIAim, AIFire, GameOver }
    private GameState currentState;

    public GameState CurrentState
    {
        get => currentState;
        private set
        {
            if (currentState != value)
            {
                Debug.Log($"Game State changed: {currentState} → {value}");
                currentState = value;
            }
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
            Debug.LogWarning("Duplicate GameManager detected and destroyed.");
            DestroyImmediate(gameObject);
        }
    }

    private void Start()
    {
        guideText.text = string.Empty;
        CurrentState = GameState.WaitingForStart;
    }

    private void Update()
    {
        if (CurrentState == GameState.WaitingForStart && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }
    private void StartGame()
    {
        Debug.Log("Game Started");
        playerTank.OnTankDestroyed += HandleDefeat;
        aiTank.OnTankDestroyed += HandleVictory;

        uiStart.SetActive(false);
        guideText.text = "Pick your starting position";
        CurrentState = GameState.PlayerPickPosition;
        playerTank.EnablePositionPicking();

        StartCoroutine(GameLoop());
    }
    private IEnumerator GameLoop()
    {
        while (CurrentState != GameState.GameOver)
        {
            switch (CurrentState)
            {
                case GameState.PlayerPickPosition:
                    yield return new WaitUntil(playerTank.HasPickedPosition);
                    AdvanceToState(GameState.AIPickPosition, "AI picking position...");
                    aiTank.PickRandomPosition();
                    yield return new WaitUntil(aiTank.ReachedDestination);
                    AdvanceToState(GameState.PlayerMove, "Pick new position or press 'Space' to stay.");
                    break;

                case GameState.PlayerMove:
                    if (playerTank == null) break;
                    playerTank.ActivateMovement();
                    yield return StartCoroutine(WaitForPlayerToReachDestination());
                    AdvanceToState(GameState.PlayerAim, "Aim with 'A'/'D', Power with 'W'/'S', 'Space' to fire.");
                    break;

                case GameState.PlayerAim:
                    playerTank.EnableAiming();
                    yield return new WaitUntil(playerTank.HasAimed);
                    AdvanceToState(GameState.PlayerFire);
                    break;

                case GameState.PlayerFire:
                    playerTank.Fire();
                    yield return new WaitUntil(playerTank.ShotComplete);
                    AdvanceToState(GameState.AIMove, "AI moving...");
                    break;

                case GameState.AIMove:
                    if (aiTank == null) break;
                    aiTank.MoveToStrategicPosition();
                    yield return StartCoroutine(WaitForAIToReachDestination());
                    AdvanceToState(GameState.AIAim, "AI aiming...");
                    break;

                case GameState.AIAim:
                    float aimStartTime = Time.realtimeSinceStartup;
                    float maxAimDuration = aiTank.MaxAimDuration;
                    while (!aiTank.IsAimedAtPlayer() && Time.realtimeSinceStartup - aimStartTime < maxAimDuration)
                    {
                        aiTank.AimAtPlayer();
                        yield return null;
                    }
                    AdvanceToState(GameState.AIFire);
                    break;

                case GameState.AIFire:
                    aiTank.Fire();
                    yield return new WaitUntil(aiTank.ShotComplete);
                    guideText.text = string.Empty;
                    AdvanceToState(GameState.PlayerMove);
                    break;
            }
            yield return null;
        }
    }
    private void HandleVictory()
    {
        endText.text = "Victory!";
        EndGame("Player wins!");
    }
    private void HandleDefeat()
    {
        endText.text = "Defeat..";
        EndGame("AI wins.");
    }
    private IEnumerator RestartGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Restarting game...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void EndGame(string logMessage)
    {
        Debug.Log(logMessage);
        uiEnd.SetActive(true);
        CurrentState = GameState.GameOver;

        playerTank.OnTankDestroyed -= HandleDefeat;
        aiTank.OnTankDestroyed -= HandleVictory;
        StartCoroutine(RestartGameAfterDelay(5f));
    }
    private void AdvanceToState(GameState newState, string guideMessage = "")
    {
        guideText.text = guideMessage;
        CurrentState = newState;
    }
    private IEnumerator WaitForPlayerToReachDestination() => new WaitUntil(playerTank.ReachedDestination);
    private IEnumerator WaitForAIToReachDestination() => new WaitUntil(aiTank.ReachedDestination);
}