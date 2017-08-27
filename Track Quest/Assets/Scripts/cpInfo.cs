using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class cpInfo : MonoBehaviour
{
    public List<GameObject> CheckPoints = new List<GameObject>();

	// Use this for initialization
	void Start ()
    {

        for(float n = 0; n <= (transform.childCount); n++)
        {
            Debug.Log("a" + n);
            if (transform.Find("cp_" + n) != null)
            {
                CheckPoints.Add(transform.Find("cp_" + n).gameObject);
            }
        }

        /*
        //Clean up null references, just in case.
        foreach(GameObject item in CheckPoints)
        {
            if (item == null)
                CheckPoints.Remove(item);
        }
        */
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void OnDrawGizmos()
    {
        foreach(GameObject item in CheckPoints)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(item.transform.position, item.transform.position + (item.transform.forward * 5.0f));
            Gizmos.DrawLine(item.transform.position, item.transform.position +
                (-item.transform.up * item.transform.localScale.y * 0.5f));
            Gizmos.DrawLine(item.transform.position, item.transform.position +
                (-item.transform.up * item.transform.localScale.y * 0.5f) + (-item.transform.right * item.transform.localScale.x * 0.5f));
            Gizmos.DrawLine(item.transform.position, item.transform.position +
                (-item.transform.up * item.transform.localScale.y * 0.5f) + (item.transform.right * item.transform.localScale.x * 0.5f));

            if (CheckPoints.IndexOf(item) != (CheckPoints.Count - 1))
                Handles.Label(item.transform.position + (Vector3.up * 2.0f), item.name);
            else
                Handles.Label(item.transform.position + (Vector3.up * 2.0f), (item.name + "/Start/Finish"));


            Gizmos.color = Color.white;
            if (CheckPoints.IndexOf(item) != 0)
                Gizmos.DrawLine(item.transform.position, CheckPoints[CheckPoints.IndexOf(item) - 1].transform.position);
            else
                Gizmos.DrawLine(item.transform.position, CheckPoints[CheckPoints.Count - 1].transform.position);
        }
    }
}
