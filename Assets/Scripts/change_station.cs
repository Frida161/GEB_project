using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class change_station : MonoBehaviour
{
    //use for buttons to change stops
    public int current_station; //1 书院站，4启动区,8图书馆,7 TB
    public string station_name;
    public Text station_text;
    public int line_number; //0都显示，1值显示1号线，只显示2号线
    public void change_current_station()
    {
        GameObject.Find("BusControlScript").SendMessage("change_current_station", current_station);
        GameObject.Find("BusControlScript").SendMessage("change_current_display_line", line_number);
        station_text.text = "You are now at: " + station_name;
    }
}
