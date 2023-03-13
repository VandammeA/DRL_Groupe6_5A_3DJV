using System;
using System.Collections.Generic;
using Games;

using Script.ReinforcementLearning.Common;
using UnityEngine;
using Random = UnityEngine.Random;


namespace ReinforcementLearning {
    public static class DynamicProgramming {
        //------------------------------------------------------------------------------PolicyIteration-------------------------------------------------------------------------
        // Méthode d'itération de politique pour trouver une politique optimale
        public static List<Movement> PolicyIteration(AiAgent agent, GameGrid grid) {
            // Récupérer tous les états possibles de la grille et les stocker dans la variable 
            var possibleGridStates = agent.GetAllPossibleStates(grid);
            // Créer une liste pour stocker tous les états possibles
            var possibleStates = new List<GridState>();
            foreach (var possibleState in possibleGridStates) {
                // Choisir une action aléatoire pour chaque état
                int random = Random.Range(0, 4);
                GridState state = new GridState(possibleState) {
                    BestAction = (Movement)random,
                    value = 0
                };
                state.SetArrow();
                possibleStates.Add(state);
            }
            // Facteur de dévaluation et seuil de convergence
            float gamma = 0.9f;
            float theta = 0.25f;
            
            // Évaluation de politique
            PolicyEvaluation(agent, grid.gridState, possibleStates, gamma, theta);

            // Recherche de la meilleure politique
            GridState nextState = grid.gridState;
            List<Movement> policy = new List<Movement>();
            int maxIterations = 100;
            do {
                maxIterations--;
                var tmp = nextState;
                nextState = possibleStates.Find(state => state.Equals(nextState));
                policy.Add(nextState.BestAction);
                nextState = GameManager.Instance.stateDelegate.GetNextState(nextState, nextState.BestAction,
                    possibleStates, out _, out _);
                if (tmp.Equals(nextState)) break;
            } while (nextState != null && maxIterations >= 0);
            
            // Afficher les valeurs des états possibles et les actions optimales associées
            string possibleStatesVals = "Values : ";
            foreach (var state in possibleStates) {
                possibleStatesVals += state + " -> " + state.value + " ; best action" + state.BestAction + "\n";
            }

            // Afficher la politique
            //Debug.Log(possibleStatesVals);
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

            
            // Retourner la politique optimale
            return policy;
        }
        
        
        // Appliquer l'algorithme d'évaluation de la politique pour obtenir les valeurs des états
        private static void PolicyEvaluation(AiAgent agent, GridState firstState, List<GridState> possibleStates, float gamma, float theta) {
            float delta;
            do {
                delta = 0;
                // Pour chaque état possible dans la liste
                for (var index = 0; index < possibleStates.Count; index++) {
                    // Si l'état est un état final, passer à l'état suivant
                    if (agent.IsFinalState(possibleStates[index].grid, firstState.grid)) continue;
                    var temp = possibleStates[index].value;
                    // Mettre à jour la valeur de l'état en utilisant la fonction de récompense et la valeur de l'état suivant
                    possibleStates[index].value =
                        GameManager.Instance.stateDelegate.GetReward(possibleStates[index], possibleStates[index].BestAction,
                            possibleStates, out var nextState) + gamma * nextState.value;
                    delta = Mathf.Max(delta, Mathf.Abs(temp - possibleStates[index].value));
                }
                // Répéter tant que la différence maximale n'a pas atteint le seuil de convergence
            } while (delta > theta);
            // Amélioration de la politique
            PolicyImprovement(agent, firstState, possibleStates, gamma, theta);
        }

        //Amélioration de la politique de l'agent en fonction de la valeur des états
        private static void PolicyImprovement(AiAgent agent, GridState firstState, List<GridState> possibleStates, float gamma, float theta) {
            bool policyStable = true;

            for (var index = 0; index < possibleStates.Count; index++) {
                if (agent.IsFinalState(possibleStates[index].grid, firstState.grid)) continue;
                var temp = possibleStates[index].BestAction;

                var bestValue = possibleStates[index].value;
                for (int i = Enum.GetValues(typeof(Movement)).Length - 1; i >= 0; --i) {
                    var movement = (Movement)i;
                    var value = GameManager.Instance.stateDelegate.GetReward(possibleStates[index],
                        movement, possibleStates, out var nextState) + gamma * nextState.value;
                    if (value > bestValue) {
                        bestValue = value;
                        possibleStates[index].BestAction = movement;
                    }
                }

                if (temp != possibleStates[index].BestAction) policyStable = false;
            }

            if (policyStable) {
                return;
            }
            PolicyEvaluation(agent, firstState, possibleStates, gamma, theta);
        }

        //------------------------------------------------------------------------------ValueIteration-------------------------------------------------------------------------
        //Calcule la politique optimale d'un agent AI pour une grille de jeu donnée
        public static List<Movement> ValueIteration(AiAgent agent, GameGrid grid) {
            // init
            var possibleGridStates = agent.GetAllPossibleStates(grid);
            var possibleStates = new List<GridState>();
            foreach (var possibleState in possibleGridStates) {
                GridState state = new GridState(possibleState) {
                    value = 0
                };
                state.SetArrow();
                possibleStates.Add(state);
            }

            float gamma = 0.9f; // facteur de dévaluation
            float delta;
            float theta = 0.01f;// seuil de convergence


            int maxIterations = 100;
            do {
                maxIterations--;
                delta = 0;
                for (var index = 0; index < possibleStates.Count; ++index) {
                    GridState state = possibleStates[index];
                    if (agent.IsFinalState(state.grid, grid.gridState.grid)) continue;
                    float temp = state.value;

                    float max = 0;
                    foreach (Movement actionType in Enum.GetValues(typeof(Movement))) {
                        GameManager gm = GameManager.Instance;
                        IStateDelegate stateDelegate = gm.stateDelegate;

                        float reward = stateDelegate.GetReward(state, actionType, possibleStates, out var nextStateTmp);
                        float currentVal = reward + gamma * nextStateTmp.value;
                        if (max < currentVal) {
                            state.BestAction = actionType;

                            max = currentVal;
                        }
                    }

                    state.value = max;

                    delta = Mathf.Max(delta, Mathf.Abs(temp - state.value));
                }
            } while (delta > theta && maxIterations >= 0); 

            
            List<Movement> policy = new List<Movement>();
            var nextState = grid.gridState;

            maxIterations = 100;
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