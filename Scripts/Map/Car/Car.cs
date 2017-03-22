﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Car : MonoBehaviour
{
    public const float FREEZE_DRAG = 100000;
    public const float RUNNING_DRAG = 0;
    public const float RUNNING_ANGULAR_DRAG = 10;
    private Vector3 com = new Vector3(0,0,0);
    private float maxMotorTorque = 10000;
    private float maxBrakeTorque = 30000;
    private float maxSteerAngle = 15;
    public GameObject frontLeft, frontRight, rearLeft, rearRight;
    //public float topSpeed = 100 * 1000 / 3600;//(100 km/h)
    public float carVelocity;

    
    private Rigidbody rb;
    private WheelController flController, frController, rlController, rrController;

    private float maxTiltAngle = 20;


    //private float stuckSpeedThres = 3;

    private Skill mySkill;

    /* public float myHP = 100;
     private float myMaxHP = 100;
     private float myMaxMP=100;
     public float myMP=100;
     private float myAttackPower;
     public float mySkillCD;*/
    private CarStatus status;

    // indicate whether the car is stopping
    public bool stopFlag = false;
    // indicate whether the car is stopped by others
    public bool stoppedBySkill = false;



    private int respawnPositionIdx = -1;
    private float reSpawnTime = 5;
    private float reSpawnTimer = 0;

    private void Awake()
    {
        status = GetComponent<CarStatus>();
        mySkill = GetComponent<Skill>();

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = com;
        flController = frontLeft.GetComponent<WheelController>();
        frController = frontRight.GetComponent<WheelController>();
        rlController = rearLeft.GetComponent<WheelController>();
        rrController = rearRight.GetComponent<WheelController>();
    }

  /*  // Use this for initialization 
    void Start()
    {
        
    }

    */

    // Update is called once per frame 
    void Update()
    {
        
        if (!stopFlag && !stoppedBySkill)
        {
            reSpawnTimer += Time.deltaTime;
            if (reSpawnTimer > reSpawnTime)
            {
                respawn();
            }
        }



        limitAngleAndVelocity();
    }

    private void limitAngleAndVelocity()
    {
        carVelocity = GetComponent<Rigidbody>().velocity.magnitude;
        if (transform.rotation.eulerAngles.x > 180)
        {
            transform.rotation = Quaternion.Euler(
                 Mathf.Clamp(transform.rotation.eulerAngles.x, 360 - maxTiltAngle, 360),
                 transform.rotation.eulerAngles.y,
                 transform.rotation.eulerAngles.z
                 );
        }
        else if (transform.rotation.eulerAngles.x < 180)
        {
            transform.rotation = Quaternion.Euler(
                 Mathf.Clamp(transform.rotation.eulerAngles.x, 0, maxTiltAngle),
                 transform.rotation.eulerAngles.y,
                 transform.rotation.eulerAngles.z
                 );
        }

        if (transform.rotation.eulerAngles.z > 180)
        {
            transform.rotation = Quaternion.Euler(
                 transform.rotation.eulerAngles.x,
                 transform.rotation.eulerAngles.y,
                 Mathf.Clamp(transform.rotation.eulerAngles.z, 360 - maxTiltAngle, 360)
                 );
        }
        else if (transform.rotation.eulerAngles.z < 180)
        {
            transform.rotation = Quaternion.Euler(
                 transform.rotation.eulerAngles.x,
                 transform.rotation.eulerAngles.y,
                 Mathf.Clamp(transform.rotation.eulerAngles.z, 0, maxTiltAngle)
                 );
        }

        if (rb.velocity.magnitude > status.topSpeed * status.topSpeedModifier)
        {
            float slowDownRatio = rb.velocity.magnitude / (status.topSpeed * status.topSpeedModifier);
            rb.velocity /= slowDownRatio;
        }
    }

    void FixedUpdate()
    {
        //Debug.Log("static flags = (" + StaticVariables.gameStarts + "," + StaticVariables.gameIsOver + ")");
        if (!StaticVariables.gameStarts || StaticVariables.gameIsOver || stoppedBySkill)
        {
            stopRunning();
            return;
        }
        startRunning();
                
        
    }
    

    public void ApplyThrottle(float throttleFactor)
    {
        if (stopFlag)
        {
            throttleFactor = 0;
        }
        flController.ApplyThrottle(maxMotorTorque * throttleFactor);
        frController.ApplyThrottle(maxMotorTorque * throttleFactor);
        rlController.ApplyThrottle(maxMotorTorque * throttleFactor);
        rrController.ApplyThrottle(maxMotorTorque * throttleFactor);
    }

    public void ApplyBrake(float brakeFactor)
    {
        flController.ApplyBrake(maxBrakeTorque* brakeFactor);
        frController.ApplyBrake(maxBrakeTorque* brakeFactor);
        rlController.ApplyBrake(maxBrakeTorque* brakeFactor);
        rrController.ApplyBrake(maxBrakeTorque* brakeFactor);
    }

    public bool useSkill()
    {
        if (status.currMP == status.maxMP && !stoppedBySkill)
        {
            mySkill.activateSkill();
            status.currMP = 0;
            return true;
        }
        return false;
    }

    public void stopBySkill(bool flag)
    {
        stoppedBySkill = flag;
    }
   

    public void ApplyPedal(float motorTorqueFactor, float brakeTorqueFactor)
    {
        ApplyBrake(brakeTorqueFactor);
        ApplyThrottle(motorTorqueFactor);
    }

    public void ApplySteer(float steerFactor)
    {
        float steerAngle = steerFactor * maxSteerAngle;
        flController.ApplySteer(steerAngle);
        frController.ApplySteer(steerAngle);
        
    }

    private AudioSource SetUpEngineAudioSource(AudioClip clip)
    {
        // create the new audio source component on the game object and set up its properties
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0;
        source.loop = true;

        // start the clip from a random point
        source.time = Random.Range(0f, clip.length);
        source.Play();
        //source.minDistance = 5;
        //source.dopplerLevel = 0;
        return source;
    }

    

    public float getTopSpeed()
    {
        return status.topSpeed;
    }

    private void diasbleAllController()
    {

        AIScript ai = GetComponent<AIScript>();
        PlayerController player = GetComponent<PlayerController>();
        WheelController[] wheels = GetComponentsInChildren<WheelController>();
        if (ai != null)
        {
            ai.PauseAI();
        }
        if (player != null)
        {
            player.enabled = false;
        }

        foreach (WheelController wheel in wheels)
        {
            if (wheel != null)
            {
                //wheel.enabled = false;
                // fake freezing
                wheel.setMeshRotationFlag(false);
            }
        }
    }

    private void enableAllController()
    {
        AIScript ai = GetComponent<AIScript>();
        PlayerController player = GetComponent<PlayerController>();
        WheelController[] wheels = GetComponentsInChildren<WheelController>();
        if (ai != null)
        {
            ai.resumeAI();
        }
        if (player != null)
        {
            player.enabled = true;
        }

        foreach (WheelController wheel in wheels)
        {
            if (wheel != null)
            {
                wheel.setMeshRotationFlag(true);
            }
        }
    }

    public void stopRunning()
    {
        if (!stopFlag)
        {
            foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
            {
                rb.drag = FREEZE_DRAG;
                rb.angularDrag = FREEZE_DRAG;
            }

            diasbleAllController();

            stopFlag = true;

            mySkill.stopSkill();
        }
    }

    public void startRunning()
    {
        if (stopFlag)
        {
            foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
            {
                rb.drag = RUNNING_DRAG;
                rb.angularDrag = RUNNING_ANGULAR_DRAG;
            }

            enableAllController();
            stopFlag = false;
        }
    }

    

    

    

    

    




    private void respawn()
    {
        CarCheckPoint respawnPoint = findCheckPoint(respawnPositionIdx);
        Vector3 pos = new Vector3(
            respawnPoint.transform.position.x,
            respawnPoint.transform.position.y -
            respawnPoint.transform.localScale.y/2 + 1,
            respawnPoint.transform.position.z
            );
        Quaternion rot = respawnPoint.transform.rotation;
        
        transform.position = pos;
        transform.rotation = rot;
        reSpawnTimer = 0;
    }

    public void setRespawnIdx(int idx)
    {
        if (idx > respawnPositionIdx)
        {
            respawnPositionIdx = idx;
            reSpawnTimer = 0;
        }
    }

    private CarCheckPoint findCheckPoint(int dist)
    {
        CarCheckPoint[] checkpoints = FindObjectsOfType<CarCheckPoint>();
        dist = Mathf.Clamp(dist,0, checkpoints.Length-1); 
        foreach (CarCheckPoint point in checkpoints)
        {
            if (point.dist == dist)
            {
                Debug.Log("there are " + checkpoints.Length + "points; respawned at " + point.name);
                return point;
            }
        }

        Debug.Log("Error in  findCheckPoint("+ dist + ")");
        return checkpoints[0];
    }

}