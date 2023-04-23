using System;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;

	public GameObject[] asteroids;
	
	public float mouseSensitivity = 5.0f;

	public bool yInvert = false;


    //Camera variables
    public GameObject mainCamera;
    private Camera _cam;
    private int normalFov = 60;

    public Transform playerDreadnaught;
    public Mothership alienMothership;

	public GameObject[] enemyList;

    //Gamestate variables
    public bool gameStarted = false;

	public bool gameOver = false;

	private void Awake()
	{
		Instance = this;
		asteroids = GameObject.FindGameObjectsWithTag("Environment");
	}

	// Use this for initialization
	void Start () {
		//Hide Cursor = false
		Cursor.visible = true; 
		alienMothership = FindObjectOfType<Mothership>();
		enemyList = GameObject.FindGameObjectsWithTag ("Enemy");
		_cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
	
		enemyList = GameObject.FindGameObjectsWithTag ("Enemy");

		//Check to see if Game Started
        if(Input.GetKeyDown("space") && gameStarted == false) {
            gameStarted = true;
            playerDreadnaught.gameObject.SetActive(true);
            mainCamera.transform.position = playerDreadnaught.position;
            mainCamera.GetComponent<ThirdPersonCamera>().enabled = true;
            mainCamera.GetComponent<Orbit>().enabled = false;
            mainCamera.GetComponent<Camera>().fieldOfView = 179;
        }

        //FOV warping effect
        if (_cam.fieldOfView >= normalFov)
            _cam.fieldOfView -= Time.deltaTime * 100;

        //Game Over conditions met
        if (enemyList.Length == 0 && !alienMothership)
            gameOver = true;
    }
}
