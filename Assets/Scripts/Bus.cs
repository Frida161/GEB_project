using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bus : MonoBehaviour
{

    public Vector2d location;
    public Vector2d forward;
    public Vector2d target;
    public double targetMag;
    public int ID;
    public int line;
    public int dir;
    public double speed;
    public int status = -1;
    public double waiting = 0;


    private void FixedUpdate()
    {
        if (status == -1) return;
        if (waiting > 0)
        {
            waiting -= Time.deltaTime;
            return;
        }

        //Check if at any bus stops
        if ((location- BusSimulator.Lines[line][status]).magnitude >= targetMag)
        {
            status += dir;
            location = BusSimulator.Lines[line][status];
            Debug.Log(Time.realtimeSinceStartup);
            Debug.Log("Bus " + ID.ToString() + " Arrive at " + status.ToString());
            if (status == 0 || status == 9)
            {
                Arrived();
                return;
            }
            else if(BusSimulator.Stops.Contains(status))
            {
                waiting = 10f;
            }
            target = BusSimulator.Lines[line][status + dir];
            targetMag = (target - BusSimulator.Lines[line][status]).magnitude;
            forward = (target - location).normalized;

            if (status == 2 || status == 3) speed = 0.0003;
            else speed = 0.0002;
        }
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

    public void SetBus(int l)
    {
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
