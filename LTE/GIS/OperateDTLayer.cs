using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Analyst3D;
using LTE.DB;
using LTE.InternalInterference.Grid;
using LTE.Model;
namespace LTE.GIS
{
    /// <summary>
    /// 覆盖网格，gxid,gyid,enodeb,ci,cellname
    /// </summary>
    public class OperateDTLayer
    {
        private IFeatureLayer pFeatureLayer;
        private IFeatureClass pFeatureClass;

        private int IdIndex;
        private int RSRPIndex;
        private int SINRIndex;
        private int xIndex;
        private int yIndex;
        private int DeviceIndex;
        private int BtsnameIndex;
        private int eNodeBIndex;
        private int PCIIndex;
        public OperateDTLayer(string layerName)
        {
            IFeatureWorkspace featureWorkspace = MapWorkSpace.getWorkSpace();
            string path = System.Configuration.ConfigurationSettings.AppSettings["GisPath"].ToString();
            //若不存在shp文件，则创建
            if (!DefineLayer.findLayer(path, layerName))
            {
                new CreateLayer(path, layerName).CreateDTLayer();//目前有问题，但是理论上不需要新建
            }
            pFeatureClass = featureWorkspace.OpenFeatureClass(layerName);
            pFeatureLayer = new FeatureLayer();
            pFeatureLayer.FeatureClass = pFeatureClass;

            
            this.IdIndex = pFeatureClass.FindField("Id");
            this.RSRPIndex = pFeatureClass.FindField("RSRP");
            this.SINRIndex = pFeatureClass.FindField("SINR");
            this.xIndex = pFeatureClass.FindField("x");
            this.yIndex = pFeatureClass.FindField("y");
            this.DeviceIndex = pFeatureClass.FindField("Device");
            //this.BtsnameIndex = pFeatureClass.FindField("Btsname");
            //this.eNodeBIndex = pFeatureClass.FindField("eNodeBID");
            //this.PCIIndex = pFeatureClass.FindField("PCI");
        }
        /// <summary>
        /// 删除图层所有要素
        /// </summary>
        public void ClearLayer()
        {
            FeatureUtilities.DeleteFeatureLayerFeatrues(this.pFeatureLayer);
        }

        /// <summary>
        /// 根据不同的参数显示DT数据
        /// </summary>
        /// <param name="btsname">是否显示某指定基站的路测数据，是则参数为具体BTSname,否则为""</param>
        /// <param name="distance">约束显示的路测点与其基站距离在distance范围内，无约束distance值小于0</param>
        /// <param name="minx">获取指定范围内的路测点，如果没有该约束，则值为负</param>
        /// <param name="miny"></param>
        /// <param name="maxx"></param>
        /// <param name="maxy"></param>
        /// <returns></returns>
        public bool constuctDTGrids(string btsname, double distance, double minx, double miny, double maxx, double maxy)
        {
            DataTable dtinfo = new DataTable();
            Hashtable ht = new Hashtable();
            ht["btsname"] = btsname;
            ht["distance"] = distance;

            if (distance < 0 && btsname == "")
            {
                if (minx < 1 || miny < 1 || maxx < 1 || maxy < 1)
                {
                    Debug.WriteLine("无限制");
                    dtinfo = IbatisHelper.ExecuteQueryForDataTable("GetDTInfo", null);
                }
                else
                {
                    ht["minx"] = minx;
                    ht["miny"] = miny;
                    ht["maxx"] = maxx;
                    ht["maxy"] = maxy;
                    dtinfo = IbatisHelper.ExecuteQueryForDataTable("GetDTByRangeXY", ht);
                }
            }
            else if (distance > 0 && btsname == "")//只记录与目标距离少于dis的dt数据
            {
                Debug.WriteLine("限制距离" + btsname);
                dtinfo = IbatisHelper.ExecuteQueryForDataTable("GetDTInfoWithDis", ht);
            }
            else if (distance < 1)//只获取指定bts的路测
            {
                Debug.WriteLine("限制Bts");
                dtinfo = IbatisHelper.ExecuteQueryForDataTable("GetDTInfoWithBts", ht);
            }
            else//两个约束都有
            {
                Debug.WriteLine("限制Bts&&dis");
                dtinfo = IbatisHelper.ExecuteQueryForDataTable("GetDTInfoWithBoth", ht);
            }
            if (dtinfo.Rows.Count < 1)
            {
                return false;
            }

            IDataset dataset = (IDataset)pFeatureLayer.FeatureClass;
            IWorkspace workspace = dataset.Workspace;
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;
            workspaceEdit.StartEditing(true);
            workspaceEdit.StartEditOperation();

            IFeatureCursor pFeatureCursor = pFeatureClass.Insert(true);
            IFeatureBuffer pFeatureBuffer;

            double x, y;
            double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            double RSRP, SINR;
            int ID;
            //int ID, PCI, eNodeB;
            //循环添加
            int i = 0;
            LoadInfo loadInfo = new LoadInfo();
            loadInfo.count = dtinfo.Rows.Count;
            loadInfo.loadCreate();
            foreach (DataRow dataRow in dtinfo.Rows)
            {
                
                if (i++ % 1000 == 0)
                {
                    loadInfo.cnt = i;
                    loadInfo.loadUpdate();
                    Console.WriteLine("已计算  " + i + "/" + dtinfo.Rows.Count);
                }
                if (!(double.TryParse(dataRow["x"].ToString(), out x)
                    && double.TryParse(dataRow["y"].ToString(), out y)
                    && int.TryParse(dataRow["ID"].ToString(), out ID)
                    //&& int.TryParse(dataRow["PCI"].ToString(), out PCI)
                    //&& int.TryParse(dataRow["eNodeBID"].ToString(), out eNodeB)
                    && double.TryParse(dataRow["SINR"].ToString(), out SINR)
                    && double.TryParse(dataRow["RSRP"].ToString(), out RSRP)))
                    continue;

                if (!GridHelper.getInstance().XYGetGridXY(x, y, ref x1, ref y1, ref x2, ref y2))
                {
                    //不在该范围内
                    continue;
                }
                //根据x,y所在栅格，计算所在栅格的最大最小坐标
                IPoint pointA = GeometryUtilities.ConstructPoint2D(x1, y1);
                IPoint pointB = GeometryUtilities.ConstructPoint2D(x2, y1);
                IPoint pointC = GeometryUtilities.ConstructPoint2D(x2, y2);
                IPoint pointD = GeometryUtilities.ConstructPoint2D(x1, y2);

                IGeometryCollection pGeometryColl = GeometryUtilities.ConstructPolygon(new IPoint[] { pointA, pointB, pointC, pointD });

                pFeatureBuffer = pFeatureClass.CreateFeatureBuffer();
                pFeatureBuffer.Shape = pGeometryColl as IGeometry;
                pFeatureBuffer.set_Value(this.IdIndex, ID);
                pFeatureBuffer.set_Value(this.DeviceIndex, dataRow["Device"].ToString());
                pFeatureBuffer.set_Value(this.xIndex, x);
                pFeatureBuffer.set_Value(this.yIndex, y);
                pFeatureBuffer.set_Value(this.RSRPIndex, RSRP);
                pFeatureBuffer.set_Value(this.SINRIndex, SINR);
                //pFeatureBuffer.set_Value(this.eNodeBIndex, eNodeB);
                //pFeatureBuffer.set_Value(this.BtsnameIndex, dataRow["Btsname"].ToString());
                //pFeatureBuffer.set_Value(this.PCIIndex, PCI);
                pFeatureCursor.InsertFeature(pFeatureBuffer);
            }

            //一次性提交
            pFeatureCursor.Flush();

            //stop editing   
            workspaceEdit.StopEditOperation();
            workspaceEdit.StopEditing(true);

            GISMapApplication.Instance.RefreshLayer(pFeatureLayer);
            loadInfo.cnt = i;
            loadInfo.loadUpdate();
            return true;
        }
    }
}
