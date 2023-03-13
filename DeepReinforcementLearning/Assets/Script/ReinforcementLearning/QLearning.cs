using System;
using System.Collections.Generic;
using Games;
using Script.ReinforcementLearning.Common;
using UnityEngine;

namespace ReinforcementLearning {
    public static class QLearning {
        public static List<Movement> RunQLearning(AiAgent agent, GameGrid grid, int maxIterations = 1000) {
            //Obtenir tous les états possibles du plateau de jeu
            var possibleGridStates = agent.GetAllPossibleStates(grid);
            //Créer une liste d'états possibles
            var possibleStates = new List<GridState>();
            //Boucle pour parcourir chaque état possible du plateau de jeu
            foreach (var possibleState in possibleGridStates) {
                //Créer un nouvel état avec la valeur 0 et la flèche par défaut
                GridState state = new GridState(possibleState) {
                    value = 0
                };
                state.SetArrow();
                //Ajouter l'état à la liste des états possibles
                possibleStates.Add(state);
            }

            //Initialiser les valeurs des hyperparamètres alpha, gamma, epsilon et threshold
            float alpha = 0.1f; // taux d'apprentissage
            float gamma = 0.9f; // facteur d'actualisation
            float epsilon = 0.1f; // taux d'exploration
            float threshold = 0.01f; // seuil d'arrêt

            //Boucle pour exécuter l'algorithme Q-learning pour un nombre maximum d'itérations donné
            for (int i = 0; i < maxIterations; i++) {
                //Sélectionner un état au hasard parmi les états possibles
                GridState state = possibleStates[UnityEngine.Random.Range(0, possibleStates.Count)];
                //Si l'état sélectionné est un état final, passer à l'itération suivante
                if (agent.IsFinalState(state.grid, grid.gridState.grid)) continue;

                //Sélectionner une action au hasard avec une probabilité epsilon ou sélectionner la meilleure action pour l'état actuel
                Movement action;
                if (UnityEngine.Random.value < epsilon) {
                    action = (Movement)UnityEngine.Random.Range(0, 4);
                } else {
                    action = state.BestAction;
                }

                //Obtenir la récompense pour l'état actuel et l'action sélectionnée et le prochain état résultant
                float reward = GameManager.Instance.stateDelegate.GetReward(state, action, possibleStates, out var nextState);
                // Calculer la valeur cible en utilisant la fonction de récompense et la valeur Q de l'état suivant
                float target = reward + gamma * nextState.value;
                // Calculer l'erreur entre la valeur cible et la valeur Q de l'état actuel
                float error = target - state.value;
                // Mettre à jour la valeur Q de l'état actuel en utilisant l'erreur et le taux d'apprentissage alpha
                state.value += alpha * error;
                
                // Mettre à jour la meilleure action de l'état actuel si l'erreur est supérieure au seuil
                if (error > threshold) {
                    // update best action
                    float max = 0;
                    for (int j = 0; j < 4; j++) {
                        Movement nextAction = (Movement)j;
                        float value = GameManager.Instance.stateDelegate.GetReward(state, nextAction, possibleStates, out var nextStateTmp) + gamma * nextStateTmp.value;
                        if (value > max) {
                            max = value;
                            state.BestAction = nextAction;
                        }
                    }
                }
            }
            // Détermination de la politique optimale en utilisant la valeur Q mise à jour
            GridState nextStateGrid = grid.gridState;
            List<Movement> policy = new List<Movement>();
            int iter = 0;
            do {
                iter++;
                var tmp = nextStateGrid;
                nextStateGrid = possibleStates.Find(state => state.Equals(nextStateGrid));
                policy.Add(nextStateGrid.BestAction);
                nextStateGrid = GameManager.Instance.stateDelegate.GetNextState(nextStateGrid, nextStateGrid.BestAction,
                    possibleStates, out _, out _);
                if (tmp.Equals(nextStateGrid)) break;
            } while (nextStateGrid != null && iter < maxIterations);

            return policy;
        }
    }
}