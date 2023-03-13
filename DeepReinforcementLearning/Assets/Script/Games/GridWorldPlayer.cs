using System.Collections.Generic;
using Script.ReinforcementLearning.Common;
using UnityEngine;

namespace Games {
    public class GridWorldPlayer : AiAgent, IStateDelegate
    {

        private void Start()
        {
            
            GameManager.Instance.stateDelegate = this;
        }

        public override void Move(Vector2 direction)
        {
            Vector3 destination = transform.position + (direction.x * Vector3.right + direction.y * Vector3.up) * gridSize;
            if (Physics.Raycast(destination + Vector3.back * 5, Vector3.forward, out var hit, 10))
            {
                if (walkableMask == (walkableMask | (1 << hit.collider.gameObject.layer)))
                {
                    transform.position = destination;
                }
            }
        }

        public override int[][][] GetAllPossibleStates(GameGrid grid) {
            int layerPlayer = Layers.IntValue("Player");
            int layerGround = Layers.IntValue("Ground");
            
            var result = new List<int[][]>();

            var cleanedGrid = grid.gridState.grid.CloneGrid();
            int gridHeight = cleanedGrid.Length;
            int gridWidth = cleanedGrid[0].Length;
            foreach (var row in cleanedGrid) {
                for (var j = 0; j < gridWidth; ++j) {
                    if (row[j] == layerPlayer) row[j] = layerGround;
                }
            }
            
            // select all possible states
            for (int i = 0; i < gridHeight; ++i) {
                for (int j = 0; j < gridWidth; ++j) {
                    if (cleanedGrid[i][j] == layerGround) {
                        cleanedGrid[i][j] = layerPlayer;
                        result.Add(cleanedGrid.CloneGrid());  // just a copy
                        cleanedGrid[i][j] = layerGround;
                    }
                }
            }

            stateCount = result.Count;
            return result.ToArray();
        }

        public override bool IsFinalState(int[][] grid, int[][] firstState) {
            int layerArrival = Layers.IntValue("Arrival");
            int layerPlayer = Layers.IntValue("Player");
            var arrivalPos = firstState.Find(layerArrival);
            var playerPos = grid.Find(layerPlayer);
            return playerPos[0] == arrivalPos[0] && playerPos[1] == arrivalPos[1];
        }

        public int GetReward(GridState currentState, Movement action, List<GridState> possibleStates, out GridState nextState)
        {
            var layerArrival = Layers.IntValue("Arrival");
            nextState = GetNextState(currentState, action, possibleStates, out var playerI, out var playerJ);
            if (currentState.grid[playerI][playerJ] == layerArrival) {
                return 1;
            }

            return 0;
        }
        public override float GetReward(int[][] prevState, int[][] nextState)
        {
            // Calculate the reward based on the difference between the previous and next state
            int layerArrival = Layers.IntValue("Arrival");
            int[][] currentGrid = nextState; // The grid after the agent has taken an action
            int[][] previousGrid = prevState; // The grid before the agent has taken an action

            int playerI = -1;
            int playerJ = -1;
            for (int i = 0; i < currentGrid.Length; i++)
            {
                for (int j = 0; j < currentGrid[0].Length; j++)
                {
                    if (currentGrid[i][j] == Layers.IntValue("Player"))
                    {
                        playerI = i;
                        playerJ = j;
                        break;
                    }
                }
            }

            // Check if the player has reached the goal
            if (currentGrid[playerI][playerJ] == layerArrival)
            {
                return 1.0f; // The agent receives a reward of 1 if it reaches the goal
            }

            return 0.0f; // The agent receives a reward of 0 for all other actions
        }
        

        public GridState GetNextState(GridState currentState, Movement action, List<GridState> possibleStates, out int playerNewI,
            out int playerNewJ)
        {
            var layerPlayer = Layers.IntValue("Player");
            var layerGround = Layers.IntValue("Ground");
            var layerArrival = Layers.IntValue("Arrival");
            var nextState = new GridState(currentState.grid.CloneGrid());
            playerNewI = -1;
            playerNewJ = -1;
            Vector2Int gridSize = new Vector2Int(currentState.grid[0].Length, currentState.grid.Length);
            for (int i = 0; i < gridSize.y; ++i) {
                for (int j = 0; j < gridSize.x; ++j) {
                    if (currentState.grid[i][j] == layerPlayer) {
                        int testCell = 0;
                        int newI = i, newJ = j;
                        switch (action) {
                            case Movement.Up:
                                newI = i + 1 < gridSize.y ? i + 1 : i;
                                break;
                            case Movement.Right:
                                newJ = j + 1 < gridSize.x ? j + 1 : j;
                                break;
                            case Movement.Down:
                                newI = i - 1 >= 0 ? i - 1 : i;
                                break;
                            case Movement.Left:
                                newJ = j - 1 >= 0 ? j - 1 : j;
                                break;
                        }

                        playerNewI = newI;
                        playerNewJ = newJ;
                        testCell = nextState.grid[newI][newJ];
                        if (testCell == layerGround || testCell == layerArrival) {
                            nextState.grid[i][j] = layerGround;
                            nextState.grid[newI][newJ] = layerPlayer;

                            var result = possibleStates.Find(state => state.Equals(nextState));
                            return result ?? currentState;
                        }
                    }
                }
            }
            
            return currentState;
        }
        
        public int GetStateCount()
        {
            return stateCount;
        }
    }
    
}
