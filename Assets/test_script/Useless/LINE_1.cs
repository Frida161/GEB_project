using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LINE_1 : MonoBehaviour
{
    public Transform[] map_stop;//地图上的empty object对应的坐标,顺序为教职工宿舍，教职工和书院的十字路口（ 114.208618f, 22.697022f,暂时取消），书院站，拐点，体育馆，启动区，1,2号线分岔点，拐点2（if那边），学活，图书馆(目前一共9段)
    private double[] latitude = { 114.207266f, 114.209234f, 114.209261f, 114.216622f, 114.217826f, 114.218024f, 114.217341f, 114.218131f, 114.21938f };//经纬度第一个值
    private double[] longtitude = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.693813f, 22.695234f, 22.696576f };//经纬度第二个值
    private float[] line_running_time = { 1f, 2f, 3.5f, 1f, 0.5f, 0.5f, 0.5f, 1f };//每一段路程的行驶时间,例如教职工宿舍和书院行驶时间为1min,

    private double[] slope = new double[10];//每一条线段的斜率
    private double[] y_axis = new double[10];//每一条线段的截距


    //UI转向的y,z坐标  上园开往中园拐点，中园拐点开往下园，下园开往中园拐点，中园拐点开往上园
    private float[] UI_turning_z = { 26.03f, -7.463f, -7.463f, -26.03f };
    private float[] UI_turning_y = { 0f, -180f, 0, 180f };


    private int bus_oritation = 0; //0朝着下园，1朝着上园
    private int Last_BusStop = 0;//目前已经经过的最近站点
    private Transform last_position;//上一个点的transform

    //等待时间和车上空余座位UI
    public Text Waiting_time_text;
    public Text Remaining_Seat_number_text;
    public GameObject reminder;//框，不能反转


    // private double[] simulate_GPS_x = { 114.209234f, 114.209234f, 114.209261f, 114.216622f, 114.217826f, 114.218024f, 114.217341f, 114.218131f, 114.21938f, 114.218131f , 114.217341f , 114.218024f, 114.217826f, 114.216622f, 114.209261f, 114.209234f, 114.209234f};
    // private double[] simulate_GPS_y = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.693813f, 22.695234f, 22.696576f , 22.695234f, 22.693813f, 22.693034f, 22.692925f, 22.691858f, 22.692097f, 22.69631f, 22.696781f};

    private double[] simulate_GPS_x = { 114.207266f, 114.209234f, 114.209261f,  114.216622f, 114.217826f, 114.218024f, 114.217341f, 114.218131f, 114.21938f };
    private double[] simulate_GPS_y = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.693813f, 22.695234f, 22.696576f};
    public int now_step_number = 8;

    void Start()
    {
        //计算每一条线段的斜率和截距
        calculate_slope_and_y_axis();
        final_run_logic(simulate_GPS_x[0], simulate_GPS_y[0]);
    }

    //最终公交车图上的运行逻辑
    public void final_run_logic(double x, double y)
    {
        int line_number = calculate_min_distance(x, y);
        float partition = calculate_partition(x, y, line_number);
        final_move(Last_BusStop, line_number, partition);

    }

    //最终公交车上UI更新逻辑
    public void final_UI_logic(double x, double y, int current_step_number)
    { 
        int line_number = calculate_min_distance(x, y);
        float partition = calculate_partition(x, y, line_number);
        float waiting_time = calculate_waiting_time(current_step_number, line_number, partition);
        //计算等待时间
        Waiting_time_text.text = waiting_time.ToString();
        //计算剩余座位
        Remaining_Seat_number_text.text = random_remain_seat().ToString();

    }

    //车上空余座位模拟，随机数字
    public int random_remain_seat()
    {
        int random_number = Random.Range(1, 30);
        return random_number;
    }

    //等待时间模拟,目前在等待的车站，目前车的位置，目前车的朝向
    private float calculate_waiting_time(int current_step_number, int line_number, float partition)
    {

        float total_waiting_time = 0;
        //上园往下园开
        if (bus_oritation == 0)
        {
            //到站
            //下一站就到了
            if (current_step_number == line_number + 1)
            {
                return 0f;
            }
            //已经过站了要绕一圈
            else if (current_step_number < line_number)
            {
                total_waiting_time = (1 - partition) * line_running_time[line_number];
                for (int i = line_number; i < 8; i++)
                {
                    total_waiting_time += line_running_time[i] * 2;
                }
                total_waiting_time = Mathf.Round(total_waiting_time);
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
                total_waiting_time = Mathf.Round(total_waiting_time);
                return total_waiting_time;
            }
        }
        //下园往上园开
        else
        {
            //到站
            //下一站就到了
            //Debug.Log("hi3:" + current_step_number + " " + line_number + " " + partition);
            if (current_step_number == line_number - 1)
            {
                return 0f;
            }
            //已经过站了要绕一圈
            else if (current_step_number >= line_number)
            {
                //Debug.Log("hi4:" + current_step_number + " " + line_number + " " + partition);
                //float total_waiting_time = (1 - partition) * line_running_time[line_number-1];
                total_waiting_time = 0f;
                for (int i = current_step_number-1; i > line_number; i--)
                {
                    total_waiting_time += line_running_time[i];
                }
                total_waiting_time = 17 - Mathf.Round(total_waiting_time);
                return total_waiting_time;
            }
            //下一站没到，还远着
            else
            {
                //Debug.Log("hi5:" + current_step_number + " " + line_number + " " + partition);
                total_waiting_time = 0f;
                if (line_number != 8)
                {
                    total_waiting_time = (1 - partition) * line_running_time[line_number];
                }

                for (int i = line_number; i >= current_step_number; i--)
                {
                    total_waiting_time += line_running_time[i];
                }
                total_waiting_time = Mathf.Round(total_waiting_time);
                return total_waiting_time;
            }
        }
        

    }

    //计算每一条线段的斜率和截距
    private void calculate_slope_and_y_axis()
    {
        int index = 0;
        for (int i = 0; i < 8; i++)
        {
            double k = (longtitude[i] - longtitude[i + 1]) / (latitude[i] - latitude[i + 1]);
            double b = (latitude[i] * longtitude[i + 1] - latitude[i + 1] * longtitude[i]) / (latitude[i] - latitude[i + 1]);
            slope[index] = k;
            y_axis[index] = b;
            index++;
        }
    }

    //计算GPS点到某段线段最短的距离,x-->latitude,y-->longtitude，d=|A*x0+B*y0+C|/√(A*A+B*B)， B = -1,返回那一条线段的坐标
    private int calculate_min_distance(double x, double y)
    {
        double min_distance = 100000;
        int min_index = -1;
        for (int i = 0; i < 9; i++)
        {
            double distance = (slope[i] * x - y + y_axis[i]) * (slope[i] * x - y + y_axis[i]) / (slope[i] * slope[i] + 1);

            if (distance < min_distance)
            {

                min_distance = distance;
                min_index = i;
            }
        }
       // if (min_distance == 100000) min_index = Last_BusStop;
        return min_index;
    }


    //计算这个点在这条线的几分之几处,返回距离靠左边的点的几分之几
    private float calculate_partition(double x, double y, int line_number)
    {
        double point_line_slope = -1 / slope[line_number];
        double b = -point_line_slope * x + y;
        double cross_x = (b - y_axis[line_number]) / (slope[line_number] - point_line_slope);
        double portion = (latitude[line_number] - cross_x) / (latitude[line_number] - latitude[line_number + 1]);
        //if (portion > 1.1) portion = 0.5f;
        float portion_convert = (float)(portion);
        if (i == 3)
        {
            Debug.Log("portion:" + portion);
            Debug.Log("cross_x:" + cross_x);
            Debug.Log("latitude:" + latitude[line_number]);
            Debug.Log("latitude2:" + latitude[line_number + 1]);
        }
        float result = Mathf.Round(portion_convert * 10) / 10;//保留一位小数
        return result;
    }



    //朝着某个点位移,input 哪一条线段的起始点下标，距离起始点的位移比例
    private void move(int line_number, float portion, int time)
    {
        float target_x = map_stop[line_number].position.x + (map_stop[line_number + 1].position.x - map_stop[line_number].position.x) * portion;
        float target_y = map_stop[line_number].position.y + (map_stop[line_number + 1].position.y - map_stop[line_number].position.y) * portion;
        Vector3 target_Postion = new Vector3(target_x, target_y, 0f);

        transform.position = Vector3.Lerp(transform.position, target_Postion, time * Time.deltaTime);//在5s内移动到指定点
        StartCoroutine(MoveToPortion(transform, target_Postion, time,line_number,portion));
    }

    //运行到指定车站,车站点，时间，朝向（上园或者下园）
    private void move_to_step(int step_number, int time, int ori)
    {
        StartCoroutine(MoveTo(transform, map_stop[step_number].position, time, ori, step_number));

    }


    //固定时间内移动距离,到达路中的某一段
    private IEnumerator MoveToPortion(Transform tr, Vector3 pos, float time, int step_number, float portion)
    {
        float t = 0;
        Vector3 startPos = tr.position;
        while (true)
        {
            t += Time.deltaTime;
            float a = t / time;
            tr.position = Vector3.Lerp(startPos, pos, t / time);
            yield return new WaitForEndOfFrame();
            if (a >= 1.0f)
            {
                Waiting_time_text.text = calculate_waiting_time(now_step_number, step_number, portion).ToString();
                StartCoroutine(Func());
                break;
            }
            yield return null;
        }

    }



    //固定时间内移动距离,到站
    private IEnumerator MoveTo(Transform tr, Vector3 pos, float time, int ori, int step_number)
    {
        float t = 0;
        Vector3 startPos = tr.position;
        while (true)
        {
            t += Time.deltaTime;
            float a = t / time;
            tr.position = Vector3.Lerp(startPos, pos, t/time);
            yield return new WaitForEndOfFrame();
            if (a >= 1.0f)
            {
                tr.position = pos;
                //朝着下园开而且开到了中园拐点处，变换车头方向
                if (ori == 0 && step_number == 2)
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, UI_turning_y[1], UI_turning_z[1]));
                    reminder.transform.rotation = Quaternion.Euler(new Vector3(0, 0, UI_turning_z[1]));
                }
                //朝着上园开而且开到了中园拐点处，变换车头方向
                else if (ori == 1 && step_number == 2)
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, UI_turning_y[3], UI_turning_z[3]));
                    reminder.transform.rotation = Quaternion.Euler(new Vector3(0, 0, UI_turning_z[1]));
                }

                //开到教职工宿舍，变换车头方向
                if (ori == 0 && step_number == 8)
                {
                    bus_oritation = 1;
                    Last_BusStop = 8;
                    transform.rotation = Quaternion.Euler(new Vector3(0, UI_turning_y[2], UI_turning_z[2]));
                    reminder.transform.rotation = Quaternion.Euler(new Vector3(0, 0, UI_turning_z[1]));
                }
                //开到图书馆，变换车头方向
                else if (ori == 1 && step_number == 0)
                {
                    bus_oritation = 0;
                    Last_BusStop = 0;
                    transform.rotation = Quaternion.Euler(new Vector3(0, UI_turning_y[0], UI_turning_z[0]));
                    reminder.transform.rotation = Quaternion.Euler(new Vector3(0, 0, UI_turning_z[1]));
                }

                //到站更新座位数量
                if(step_number!=2&& step_number!=5&& step_number!=6)
                {
                    Remaining_Seat_number_text.text = random_remain_seat().ToString();
                }

                //更新时间
                Waiting_time_text.text = calculate_waiting_time(now_step_number, step_number, 1f).ToString();
                StartCoroutine(Func());
                break;
            }
            yield return null;
        }

    }



    //计算是否跨站以及最终移动逻辑,input 目前朝向，上一次的站点，目前要去的位置
    private void final_move(int last_step, int line_number, float partition)
    {
        int total_time = 1;
        int half_time = 1;
        if (i == 3) Debug.Log("ori?:" + bus_oritation + " " + line_number + " " + partition + " " + last_step);
        //朝着下园开的时候
        if (bus_oritation == 0)
        {
             //到达图书馆
            if (partition > 0.9 && line_number == 7)
            {
                move_to_step(line_number + 1, total_time, 0);
                //朝向上园,改变车头朝向
                bus_oritation = 1;
                Last_BusStop = 8;
            }
            //固定时间内刚好到达站点,且没有跨站
            else if (partition > 0.9 && (line_number == last_step))
            {

                move_to_step(line_number + 1, total_time, 0);
                Last_BusStop = line_number + 1;
            }
            else if (partition < 0.1 && (line_number == last_step +1))
            {
               // Debug.Log("ori:" + bus_oritation + " " + line_number + " " + partition + " " + last_step);
                move_to_step(line_number, total_time, 0);
                Last_BusStop = line_number;
            }
            //固定时间内没有行驶完一站的距离
            else if (partition > 0.9 && (line_number == last_step))
            {
                move(line_number, partition, total_time);
                Last_BusStop = line_number;
            }
            //跨站（大概只有中园拐点会出现这个情况）
            else if (line_number - 1 == last_step)
            {
                move_to_step(line_number, half_time, 0);
                move(line_number, partition, half_time);
                Last_BusStop = line_number;
            }
            else
            {
                StartCoroutine(Func());
            }
        }
        //朝着上园开的时候
        else
        {
            //Debug.Log("ori:" + bus_oritation + " " + line_number + " " + partition + " " + last_step);
            //到达教职工宿舍
            if (partition < 0.1 && line_number == 0)
            {
                move_to_step(0, total_time, 1);
                //朝向下园
                bus_oritation = 0;
                Last_BusStop = 0;
            }
            //到达图书馆站,图书馆往回走都是预计1min到达站点
            else if (last_step == 8)
            {
                //固定时间内刚好到达站点,且没有跨站
                if (partition > 0.9 && line_number ==6)
                {
                    move_to_step(line_number + 1, total_time, 1);
                    Last_BusStop = line_number + 1;
                }
                else if (partition <0.1 && line_number == 7)
                {
                    move_to_step(line_number, total_time, 1);
                    Last_BusStop = line_number;
                }
            }
            //固定时间内刚好到达站点,且没有跨站
            else if (partition >0.9 && (line_number == last_step-2))
            {
                move_to_step(line_number+1, total_time, 1);
                Last_BusStop = line_number + 1;
            }
            else if (partition < 0.1 && (line_number == last_step -1))
            {
                move_to_step(line_number, total_time, 1);
                Last_BusStop = line_number;
            }
            //固定时间内没有行驶完一站的距离
            else if (partition < 0.9 && (line_number == last_step-1))
            {
                move(line_number, partition, total_time);
                Last_BusStop = line_number;
            }
            //跨站（大概只有中园拐点会出现这个情况）
            else if (line_number == last_step-3)
            {
                move_to_step(line_number, half_time, 1);
                move(line_number, partition, half_time);
                Last_BusStop = line_number;
            }
            else
            {
                StartCoroutine(Func());
            }
        }


    }


    int i = 1;
    //每2秒更新，模拟运行轨迹
    IEnumerator Func()
    {
        yield return new WaitForSeconds(0.5f);

        if (i == -1)
        {
            final_run_logic(simulate_GPS_x[1], simulate_GPS_y[1]);
            i = 1;
        }
        else if (i < 9 && bus_oritation == 0)
        {
            final_run_logic(simulate_GPS_x[i], simulate_GPS_y[i]);
            i = i + 1;
        }
        else if (i == 9)
        {
            final_run_logic(simulate_GPS_x[7], simulate_GPS_y[7]);
            i = 7;
        }
        else if (i>= 0 && bus_oritation == 1)
        {
            final_run_logic(simulate_GPS_x[i], simulate_GPS_y[i]);
            i = i - 1;
        }

        /* int i = 0;
         while (true)// or for(i;i;i)
         {
             if (i == 5) break;
             final_run_logic(simulate_GPS_x[i], simulate_GPS_y[i]);
             yield return new WaitForSeconds(3f);
             i = i + 1;
         }*/
    }



}