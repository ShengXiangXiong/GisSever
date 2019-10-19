using LTE.Geometric;
using LTE.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTE
{
    class Test
    {
        public void getBuilding()
        {




            Hashtable ht = new Hashtable();
            ht["minGXID"] = 0;
            ht["maxGXID"] = 500;
            ht["minGYID"] = 0;
            ht["maxGYID"] = 500;

            DataTable dt = IbatisHelper.ExecuteQueryForDataTable("GetAccelerateStructTIN", ht);
            Dictionary<int, List<Point>> buildingCenter = new Dictionary<int, List<Point>>();
            int bid;
            double x, y, z;
            for (int i = 0; i < dt.Rows.Count; i++)//按行遍历DataTable
            {
                bid = Convert.ToInt32(dt.Rows[i]["TINID"]);
                x = Convert.ToDouble(dt.Rows[i]["GXID"]);
                y = Convert.ToDouble(dt.Rows[i]["GYID"]);
                z = Convert.ToDouble(dt.Rows[i]["GZID"]);
                if (buildingCenter.ContainsKey(bid))
                {
                    buildingCenter[bid].Add(new Point(x, y, z));
                }
                else
                {
                    List<Point> ts = new List<Point>();
                    ts.Add(new Point(x, y, z));
                    buildingCenter.Add(bid, ts);
                }
            }
            Console.WriteLine(buildingCenter.Count);
        }
    }
}
