using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

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
using LTE.Model;

namespace LTE.GIS
{
    // 2019.5.30 地形
    public class OperateTINLayer
    {
        private IFeatureLayer pFeatureLayer;
        private IFeatureClass pFeatureClass;
        private int point1Index;
        private int point2Index;
        private int point3Index;

        // 列名
        public OperateTINLayer(string layerName)
        {
            //IFeatureWorkspace featureWorkspace = MapWorkSpace.getWorkSpace();
            //IFeatureClass fclass = featureWorkspace.OpenFeatureClass(layerName);
            //IFeatureLayer flayer = new FeatureLayer();
            //pFeatureLayer.FeatureClass = pFeatureClass;

            IFeatureWorkspace featureWorkspace = MapWorkSpace.getWorkSpace();
            string path = System.Configuration.ConfigurationSettings.AppSettings["GisPath"].ToString();
            //若不存在shp文件，则创建
            if (!DefineLayer.findLayer(path, layerName))
            {
                new CreateLayer(path, layerName).CreateTinLayer();
            }
            pFeatureClass = featureWorkspace.OpenFeatureClass(layerName);
            pFeatureLayer = new FeatureLayer();
            pFeatureLayer.FeatureClass = pFeatureClass;
            this.point1Index = pFeatureClass.FindField("point1");
            this.point2Index = pFeatureClass.FindField("point2");
            this.point3Index = pFeatureClass.FindField("point3");

            //pFeatureLayer = GISMapApplication.Instance.GetLayer(LayerNames.TIN) as IFeatureLayer;
            //pFeatureClass = pFeatureLayer.FeatureClass;
        }


        /// <summary>
        /// 删除图层所有要素
        /// </summary>
        public void ClearLayer()
        {
            FeatureUtilities.DeleteFeatureLayerFeatrues(this.pFeatureLayer);
        }

        /// <summary>
        /// 构造 TIN
        /// </summary>
        public bool constuctTIN1()
        {
            double minX = 0, minY = 0, maxX = 0, maxY = 0;
            InternalInterference.Grid.GridHelper.getInstance().getMinXY(ref minX, ref minY);
            InternalInterference.Grid.GridHelper.getInstance().getMaxXY(ref maxX, ref maxY);

            Hashtable ht = new Hashtable();
            ht["minX"] = minX;
            ht["maxX"] = maxX;
            ht["minY"] = minY;
            ht["maxY"] = maxY;
            DataTable gridTable = IbatisHelper.ExecuteQueryForDataTable("GetTINVertexByArea", ht);
            if (gridTable.Rows.Count < 1)
                return false;

            IDataset dataset = (IDataset)pFeatureLayer.FeatureClass;
            IWorkspace workspace = dataset.Workspace;
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;
            workspaceEdit.StartEditing(true);
            workspaceEdit.StartEditOperation();

            IFeatureCursor pFeatureCursor = pFeatureClass.Insert(true);
            IFeatureBuffer pFeatureBuffer;

            float x, y, z;
            List<IPoint> pts = new List<IPoint>();
            //循环添加
            int cnt = 0;
            //初始化进度信息
            LoadInfo loadInfo = new LoadInfo();
            loadInfo.count = gridTable.Rows.Count;
            loadInfo.loadCreate();
            //循环添加
            foreach (DataRow dataRow in gridTable.Rows)
            {
                if (cnt++ % 1000 == 0)
                {
                    loadInfo.cnt = cnt;
                    loadInfo.loadUpdate();
                    Console.WriteLine("已计算  " + cnt + "/" + gridTable.Rows.Count);
                }
                if (!(float.TryParse(dataRow["VertexX"].ToString(), out x) && float.TryParse(dataRow["VertexY"].ToString(), out y) && float.TryParse(dataRow["VertexHeight"].ToString(), out z)))
                    continue;

                IPoint pointA = GeometryUtilities.ConstructPoint3D(x, y, z);
                pts.Add(pointA);

                if (pts.Count >= 3)
                {
                    IGeometryCollection pGeometryColl = GeometryUtilities.ConstructMultiPath(pts);
                    pFeatureBuffer = pFeatureClass.CreateFeatureBuffer();
                    pFeatureBuffer.Shape = pGeometryColl as IGeometry;
                    pFeatureCursor.InsertFeature(pFeatureBuffer);

                    pts.Clear();
                }
            }

            //一次性提交
            pFeatureCursor.Flush();

            //stop editing   
            workspaceEdit.StopEditOperation();
            workspaceEdit.StopEditing(true);

            IFeatureClassManage pFeatureClassManage = (IFeatureClassManage)pFeatureClass;
            pFeatureClassManage.UpdateExtent();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureClassManage);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(dataset);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(workspace);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);

            //更新完成进度信息
            loadInfo.cnt = cnt;
            loadInfo.loadUpdate();
            //GISMapApplication.Instance.RefreshLayer(pFeatureLayer);
            return true;
        }

        /// <summary>
        /// 构造 TIN 面
        /// </summary>
        public bool constuctTIN()
        {
            double minX = 0, minY = 0, maxX = 0, maxY = 0;
            InternalInterference.Grid.GridHelper.getInstance().getMinXY(ref minX, ref minY);
            InternalInterference.Grid.GridHelper.getInstance().getMaxXY(ref maxX, ref maxY);

            Hashtable ht = new Hashtable();
            ht["minX"] = minX;
            ht["maxX"] = maxX;
            ht["minY"] = minY;
            ht["maxY"] = maxY;
            DataTable gridTable = IbatisHelper.ExecuteQueryForDataTable("GetTINVertexByArea", ht);
            if (gridTable.Rows.Count < 1)
                return false;

            IDataset dataset = (IDataset)pFeatureLayer.FeatureClass;
            IWorkspace workspace = dataset.Workspace;
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;
            workspaceEdit.StartEditing(true);
            workspaceEdit.StartEditOperation();

            IFeatureCursor pFeatureCursor = pFeatureClass.Insert(true);
            IFeatureBuffer pFeatureBuffer;

            float x, y, z;
            List<IPoint> pts = new List<IPoint>();
            //循环添加
            int cnt = 0;
            //初始化进度信息
            LoadInfo loadInfo = new LoadInfo();
            loadInfo.count = gridTable.Rows.Count;
            loadInfo.loadCreate();
            //循环添加
            foreach (DataRow dataRow in gridTable.Rows)
            {
                if (cnt++ % 1000 == 0)
                {
                    loadInfo.cnt = cnt;
                    loadInfo.loadUpdate();
                    Console.WriteLine("已计算  " + cnt + "/" + gridTable.Rows.Count);
                }
                if (!(float.TryParse(dataRow["VertexX"].ToString(), out x) && float.TryParse(dataRow["VertexY"].ToString(), out y) && float.TryParse(dataRow["VertexHeight"].ToString(), out z)))
                    continue;

                IPoint pointA = GeometryUtilities.ConstructPoint3D(x, y, z);
                pts.Add(pointA);

                if (pts.Count >= 3)
                {
                    IGeometryCollection pGeometryColl = GeometryUtilities.ConstructPolygon(new IPoint[] { pts[0], pts[1], pts[2] });
                    GeometryUtilities.MakeZAware(pGeometryColl as IGeometry);
                    pFeatureBuffer = pFeatureClass.CreateFeatureBuffer();
                    pFeatureBuffer.Shape = pGeometryColl as IGeometry;
                    PointConvert.Instance.GetGeoPoint(pts[0]);
                    PointConvert.Instance.GetGeoPoint(pts[1]);
                    PointConvert.Instance.GetGeoPoint(pts[2]);
                    pFeatureBuffer.set_Value(this.point1Index, string.Format("{0},{1},{2}",pts[0].X,pts[0].Y,pts[0].Z));
                    pFeatureBuffer.set_Value(this.point2Index, string.Format("{0},{1},{2}", pts[1].X, pts[1].Y, pts[1].Z));
                    pFeatureBuffer.set_Value(this.point3Index, string.Format("{0},{1},{2}", pts[2].X, pts[2].Y, pts[2].Z));

                    pFeatureCursor.InsertFeature(pFeatureBuffer);
                    pts.Clear();
                }
            }

            //一次性提交
            pFeatureCursor.Flush();

            //stop editing   
            workspaceEdit.StopEditOperation();
            workspaceEdit.StopEditing(true);

            IFeatureClassManage pFeatureClassManage = (IFeatureClassManage)pFeatureClass;
            pFeatureClassManage.UpdateExtent();

            //GISMapApplication.Instance.RefreshLayer(pFeatureLayer);

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureClassManage);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(dataset);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(workspace);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);

            //更新完成进度信息
            loadInfo.cnt = cnt;
            loadInfo.loadUpdate();
            return true;
        }
    }
}
