using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StatClasses;

public class carScript : MonoBehaviour
{
    //public carStats stats = new carStats();

    public float topSpeed, acceleration, steering, brakes;
    public float onRoadGrip, offRoadGrip;
    public bool isAI;

    //=======================================================
    //TRANSFORMS
    private Transform cam;
    private Transform wheel_fl, wheel_fr, wheel_bl, wheel_br;
    private Transform bodyOrigin;
    //=======================================================

    //=======================================================
    //TIMERS
    private timer reverseTransmission = new timer(0.3f);
    //=======================================================

    private float terrainModifier, finalTopSpeed, currentSpeed, deaccelRate;

    private float currentTurnSpeed, targetAngle, currentVelocityAngle;

    private float turnChange, modSteering;

    private float driftModifier = 1.4f;

    private float targetBank = 0, targetRoll = 0;

    private float gravity = 2.0f, yVelocity = 0, terminalVelocity = 6.0f;

    private Vector3 velocityDirection, newPos;

    //======================================================
    //ENUMS
    private terrainTypes currentTerrain = terrainTypes.road;
    private List<driveState> ListOfStates = new List<driveState>();
    public driverNumbers driverNumber;
    //======================================================
    //Sounds
    private AudioSource s_engine;

    float enginePitch = 1.0f;
    //======================================================

    private

	void Start()
    {
        transform.name = "car" + driverNumberStrings(driverNumber);

        deaccelRate = acceleration / 3;

        finalTopSpeed = topSpeed;
        velocityDirection = transform.forward;
        targetAngle = transform.rotation.y;
        turnChange = 0;

        currentVelocityAngle = transform.rotation.y;

        wheel_fl = transform.Find("wheels").Find("wheel_fl");
        wheel_fr = transform.Find("wheels").Find("wheel_fr");
        wheel_bl = transform.Find("wheels").Find("wheel_bl");
        wheel_br = transform.Find("wheels").Find("wheel_br");

        cam = transform.Find("camPivot");
        cam.name = "camPivot" + driverNumberStrings(driverNumber);
        cam.parent = null;

        bodyOrigin = transform.Find("chassis");

        newPos = transform.position;

        InitSounds();
	}

    void Update()
    {
        currentTerrain = GetTerrain();
        finalTopSpeed = topSpeed * GetTopSpeedModifier();

        if (!ListOfStates.Contains(driveState.in_air))
        {
        
            //Forward and backwards
            if (Input.GetAxis("Vertical") > 0)
            {
                reverseTransmission.Reset();
                if (currentSpeed > finalTopSpeed)
                    currentSpeed -= (deaccelRate * Time.deltaTime);
                else
                    currentSpeed += ((acceleration * Input.GetAxis("Vertical")) * Time.deltaTime);
            }
            else if (Input.GetAxis("Vertical") == 0)
            {
                reverseTransmission.Reset();
                if (currentSpeed > (deaccelRate * Time.deltaTime))
                {
                    currentSpeed -= (deaccelRate * Time.deltaTime);
                }
                else if (currentSpeed < (-deaccelRate * Time.deltaTime))
                {
                    currentSpeed += (deaccelRate * Time.deltaTime);
                }
                else
                {
                    currentSpeed = 0;
                }
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                if (currentSpeed > (brakes * Time.deltaTime))
                {
                    currentSpeed -= (brakes * Time.deltaTime);
                }
                else if (currentSpeed < (-topSpeed / 4))
                {
                    currentSpeed = (-finalTopSpeed / 4);
                }
                else if ((currentSpeed <= 0) && (reverseTransmission.Ended()))
                {
                    currentSpeed -= (acceleration * Time.deltaTime);
                }
                else
                {
                    reverseTransmission.Tick();
                    currentSpeed = 0;
                }
            }
        }

        //Turning
        if (Input.GetAxis("Horizontal") != 0)
        {
            modSteering = GetSteeringAfterModifier();

            if (currentSpeed > 0)
            {
                targetAngle += ((modSteering * Input.GetAxis("Horizontal")) * Time.deltaTime);
                turnChange += ((modSteering * Input.GetAxis("Horizontal")) * Time.deltaTime);
            }
            else if (currentSpeed < 0)
            {
                targetAngle -= ((modSteering * Input.GetAxis("Horizontal")) * Time.deltaTime);
                turnChange -= ((modSteering * Input.GetAxis("Horizontal")) * Time.deltaTime);
            }
        }

        //===================================================================================================
        if (turnChange > 90.0f)
        {
            turnChange = 90.0f;
            targetAngle = currentVelocityAngle + 90.0f;
        }
        if (turnChange < -90.0f)
        {
            turnChange = -90.0f;
            targetAngle = currentVelocityAngle - 90.0f;
        }
        //===================================================================================================

        //===================================================================================================
        //Keep the numbers from overinflating by knocking them down/up by 360 whenever the amount goes over.
        if (targetAngle > 360.0f)
        {
            targetAngle = 0;
            currentVelocityAngle -= 360.0f;
        }
        if (targetAngle < -360.0f)
        {
            targetAngle = 0;
            currentVelocityAngle += 360.0f;
        }
        //====================================================================================================

        //==========================================================
        //Simulation of the reduced ability to turn at higher speeds
        float gForceFactor = ((currentSpeed / 2) / finalTopSpeed);
        if (gForceFactor > 1)
            gForceFactor = 1;

        currentTurnSpeed =  (GetGripModifier() *
                            (1.0f - gForceFactor));
        //==========================================================

        if (currentVelocityAngle != targetAngle)
        {
            float speedModifier = 0;
            if (turnChange > 0)
                speedModifier = turnChange;
            else if (turnChange < 0)
                speedModifier = -turnChange;

            if (speedModifier > 90.0f)
                speedModifier = 90;
            if (speedModifier < 0)
                speedModifier = 0;

            currentSpeed *= (1 - ((speedModifier / 90) / 40));

            if (turnChange > 0)
            {
                if (turnChange < (currentTurnSpeed * Time.deltaTime))
                {
                    currentVelocityAngle = targetAngle;
                    turnChange = 0;
                }
                else
                {
                    currentVelocityAngle += (currentTurnSpeed * Time.deltaTime);
                    turnChange -= (currentTurnSpeed * Time.deltaTime);
                }
            }
            else if (turnChange < 0)
            {
                if (turnChange > -(currentTurnSpeed * Time.deltaTime))
                {
                    currentVelocityAngle = targetAngle;
                    turnChange = 0;
                }
                else
                {
                    currentVelocityAngle -= (currentTurnSpeed * Time.deltaTime);
                    turnChange += (currentTurnSpeed * Time.deltaTime);
                }
            }
        }
        
        GetSlopeRotations();

        velocityDirection = Quaternion.Euler(targetBank, currentVelocityAngle, targetRoll) * Vector3.forward;

        SoundHandler_Update();

        //Debug.Log((int)currentSpeed + " / " + (int)targetAngle + " / " + (int)turnChange + " / " + currentTerrain.ToString() + " / " + targetBank + ":" + targetRoll);
    }

    void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(targetBank, targetAngle, targetRoll);

        if (!ListOfStates.Contains(driveState.in_air))
        {
            transform.position +=
                (velocityDirection
                    *
                (currentSpeed * Time.fixedDeltaTime));
            yVelocity = velocityDirection.y * (currentSpeed * Time.fixedDeltaTime);
        }
        else
        {
            if (yVelocity < -terminalVelocity)
                yVelocity = -terminalVelocity;
            else
                yVelocity -= (gravity * Time.fixedDeltaTime);

            newPos = transform.position + (velocityDirection
                                            *
                                            (currentSpeed * Time.fixedDeltaTime));
            newPos.y += yVelocity;

            transform.position = newPos;
        }

        //Rotate the wheels
        wheel_fl.localRotation = Quaternion.Lerp(wheel_fl.localRotation, Quaternion.Euler(0, 90 + (40.0f * Input.GetAxis("Horizontal")), 0), 0.3f);
        wheel_fr.localRotation = Quaternion.Lerp(wheel_fr.localRotation, Quaternion.Euler(0, 90 + (40.0f * Input.GetAxis("Horizontal")), 0), 0.3f);

        cam.position = bodyOrigin.position;
        cam.rotation = Quaternion.Lerp(cam.rotation, new Quaternion(0, transform.rotation.y,0, transform.rotation.w), 0.03f);
    }

    public string driverNumberStrings(driverNumbers input)
    {
        switch (input)
        {
            case driverNumbers.P1:
                return "P1";
            case driverNumbers.P2:
                return "P2";
            case driverNumbers.P3:
                return "P3";
            case driverNumbers.P4:
                return "P4";
            case driverNumbers.P5:
                return "P5";
            case driverNumbers.P6:
                return "P6";
            case driverNumbers.P7:
                return "P7";
            case driverNumbers.P8:
                return "P8";
        }

        return null;
    }

    public terrainTypes GetTerrain()
    {
        Ray roadCheck = new Ray();

        roadCheck.origin = new Vector3 (bodyOrigin.position.x, bodyOrigin.position.y + 0.5f, bodyOrigin.position.z);
        roadCheck.direction = -transform.up;

        RaycastHit hitInfo;
        int LayerMask = 1 << 8;

        //Debug.DrawRay(roadCheck.origin, roadCheck.direction, Color.red);

        if (Physics.Raycast(roadCheck, out hitInfo, 1.0f, LayerMask))
        {
            if (hitInfo.transform.GetComponent<materialType>() != null)
            {
                return hitInfo.transform.GetComponent<materialType>().type;
            }
        }
        return terrainTypes.road;
    }

    public float GetGripModifier()
    {
        switch (currentTerrain)
        {
            case terrainTypes.road:
                return onRoadGrip;

            case terrainTypes.dirtRoad:
                return offRoadGrip;

            case terrainTypes.grass:
                return offRoadGrip * 0.9f;

            case terrainTypes.sand:
                return offRoadGrip * 0.9f;

            case terrainTypes.snow:
                return offRoadGrip * 0.7f;

            case terrainTypes.ice:
                return offRoadGrip * 0.6f;
        }


        return onRoadGrip;
    }

    public float GetTopSpeedModifier()
    {
        if (ListOfStates.Contains(driveState.boosting))
            return 1.4f;
        else if (ListOfStates.Contains(driveState.mini_boosting))
            return 1.2f;

        switch(currentTerrain)
        {
            case terrainTypes.road:
                return 1.0f;

            case terrainTypes.dirtRoad:
                return 1.0f;

            case terrainTypes.grass:
                return 0.6f;

            case terrainTypes.sand:
                return 1.0f;

            case terrainTypes.snow:
                return 0.9f;

            case terrainTypes.ice:
                return 1.0f;
        }

        return 1.0f;
    }

    public float GetAccelAfterModifier()
    {
        if (ListOfStates.Contains(driveState.boosting))
            return acceleration * 1.6f;
        else if (ListOfStates.Contains(driveState.mini_boosting))
            return acceleration * 1.5f;

        return acceleration;
    }

    public float GetSteeringAfterModifier()
    {
        float finalValue = 1.0f;
        float baseFactor = 1.4f;

        //Reduces your ability to turn based on how fast you are going. G-force resistance and all that.
        //Increasing base factor causes the maximum effect to hamper steering MORE, and decreasing it does the reverse.
        finalValue = 1.0f / (baseFactor +
            (currentSpeed / topSpeed)
            );

        return steering * finalValue;
    }

    public void GetSlopeRotations()
    {
        LayerMask lMask = 1 << 8;

        Vector3 fl_point = new Vector3(), fr_point = new Vector3(), bl_point = new Vector3(), br_point = new Vector3();

        Ray downCheck = new Ray();
        RaycastHit hitInfo;

        //===============================================
        //Origin
        downCheck.origin = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z);
        downCheck.direction = Vector3.down;

        float downV = 0;

        if ((yVelocity < 0) && (ListOfStates.Contains(driveState.in_air)))
            downV = (yVelocity * Time.fixedDeltaTime);
        
        Debug.DrawRay(downCheck.origin, downCheck.direction * (0.4f + downV), Color.yellow);

        //Downwards gravity checks. If the car is on a surface or not.
        if (Physics.Raycast(downCheck, out hitInfo, 0.4f + downV, lMask))
        {
            DeactivateState(driveState.in_air);
            transform.position = hitInfo.point;
        }
        else
        {
            downCheck.origin = (downCheck.origin + (transform.forward * 2.0f));

            Debug.DrawRay(downCheck.origin, downCheck.direction * (0.4f + downV), Color.yellow);
            if (Physics.Raycast(downCheck, out hitInfo, 0.4f + downV, lMask))
            {
                Debug.Log("land");
                DeactivateState(driveState.in_air);
                transform.position = hitInfo.point - (transform.forward * 2.0f);
            }
            else
            {
                ActivateState(driveState.in_air);
            }
        }

        if (!ListOfStates.Contains(driveState.in_air))
        {
            downCheck.direction = -transform.up;
            //===============================================
            //Front Left
            downCheck.origin = new Vector3(wheel_fl.position.x, wheel_fl.position.y + 0.3f, wheel_fl.position.z);
            Debug.DrawRay(downCheck.origin, downCheck.direction * 1.0f, Color.yellow);

            if (Physics.Raycast(downCheck, out hitInfo, 1.0f, lMask))
            {
                fl_point = hitInfo.normal;
            }
            //==============================================
            //Front Right
            downCheck.origin = new Vector3(wheel_fr.position.x, wheel_fr.position.y + 0.3f, wheel_fr.position.z);
            Debug.DrawRay(downCheck.origin, downCheck.direction * 1.0f, Color.yellow);

            if (Physics.Raycast(downCheck, out hitInfo, 1.0f, lMask))
            {
                fr_point = hitInfo.normal;
            }
            //===============================================
            //Back Left
            downCheck.origin = new Vector3(wheel_bl.position.x, wheel_bl.position.y + 0.3f, wheel_bl.position.z);
            Debug.DrawRay(downCheck.origin, downCheck.direction * 1.0f, Color.yellow);

            if (Physics.Raycast(downCheck, out hitInfo, 1.0f, lMask))
            {
                bl_point = hitInfo.normal;
            }
            //==============================================
            //Front Right
            downCheck.origin = new Vector3(wheel_br.position.x, wheel_br.position.y + 0.3f, wheel_br.position.z);
            Debug.DrawRay(downCheck.origin, downCheck.direction * 1.0f, Color.yellow);

            if (Physics.Raycast(downCheck, out hitInfo, 1.0f, lMask))
            {
                br_point = hitInfo.normal;
            }
            //==============================================

            Vector3 fwd_noflip = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            Vector3 right_noflip = Quaternion.Euler(0, targetAngle, 0) * Vector3.right;

            Vector3 averageNormal = (fl_point + fr_point + bl_point + br_point) / 4;
            targetBank = -(Vector3.Angle(averageNormal, fwd_noflip) - 90);
            targetRoll = (Vector3.Angle(averageNormal, right_noflip) - 90);
        }
        else
        {
            targetBank = Mathf.LerpAngle(targetBank, 0.0f, 0.1f);
            targetRoll = Mathf.LerpAngle(targetRoll, 0.0f, 0.1f);
        }
    }

    //===============================================================
    //Sound Handlers
    void InitSounds()
    {
        Transform s_parent = transform.Find("sounds");

        s_engine = s_parent.Find("engine").GetComponent<AudioSource>();
    }

    void SoundHandler_Update()
    {
        if (!ListOfStates.Contains(driveState.in_air))
            enginePitch = (
                0.5f + (currentSpeed / 40.0f));
        else
        {
            enginePitch = Mathf.Lerp(enginePitch, (0.5f + (currentSpeed / 20.0f)), 0.1f);
        }
        
        s_engine.pitch = enginePitch;
    }
    //===============================================================

    //===============================================================
    //State List Interactions
    public void ActivateState(driveState targetState)
    {
        if (!ListOfStates.Contains(targetState))
        {
            driveState[] overwrites = StatesToOverwrite(targetState);
            if (overwrites != null)
            {
                foreach (driveState item in overwrites)
                    DeactivateState(item);
            }
            ListOfStates.Add(targetState);
        }
    }

    public void DeactivateState(driveState targetState)
    {
        if (ListOfStates.Contains(targetState))
            ListOfStates.Remove(targetState);
    }

    public driveState[] StatesToOverwrite(driveState targetState)
    {
        switch (targetState)
        {
            case driveState.still:
                return new driveState[4] { driveState.accelerating, driveState.deaccelerating, driveState.braking, driveState.reversing };

            case driveState.accelerating:
                return new driveState[4] { driveState.still, driveState.deaccelerating, driveState.braking, driveState.reversing };

            case driveState.deaccelerating:
                return new driveState[4] { driveState.still, driveState.accelerating, driveState.braking, driveState.reversing };

            case driveState.reversing:
                return new driveState[4] { driveState.still, driveState.accelerating, driveState.braking, driveState.deaccelerating };

            case driveState.boosting:
                return new driveState[1] { driveState.mini_boosting };
        }

        return null;
    }
    //================================================================
}
