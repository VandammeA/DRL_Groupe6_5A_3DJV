using Script.ReinforcementLearning.Common;
using UnityEngine;

public class Arrival : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == Layers.IntValue("Player")) {
            GameManager.Instance.OnPlayerSuccess();
        }
    }
}