using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Script.ReinforcementLearning.Common
{
    public enum Movement {
        Up, Right, Down, Left, None
    }
    public interface IStateDelegate
    {
        public int GetReward(GridState currentState, Movement action, List<GridState> possibleStates, out GridState nextState);

        public GridState GetNextState(GridState currentState, Movement action, List<GridState> possibleStates, out int playerNewI,
            out int playerNewJ);

        public int GetStateCount();
    }
    
    [InitializeOnLoad]
    public static class Layers {
        private static readonly Dictionary<string, int> LayersInt;

        static Layers() {
            LayersInt = new Dictionary<string, int>();
            for (int i = 0; i <= 31; ++i) {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName)) {
                    LayersInt[layerName] = i;
                }
            }
        }

        public static int IntValue(string value) {
            return LayersInt[value];
        }
    }
    public static class GridUtilsa {
        public static int[][] CloneGrid(this int[][] source) {
            var result = new int[source.Length][];

            for (int i = 0; i < source.Length; ++i) {
                result[i] = new int[source[i].Length];
                for (int j = 0; j < source[i].Length; ++j) {
                    result[i][j] = source[i][j];
                }
            }
        
            return result;
        }

        public static int[] Find(this int[][] grid, int value) {
            for (int i = grid.Length - 1; i >= 0; --i) {
                for (int j = grid[0].Length - 1; j >= 0; --j) {
                    if (grid[i][j] == value) return new []{ i, j };
                }
            }
            return null;
        }
    }
    
    public class GridState {
        public int[][] grid;
        public float value;

        private Movement _bestAction = Movement.Down;
        public Movement BestAction {
            get => _bestAction;
            set {
                _bestAction = value;
                if (_gridArrow == null) return;
                switch (value) {
                    case Movement.Down:
                        _gridArrow.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                    case Movement.Left:
                        _gridArrow.rotation = Quaternion.Euler(0, 180, 0);
                        break;
                    case Movement.Right:
                        _gridArrow.rotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case Movement.Up:
                        _gridArrow.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                }
            }
        }

        private Transform _gridArrow;

        public GridState(int[][] gridP) {
            grid = gridP;
        }

        public void SetArrow() {
            int i = 0;
            int j = 0;
            for (i = grid.Length - 1; i >= 0; --i) {
                for (j = grid[i].Length - 1; j >= 0; --j) {
                    if (grid[i][j] == LayerMask.NameToLayer("Player")) {
                        
                        _gridArrow = GameManager.Instance.arrowsManager.GetArrow(i, j);
                        break;
                    }
                }
            }
        }

        public override string ToString() {
            string log = "";

            for (int i = 0; i < grid.Length; ++i) {
                for (int j = 0; j < grid[i].Length; ++j) {
                    log = log + grid[i][j] + " ";
                }

                if(i < grid.Length - 1)
                    log += "\n";
            }
            
            return log;
        }

        public override bool Equals(object other) {
            return Equals((GridState)other);
        }

        protected bool Equals(GridState other) {
            if (other == null) return false;
            Vector2Int gridSize = new Vector2Int(grid[0].Length, grid.Length);
            for (int i = 0; i < gridSize.y; ++i) {
                for (int j = 0; j < gridSize.x; ++j) {
                    if (grid[i][j] != other.grid[i][j]) return false;
                }
            }
            return true;
        }
    }
}
