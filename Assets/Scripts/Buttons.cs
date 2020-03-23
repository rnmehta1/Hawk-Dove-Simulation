using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    public RandomGenerator generator;
    public PopulateFields populate;
    public HawkDoveSim hawkDoveSim;
    public Button StartButton;
    public Button NextStepButton;

    int count = 0;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Enter Start");
        if ((generator == null) && (GetComponent<RandomGenerator>() != null)) {
            generator = GetComponent<RandomGenerator>();
        } else {
            Debug.Log("Missing RandomGenerator start component. Please add one");
        }

        if ((populate == null) && (GetComponent<PopulateFields>() != null))
        {
            populate = GetComponent<PopulateFields>();
            populate.putText("100", "InputHawk");
            populate.putText("100", "InputDove");
            populate.putText("100", "InputFood");
            populate.putText("50", "InputEVFood");
            populate.putText("10", "InputELBluff");
            populate.putText("100", "InputELInjury");
            populate.putText("140", "InputReprodThresh");
            populate.putText("2", "InputBaseEnergy");
            populate.putText("20", "InputDeathThresh");
            populate.putText("5", "InputFoodExp");
            populate.putText("0", "InputGenerationRound");

        }
        else
        {
            Debug.Log("Missing PopulateFields start component. Please add one");
        }

        
    }

    public void StartClicked()
    {
        Debug.Log("Enter Start Clicked");
        if (generator == null) {
            Debug.Log("Missing RandomGenerator click component. Please add one");
        } else {
            Debug.Log("Found RandomGenerator click component.");
            generator.generation();
        }

        if (populate == null)
        {
            Debug.Log("Missing PopulateFields click component. Please add one");
        }
        else
        {
            Debug.Log("Found PopulateFields click component.");
            populate.putText("100", "InputHawk");
            populate.putText("100", "InputDove");
            populate.putText("100", "InputFood");
            populate.putText("50", "InputEVFood");
            populate.putText("10", "InputELBluff");
            populate.putText("100", "InputELInjury");
            populate.putText("140", "InputReprodThresh");
            populate.putText("2", "InputBaseEnergy");
            populate.putText("20", "InputDeathThresh");
            populate.putText("5", "InputFoodExp");
            populate.putText("0", "InputGenerationRound");

            hawkDoveSim = GetComponent<HawkDoveSim>();
            hawkDoveSim.startSimulation();

        }

        StartButton.interactable = false;
        NextStepButton.interactable = false;

    }

    // Update is called once per frame
    void Update()
    {
    }
}
