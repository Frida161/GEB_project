using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

public class BusControl : MonoBehaviour
{

    private GameObject[] Bus = new GameObject[10];
    public GameObject[] Two_kind_bus; //第一种1号线，第二种2号线
    public Transform[] Two_bus_waiting_station;//第一个上园站点，第二种下园站点
    public Text bus_info_text;
    public GameObject canvas;

    //顺序为教职工宿舍，教职工和书院的十字路口（ 114.208618f, 22.697022f, mapping地图上没有，暂时取消），书院站，拐点，体育馆，启动区(缺少)，1,2号线分岔点，拐点3（到TB的拐点），学活，图书馆(目前一共9段)
    private double[] latitude = { 114.207266f, 114.209234f, 114.209261f, 114.216622f, 114.217826f, 114.218024f,114.217341f, 114.218131f, 114.219380f, };//经纬度第一个值
    private double[] longtitude = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.693813f, 22.695234f, 22.696576f };//经纬度第二个值

    private double[] latitude_2 = { 114.207266f, 114.209234f, 114.209261f, 114.216622f, 114.217826f, 114.218024f,114.218455f, 114.220224f, 114.221163f };//经纬度第一个值
    private double[] longtitude_2 = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.692742f, 22.694088f, 22.69508f };//经纬度第二个值


    private double[] slope = new double[10];//每一条线段的斜率,
    private double[] y_axis = new double[10];//每一条线段的截距

    private double[] slope_2 = new double[10];//每一条线段的斜率,
    private double[] y_axis_2 = new double[10];//每一条线段的截距

    private float[] line_running_time = { 1f, 2f, 3.5f, 1f, 0.5f, 0.5f, 0.5f, 1f };//每一段路程的行驶时间,例如教职工宿舍和书院行驶时间为1min,单程9分钟
    private float single_route_time = 10f;

    //记录每一辆车的上一站
    //在上园的停车场的车，初始值为 Last_BusStop = 0， last_bus_ori = 0
    //在下园的停车场的车，初始值为 Last_BusStop = 6， last_bus_ori = 1
    private int[] All_Last_BusStop = {0,0,0,0,0,0,0,0,0,0,0,0};
    private int[] All_Last_bus_ori = {0,0,0,0,0,0,0,0,0,0,0,0};
    private int current_waiting_step = 2;//当前人所在的车站
    private int[] All_bus_route_number = {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}; //1->1号线，2->2号线

    private float[] All_bus_waiting_time = { 0,0,0,0,0,0,0,0,0,0};
    private int[] All_bus_seat_number = { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20 };//所有车初始有30个座位
    private int[] All_current_bus_oir = { 0,0,0,0,0,0,0,0,0,0,0,0,0};      //所有车的运行方向
    private int[] All_bus_line_number = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };//所有车的路线号码，1号线或者2号线
    private int[] All_bus_compareStop = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private int[] recorded_line_number = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //所有车的路线号码，记录上一次的（发车会变化

    private float[] last_waiting_time = { 0, 0, 0, 0 };//上一次等待时间，用于停车
    void Start()
    {
        calculate_slope_and_y_axis();
        generate_initial_position();
    }

    public void change_current_station(int number)
    {
        current_waiting_step = number;
    }

    private void FixedUpdate()
    {
        test();
    }

    #region 模拟运行

    //所有车的初始位置生成
    public void generate_initial_position()
    {
        foreach (BusInfo bus_info in BusSimulator.singleton.GetInfo())
        {
            int bus_id = bus_info.busId;
            int direction = bus_info.dir;
            int line = bus_info.line + 1;
            int status = bus_info.status;

            Vector2d location = bus_info.location;


            if (direction == 1) direction = 0;
            else direction = 1;

            if (status == 1) status = 0;
            if (status > 1) status = status - 1;
            if (status == 8 || status == 9) status = 7;

           // Debug.Log("status:" + status);
            if (status != -1)
            {
                //生成的1号线车在上园，即将往下园开
                if (direction == 0 && line == 1)
                {
                    Bus[bus_id] = Instantiate(Two_kind_bus[0], canvas.transform);
                    Bus[bus_id].transform.localPosition = new Vector3(-579, 893, 9);
                    All_Last_BusStop[bus_id] = 0;
                    All_Last_bus_ori[bus_id] = direction;

                }
                //生成的2号线车在下园园，即将往上园开
                else if (direction == 1 && line == 2)
                {
                    Bus[bus_id] = Instantiate(Two_kind_bus[1], canvas.transform);
                    Bus[bus_id].transform.localPosition = new Vector3(521, -736, 0);
                    All_Last_BusStop[bus_id] = 6;
                    All_Last_bus_ori[bus_id] = direction;
                }
                //生成的1号线车在下园园，即将往上园开
                else if (direction == 1 && line == 2)
                {
                    Bus[bus_id] = Instantiate(Two_kind_bus[0], canvas.transform);
                    Bus[bus_id].transform.localPosition = new Vector3(425, -147.2f, 0);
                    All_Last_BusStop[bus_id] = 6;
                    All_Last_bus_ori[bus_id] = direction;
                }
                //生成的2号线车在上园，即将往下园开
                else
                {
                    Bus[bus_id] = Instantiate(Two_kind_bus[1], canvas.transform);
                    Bus[bus_id].transform.localPosition = new Vector3(-579, 893, 9);
                    All_Last_BusStop[bus_id] = 0;
                    All_Last_bus_ori[bus_id] = direction;
                }
                All_bus_route_number[bus_id] = line;
                All_bus_compareStop[bus_id] = status;
            }
            recorded_line_number[bus_id] = line;
            // Bus[bus_id].SetActive(true);
        }

    }

    //接收GPS并且运行
    public void test()
    {       
            foreach (BusInfo bus_info in BusSimulator.singleton.GetInfo())
            {
                int bus_id = bus_info.busId;
                int direction = bus_info.dir;
                int status = bus_info.status;
                int line = bus_info.line + 1;
                Vector2d location = bus_info.location;

                if (direction == 1) direction = 0;
                else direction = 1;

             Debug.Log("p:" + status + " " + location.x + " " + location.y);
             if (status == 1) status = 0;
             if (status > 1 && direction == 0) status = status - 1;
             if (status == 8 && direction == 0) status = 7;
             Debug.Log("status:" + status);

            //朝上园开
            if (direction == 1 && status>=2)
             {
                 status = status-2;
             }
             if (status == 8 && direction == 1) status = 7;
             if (status == 9 && direction == 1) status = 7;

            //生成的车在上园，即将往下园开
            if (direction == 0)
                {
                    All_Last_BusStop[bus_id] = 0;
                    All_Last_bus_ori[bus_id] = direction;

                }
                //生成的车在上园，即将往上园开
                else
                {
                    All_Last_BusStop[bus_id] = 7;
                    All_Last_bus_ori[bus_id] = direction;
                }
                if (status == -1) Destroy(Bus[bus_id]);
                //上一次记录的车和这次收到的车不一样，代表上下园换车发车
                else {
                if (Bus[bus_id] == null || recorded_line_number[bus_id]!=line)
                {
                    Destroy(Bus[bus_id]);
                    //生成的1号线车在下园园，即将往下园开
                    if (direction == 0 && line == 1)
                    {
                        Bus[bus_id] = Instantiate(Two_kind_bus[0], canvas.transform);
                        Bus[bus_id].transform.localPosition = new Vector3(-579, 893, 9);
                        All_Last_BusStop[bus_id] = 0;
                        All_Last_bus_ori[bus_id] = direction;

                    }
                    //生成的2号线车在下园园，即将往上园开
                    else if (direction == 1 && line == 2)
                    {
                        Bus[bus_id] = Instantiate(Two_kind_bus[1], canvas.transform);
                        Bus[bus_id].transform.localPosition = new Vector3(521, -736, 0);
                        All_Last_BusStop[bus_id] = 7;
                        All_Last_bus_ori[bus_id] = direction;
                    }
                    //生成的1号线车在下园园，即将往上园开
                    else if (direction == 1 && line == 2)
                    {
                        Bus[bus_id] = Instantiate(Two_kind_bus[0], canvas.transform);
                        Bus[bus_id].transform.localPosition = new Vector3(425, -147.2f, 0);
                        All_Last_BusStop[bus_id] = 7;
                        All_Last_bus_ori[bus_id] = direction;
                    }
                    //生成的2号线车在上园，即将往下园开
                    else
                    {
                        Bus[bus_id] = Instantiate(Two_kind_bus[1], canvas.transform);
                        Bus[bus_id].transform.localPosition = new Vector3(-579, 893, 9);
                        All_Last_BusStop[bus_id] = 0;
                        All_Last_bus_ori[bus_id] = direction;
                    }
                    Bus[bus_id].SetActive(true);
                }
                recorded_line_number[bus_id] = line;
                All_bus_route_number[bus_id] = line;
                final_run_logic(location.x, location.y, direction, current_waiting_step, bus_id);
            }

            All_bus_line_number[bus_id] = status;
            //更新这个车的方向，用来显示
            All_current_bus_oir[bus_id] = direction;
            }

        //更新总榜ui然后字典清空
        update_all_UI();


    }
    #endregion


    #region 更新总榜UI

    private void update_all_UI()
    {
        bus_info_text.text = return_bus_info();
    }


    private string return_bus_info()
    {
        //升序输出时间
        string update_all_bus_info = "Est arriving time:\n";

        Dictionary<int, float> result_line_1, result_line_2;
        result_line_1 = BusSimulator.singleton.GetEstArriveTime(0, current_waiting_step); //1号线的最近结果
        result_line_2 = BusSimulator.singleton.GetEstArriveTime(1, current_waiting_step); //2号线的最近结果

        if (current_waiting_step != 0)
        {
            if (result_line_1[1] == -1 && result_line_2[1] == -1)
            {
                update_all_bus_info += "To upper campus: " + "wait for bus departure.\n";
            }
            else
            {
                if (result_line_1[1] == -1) update_all_bus_info += "To upper campus:" + Mathf.Round(result_line_2[1] * 5 / 60 * 10) / 10 + "min\n";
                else if (result_line_2[1] == -1) update_all_bus_info += "To upper campus:" + Mathf.Round(result_line_1[1] * 5 / 60 * 10) / 10 + "min\n";
                else if (result_line_1[1] < result_line_2[1]) update_all_bus_info += "To upper campus:" + Mathf.Round(result_line_1[1] * 5 / 60 * 10) / 10 + "min\n";
                else update_all_bus_info += "To upper campus:" + Mathf.Round(result_line_2[1] * 5 / 60 * 10) / 10 + "min\n";
            }
        }

        if (current_waiting_step != 9)
        {
            if (result_line_1[0] == -1 && result_line_2[0] == -1)
            {
                update_all_bus_info += "To lower campus: " + "wait for bus departure.\n";
            }
            else
            {
                if (result_line_1[0] == -1) update_all_bus_info += "To lower campus:" + Mathf.Round(result_line_2[0] * 5 / 60 * 10) / 10 + "min\n";
                else if (result_line_2[0] == -1) update_all_bus_info += "To lower campus:" + Mathf.Round(result_line_1[0] * 5 / 60 * 10) / 10 + "min\n";
                else if (result_line_1[0] < result_line_2[0]) update_all_bus_info += "To lower campus:" + Mathf.Round(result_line_1[0] * 5 / 60 * 10) / 10 + "min\n";
                else update_all_bus_info += "To lower campus:" + Mathf.Round(result_line_2[0] * 5 / 60 * 10) / 10 + "min\n";
            }
        }




        //TD或者图书馆，只有往上园方向的车

        return update_all_bus_info;
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
            double k2 = (longtitude_2[i] - longtitude_2[i + 1]) / (latitude_2[i] - latitude_2[i + 1]);
            double b2 = (latitude_2[i] * longtitude_2[i + 1] - latitude_2[i + 1] * longtitude_2[i]) / (latitude_2[i] - latitude_2[i + 1]);
            slope_2[index] = k2;
            y_axis_2[index] = b2;

            index++;
        }
    }

    #region 计算最短距离
    //pass the test, 计算GPS点到某段线段最短的距离,x-->latitude,y-->longtitude，d=|A*x0+B*y0+C|/√(A*A+B*B)， B = -1,返回那一条线段起始点的下标
    //direction 0 向上园开，direction 1向下园开， direction 2转向
    //一号线的计算
    private int calculate_min_distance(double x, double y, int direction, int pass_stop)
    {
        if (direction == 0 || direction == 1)
        {
            int limit_stop = latitude.Length - 2;
            if (direction == 0)//往下园开
            {
                if ((pass_stop + 1) <= latitude.Length - 2) limit_stop = pass_stop + 1;
            }
            else //往上园开
            {
                limit_stop = 0;
                if ((pass_stop - 1) >= 0) limit_stop = pass_stop - 1;
            }

            //计算是否在该路径上
            int i = pass_stop;
            if (x == latitude[pass_stop])
            {
                return pass_stop;
            }
            //两条线完全平行，一般不太可能
            if (slope[pass_stop] == 0)
            {
                return pass_stop;
            }
            double distance = (slope[i] * x - y + y_axis[i]) * (slope[i] * x - y + y_axis[i]) / (slope[i] * slope[i] + 1);
            double point_line_slope = -1 / slope[i];
            double b = -point_line_slope * x + y;
            double cross_x = (b - y_axis[i]) / (slope[i] - point_line_slope);
            double portion = (latitude[i] - cross_x) / (latitude[i] - latitude[i + 1]);
            float portion_convert = (float)(portion);
            float result = Mathf.Round(portion_convert * 100) / 100;//保留一位小数
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

    //二号线的计算
    private int calculate_min_distance_2(double x, double y, int direction, int pass_stop)
    {
        if (direction == 0 || direction == 1)
        {
            int limit_stop = latitude.Length - 2;
            if (direction == 0)//往下园开
            {
                if ((pass_stop + 1) <= latitude.Length - 2) limit_stop = pass_stop + 1;
            }
            else //往上园开
            {
                limit_stop = 0;
                if ((pass_stop - 1) >= 0) limit_stop = pass_stop - 1;
            }

            //计算是否在该路径上
            int i = pass_stop;
            if (x == latitude_2[pass_stop])
            {
                return pass_stop;
            }
            //两条线完全平行，一般不太可能
            if (slope_2[pass_stop] == 0)
            {
                return pass_stop;
            }
            double distance = (slope_2[i] * x - y + y_axis_2[i]) * (slope_2[i] * x - y + y_axis_2[i]) / (slope_2[i] * slope_2[i] + 1);
            double point_line_slope = -1 / slope_2[i];
            double b = -point_line_slope * x + y;
            double cross_x = (b - y_axis_2[i]) / (slope_2[i] - point_line_slope);
            double portion = (latitude_2[i] - cross_x) / (latitude_2[i] - latitude_2[i + 1]);
            float portion_convert = (float)(portion);
            float result = Mathf.Round(portion_convert * 100) / 100;//保留一位小数
            if (result >= -0.1 && result <= 1.1) return pass_stop;
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

    #endregion

    //pass the test, 计算这个点在这条线的几分之几处,返回距离靠左边的点的几分之几,计算1号线车在这一段的几分之几
    private float calculate_partition(double x, double y, int line_number)
    {
        double line1 = ((latitude[line_number] - x) * (latitude[line_number] - x) + (longtitude[line_number] - y) * (longtitude[line_number] - y));
        double line2 = ((latitude[line_number] - latitude[line_number+1]) * (latitude[line_number] - latitude[line_number + 1]) + (longtitude[line_number] - longtitude[line_number+1]) * (longtitude[line_number] - longtitude[line_number+1]));
        float result = (float)(line1 / line2);
        if (result > 1) return 1;
        else if (result < 0) return 0;
        else return result;
    }
    //pass the test, 计算这个点在这条线的几分之几处,返回距离靠左边的点的几分之几,计算2号线车在这一段的几分之几
    private float calculate_partition_2(double x, double y, int line_number)
    {
        Debug.Log("line_number:" + line_number);
        double line1 = ((latitude_2[line_number] - x) * (latitude_2[line_number] - x) + (longtitude_2[line_number] - y) * (longtitude_2[line_number] - y));
        double line2 = ((latitude_2[line_number] - latitude_2[line_number + 1]) * (latitude_2[line_number] - latitude_2[line_number + 1]) + (longtitude_2[line_number] - longtitude_2[line_number + 1]) * (longtitude_2[line_number] - longtitude_2[line_number + 1]));
        float result = (float)(line1 / line2);
        if (result > 1) return 1;
        else if (result < 0) return 0;
        else return result;
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
            if (current_step_number == line_number + 1 && partition < 0.1)
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
                for (int i = 0; i < current_step_number; i++)
                {
                    total_waiting_time += line_running_time[i];
                }
                total_waiting_time = single_route_time * 2 - (Mathf.Round(total_waiting_time * 10) / 10);
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
                total_waiting_time = Mathf.Round(total_waiting_time * 10) / 10;
                return total_waiting_time;
            }
        }
        //下园往上园开
        else
        {
            //到站
            //下一站就到了
            if ((current_step_number == line_number && partition < 0.1))
            {
                return 0f;
            }
            //已经过站了要绕一圈
            else if (current_step_number > line_number)
            {
                total_waiting_time = (1 - partition) * line_running_time[line_number];
                for (int i = current_step_number; i < latitude.Length - 2; i++)
                {
                  //  Debug.Log(i);
                    total_waiting_time += line_running_time[i];
                }
                for (int i = line_number + 1; i < latitude.Length - 2; i++)
                {
                    total_waiting_time += line_running_time[i];
                }
                //总时长经过已经走过的距离
                total_waiting_time = single_route_time * 2 - (Mathf.Round(total_waiting_time * 10) / 10);
                return total_waiting_time;
            }
            //下一站没到，还远着
            else
            {
                // Debug.Log("hi5:" + current_step_number + " " + line_number + " " + partition);
                total_waiting_time = 0f;
                if (line_number != 8)
                {
                    total_waiting_time = partition * line_running_time[line_number];
                }

                for (int i = line_number - 1; i >= current_step_number; i--)
                {
                    total_waiting_time += line_running_time[i];
                }
                total_waiting_time = Mathf.Round(total_waiting_time * 10) / 10;
                return total_waiting_time;
            }
        }




    }
    #endregion


    //经纬度，现在车的运行方向，现在学生的等待车站，目前车的序号（从0开始）
    public void final_run_logic(double x, double y, int bus_running_oir, int current_student_wait_step, int bus_number)
    {
        if (All_bus_line_number[bus_number] == -1) return;

        float partition = 0;
        int line_number = All_bus_line_number[bus_number];
        if (All_bus_route_number[bus_number] == 1) partition = calculate_partition(x, y, line_number);
        else partition = calculate_partition_2(x, y, line_number);
        Debug.Log("p:"+line_number.ToString()+partition);

        //改成调用相应车的运动脚本
        float [] info = { (float)line_number, partition };
        Bus[bus_number].SendMessage("Bus_move", info);
       

        All_Last_BusStop[bus_number] = line_number;
        if (partition == 1 && bus_running_oir == 0)
        {
            All_Last_BusStop[bus_number] += 1;
        }

        if (partition == 0&& bus_running_oir == 1)
        {
            All_Last_BusStop[bus_number] -= 1;
        }
        All_Last_bus_ori[bus_number] = bus_running_oir;


        //经过站点且不是拐点，更新座位
        int last_number = All_bus_compareStop[bus_number];
        if (last_number != line_number && last_number != 2 && last_number != 5 && last_number != 6)
        {
                int remain_seat_number = random_remain_seat();
                All_bus_seat_number[bus_number] = remain_seat_number;
                Bus[bus_number].SendMessage("update_seat_ui", remain_seat_number);
        }
        All_bus_compareStop[bus_number] = line_number;



    }


}

