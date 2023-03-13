using System.Collections.Generic;
using Games;
using Script.ReinforcementLearning.Common;
using UnityEngine;

namespace ReinforcementLearning
{
    public static class MonteCarloES
    {
        public static List<Movement> MonteCarloControl(AiAgent agent, GameGrid grid, int numEpisodes, float gamma)
        {
            var possibleGridStates = agent.GetAllPossibleStates(grid);


            // Init
            var qValues = new Dictionary<GridState, Dictionary<Movement, float>>();
            var returns = new Dictionary<GridState, Dictionary<Movement, List<float>>>();

            foreach (var possibleState in possibleGridStates)
            {
                var state = new GridState(possibleState);
                qValues[state] = new Dictionary<Movement, float>();
                returns[state] = new Dictionary<Movement, List<float>>();

                foreach (Movement action in System.Enum.GetValues(typeof(Movement)))
                {
                    qValues[state][action] = 0;
                    returns[state][action] = new List<float>();
                }
            }

            for (var episode = 0; episode < numEpisodes; episode++)
            {
                var episodeStates = new List<GridState>();
                var episodeActions = new List<Movement>();
                var episodeRewards = new List<float>();

                GridState currentState = grid.gridState;
                int[][] currentStateArray = currentState.grid;
                bool terminalStateReached = false;

                while (!terminalStateReached)
                {
                    var possibleActions = agent.GetPossibleActions(currentStateArray);

                    Movement action = possibleActions[Random.Range(0, possibleActions.Count)];
                    
                    List<GridState> possibleStates = new List<GridState>();
                    GridState nextState = GameManager.Instance.stateDelegate.GetNextState(currentState, action, possibleStates, out int playerNewI, out int playerNewJ);

                    episodeStates.Add(currentState);
                    episodeActions.Add(action);
                    episodeRewards.Add(agent.GetReward(currentState.grid, nextState.grid));

                    if (agent.IsFinalState(currentState.grid, nextState.grid))
                    {
                        terminalStateReached = true;
                    }

                    currentState = nextState;
                }

                var g = 0f;
                for (int t = episodeStates.Count - 1; t >= 0; t--)
                {
                    var state = episodeStates[t];
                    var action = episodeActions[t];

                    g = gamma * g + episodeRewards[t];

                    if (!episodeStates.GetRange(0, t).Contains(state))
                    {
                        returns[state][action].Add(g);
                        qValues[state][action] = CalculateAverage(returns[state][action]);
                    }
                }
            }

            // Politique
            var policy = new List<Movement>();

            foreach (var possibleState in possibleGridStates)
            {
                var state = new GridState(possibleState);
                var bestAction = Movement.None;
                var bestValue = Mathf.NegativeInfinity;

                foreach (var action in qValues[state].Keys)
                {
                    if (qValues[state][action] > bestValue)
                    {
                        bestValue = qValues[state][action];
                        bestAction = action;
                    }
                }

                state.BestAction = bestAction;
                state.value = bestValue;

                policy.Add(bestAction);
            }

            return policy;
        }

        private static float CalculateAverage(List<float> values)
        {
            var sum = 0f;
            foreach (var value in values)
            {
                sum += value;
            }

            return sum / values.Count;
        }
    }
}