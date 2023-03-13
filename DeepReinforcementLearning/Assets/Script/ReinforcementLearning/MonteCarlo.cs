using System.Collections.Generic;
using System.Linq;
using Games;
using Script.ReinforcementLearning.Common;
using UnityEngine;

namespace ReinforcementLearning {
    public class Episode {
        public List<GridState> gridStates;
        public List<Movement> actions;
        public List<float> rewards;

        public Episode(AiAgent agent, GridState firstState, List<GridState> possibleStates, int maxEpisodeLength) {
            gridStates = new List<GridState>();
            actions = new List<Movement>();
            rewards = new List<float>();

            int iterations = 0;
            var currentGameState = new GridState(possibleStates[0].grid);

            while (!agent.IsFinalState(currentGameState.grid, firstState.grid) && iterations < maxEpisodeLength) {
                int index = possibleStates.IndexOf(currentGameState);
                var action = possibleStates[index].BestAction;

                gridStates.Add(new GridState(currentGameState.grid));
                actions.Add(action);
                rewards.Add(GameManager.Instance.stateDelegate.GetReward(currentGameState, action, possibleStates,
                    out var nextState));

                currentGameState = new GridState(nextState.grid);
                ++iterations;
            }
        }
    }
    
    public class MonteCarlo {
        public static List<Movement> FirstVisitMonteCarloPredictionWithExploringStart(AiAgent agent, GameGrid grid, int episodesCount, int maxEpisodeLength) {
            var possibleGridStates = agent.GetAllPossibleStates(grid);
            var possibleStates = new List<GridState>();
            foreach (var possibleState in possibleGridStates) {
                GridState state = new GridState(possibleState) {
                    value = 0
                };
                state.SetArrow();
                possibleStates.Add(state);
            }

            var n = new Dictionary<GridState, float>();
            var returns = new Dictionary<GridState, float>();
            for (int i = possibleStates.Count - 1; i >= 0; --i) {
                n.Add(possibleStates[i], 0);
                returns.Add(possibleStates[i], 0);
            }
            
            float g;
            List<Episode> episodes = new List<Episode>();
            for (int e = 0; e < episodesCount; ++e) {
                Episode episode = new Episode(agent, grid.gridState, possibleStates, maxEpisodeLength);
                g = 0;
                for (int t = episode.gridStates.Count - 2; t >= 0; --t) {
                    g += episode.rewards[t + 1];
                    if (!episode.gridStates.Take(t - 1).Contains(episode.gridStates[t])) {
                        returns[episode.gridStates[t]] += g;
                        n[episode.gridStates[t]] += 1;
                    }
                }
                episodes.Add(episode);
            }

            for (int i = possibleStates.Count - 1; i >= 0; --i) {
                possibleStates[i].value = returns[possibleStates[i]] / n[possibleStates[i]];

                float episodeValue = episodes[0].gridStates.Find(state => state.Equals(possibleStates[i])).value;
                for (int e = 1; e < episodesCount; ++e) {
                    var currentEpValue = episodes[e].gridStates.Find(state => state.Equals(possibleStates[i])).value;
                    if (episodeValue < currentEpValue) {
                        episodeValue = currentEpValue;
                        possibleStates[i].BestAction = episodes[e].actions[i];
                    }
                }
            }
            
            // build end policy
            List<Movement> policy = new List<Movement>();
            var nextState = grid.gridState;
            var maxIterations = 100;
            do {
                maxIterations--;
                var tmp = nextState;
                nextState = possibleStates.Find(state => state.Equals(nextState));
                policy.Add(nextState.BestAction);
                nextState = GameManager.Instance.stateDelegate.GetNextState(nextState, nextState.BestAction,
                    possibleStates, out _, out _);
                if (tmp.Equals(nextState)) break;
            } while (nextState != null && maxIterations >= 0);

            return policy;
        }
    }
}