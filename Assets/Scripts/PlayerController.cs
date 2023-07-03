using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.XR;

public class PlayerController : Unit
{
    [SerializeField]
    private CharacterController controller;

    private Camera playerCam; //this is the camera in our game

    [SerializeField]
    private float speed = 12f;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float gravity = -9.81f;
    [SerializeField]
    private float jumpHeight = 3f;

    Vector3 velocity;
    bool isGrounded;

    [SerializeField]
    private Transform groundCheck;
    
    private float groundDistance = 0.4f;

    [SerializeField]
    private LayerMask groundMask;
  
    private float turnSmoothVelocity;

    [SerializeField]
    private float turnSmoothTime = 0.2f;

    [SerializeField]
    GameManager gameManager;

    [SerializeField]
    private GameObject gun;

    [SerializeField]
    Laser laserPrefab;

    private float defaultView;

    [SerializeField]
    private float zoomIn = 3f;

    [SerializeField]
    private float smoothZoom = 2.0f;

    [SerializeField]
    private float zoomSmooth;

    Traps traps;

    protected override void Start()
    {
        base.Start();
        playerCam = GetComponentInChildren<Camera>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        respawnPos = this.transform.position; //Change this to the checkpoint mechanic

        defaultView = playerCam.fieldOfView;
        zoomSmooth = zoomIn / 2f;
    }
    private void ShowLasers(Vector3 targetPosition) //the target position is what we are aiming for
    {

        Laser laser = Instantiate(laserPrefab) as Laser; //the "as Laser" casts the game object to a laser; this is a technique we can use if we know we are creating a game object of a specific type (in this case, we know the laserPrefab is going to be a Laser)
        laser.Init(Color.red, gun.transform.position, targetPosition);

    }

    private Vector3 GetGunPosition()
    {
        return (gun.transform.position);//change from an array later line 12
        
    }
    void Update()
    {

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //if (traps.playerDead)
        //{
        //    isAlive = false;
        //}

        if (isAlive)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector2 input = new Vector2(x, z);
            Vector2 inputDir = input.normalized;

            // dont jumpo when paused and dies 

            //if (gameManager.gameOver == false)//stop moving if the game ends
            //{

            //}

            Vector3 move = transform.right * x + transform.forward * z;

            //Adjusting player movement considering camera position
            if (inputDir != Vector2.zero)
            {
                float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) + playerCam.transform.eulerAngles.y;
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);

            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                move *= runSpeed;
                //Code for running animation
            }
            else
            {
                move *= speed;
                //Code for stop running animation
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                Debug.Log("Crouching");
                //Code for crouching animation
            }


            controller.Move(move * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);

            //Shooting Script

            if (Input.GetButton("Fire2")) //Right mouse click
            {
                playerCam.fieldOfView = Mathf.Lerp(defaultView, defaultView / zoomIn, defaultView / zoomSmooth);
                transform.eulerAngles = playerCam.transform.eulerAngles;


                if (Input.GetButtonDown("Fire1"))
                {
                    //before we can show lasers going out into the infinite distance, we need to see if we are going to hit something
                    LayerMask mask = ~LayerMask.GetMask("AISpot", "JeanRaider", "Ground", "Interactables");


                    //we are having to do some ray casting
                    Ray ray = new Ray(GetGunPosition(), playerCam.transform.forward); //aim our ray in the direction that we are looking
                    RaycastHit hit; //our hit is going to be used as an output of a Raycast
                                    //so we need to use a layermask and a layermask is 
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                    {
                        //if this is true, we hit something
                        Attack(hit);
                        Debug.Log("Got them");
                        ShowLasers(hit.point);
                    }
                    else
                    {
                        //we now need to figure out a position we are firing
                        Vector3 targetPos = GetGunPosition() + playerCam.transform.forward * DISTANCE_SHOT_IF_NO_HIT;
                        Debug.Log("pew");
                        ShowLasers(targetPos);
                    }

                }

            }
            else
            {
                playerCam.fieldOfView = defaultView;
            }
        }
        else //If player not alive call die function
        {
            Die();
        }

        Debug.Log(respawnPos);


    }

    private void OnTriggerEnter(Collider other)//Checkpoint script
    {
        if (other.tag == "Checkpoint")
        {
            respawnPos = other.transform.position; //Setting the respawnPos to the position of the checkpoint
            Destroy(other.gameObject);
        }
    }

        //private void OnTriggerEnter(Collider other) // collectables
        //{
        //    if (other.gameObject.GetComponent<GoldCoin>())//special collectible with goldCoin script  
        //    {
        //        //GoldCoin collectableObject = other.gameObject.GetComponent<GoldCoin>();
        //        gameManager.IncreaseScore(collectableObject.GetNumPoints());
        //        Destroy(other.gameObject);
        //        HandColorer();
        //    }
        //    else if (other.gameObject.GetComponent<Collectible>()) // every other collectable
        //    {
        //        Collectible collectableObject = other.gameObject.GetComponent<Collectible>();
        //        gameManager.IncreaseScore(collectableObject.GetNumPoints());
        //        Destroy(other.gameObject);
        //    }

        //}

}

