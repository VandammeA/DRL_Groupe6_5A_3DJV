using System.Collections.Generic;
using Script.ReinforcementLearning.Common;
using UnityEngine;

namespace Games
{
    public enum TileType {
        Floor = 0,
        Player = 1,
        Obstacle = 2,
        Goal = 3
    }
    public abstract class AiAgent : MonoBehaviour {
        public float gridSize = 1;
        [SerializeField] protected LayerMask walkableMask;

        protected int stateCount = 0;

        public abstract void Move(Vector2 direction);

        public abstract int[][][] GetAllPossibleStates(GameGrid grid);
        public List<Movement> GetPossibleActions(int[][] state) {
            var actions = new List<Movement>();
            for (int i = 0; i < state.Length; i++) {
                for (int j = 0; j < state[0].Length; j++) {
                    if (state[i][j] == (int)TileType.Player) {
                        foreach (Movement movement in System.Enum.GetValues(typeof(Movement))) {
                            var (newI, newJ) = MovePosition(i, j, movement, state[0].Length, state.Length);
                            if (state[newI][newJ] == (int)TileType.Floor) {
                                actions.Add(movement);
                            }
                        }
                        break;
                    }
                }
            }

            return actions;
        }
        public abstract float GetReward(int[][] state, int[][] nextState);
        
        protected (int, int) MovePosition(int i, int j, Movement action, int gridWidth, int gridHeight)
        {
            int newI = i;
            int newJ = j;
            switch (action) {
                case Movement.Up:
                    newI = i + 1 < gridHeight ? i + 1 : i;
                    break;
                case Movement.Right:
                    newJ = j + 1 < gridWidth ? j + 1 : j;
                    break;
                case Movement.Down:
                    newI = i - 1 >= 0 ? i - 1 : i;
                    break;
                case Movement.Left:
                    newJ = j - 1 >= 0 ? j - 1 : j;
                    break;
            }

            return (newI, newJ);
        }

        public abstract bool IsFinalState(int[][] grid, int[][] firstState);

    }
}