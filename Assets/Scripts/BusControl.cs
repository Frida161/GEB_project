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

    //顺序为教职工宿舍，教职工和书院的十字路口（ 114.208618f, 22.697022f,暂时取消），书院站，拐点，体育馆，启动区(缺少)，1,2号线分岔点，拐点3（到TB的拐点），学活，图书馆(目前一共9段)
    private double[] latitude = { 114.207266f, 114.209234f, 114.209261f, 114.216622f, 114.217826f, 114.218024f,114.217341f, 114.218131f, 114.219380f, };//经纬度第一个值
    private double[] longtitude = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.693813f, 22.695234f, 22.696576f };//经纬度第二个值

    private double[] latitude_2 = { 114.207266f, 114.209234f, 114.209261f, 114.216622f, 114.217826f, 114.218024f,114.218455f, 114.220224f, 114.221163f };//经纬度第一个值
    private double[] longtitude_2 = { 22.696781f, 22.69631f, 22.692097f, 22.691858f, 22.692925f, 22.693034f, 22.692742f, 22.694088f, 22.69508f };//经纬度第二个值


    private double[] slope = new double[10];//每一条线段的斜率,
    private double[] y_axis = new double[10];//每一条线段的截距

    private double[] slope_2 = new double[10];//每一条线段的斜率,
    private double[] y_axis_2 = new double[10];//每一条线段的截距

    private float[] line_running_time = { 1f, 2f, 3.5f, 1f, 0.5f, 0.5f, 0.5f, 1f };//每一段路程的行驶时间,例如教职工宿舍和书院行驶时间为1min,单程9分钟
    private float single_route_time = 9f;

    //记录每一辆车的上一站
    //在上园的停车场的车，初始值为 Last_BusStop = 0， last_bus_ori = 0
    //在下园的停车场的车，初始值为 Last_BusStop = 6， last_bus_ori = 1
    private int[] All_Last_BusStop = {0,0,0,0,0,0,0,0,0,0,0,0};
    private int[] All_Last_bus_ori = {0,0,0,0,0,0,0,0,0,0,0,0};
    private int current_waiting_step = 1;//当前人所在的车站
    private int[] All_bus_route_number = {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}; //1->1号线，2->2号线

    private double[] simulate_GPS_x = { 114.207361f, 114.207835f, 114.208159f, 114.208693f, 114.209093f, 114.209354f, 114.209538f, 114.2209695f, 114.210027f, 114.210346f, 114.209556f, 114.213132f, 114.214363f, 114.215553f, 114.216757f, 114.21753f, 114.217633f, 114.218024f, 114.217808f, 114.218284f, };
    private double[] simulate_GPS_y = { 22.69648f, 22.696401f, 22.696553f, 22.69683f, 22.696851f, 22.696626f, 22.69633f, 22.696042f, 22.695542f, 22.694955f, 22.694275, 22.691058f, 22.691333f, 22.691179f, 22.691675f, 22.692087f, 22.692637f, 22.693392f, 22.694f, 22.69483f };

    private float[] All_bus_waiting_time = { 0,0,0,0,0,0,0,0,0,0};
    private int[] All_bus_seat_number = { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 };//所有车初始有30个座位
    private int[] All_current_bus_oir = { 0,0,0,0,0,0,0,0,0,0,0,0,0};
    private int[] All_bus_line_number = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    Dictionary<int, float> bus_time_dic = new Dictionary<int, float>();

    void Start()
    {
        calculate_slope_and_y_axis();
        generate_initial_position();
       // StartCoroutine(test());
    }

    public void change_current_station(int number)
    {
        current_waiting_step = number;
    }

    private void FixedUpdate()
    {
        test();
    }

    #region 模拟运行（待删除）

    public void generate_initial_position()
    {
        foreach (BusInfo bus_info in BusSimulator.singleton.GetInfo())
        {
            int bus_id = bus_info.busId;
            int direction = bus_info.dir;
            int line = bus_info.line + 1;
            int status = bus_info.status;
            Vector2 location = bus_info.location;
            if (direction == 1) direction = 0;
            else direction = 1;

            if (status == 1) status = 0;
            if (status > 1) status = status - 1;
            if (status == 8) status = 7;
            Debug.Log("line_number:" + line);
            //生成的车在上园，即将往下园开
            if (direction == 0)
            {
                Bus[bus_id] = Instantiate(Two_kind_bus[0],canvas.transform);
                Bus[bus_id].transform.localPosition = new Vector3(-579,893,9);
                All_Last_BusStop[bus_id] = 0;
                All_Last_bus_ori[bus_id] = direction;

            }
            //生成的车在上园，即将往上园开
            else
            {
                Bus[bus_id] = Instantiate(Two_kind_bus[1], canvas.transform);
                Bus[bus_id].transform.localPosition = new Vector3(521,-736,0);
                All_Last_BusStop[bus_id] = 6;
                All_Last_bus_ori[bus_id] = direction;
            }

            All_bus_route_number[bus_id] = line;
            if (status == -1) Bus[bus_id].SetActive(false);
            else {
                Bus[bus_id].SetActive(true); }
        }

    }
    public void test()
    {
        int start = simulate_GPS_x.Length - 1;

            //bus id 暂时没用

           /* BusInfo bus_info = BusSimulator.singleton.GetInfo()[0];
            int bus_id = bus_info.busId;
            int direction = bus_info.dir;
            int line = bus_info.line;
            int status = bus_info.status;
            Vector2 location = bus_info.location;

            //候车跳过
            if (status == -1) continue;
            if (direction == 1) direction = 0;
            else direction = 1;*/

            foreach (BusInfo bus_info in BusSimulator.singleton.GetInfo())
            {
                int bus_id = bus_info.busId;
                int direction = bus_info.dir;
                int status = bus_info.status;
                Vector2 location = bus_info.location;

                if (direction == 1) direction = 0;
                else direction = 1;

                if (status == 1) status = 0;
                if (status > 1) status = status - 1;
                if (status == 8) status = 7;
          /*  if (status!= -1)
            {
                Debug.Log("line_number:" + status);
            }*/
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

            if (status == -1)
            {
                Bus[bus_id].SetActive(false);
            }
            else
            {
                Bus[bus_id].SetActive(true);
            }
            All_bus_line_number[bus_id] = status;

            final_run_logic(location.x, location.y, direction, current_waiting_step, bus_id);

                //更新这个车的方向，用来显示
                All_current_bus_oir[bus_id] = direction;
            }


            //更新总榜ui然后字典清空
            update_all_UI();
            bus_time_dic.Clear();


    }
    #endregion


    #region 更新总榜UI

    private void update_all_UI()
    {
        bus_info_text.text = return_bus_info(bus_time_dic);
    }


    private string return_bus_info(Dictionary<int, float> dic)
    {
        //升序输出时间
        string update_all_bus_info = "";
       
        var dicSort = from objDic in dic orderby objDic.Value ascending select objDic;
        foreach (KeyValuePair<int, float> kvp in dicSort)
        {
            int bus_number = kvp.Key;
            float waiting_time = kvp.Value;

            //图书馆站只显示1号线
            if (current_waiting_step == 8 && All_bus_route_number[bus_number] == 2) continue;
        //    if (current_waiting_step == 7 && All_bus_route_number[bus_number] == 1) continue;

            update_all_bus_info  += "LINE" + All_bus_route_number[bus_number].ToString();
            //朝着上园开
            if (All_current_bus_oir[bus_number] == 1)
            {
                update_all_bus_info += "(->upper) ";
            }
            else
            {
                update_all_bus_info += "(->lower) ";
            }
            if (waiting_time == 0) update_all_bus_info +=  "<1 min ";
            else update_all_bus_info += waiting_time.ToString() + "min ";
            update_all_bus_info += All_bus_seat_number[bus_number].ToString() + "seats" + "\n";
        }
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


    //pass the test, 计算GPS点到某段线段最短的距离,x-->latitude,y-->longtitude，d=|A*x0+B*y0+C|/√(A*A+B*B)， B = -1,返回那一条线段起始点的下标
    //direction 0 向上园开，direction 1向下园开， direction 2转向
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


    //pass the test, 计算这个点在这条线的几分之几处,返回距离靠左边的点的几分之几
    private float calculate_partition(double x, double y, int line_number)
    {
        return (float)((latitude[line_number] - x) / (latitude[line_number] - latitude[line_number + 1]));
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
       // if (result >= 1) return 1;
       // if (result < 0) return 0;
        return result;
    }

    private float calculate_partition_2(double x, double y, int line_number)
    {
        return (float)((latitude_2[line_number] - x) / (latitude_2[line_number] - latitude_2[line_number + 1])); 
        //点在线上
        if (x == latitude_2[line_number])
        {
            return (float)((latitude_2[line_number] - x) / (latitude_2[line_number] - latitude_2[line_number + 1]));
        }
        //两条线完全平行，一般不太可能
        if (slope_2[line_number] == 0)
        {
            return Mathf.Abs((float)(y - y_axis_2[line_number]));
        }
        //两条线不完全平行
        double point_line_slope = -1 / slope_2[line_number];
        double b = -point_line_slope * x + y;
        double cross_x = (b - y_axis_2[line_number]) / (slope_2[line_number] - point_line_slope);
        double portion = (latitude_2[line_number] - cross_x) / (latitude_2[line_number] - latitude_2[line_number + 1]);
        float portion_convert = (float)(portion);
        float result = Mathf.Round(portion_convert * 100) / 100;//保留两位小数
       // if (result >= 1) return 1;
       // if (result < 0) return 0;
        return result;
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
        int line_number = 0;
        float partition = 0;
        //一号线
        if (All_bus_route_number[bus_number] == 1)
        {
            if (All_Last_bus_ori[bus_number] == bus_running_oir && All_Last_bus_ori[bus_number] == 0)
            {
                line_number = calculate_min_distance(x, y, 0, All_Last_BusStop[bus_number]);
            }
            else if (All_Last_bus_ori[bus_number] == bus_running_oir && All_Last_bus_ori[bus_number] == 1)
            {
                line_number = calculate_min_distance(x, y, 1, All_Last_BusStop[bus_number]);
            }
            //转向上园，在图书馆附近
            else if (All_Last_bus_ori[bus_number] != bus_running_oir && bus_running_oir == 1)
            {
                line_number = calculate_min_distance(x, y, 2, All_Last_BusStop[bus_number]);
            }
            //转向下园，在教职工宿舍附近
            else
            {
                line_number = calculate_min_distance(x, y, 3, All_Last_BusStop[bus_number]);
            }
            partition = calculate_partition(x, y, line_number);
        }
        //二号线
        else
        {
            if (All_Last_bus_ori[bus_number] == bus_running_oir && All_Last_bus_ori[bus_number] == 0)
            {
                line_number = calculate_min_distance_2(x, y, 0, All_Last_BusStop[bus_number]);
            }
            else if (All_Last_bus_ori[bus_number] == bus_running_oir && All_Last_bus_ori[bus_number] == 1)
            {
                line_number = calculate_min_distance_2(x, y, 1, All_Last_BusStop[bus_number]);
            }
            //转向上园，在图书馆附近
            else if (All_Last_bus_ori[bus_number] != bus_running_oir && bus_running_oir == 1)
            {
                line_number = calculate_min_distance_2(x, y, 2, All_Last_BusStop[bus_number]);
            }
            //转向下园，在教职工宿舍附近
            else
            {
                line_number = calculate_min_distance_2(x, y, 3, All_Last_BusStop[bus_number]);
            }
            partition = calculate_partition_2(x, y, line_number);
        }

        line_number = All_bus_line_number[bus_number];
        if (All_bus_route_number[bus_number] == 1) partition = calculate_partition(x, y, line_number);
        else partition = calculate_partition_2(x, y, line_number);
       // Debug.Log("p:"+line_number.ToString()+partition);
        //   Debug.Log("info:"+line_number.ToString()+ "  "+partition);

            //改成调用相应车的运动脚本
            float [] info = { (float)line_number, partition };
        Bus[bus_number].SendMessage("Bus_move", info);
        //向下园开
        int last_number = All_Last_BusStop[bus_number];

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
        if (last_number != line_number && last_number != 2 && last_number != 5 && last_number != 6)
        {
            //更新相应车的座位信息
            if (partition > 0.1 && partition < 0.9)
            {
                int remain_seat_number = random_remain_seat();
                All_bus_seat_number[bus_number] = remain_seat_number;
                Bus[bus_number].SendMessage("update_seat_ui", remain_seat_number);
            }
        }

        //更新等待时间
        float bus_waiting_time = calculate_waiting_time(current_student_wait_step, line_number, partition, bus_running_oir);
        //更新相应车的座位信息
       // All_bus_waiting_time[bus_number] = bus_waiting_time;
        Bus[bus_number].SendMessage("update_waiting_time_ui", bus_waiting_time);
        bus_time_dic.Add(bus_number, bus_waiting_time);

    }


}
