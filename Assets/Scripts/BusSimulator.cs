using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BusInfo
{
    public int busId;
    public int dir; //-1: Go upper; 1: Go lower
    public int line; //0 or 1
    public int status; //-1 means waiting in parking lot
    public Vector2d location;

    public BusInfo(Bus bus)
    {
        busId = bus.ID;
        dir = bus.dir;
        line = bus.line;
        status = bus.status;
        location = bus.location;
    }
}


public class BusSimulator : MonoBehaviour
{
    /*
     * This class serves as the brain of the bus simulator
     * It will decide when to set off the bus
     * and collect bus information from each bus
     * object.
     * So, this class is just like a centural server that
     * managing all datas from the bus instances.
     * 
     */

    //The nodes for bus line 1
    public static List<Vector2d> Line1 = new List<Vector2d>
    {
        new Vector2d(114.207266f, 22.696781f), //教职工宿舍
        new Vector2d(114.208618f, 22.697022f), //教职-书院拐点
        new Vector2d(114.209234f, 22.696310f), //书院
        new Vector2d(114.209261f, 22.692097f), //中园拐点
        new Vector2d(114.216622f, 22.691858f), //体育馆
        new Vector2d(114.217826f, 22.692925f), //启动区
        new Vector2d(114.218024f, 22.693034f), //1,2号线拐点
        new Vector2d(114.217341f, 22.693813f), //拐点(IF)
        new Vector2d(114.218131f, 22.695234f), //学活
        new Vector2d(114.219380f, 22.6965760f), //图书馆
    };

    //The nodes for bus line 2
    public static List<Vector2d> Line2 = new List<Vector2d>
    {
        new Vector2d(114.207266f, 22.696781f), //教职工宿舍
        new Vector2d(114.208618f, 22.697022f), //教职-书院拐点
        new Vector2d(114.209234f, 22.696310f), //书院
        new Vector2d(114.209261f, 22.692097f), //中园拐点
        new Vector2d(114.216622f, 22.691858f), //体育馆
        new Vector2d(114.217826f, 22.692925f), //启动区
        new Vector2d(114.218024f, 22.693034f), //1,2号线拐点
        new Vector2d(114.218455f, 22.692742f), //拐点(TD)
        new Vector2d(114.220224f, 22.694088f), //TD
        new Vector2d(114.221163f, 22.695080f), //TB
    };

    public static List<Vector2d>[] Lines = new List<Vector2d>[2] { Line1, Line2 };

    //The nodes which are bus stops
    public static HashSet<int> Stops = new HashSet<int> { 0, 2, 4, 5, 8, 9 };
    public static BusSimulator singleton;

    //Bus prefab and object container
    public GameObject BusPrefab;
    public List<Bus> BusList;

    //Parking lot
    public Queue<Bus> ParkingLotUpper;
    public Queue<Bus> ParkingLotTB;
    public Queue<Bus> ParkingLotLib;

    //The line of the next bus set from upper campus
    private int UpperLine = 0;

    //Set off bus cooldown
    private float UpperCooldown = 0f;
    private float LibCooldown = 60f;
    private float TBCooldown = 0f;

    private void Awake()
    {
        singleton = this;
        BusList = new List<Bus>();
        ParkingLotUpper = new Queue<Bus>();
        ParkingLotTB = new Queue<Bus>();
        ParkingLotLib = new Queue<Bus>();
    }

    private void Start()
    {
        //Init Upper parkinglot
        for (int i = 0; i < 4; i++)
        {
            Bus bus = Instantiate(BusPrefab).GetComponent<Bus>();
            bus.Initialize(Line1[0], i, 0, 1);
            BusList.Add(bus);
            ParkingLotUpper.Enqueue(bus);
        }

        //Init TB parkinglot
        for (int i = 4; i < 6; i++)
        {
            Bus bus = Instantiate(BusPrefab).GetComponent<Bus>();
            bus.Initialize(Line2[9], i, 1, -1);
            BusList.Add(bus);
            ParkingLotTB.Enqueue(bus);
        }

        //Init Lib parkinglot
        Bus b = Instantiate(BusPrefab).GetComponent<Bus>();
        b.Initialize(Line1[9], 6, 0, -1);
        BusList.Add(b);
        ParkingLotLib.Enqueue(b);

    }

    private void Update()
    {
        UpperCooldown -= Time.deltaTime;
        TBCooldown -= Time.deltaTime;
        LibCooldown -= Time.deltaTime;

        //If the parking lot has bus and cooldown reaches 0, set off a bus
        if (UpperCooldown <= 0f && ParkingLotUpper.Count > 0)
        {
            UpperCooldown = 60f;
            ParkingLotUpper.Dequeue().SetBus(UpperLine);
            UpperLine = (UpperLine == 0) ? 1 : 0;
        }

        if (TBCooldown <= 0f && ParkingLotTB.Count > 0)
        {
            TBCooldown = 120f;
            ParkingLotTB.Dequeue().SetBus(1);
        }

        if (LibCooldown <= 0f && ParkingLotLib.Count > 0)
        {
            LibCooldown = 120f;
            ParkingLotLib.Dequeue().SetBus(0);
        }
    }

    /* 
     * An API used to simulate the behavior that a client
     * want to request the bus information from the server
     * and server sends back a serilized data (like in json format)
     * back to the client
     */
    public List<BusInfo> GetInfo()
    {
        int l = BusList.Count;
        List<BusInfo> r = new List<BusInfo>();
        for (int i = 0; i < l; i++)
        {
            r.Add(new BusInfo(BusList[i]));
        }
        return r;
    }

}
