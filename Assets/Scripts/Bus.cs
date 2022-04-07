using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bus : MonoBehaviour
{

    public Vector2 location;
    public Vector2 forward;
    public Vector2 target;
    public int ID;
    public int line;
    public int dir;
    public float speed = 10f;
    public int status = -1;
    public float waiting = 0;


    private void FixedUpdate()
    {
        if (status == -1) return;
        if (waiting > 0)
        {
            waiting -= Time.deltaTime;
            return;
        }

        //Check if at any bus stops
        if ((location-target).magnitude < 0.00001f)
        {
            location = BusSimulator.Lines[line][status];
            status += dir;
            if (status == 0 || status == 9)
            {
                Arrived();
            }
            else if(BusSimulator.Stops.Contains(status))
            {
                waiting = 1f;
                target = BusSimulator.Lines[line][status + dir];
                forward = (target - location).normalized;
                Debug.Log("Arrive at " + status.ToString());
            }
            
        }
        else
        {
            location += forward * speed * Time.deltaTime;
            //Debug.Log("Current postion: " + location.ToString());
            Debug.Log(speed * Time.deltaTime);
        }
        
    }

    public void Initialize(Vector2 loc, int id, int l, int d)
    {
        location = loc;
        ID = id;
        line = l;
        dir = d;
    }

    public void Arrived()
    {
        if (status == 0)
        {
            Debug.Log("Arrive at parking lot upper campus");
            BusSimulator.singleton.ParkingLotUpper.Enqueue(this);
            dir = 1;
        }
        else
        {
            dir = -1;
            if (line == 0)
            {
                Debug.Log("Arrive at parking lot libary");
                BusSimulator.singleton.ParkingLotLib.Enqueue(this);
            }
            else
            {
                Debug.Log("Arrive at parking lot TB");
                BusSimulator.singleton.ParkingLotTB.Enqueue(this);
            }
        }
        status = -1;
    }

    public void SetBus(int l)
    {
        line = l;
        if (dir == 1)
        {
            status = 0;
            target = BusSimulator.Lines[l][1];
            forward = (target - location).normalized;
        }
        else
        {
            status = 9;
            target = BusSimulator.Lines[l][8];
            forward = (target - location).normalized;
        }
    }


}
