using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class change_station : MonoBehaviour
{
    // Start is called before the first frame update
    public int current_station; //1 书院站，4启动区,8图书馆,7 TB
    public string station_name;
    public Text station_text;
    public void change_current_station()
    {
        GameObject.Find("BusControlScript").SendMessage("change_current_station", current_station);
        station_text.text = "You are now at: " + station_name;
    }
}
