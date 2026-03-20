using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcceptCar : MonoBehaviour
{
    [SerializeField]
    private DrawCar drawCar;

    private int numberOfWheelsAccepted = 0;
    private int previousNumberOfWheels = 0;

    private MeshRenderer meshRenderer;
    private IEnumerator waitingForWin;

    public CarAgent CarAgent { get; set; }
    bool hasTouchedTheParkingSpace = false;
    bool checkpoint0 = false, checkpoint1 = false, checkpoint2 = false;


    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    private void OnEnable()
    {
        hasTouchedTheParkingSpace = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Wheel")
        {
            previousNumberOfWheels = numberOfWheelsAccepted;
            numberOfWheelsAccepted++;
            ChangeColorOHighlight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Wheel")
        {
            previousNumberOfWheels = numberOfWheelsAccepted;
            numberOfWheelsAccepted--;
            ChangeColorOHighlight();
        }
    }
    private void ChangeColorOHighlight()
    {
        if (numberOfWheelsAccepted >= 4 && previousNumberOfWheels <= 3)
        {
            meshRenderer.material = drawCar.AcceptMaterial;
            waitingForWin = WaitForWin();

            StartCoroutine(waitingForWin);
        }
        else if (numberOfWheelsAccepted > 0 && numberOfWheelsAccepted < 4 && (previousNumberOfWheels <= 0 || previousNumberOfWheels >= 4))
        {
            if (waitingForWin != null) StopCoroutine(waitingForWin);
            meshRenderer.material = drawCar.DeclineMaterial;
            if (!hasTouchedTheParkingSpace)
            {
                hasTouchedTheParkingSpace = true;
                CarAgent.GiveRewardToAgent(Score.instance.StandInCorrectSpotReward/3);
            }
        }
        else if (numberOfWheelsAccepted <= 0 && previousNumberOfWheels >= 1)
        {
            meshRenderer.material = drawCar.WaitMaterial;
        }
    }
    private IEnumerator WaitForWin()
    {
        if (!checkpoint0)
        {
            CarAgent.GiveRewardToAgent(Score.instance.StandInCorrectSpotReward / 3);
            checkpoint0 = true;
        }
        yield return new WaitForSeconds(drawCar.TimeToWin/3);
        if (!checkpoint1)
        {
            CarAgent.GiveRewardToAgent(Score.instance.StandInCorrectSpotReward / 3);
            checkpoint1 = true;
        }
        yield return new WaitForSeconds(drawCar.TimeToWin/3);
        if (!checkpoint2)
        {
            CarAgent.GiveRewardToAgent(Score.instance.StandInCorrectSpotReward / 3);
            checkpoint2 = true;
        }
        yield return new WaitForSeconds(drawCar.TimeToWin/3);
        CarAgent.Win();
        yield return null;
    }

    private void OnDisable()
    {
        numberOfWheelsAccepted = 0;
        previousNumberOfWheels = 0;
        if (waitingForWin != null) StopCoroutine(waitingForWin);
    }



}
