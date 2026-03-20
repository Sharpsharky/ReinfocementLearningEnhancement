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

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 directionToTarget = targetTransform.position - transform.position;
        sensor.AddObservation(directionToTarget);           // 3 inputs
        sensor.AddObservation(transform.rotation);          // 4 inputs
        sensor.AddObservation(targetTransform.rotation);    // 4 inputs
        sensor.AddObservation(prometeoCarController.carSpeed); // 1 input
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
        countOfObjBeingTouched = 0;
        isFinishing = false;
        StartComparingDistanceFromParkingSpace();
    }

    private void GiveRewardAccordingToSpeed()
    {
        if(prometeoCarController.carSpeed < -5)
        {
            GiveRewardToAgent(-0.001f);
        }
    }


    private IEnumerator distanceCoroutine;

    private void StartComparingDistanceFromParkingSpace()
    {
        if (distanceCoroutine != null) StopCoroutine(distanceCoroutine);
        distanceCoroutine = CheckDistanceAndGiveReward();
        StartCoroutine(distanceCoroutine);
    }

    int countOfObjBeingTouched = 0;
    bool isFinishing = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Curb") return;
        if (isFinishing) return;

        countOfObjBeingTouched++;
        GiveRewardToAgent(-0.5f);
        Debug.LogError(transform.parent.name + " hit: " + collision.gameObject.tag + " (" + collision.gameObject+")");
        isFinishing = true;
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

    private const float EpisodeTimeLimit = 30f;

    private IEnumerator CheckDistanceAndGiveReward()
    {
        yield return new WaitForSeconds(0.1f);
        previousDistanceFromParkingSpace = Vector3.Distance(transform.position, targetTransform.position);
        float episodeStartTime = Time.time;

        while (true)
        {
            yield return new WaitForSeconds(2f);

            if (Time.time - episodeStartTime >= EpisodeTimeLimit)
            {
                GiveRewardToAgent(-0.5f);
                FinishTheEpisode();
                yield break;
            }

            currentDistanceFromParkingSpace = Vector3.Distance(transform.position, targetTransform.position);
            float distanceChange = previousDistanceFromParkingSpace - currentDistanceFromParkingSpace;
            GiveRewardToAgent(0.2f * distanceChange);

            if (Mathf.Abs(distanceChange) < 2 && currentDistanceFromParkingSpace > 2)
            {
                GiveRewardToAgent(-0.05f);
            }

            if (currentDistanceFromParkingSpace < 5f)
            {
                GiveRewardToAgent(0.1f);
            }

            previousDistanceFromParkingSpace = currentDistanceFromParkingSpace;

            if (countOfObjBeingTouched > 0)
            {
                GiveRewardToAgent(-0.1f);
            }
        }
        yield return null;
    }

}
