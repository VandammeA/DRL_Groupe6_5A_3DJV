using System.Collections.Generic;
using Games;
using Script.ReinforcementLearning.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ReinforcementLearning {
    public static class Sarsa {
        public static List<Movement> EpsilonGreedy(AiAgent agent, GameGrid grid, float alpha, float gamma, float epsilon, int numEpisodes) {
            // Initialisation des états possibles de la grille de jeu
            var possibleGridStates = agent.GetAllPossibleStates(grid);
            var possibleStates = new List<GridState>();
            foreach (var possibleState in possibleGridStates) {
                // Initialisation aléatoire de l'action optimale et de la valeur de l'état
                int random = Random.Range(0, 4);
                GridState state = new GridState(possibleState) {
                    BestAction = (Movement)random,
                    value = 0
                };
                state.SetArrow();
                possibleStates.Add(state);
            }
            // Initialisation de la politique à une liste vide
            List<Movement> policy = new List<Movement>();
             // Boucle d'apprentissage sur le nombre d'épisodes spécifié
        for (int episode = 0; episode < numEpisodes; episode++) {
            // Initialisation de l'état de départ et de l'action initiale
            GridState state = grid.gridState;
            int random = Random.Range(0, 4);
            Movement action = (Movement)random;
            int reward = 0;
            GridState nextState = null;

            // Boucle sur chaque étape de l'épisode
            do {
                // Sauvegarde de l'état et de l'action précédents
                GridState prevState = state;
                Movement prevAction = action;

                // Choix de l'action suivante selon la politique epsilon-greedy
                if (Random.value < epsilon) {
                    int randomAction = Random.Range(0, 4);
                    action = (Movement)randomAction;
                } else {
                    action = prevState.BestAction;
                }

                // Calcul de la récompense et de l'état suivant
                nextState = GameManager.Instance.stateDelegate.GetNextState(prevState, action, possibleStates, out _, out reward);

                // Conversion des états en états possibles de la grille
                prevState = possibleStates.Find(s => s.Equals(prevState));
                nextState = possibleStates.Find(s => s.Equals(nextState));

                // Mise à jour de la Q-value de l'état précédent
                float qValue = prevState.value + alpha * (reward + gamma * nextState.value - prevState.value);
                prevState.value = qValue;

                // Mise à jour de l'action optimale de l'état précédent si l'action a changé
                if (prevState.BestAction != action) {
                    prevState.BestAction = action;
                }

                // Passage à l'état suivant
                state = nextState;
            } while (!agent.IsFinalState(state.grid, grid.gridState.grid));
        }

        // Affichage des valeurs et des actions optimales de chaque état
        foreach (var state in possibleStates) {
            Debug.Log("State: " + state + ", Value: " + state.value + ", Best Action: " + state.BestAction);
        }

        // Construction de la politique optimale à partir des actions optimales de chaque état
        GridState nextStateForPolicy = grid.gridState;
            int maxIterations = 100;
            do {
                maxIterations--;
                var tmp = nextStateForPolicy;
                nextStateForPolicy = possibleStates.Find(state => state.Equals(nextStateForPolicy));
                policy.Add(nextStateForPolicy.BestAction);
                nextStateForPolicy = GameManager.Instance.stateDelegate.GetNextState(nextStateForPolicy, nextStateForPolicy.BestAction,
                    possibleStates, out _, out _);
                if (tmp.Equals(nextStateForPolicy)) break;
            } while (nextStateForPolicy != null && maxIterations >= 0);

            string policyLog = "Policy (" + policy.Count + ") :";
            foreach (var move in policy) {
                string dirname = "";
                switch (move) {
                    case Movement.Right:
                        dirname = ">";
                        break;
                    case Movement.Down:
                        dirname = "v";
                        break;
                    case Movement.Left:
                        dirname = "<";
                        break;
                    case Movement.Up:
                        dirname = "^";
                        break;
                }

                policyLog += " " + dirname;
            }

            Debug.Log(policyLog);

            return policy;
        }
    }
}