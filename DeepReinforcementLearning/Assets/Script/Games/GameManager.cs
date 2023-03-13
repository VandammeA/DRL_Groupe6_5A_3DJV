using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Games;
using ReinforcementLearning;
using Script.ReinforcementLearning.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public enum SolvingAlgorithm
{
    ValueIteration, PolicyIteration, MonteCarlo,MonteCarloES, Sarsa, QLearning
}
public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    public IStateDelegate stateDelegate;

    [SerializeField] private SolvingAlgorithm algorithm = SolvingAlgorithm.ValueIteration;
    
    [SerializeField] private AiAgent player;
    [SerializeField] private Transform groundPlane;
    [SerializeField] public ArrowsFromGrid arrowsManager;

    [SerializeField] private Canvas successCanvas;
    [SerializeField] private SolverCanvas solverCanvas;

    private GameGrid gameGrid;
    private List<Movement> moves;
    
    private void Awake() {
        if (_instance != null) {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void Start() {
        successCanvas.gameObject.SetActive(false);
        gameGrid = new GameGrid();
        gameGrid.Init(groundPlane, arrowsManager);

    }

    public void SolveGame()
    {
        var stopwatch = new Stopwatch();
        
        stopwatch.Start();
        
        switch (algorithm)
        {
            case SolvingAlgorithm.ValueIteration:
                moves = DynamicProgramming.ValueIteration(player, gameGrid);
                break;
            case SolvingAlgorithm.PolicyIteration:
                moves = DynamicProgramming.PolicyIteration(player, gameGrid);
                break;
            case SolvingAlgorithm.MonteCarlo:
                moves = MonteCarlo.FirstVisitMonteCarloPredictionWithExploringStart(player, gameGrid, 100, 100);
                break;
            case SolvingAlgorithm.MonteCarloES:
                moves = MonteCarloES.MonteCarloControl(player, gameGrid, 100, 100);
                break;
            default:
                
                moves = new List<Movement>();
                break;
        }
        
        stopwatch.Stop();
        float timeElapsed = (stopwatch.ElapsedMilliseconds / 1000f);
        Debug.Log("Time : " + timeElapsed + " s with " + moves.Count +" Moves" );
        solverCanvas.Solved(timeElapsed, stateDelegate.GetStateCount(),moves.Count);
    }

    public void StartAnimatingGame()
    {
        StartCoroutine(MovePlayer(moves));
    }

    private IEnumerator MovePlayer(List<Movement> moves) {
        var wait = new WaitForSeconds(0.25f);
        foreach (var move in moves) {
            yield return wait;
            Vector2 dir = GetDirection(move);
            player.Move(dir);
        }
        yield return null;
    }

    private Vector2 GetDirection(Movement move) {
        switch (move) {
            case Movement.Down:
                return new Vector2(0, -1);
            case Movement.Left:
                return new Vector2(-1, 0);
            case Movement.Right:
                return new Vector2(1, 0);
            case Movement.Up:
                return new Vector2(0, 1);
        }

        return Vector2.zero;
    }

    public void OnPlayerSuccess() {
        successCanvas.gameObject.SetActive(true);
    }

    public void Reload() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
