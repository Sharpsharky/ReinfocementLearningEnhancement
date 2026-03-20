using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class ObservableParameters
{
    public float currentSpeed;

}

public class CarAgent : Agent
{
    [SerializeField] private PrometeoCarController prometeoCarController;

    private DrawCar drawCarController;
    private Transform targetTransform;
    private float currentReward = 0;
    //[SerializeField] private ObservableParameters observableParameters;

    public void InitializeAgent(DrawCar drawCar)
    {
        drawCarController = drawCar;
        targetTransform = drawCarController.PlaceToPark;
        targetTransform.gameObject.GetComponent<AcceptCar>().CarAgent = this;
        //Debug.Log("drawCarController: " + drawCarController.name);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int isAccelerating = actions.DiscreteActions[0]; // 0 - no accelerating, 1 - accelerating, 2 - accelerating backwards
        int isTurnningDirection = actions.DiscreteActions[1]; // 0 - no turning, 1 - left, 2 - right
        Debug.Log("AccelerateCar: " + isAccelerating);
        prometeoCarController.AccelerateCar(isAccelerating);
        prometeoCarController.TurnCar(isTurnningDirection);

        GiveRewardToAgent(-1f / MaxStep);
        //GiveRewardAccordingToSpeed();
        if (StepCount >= MaxStep) FinishTheEpisode();
    }

    public override void CollectObservations(VectorSensor sensor) //TO DO: Change the observation to correctly set up car on a parking spot
    {
        sensor.AddObservation(transform.localPosition); //3 inputs
        sensor.AddObservation(transform.rotation); //3 inputs

        sensor.AddObservation(targetTransform.localPosition); //3 inputs
        sensor.AddObservation(targetTransform.rotation); //3 inputs

        //sensor.AddObservation(prometeoCarController.carSpeed); //1 input
        //sensor.AddObservation(Vector3.Distance(transform.localPosition,targetTransform.localPosition)); //1 input
    }

    public void GiveRewardToAgent(float reward)
    {
        AddReward(reward);
        currentReward += reward;
        //Debug.Log("Current reward: " + currentReward);
        //Debug.Log("Current reward: " + StepCount);
    }

    public void FinishTheEpisode()
    {
        Score.instance.ChangeScore(1, 0);
        drawCarController.StartSimulation();
        EndEpisode();

    }

    public void Win()
    {
        Score.instance.ChangeScore(0, 1);
        GiveRewardToAgent(Score.instance.StandInCorrectSpotReward);
        FinishTheEpisode();
    }

    public override void OnEpisodeBegin()
    {
            StartComparingDistanceFromParkingSpace();
    }

    private void GiveRewardAccordingToSpeed()
    {
        if(prometeoCarController.carSpeed < -5)
        {
            GiveRewardToAgent(-0.001f);
        }
    }


    private void StartComparingDistanceFromParkingSpace()
    {
        StartCoroutine(CheckDistanceAndGiveReward());

    }

    int countOfObjBeingTouched = 0;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Curb") return;

        countOfObjBeingTouched++;
        GiveRewardToAgent(-0.5f);
        Debug.LogError(transform.parent.name + " hit: " + collision.gameObject.tag + " (" + collision.gameObject+")");
        StartCoroutine(Wait());
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.2f);

        FinishTheEpisode();

        yield return null;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Curb") return;
        countOfObjBeingTouched--;
        if (countOfObjBeingTouched < 0) countOfObjBeingTouched = 0;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteAtions = actionsOut.DiscreteActions;



        if (Input.GetKey(KeyCode.UpArrow))
        {
            Debug.Log("UpArrow");
            discreteAtions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            Debug.Log("DownArrow");
            discreteAtions[0] = 2;
        }
        else
        {
            discreteAtions[0] = 0;

        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Debug.Log("LeftArrow");
            discreteAtions[1] = 1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Debug.Log("RightArrow");
            discreteAtions[1] = 2;
        }
        else
        {
            discreteAtions[1] = 0;

        }

    }
    

    private float previousDistanceFromParkingSpace;
    private float currentDistanceFromParkingSpace;

    private IEnumerator CheckDistanceAndGiveReward()
    {
        yield return new WaitForSeconds(0.1f);
        previousDistanceFromParkingSpace = Vector3.Distance(transform.position, targetTransform.position);

        //Debug.Log("CheckDistanceAndGiveReward");
        while (true) 
        {
            yield return new WaitForSeconds(2f);

            currentDistanceFromParkingSpace = Vector3.Distance(transform.position, targetTransform.position);
            if (currentDistanceFromParkingSpace >= previousDistanceFromParkingSpace)
            {
                float distanceChange = currentDistanceFromParkingSpace - previousDistanceFromParkingSpace;

                //Debug.Log("Reward: " + (-3 * distanceCoefficient));
                GiveRewardToAgent(-0.05f * distanceChange);
            }
            else
            {
                float distanceChange = previousDistanceFromParkingSpace - currentDistanceFromParkingSpace;
                //Debug.Log("Reward: " + (3 * distanceCoefficient));
                GiveRewardToAgent(0.05f * distanceChange);
            }
            previousDistanceFromParkingSpace = currentDistanceFromParkingSpace;


            if(Mathf.Abs(currentDistanceFromParkingSpace-previousDistanceFromParkingSpace) < 2 && currentDistanceFromParkingSpace > 2)
            {
                //Debug.Log("Reward: " + (-10));

                GiveRewardToAgent(-0.1f);

            }

            if(countOfObjBeingTouched > 0)
            {
                GiveRewardToAgent(-0.1f);

            }


        }
        yield return null;
    }

}
