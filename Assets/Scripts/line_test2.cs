using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class line_test2 : MonoBehaviour
{
    public Transform[] map_stop; //地图上的empty object对应的坐标,顺序为教职工宿舍，教职工和书院的十字路口（ 114.208618f, 22.697022f,暂时取消），书院站，拐点，体育馆，启动区，1,2号线分岔点，拐点3（到TB的拐点），TD，TB(目前一共9段)
    public Text seat_number_text;
    public Text waiting_time_text;

    private double[] latitude = { 114.207266f, 114.209234f, 114.209261f, 114.216622f, 114.217826f, 114.218455f, 114.220224f, 114.221163f };//经纬度第一个值
    private double[] longtitude = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.692742f, 22.694088f, 22.69508f };//经纬度第二个值
    private double[] slope = new double[10];//每一条线段的斜率,
    private double[] y_axis = new double[10];//每一条线段的截距

    private float[] line_running_time = { 1f, 2f, 2.5f, 1f, 0.5f, 0.5f, 0.5f, 1f };//每一段路程的行驶时间,例如教职工宿舍和书院行驶时间为1min,单程9分钟
    private float single_route_time = 9f;

    //在上园的停车场的车，初始值为 Last_BusStop = 0， last_bus_ori = 0
    //在下园的停车场的车，初始值为 Last_BusStop = 6， last_bus_ori = 1
    private int Last_BusStop = 6;  //上一次车辆所在的经过的最近站点
    private int last_bus_ori = 1; //上一次车辆方向，0代表朝着下园开，1代表朝着上园开
    private int current_waiting_step = 1;//当前人所在的车站

    private int total_route_step = 9;


    //图上公交的移动
    public void Bus_move(float[] info)
    {
        int line_number = (int)info[0];
        float portion = info[1];
        float target_x = map_stop[line_number].position.x + (map_stop[line_number + 1].position.x - map_stop[line_number].position.x) * portion;
        float target_y = map_stop[line_number].position.y + (map_stop[line_number + 1].position.y - map_stop[line_number].position.y) * portion;
        Vector3 target_Postion = new Vector3(target_x, target_y, 0f);
        gameObject.transform.localPosition = target_Postion;
    }


    //由Buscontrol脚本控制，更新座位UI
    public void update_seat_ui(int seat)
    {
        seat_number_text.text = seat.ToString();
    }

    //由Buscontrol脚本控制，更新等待时间ui
    public void update_waiting_time_ui(float time)
    {
        if (time < 1)
        {
            waiting_time_text.text = "< 1";
        }
        else
        {
            waiting_time_text.text = time.ToString();
        }
    }

}