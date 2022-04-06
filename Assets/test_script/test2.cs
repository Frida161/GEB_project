using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test2 : MonoBehaviour
{
    public Transform[] map_stop; //地图上的empty object对应的坐标,顺序为教职工宿舍，教职工和书院的十字路口（ 114.208618f, 22.697022f,暂时取消），书院站，拐点，体育馆，启动区，1,2号线分岔点，拐点3，TB,TD(目前一共9段)

    // private double[] latitude = { 1, 2, 3 };//经纬度第一个值
    // private double[] longtitude = { 1, 2, 1 };//经纬度第二个值


    private double[] latitude = { 114.207266f, 114.209234f, 114.209261f, 114.216622f, 114.217826f, 114.218024f, 114.218455f, 114.220224f, 114.221163f };//经纬度第一个值
    private double[] longtitude = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.692742f, 22.694088f, 22.69508f };//经纬度第二个值
    private double[] slope = new double[10];//每一条线段的斜率
    private double[] y_axis = new double[10];//每一条线段的截距

    private int Last_BusStop = 0;
    private int current_bus_ori = 0;
    private int last_bus_ori = 0; //朝着下园开
    private int current_stop = 0;
    private float current_partition;

    private int[] bus_oritation = {1,1,1,1,1,1,1,1};
    private int total_route_step = 9;
    // private double[] simulate_GPS_x = { 114.207266f, 114.209234f, 114.209261f, 114.216622f, 114.217826f, 114.218024f, 114.217341f, 114.218131f, 114.21938f };
    // private double[] simulate_GPS_y = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.693813f, 22.695234f, 22.696576f };
    private double[] simulate_GPS_x = { 114.207266f, 114.209234f, 114.211179f, 114.213703f, 114.21655f, 114.218006f, 114.218069f,  114.21938f };
    private double[] simulate_GPS_y = { 22.696781f, 22.69631f, 22.691441f, 22.691116f, 22.691566f, 22.692767f, 22.694309f, 22.696576f };

    void Start()
    {
        calculate_slope_and_y_axis();
        final_run_logic(simulate_GPS_x[0], simulate_GPS_y[0],0);
        //final_move(Last_BusStop, current_stop, current_partition, current_bus_ori);
    }

    // Update is called once per frame
    void Update()
    {

    }

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
        double min_distance = 100000;
        int min_index = -1;
        //往下园开，不计算已经经过的路程的线段
        if (direction == 0)
        {
            int limit_stop = latitude.Length - 1;
            if ((pass_stop +3 )<= latitude.Length - 1) limit_stop = pass_stop + 3;
            for (int i = pass_stop; i < limit_stop; i++)
            {
                double distance = (slope[i] * x - y + y_axis[i]) * (slope[i] * x - y + y_axis[i]) / (slope[i] * slope[i] + 1);
                double point_line_slope = -1 / slope[i];
                double b = -point_line_slope * x + y;
                double cross_x = (b - y_axis[i]) / (slope[i] - point_line_slope);
                double portion = (latitude[i] - cross_x) / (latitude[i] - latitude[i + 1]);
                float portion_convert = (float)(portion);
                float result = Mathf.Round(portion_convert * 10) / 10;//保留一位小数
                if (distance < min_distance && result >= 0)
                {
                    min_distance = distance;
                    min_index = i;
                }
            }
        }
        //往上园开
        else if (direction == 1)
        {
            int limit_stop = 0;
            if ( (pass_stop - 3) >= 0) limit_stop = pass_stop-3;
            for (int i = limit_stop; i <= pass_stop; i++)
            {
                double distance = (slope[i] * x - y + y_axis[i]) * (slope[i] * x - y + y_axis[i]) / (slope[i] * slope[i] + 1);
                double point_line_slope = -1 / slope[i];
                double b = -point_line_slope * x + y;
                double cross_x = (b - y_axis[i]) / (slope[i] - point_line_slope);
                double portion = (latitude[i] - cross_x) / (latitude[i] - latitude[i + 1]);
                float portion_convert = (float)(portion);
                float result = Mathf.Round(portion_convert * 10) / 10;//保留一位小数
                if (distance < min_distance && result >= 0)
                {
                    min_distance = distance;
                    min_index = i;
                }
            }
        }
        else if (direction == 2)
        {
            for (int i = latitude.Length-3; i < latitude.Length - 1; i++)
            {
                double distance = (slope[i] * x - y + y_axis[i]) * (slope[i] * x - y + y_axis[i]) / (slope[i] * slope[i] + 1);
                double point_line_slope = -1 / slope[i];
                double b = -point_line_slope * x + y;
                double cross_x = (b - y_axis[i]) / (slope[i] - point_line_slope);
                double portion = (latitude[i] - cross_x) / (latitude[i] - latitude[i + 1]);
                float portion_convert = (float)(portion);
                float result = Mathf.Round(portion_convert * 10) / 10;//保留一位小数
                if (distance < min_distance && result>=0)
                {
                    min_distance = distance;
                    min_index = i;
                }
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                double distance = (slope[i] * x - y + y_axis[i]) * (slope[i] * x - y + y_axis[i]) / (slope[i] * slope[i] + 1);
                double point_line_slope = -1 / slope[i];
                double b = -point_line_slope * x + y;
                double cross_x = (b - y_axis[i]) / (slope[i] - point_line_slope);
                double portion = (latitude[i] - cross_x) / (latitude[i] - latitude[i + 1]);
                float portion_convert = (float)(portion);
                float result = Mathf.Round(portion_convert * 10) / 10;//保留一位小数
                Debug.Log("distance:"+distance);
                Debug.Log("result:" + result);
                if (distance < min_distance && result >= 0)
                {
                    min_distance = distance;
                    min_index = i;
                }
            }
        }
        return min_index;
    }

    //pass the test, 计算这个点在这条线的几分之几处,返回距离靠左边的点的几分之几
    private float calculate_partition(double x, double y, int line_number)
    {
        //点在线上
        Debug.Log("line:" + line_number);
        if (x == latitude[line_number])
        {
            return (float)((latitude[line_number] - x) / (latitude[line_number] - latitude[line_number + 1]));
        }
        //两条线完全平行，一般不太可能
       if (slope[line_number] == 0)
        {
         //   Debug.Log("what??");
            return Mathf.Abs((float)(y - y_axis[line_number]));
        }
        //两条线不完全垂直
        double point_line_slope = -1 / slope[line_number];
        double b = -point_line_slope * x + y;
        double cross_x = (b - y_axis[line_number]) / (slope[line_number] - point_line_slope);
        double portion = (latitude[line_number] - cross_x) / (latitude[line_number] - latitude[line_number + 1]);
        float portion_convert = (float)(portion);
        float result = Mathf.Round(portion_convert * 10) / 10;//保留一位小数
        return result;
    }





    #endregion




    #region 图上公交的移动

    private void move(int line_number, float portion, float time)
    {
        float target_x = map_stop[line_number].position.x + (map_stop[line_number + 1].position.x - map_stop[line_number].position.x) * portion;
        float target_y = map_stop[line_number].position.y + (map_stop[line_number + 1].position.y - map_stop[line_number].position.y) * portion;
        Vector3 target_Postion = new Vector3(target_x, target_y, 0f);

        transform.position = Vector3.Lerp(transform.position, target_Postion, time * Time.deltaTime);//在5s内移动到指定点
        StartCoroutine(MoveToPortion(transform, target_Postion, time, line_number, portion));
    }


    //pass the test, 固定时间内移动距离,到达路中的某一段
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
               // Waiting_time_text.text = calculate_waiting_time(now_step_number, step_number, portion).ToString();
                StartCoroutine(Func());
                break;
            }
            yield return null;
        }

    }

    //朝下园开，到站
    private void move_to_step(int step_number, float time, int ori, int final_step, float portion)
    {
        StartCoroutine(MoveTo(transform, time, ori, step_number, final_step, portion));
    }

    //pass the test, 固定时间内移动距离,向下园开跨站
    private IEnumerator MoveTo(Transform tr, float time, int ori, int step_number, int final_step, float portion)
    {
        Vector3 pos = map_stop[step_number].position;

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
                tr.position = pos;
                //不转向，只是跨站
                if (ori==0)
                {
                    if (step_number != final_step)
                    {
                        StartCoroutine(MoveTo(tr, time, ori, step_number + 1, final_step, portion));
                    }
                    else
                    {
                        move(final_step, portion, time);
                    }
                }
                //转向,从上园开往下园然后图书馆转向
                else
                {
                    if (step_number !=(latitude.Length-1))
                    {
                        StartCoroutine(MoveTo(tr, time, ori, step_number + 1, final_step, portion));
                    }
                    else
                    {
                        move_back_step(latitude.Length - 1, time, 0, final_step, portion);
                    }
                }
                //到站更新座位数量
               /* if (step_number != 2 && step_number != 5 && step_number != 6)
                {
                    Remaining_Seat_number_text.text = random_remain_seat().ToString();
                }

                //更新时间
                Waiting_time_text.text = calculate_waiting_time(now_step_number, step_number, 1f).ToString();
                StartCoroutine(Func());*/
                break;
            }
            yield return null;
        }

    }

    //朝上园开，到站
    private void move_back_step(int step_number, float time, int ori, int final_step, float portion)
    {
        StartCoroutine(MoveBack(transform, time, ori, step_number, final_step, portion));
    }

    //pass the test, 固定时间内移动距离,向上园开跨站,ori(是否转向，1转向，0不转向)
    private IEnumerator MoveBack(Transform tr, float time, int ori, int step_number, int final_step, float portion)
    {
        Vector3 pos = map_stop[step_number].position;

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
                tr.position = pos;
                if (ori == 0)
                {
                    if (step_number != (final_step + 1))
                    {
                        StartCoroutine(MoveBack(tr, time, ori, step_number - 1, final_step, portion));
                    }
                    else
                    {
                        move(final_step, portion, time);
                    }
                }
                else
                {
                    if (step_number != 0)
                    {
                        StartCoroutine(MoveBack(tr, time, ori, step_number - 1, final_step, portion));
                    }
                    else
                    {
                        move_to_step(0, time, 0, final_step, portion);
                    }
                }
                //到站更新座位数量
                /* if (step_number != 2 && step_number != 5 && step_number != 6)
                 {
                     Remaining_Seat_number_text.text = random_remain_seat().ToString();
                 }

                 //更新时间
                 Waiting_time_text.text = calculate_waiting_time(now_step_number, step_number, 1f).ToString();
                 StartCoroutine(Func());*/
                break;
            }
            yield return null;
        }

    }


    private void final_move(int last_step, int line_number, float partition, int bus_oritation)
    {
        float total_time = 1;
        //朝着下园开
        if (bus_oritation == 0)
        {
            //经过教职工宿舍转向，bus_oritation由0转到1
            if (last_bus_ori == 1)
            {
               /* Debug.Log("trunng!");
                Debug.Log("last_step:" + last_step);
                Debug.Log("line_number:" + line_number);*/
                int total_need_pass_number = last_step + line_number + 2;
                float sub_time = total_time / total_need_pass_number;
                move_back_step(last_step, sub_time, 1, line_number, partition);
            }
            //不跨站,还在本段行驶
            else if (last_step == line_number)
            {
                move(line_number, partition, total_time);
            }
            //跨站
            else if (last_step < line_number)
            {
                //总共需要经过的站数量
                int total_need_pass_number = line_number - last_step + 1;
                float sub_time = total_time / total_need_pass_number;
                //经过的完整的站
                move_to_step(last_step + 1, sub_time, 0, line_number, partition);
            }
        }
        //朝着上园开
        else
        {
            
            //经过图书馆转向，bus_oritation由0转到1
            if (last_bus_ori == 0)
            {
                int total_need_pass_number = latitude.Length - last_step + latitude.Length - line_number;
                float sub_time = total_time / total_need_pass_number;
                move_to_step(last_step + 1, sub_time, 1, line_number, partition);

            }
            //不跨站,还在本段行驶
            else if (last_step == line_number)
            {
                move(line_number, partition, total_time);
            }
            //跨站
            else
            {
                //总共需要经过的站数量
                int total_need_pass_number = last_step - line_number + 1;
                float sub_time = total_time / total_need_pass_number;
                //经过的完整的站
                move_back_step(last_step, sub_time, 0, line_number, partition);
            }

        }

        Last_BusStop = line_number;
        last_bus_ori = bus_oritation;
        
    }

    #endregion



    //最终公交车图上的运行逻辑
    public void final_run_logic(double x, double y, int bus_running_oir)
    {
        int line_number;
        if (last_bus_ori == bus_running_oir && last_bus_ori == 0)
        {
            Debug.Log("chose1");
            line_number = calculate_min_distance(x, y, 0, Last_BusStop);
        }
        else if (last_bus_ori == bus_running_oir && last_bus_ori == 1)
        {
            Debug.Log("chose2");
            line_number = calculate_min_distance(x, y, 1, Last_BusStop);
        }
        //向上园转弯
        else if (last_bus_ori!=bus_running_oir && bus_running_oir == 1)
        {
            Debug.Log("chose3");
            line_number = calculate_min_distance(x, y, 2, Last_BusStop);
        }
        //向下园转弯
        else
        {
            Debug.Log("chose4");
            line_number = calculate_min_distance(x, y, 3, Last_BusStop);
        }
        float partition = calculate_partition(x, y, line_number);
        Debug.Log("line_number:" + line_number);
        Debug.Log("partition:"+partition);
        final_move(Last_BusStop, line_number, partition, bus_running_oir);

    }

    int i = 1;
    //每2秒更新，模拟运行轨迹
    IEnumerator Func()
    {
        Debug.Log("i:"+i);
        yield return new WaitForSeconds(1f);

        if (i == -1)
        {
            final_run_logic(simulate_GPS_x[0], simulate_GPS_y[0],0);
            i = 1;
        }
        else if (i < simulate_GPS_x.Length && last_bus_ori == 0)
        {
            final_run_logic(simulate_GPS_x[i], simulate_GPS_y[i],0);
            i = i + 1;
        }
        else if (i == simulate_GPS_x.Length)
        {
            final_run_logic(simulate_GPS_x[simulate_GPS_x.Length-2], simulate_GPS_y[simulate_GPS_x.Length-2],1);
            i = simulate_GPS_x.Length-2;
        }
        else if (i >= 0 && last_bus_ori == 1)
        {
            final_run_logic(simulate_GPS_x[i], simulate_GPS_y[i],1);
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
