using UnityEngine;


    public class ArrowsFromGrid : MonoBehaviour {
        public GameObject arrowPrefab;

        private Transform[][] arrowGrid;
        private Quaternion rotate;
        
        public void Init(int[][] grid, Vector3 firstPos) {
            arrowGrid = new Transform[grid.Length][];
            for (int i = 0; i < grid.Length; ++i) {
                arrowGrid[i] = new Transform[grid[i].Length];
                for (int j = 0; j < grid[i].Length; ++j) {
                    Vector3 arrowPos = firstPos + j * Vector3.right + i * Vector3.up + new Vector3(0.5f, 0.5f, -0.05f);
                    var go = Instantiate(arrowPrefab, transform);
                    rotate.x = 120f;
                    rotate.y = 0f;
                    rotate.z = 0f;
                    go.transform.position = arrowPos;
                    go.transform.rotation=rotate;
                    arrowGrid[i][j] = go.transform;
                }
            }
        }

        public Transform GetArrow(int i, int j) {
            return arrowGrid[i][j];
        }
    }

