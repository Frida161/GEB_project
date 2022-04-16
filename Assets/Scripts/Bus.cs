using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bus : MonoBehaviour
{
    /*
     * The is the object of a simulated bus.
     * The routine of the bus is divided into many nodes
     * Some nodes are bus stops while others are just turning point
     * The time scale of our project is 1min in app is 5 min in reality.
     */

    //The current latitude and longtitude of this bus object
    public Vector2d location;

    //The forwarding direction of this bus
    public Vector2d forward;

    //The target node of the bus
    public Vector2d target;

    //The magnitude from the last node to the next node
    public double targetMag;

    //Bus ID
    public int ID;

    //0 means line 1 and 1 means line 2
    public int line;

    //-1 means to upper campus and 1 means to lower campus
    public int dir;

    //Bus speed
    public double speed;

    //-1: in bus parking lot. 0-9: current node id
    public int status = -1;

    //The remaining waiting time after arriving at a bus stop
    public double waiting = 0;


    //This function is called every 0.1 second
    private void FixedUpdate()
    {
        //If the bus is waiting in parking lot, do nothing.
        if (status == -1) return;
        if (waiting > 0)
        {
            waiting -= Time.deltaTime;
            return;
        }

        //Check if at any bus stops

        //Check by the distance the bus has passed
        if ((location- BusSimulator.Lines[line][status]).magnitude >= targetMag)
        {
            //If arrived, update status
            status += dir;

            //Align the location
            location = BusSimulator.Lines[line][status];
            Debug.Log(Time.realtimeSinceStartup);
            Debug.Log("Bus " + ID.ToString() + " Arrive at " + status.ToString());

            //If arrived at destination
            if (status == 0 || status == 9)
            {
                Arrived();
                return;
            }
            //If arrived at any bus stops, wait for passengers.
            else if(BusSimulator.Stops.Contains(status))
            {
                waiting = 10f;
            }

            //At normal turning point
            //Redirect bus and calculate direction
            target = BusSimulator.Lines[line][status + dir];
            targetMag = (target - BusSimulator.Lines[line][status]).magnitude;
            forward = (target - location).normalized;

            //Go faster when in middle campus
            if (status == 2 || status == 3) speed = 0.0003;
            else speed = 0.0002;
        }
        //If not at any nodes, just update location
        else
        {
            location += forward * speed * Time.deltaTime;
        }
        
    }

    public void Initialize(Vector2d loc, int id, int l, int d)
    {
        location = loc;
        ID = id;
        line = l;
        dir = d;
    }

    public void Arrived()
    {
        //If arrived at bus station, add the object into parking lot
        //and reverse direction
        if (status == 0)
        {
            Debug.Log("Bus " + ID.ToString() + "Arrive at parking lot upper campus");
            BusSimulator.singleton.ParkingLotUpper.Enqueue(this);
            dir = 1;
        }
        else
        {
            dir = -1;
            if (line == 0)
            {
                Debug.Log("Bus " + ID.ToString() + "Arrive at parking lot libary");
                BusSimulator.singleton.ParkingLotLib.Enqueue(this);
            }
            else
            {
                Debug.Log("Bus " + ID.ToString() + "Arrive at parking lot TB");
                BusSimulator.singleton.ParkingLotTB.Enqueue(this);
            }
        }
        status = -1;
    }

    //Set off the bus
    public void SetBus(int l)
    {
        //Set line and set direction
        line = l;
        if (dir == 1)
        {
            status = 0;
            target = BusSimulator.Lines[l][1];
            targetMag = (target - BusSimulator.Lines[l][status]).magnitude;
            forward = (target - location).normalized;
        }
        else
        {
            status = 9;
            target = BusSimulator.Lines[l][8];
            targetMag = (target - BusSimulator.Lines[l][status]).magnitude;
            forward = (target - location).normalized;
        }
    }


}
