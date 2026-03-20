using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class DrawCar : MonoBehaviour
{

    [SerializeField]
    private int numberOfRandomCarsToSpawn = 10;
    [SerializeField]
    private float randomRotationOfRandomCar = 15;
    [SerializeField]
    private float altitudeToSpawnCar = 3;
    [SerializeField]
    private float randomnessOfAltitude = 1;
    [SerializeField]
    private float timeToWin = 3; //Stay this long in the spot to win
    [SerializeField]
    private bool spawnCarsOneByOne = false;
    [SerializeField]
    private bool spawnAgentInCube = false;
    [SerializeField]
    private float defaultRotOfAgent = 0;

    [Header("Spawn cube")]
    [SerializeField]
    private Transform spawnCubeCenter; //Stay this long in the spot to win
    [SerializeField]
    private float spawnCubeWidth = 3; //Stay this long in the spot to win
    [SerializeField]
    private float spawnCubeLength = 3; //Stay this long in the spot to win
    [SerializeField]
    private bool randomRotationAndPosition = false;

    [Space]
    [SerializeField]
    private Transform highlightsParent;
    [SerializeField]
    private Material acceptMaterial;
    [SerializeField]
    private Material waitMaterial;
    [SerializeField]
    private Material declineMaterial;
    [SerializeField]
    private GameObject carControlled;
    [SerializeField]
    private List<GameObject> randomCarsList = new List<GameObject>();
    [SerializeField]
    private Transform carsHolder;

    private GameObject agent;
    private List<int> takenSpots = new List<int>();
    IEnumerator drawCarsEnumarator;

    public Material AcceptMaterial { get => acceptMaterial;}
    public Material DeclineMaterial { get => declineMaterial;}
    public Material WaitMaterial { get => waitMaterial; }
    public float TimeToWin { get => timeToWin;}
    public Transform PlaceToPark { get; set; }

    public void Start()
    {
        #if UNITY_EDITOR
            if (PrefabUtility.GetPrefabInstanceHandle(gameObject) != null) PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        #endif

        StartSimulation();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartSimulation();
        }
    }

    public void StartSimulation()
    {
        ResetParkingLot();
        DrawSpotToPark();
        SpawnRandomCars();
        SpawnCarControlled();
    }



    private void OnDrawGizmos()
    {


        Gizmos.color = new Color(0.811f, 0.294f, 0.258f, 0.3f);
        
        Gizmos.DrawCube(spawnCubeCenter.position, new Vector3(spawnCubeWidth,2, spawnCubeLength));
    }

    private void ResetParkingLot()
    {
        if (drawCarsEnumarator != null) StopCoroutine(drawCarsEnumarator);
        takenSpots = new List<int>();
        DestroyCars(carsHolder);
        Destroy(agent);
        DisableAllHighlights();
    }

    private void DestroyCars(Transform carHolder)
    {
        foreach (Transform car in carHolder)
        {
            Destroy(car.gameObject);
        }
    }
    private void DisableAllHighlights()
    {
        foreach(Transform highlight in highlightsParent)
        {
            if (highlight.gameObject.activeSelf) highlight.gameObject.SetActive(false);
        }
    }
    private void SpawnCarControlled()
    {
        GameObject agentCar;
        if (!spawnAgentInCube)
        {
            agentCar = SpawnCarAndRotateIt(carControlled, DrawSpot().position, transform, false, defaultRotOfAgent);
        }
        else
        {
            if(randomRotationAndPosition)
            agentCar = SpawnCarAndRotateIt(carControlled, DrawPositionFromCube(), transform, true, defaultRotOfAgent, true);
            else
            agentCar = SpawnCarAndRotateIt(carControlled, spawnCubeCenter.position, transform, false, defaultRotOfAgent);

        }
        agent = agentCar;
        agentCar.GetComponent<CarAgent>().InitializeAgent(this);
    }

    private Vector3 DrawPositionFromCube()
    {
        Vector3 middlePoint = spawnCubeCenter.position;
        float x = Random.Range(middlePoint.x - (spawnCubeWidth / 2), middlePoint.x + (spawnCubeWidth / 2));
        float z = Random.Range(middlePoint.z - (spawnCubeLength / 2), middlePoint.z + (spawnCubeLength / 2));

        return new Vector3(x, middlePoint.y, z);
    }

    private void SpawnRandomCars()
    {
        if (spawnCarsOneByOne)
        {
            drawCarsEnumarator = DrawRandomCars();
            StartCoroutine(drawCarsEnumarator);
        }
        else
        {
            DrawRandomCarsAtOnce();
        }
    }

    private IEnumerator DrawRandomCars()
    {
        for(int i = 0; i <numberOfRandomCarsToSpawn; i++)
        {
            DrawSingleRandomCar();
            yield return new WaitForSeconds(Random.Range(0.05f,0.015f));
        }
        yield return null;
    }

    private void DrawRandomCarsAtOnce()
    {
        for (int i = 0; i < numberOfRandomCarsToSpawn; i++)
        {
            DrawSingleRandomCar();
        }
    }

    private void DrawSingleRandomCar()
    {
        Transform spotToParkNewCar = DrawSpot();
        int randomCar = Random.Range(0, randomCarsList.Count);
        SpawnCarAndRotateIt(randomCarsList[randomCar], spotToParkNewCar.position, carsHolder.transform);
    }

    private GameObject SpawnCarAndRotateIt(GameObject car, Vector3 spotToPark, Transform parent, bool rotateCar = true, float defaultRotation = 0, bool randomRotation = false)
    {
        GameObject newCar = SpawnCar(car, spotToPark, parent);

        Vector3 rotationOfCar = newCar.transform.eulerAngles;
        
        if (rotateCar)
        {
            if (!randomRotation)
            {
                int randomRot = Random.Range(0, 2);
                if (randomRot == 0) rotationOfCar.y = 90;
                else rotationOfCar.y = -90;
                rotationOfCar.y += Random.Range(-randomRotationOfRandomCar, randomRotationOfRandomCar);
            }
            else
            {
                rotationOfCar.y += Random.Range(0.0f, 360.0f);
            }


        }
        else
        {
            rotationOfCar.y += defaultRotation;

        }

        newCar.transform.eulerAngles = rotationOfCar;

        return newCar;
    }

    private GameObject SpawnCar(GameObject car, Vector3 spotToPark, Transform parent)
    {
        spotToPark.y += Random.Range(altitudeToSpawnCar-randomnessOfAltitude, altitudeToSpawnCar+randomnessOfAltitude);
        //Debug.Log("car2: " + car.name + " spotToPark: " + spotToPark);
        GameObject spawnedCar = Instantiate(car, spotToPark, Quaternion.identity);
        spawnedCar.transform.parent = parent;
        return spawnedCar;
    }

    private Transform DrawSpot()
    {
        int randomSpot;
        do
        {
            randomSpot = Random.Range(0, highlightsParent.childCount);


        } while (takenSpots.Contains(randomSpot));

        takenSpots.Add(randomSpot);

        return highlightsParent.GetChild(randomSpot);
    }


    private void DrawSpotToPark()
    {

        PlaceToPark = DrawSpot();

        HighlightSpot(PlaceToPark, WaitMaterial);
        
    }

    private void HighlightSpot(Transform spot, Material materialHighlight)
    {
        spot.gameObject.SetActive(true);
        spot.gameObject.GetComponent<MeshRenderer>().material = materialHighlight;
    }

}
