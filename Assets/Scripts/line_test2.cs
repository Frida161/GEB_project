using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class line_test2 : MonoBehaviour
{
    public Transform[] map_stop; //地图上的empty object对应的坐标,顺序为教职工宿舍，教职工和书院的十字路口（ 114.208618f, 22.697022f,暂时取消），书院站，拐点，体育馆，启动区，1,2号线分岔点，拐点3（到TB的拐点），TD，TB(目前一共9段)
    public Text seat_number_text;
    public Text waiting_time_text;
    // private double[] latitude = { 1, 2, 3 };//经纬度第一个值
    // private double[] longtitude = { 1, 2, 1 };//经纬度第二个值

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
    // private double[] simulate_GPS_x = { 114.207266f, 114.209234f, 114.209261f, 114.216622f, 114.217826f, 114.218024f, 114.217341f, 114.218131f, 114.21938f };
    // private double[] simulate_GPS_y = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.693813f, 22.695234f, 22.696576f };
    private double[] simulate_GPS_x = { 114.207361f, 114.207835f, 114.208159f, 114.208693f, 114.209093f, 114.209354f, 114.209538f, 114.2209695f, 114.210027f,114.210346f,114.209556f, 114.213132f, 114.214363f, 114.215553f, 114.216757f, 114.21753f, 114.217633f, 114.218024f, 114.217808f, 114.218284f, };
    private double[] simulate_GPS_y = { 22.69648f, 22.696401f, 22.696553f, 22.69683f, 22.696851f, 22.696626f, 22.69633f, 22.696042f, 22.695542f,22.694955f,22.694275, 22.691058f, 22.691333f, 22.691179f, 22.691675f, 22.692087f, 22.692637f, 22.693392f, 22.694f, 22.69483f };

    void Start()
    {
        calculate_slope_and_y_axis();
        StartCoroutine(Func());
    }


    #region 模拟运行（待删除）
    IEnumerator Func()
    {
        int start = simulate_GPS_x.Length-1;
        while (true)
        {
            yield return new WaitForSeconds(2.0f);
            final_run_logic(simulate_GPS_x[start], simulate_GPS_y[start], 1, current_waiting_step);
            start = start - 1;
            //if (start > simulate_GPS_x.Length-1) break;
            if (start < 0)
            {
                start = simulate_GPS_x.Length - 1;
                StartCoroutine(Func2());
                break;
            }
        }
    }


    IEnumerator Func2()
    {
        int start = 0;
        while (true)
        {
            yield return new WaitForSeconds(2.0f);
            final_run_logic(simulate_GPS_x[start], simulate_GPS_y[start], 0, current_waiting_step);
            start = start + 1;
            if (start > simulate_GPS_x.Length - 1)
            {
                start = 0;
                StartCoroutine(Func());
                break;
            }
            //if (start > simulate_GPS_x.Length-1) break;
        }
    }
    #endregion

    #region 数学计算部分
    //pass the test，计算每一条线段的斜率和截距
    private void calculate_slope_and_y_axis()
    {
        int index = 0;
        for (int i = 0; i < latitude.Length - 1; i++)
        {
            double k = (longtitude[i] - longtitude[i + 1]) / (latitude[i] - latitude[i + 1]);
            double b = (latitude[i] * longtitude[i + 1] - latitude[i + 1] * longtitude[i]) / (latitude[i] - latitude[i + 1]);
            slope[index] = k;
            y_axis[index] = b;
            index++;
        }
    }


    //pass the test, 计算GPS点到某段线段最短的距离,x-->latitude,y-->longtitude，d=|A*x0+B*y0+C|/√(A*A+B*B)， B = -1,返回那一条线段起始点的下标
    //direction 0 向上园开，direction 1向下园开， direction 2转向
    private int calculate_min_distance(double x, double y, int direction, int pass_stop)
    {
        if (direction == 0 || direction == 1)
        {
            int limit_stop = latitude.Length - 1;
            if (direction == 0)//往下园开
            {
                if ((pass_stop + 1) <= latitude.Length - 1) limit_stop = pass_stop + 1;
            }
            else //往上园开
            {
                limit_stop = 0;
                if ((pass_stop - 1) >= 0) limit_stop = pass_stop - 1;
            }

            //计算是否在该路径上
            int i = pass_stop;
            double distance = (slope[i] * x - y + y_axis[i]) * (slope[i] * x - y + y_axis[i]) / (slope[i] * slope[i] + 1);
            double point_line_slope = -1 / slope[i];
            double b = -point_line_slope * x + y;
            double cross_x = (b - y_axis[i]) / (slope[i] - point_line_slope);
            double portion = (latitude[i] - cross_x) / (latitude[i] - latitude[i + 1]);
            float portion_convert = (float)(portion);
            float result = Mathf.Round(portion_convert * 100) / 100;//保留一位小数
            Debug.Log("result:" + result);
            if (result >= 0 && result <= 1) return pass_stop;
            else return limit_stop;

        }
        //到了图书馆附近转弯
        else if (direction == 2)
        {
          //  return 1;
            return latitude.Length - 2;

        }
        //到了教职工宿舍附近转弯
        else
        {
            return 0;
        }
    }



    //pass the test, 计算这个点在这条线的几分之几处,返回距离靠左边的点的几分之几
    private float calculate_partition(double x, double y, int line_number)
    {
        //点在线上
        if (x == latitude[line_number])
        {
            return (float)((latitude[line_number] - x) / (latitude[line_number] - latitude[line_number + 1]));
        }
        //两条线完全平行，一般不太可能
        if (slope[line_number] == 0)
        {
            return Mathf.Abs((float)(y - y_axis[line_number]));
        }
        //两条线不完全平行
        double point_line_slope = -1 / slope[line_number];
        double b = -point_line_slope * x + y;
        double cross_x = (b - y_axis[line_number]) / (slope[line_number] - point_line_slope);
        double portion = (latitude[line_number] - cross_x) / (latitude[line_number] - latitude[line_number + 1]);
        float portion_convert = (float)(portion);
        float result = Mathf.Round(portion_convert * 100) / 100;//保留两位小数
        if (result >= 1) return 1;
        return result;
    }

    #endregion


    #region 图上公交的移动

    private void final_move(int line_number, float portion)
    {
        float target_x = map_stop[line_number].position.x + (map_stop[line_number + 1].position.x - map_stop[line_number].position.x) * portion;
        float target_y = map_stop[line_number].position.y + (map_stop[line_number + 1].position.y - map_stop[line_number].position.y) * portion;
        Vector3 target_Postion = new Vector3(target_x, target_y, 0f);
        gameObject.transform.position = target_Postion;
    }

    #endregion



    #region 座位和等待时间

    //车上空余座位模拟，随机数字
    public int random_remain_seat()
    {
        int random_number = Random.Range(1, 30);
        return random_number;
    }


    //等待时间模拟,目前在等待的车站，目前车的位置，目前车的朝向
    private float calculate_waiting_time(int current_step_number, int line_number, float partition, int current_bus_ori)
    {

        float total_waiting_time = 0;
        //上园往下园开
        if (current_bus_ori == 0)
        {
            //到站
            //下一站就到了,partition > 0.1
            if (current_step_number == line_number + 1 && partition<0.1)
            {
                return 0f;
            }
            //已经过站了要绕一圈,目前我的所处的站在公交车站前面
            else if (current_step_number <= line_number)
            {
                total_waiting_time = partition * line_running_time[line_number];
                for (int i = 0; i < line_number; i++)
                {
                    total_waiting_time += line_running_time[i];
                }
                for (int i = 0;i<current_step_number;i++)
                {
                    total_waiting_time += line_running_time[i];
                }
                total_waiting_time = single_route_time *2 - (Mathf.Round(total_waiting_time*10)/10);
                return total_waiting_time;
            }
            //下一站没到，还远着
            else
            {
                total_waiting_time = (1 - partition) * line_running_time[line_number];
                for (int i = line_number + 1; i < current_step_number; i++)
                {
                    total_waiting_time += line_running_time[i];
                }
                total_waiting_time = Mathf.Round(total_waiting_time*10)/10;
                return total_waiting_time;
            }
        }
        //下园往上园开
        else
        {
            //到站
            //下一站就到了
            if ((current_step_number == line_number && partition<0.1))
            {
                return 0f;
            }
            //已经过站了要绕一圈
            else if (current_step_number > line_number)
            {
                total_waiting_time = (1 - partition) * line_running_time[line_number];
                for (int i = current_step_number ; i < latitude.Length-2 ; i++)
                {
                    Debug.Log(i);
                    total_waiting_time += line_running_time[i];
                }
                for (int i = line_number+1; i<latitude.Length-2;i++)
                {
                    total_waiting_time += line_running_time[i];
                }
                Debug.Log("total_waiting" + total_waiting_time);
                //总时长经过已经走过的距离
                total_waiting_time = single_route_time * 2 - (Mathf.Round(total_waiting_time*10)/10);
                return total_waiting_time;
            }
            //下一站没到，还远着
            else
            {
                Debug.Log("total_waiting?" + total_waiting_time);
                // Debug.Log("hi5:" + current_step_number + " " + line_number + " " + partition);
                total_waiting_time = 0f;
                if (line_number != 8)
                {
                    total_waiting_time = partition * line_running_time[line_number];
                }

                for (int i = line_number-1; i >= current_step_number; i--)
                {
                    total_waiting_time += line_running_time[i];
                }
                total_waiting_time = Mathf.Round(total_waiting_time*10)/10;
                return total_waiting_time;
            }
        }


    }



    #endregion
    //最终公交车图上的运行逻辑
    public void final_run_logic(double x, double y, int bus_running_oir, int current_student_wait_step)
    {
        int line_number;
        if (last_bus_ori == bus_running_oir && last_bus_ori == 0)
        {
            line_number = calculate_min_distance(x, y, 0, Last_BusStop);
        }
        else if (last_bus_ori == bus_running_oir && last_bus_ori == 1)
        {
            line_number = calculate_min_distance(x, y, 1, Last_BusStop);
        }
        //转向上园，在图书馆附近
        else if (last_bus_ori != bus_running_oir && bus_running_oir == 1)
        {
            line_number = calculate_min_distance(x, y, 2, Last_BusStop);
        }
        //转向下园，在教职工宿舍附近
        else
        {
            line_number = calculate_min_distance(x, y, 3, Last_BusStop);
        }
        float partition = calculate_partition(x, y, line_number);
        //Debug.Log("line_number:" + line_number);
        //Debug.Log("partition:" + partition);
        final_move(line_number, partition);
        //向下园开
        int last_number = Last_BusStop;

       /* if (partition > 0.9 && bus_running_oir == 0) Last_BusStop = line_number +1;
        else if (partition < 0.1 && bus_running_oir == 1)Last_BusStop = line_number - 1;*/
        //else
            Last_BusStop = line_number;
        last_bus_ori = bus_running_oir;

        //经过站点且不是拐点，更新座位
        if (last_number != Last_BusStop && last_number!=2 && last_number !=5 && last_number!=6)
        {
            seat_number_text.text = random_remain_seat().ToString();
        }

        //更新等待时间
        float bus_waiting_time = calculate_waiting_time(current_student_wait_step, line_number, partition, bus_running_oir);
        if (bus_waiting_time == 0)
        {
            waiting_time_text.text = "< 1";
        }
        else
        {
            waiting_time_text.text = bus_waiting_time.ToString();
        }

    }


}
