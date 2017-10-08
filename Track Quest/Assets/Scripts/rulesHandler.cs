using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StatClasses;

public struct driverStanding
{
    public driverNumbers driverID;
    public int position;
    public int currentLap;
    public int currentCP;
    public float finishTime;
}

public class rulesHandler : MonoBehaviour
{
    public int laps;

    [HideInInspector]
    public float raceTime;

    public driverNumbers[] P_AI_slots;
    private driverStanding[] driverTable;

	// Use this for initialization
	void Start ()
    {
        driverTable = new driverStanding[P_AI_slots.Length];

        int inc = 0;
        foreach (driverStanding item in driverTable)
        {
            driverTable[inc].driverID = P_AI_slots[inc];
            inc++;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
