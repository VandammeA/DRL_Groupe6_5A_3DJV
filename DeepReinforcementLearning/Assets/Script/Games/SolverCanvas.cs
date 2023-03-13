using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SolverCanvas : MonoBehaviour
{
    [SerializeField] private Text States;
    [SerializeField] private Text Time;
    [SerializeField] private Text Moves;
    [SerializeField] private Button ShowButton;
    

    public void Start()
    {
        
        ShowButton.gameObject.SetActive(false);
        States.gameObject.SetActive(false);
        Time.gameObject.SetActive(false);
        States.text = "";
        Moves.text = "";
        Time.text = "";
    }



    public void Solved(float solvingTimeInSeconds, int stateCount,int NumberOfMoves)
    {
        
        
        States.gameObject.SetActive(true);
        Time.gameObject.SetActive(true);
        ShowButton.gameObject.SetActive(true);
        Time.text = "Time : " + solvingTimeInSeconds + " s";
        Moves.text = "Move : " + NumberOfMoves;
        States.text = "State : " + stateCount;
    }


    
}
