using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using LTE.DB;
using System;
using System.Collections;
using System.Data;
using LTE.InternalInterference.Grid;
using LTE.Model;
using LTE.Utils;
namespace LTE.GIS
{
    /// <summary>
    /// 刷新图层
    /// </summary>
    public class OperateSelectPointsLayer
    {
        private IFeatureLayer pFeatureLayer;
        private IFeatureClass pFeatureClass;


        private int CIIndex;
        private int FromIndex;
        private int LonIndex;
        private int LatIndex;
        private int AziIndex;
        private int RecPwIndex;
        
        public OperateSelectPointsLayer(string layername)
        {
            IFeatureWorkspace featureWorkspace = MapWorkSpace.getWorkSpace();
            string path = System.Configuration.ConfigurationSettings.AppSettings["GisPath"].ToString();
            //若不存在shp文件，则创建
            if (!DefineLayer.findLayer(path, layername))
            {
                new CreateLayer(path, layername).CreateSelectPointLayer();//目前有问题，但是理论上不需要新建
            }
            pFeatureClass = featureWorkspace.OpenFeatureClass(layername);
            pFeatureLayer = new FeatureLayer();
            pFeatureLayer.FeatureClass = pFeatureClass;

            CIIndex = pFeatureClass.FindField("CI");
            FromIndex = pFeatureClass.FindField("fromName");
            LonIndex = pFeatureClass.FindField("Lontitude");
            LatIndex = pFeatureClass.FindField("Latitude");
            AziIndex = pFeatureClass.FindField("Azimuth");
            RecPwIndex = pFeatureClass.FindField("ReceivePW");
        }

        /// <summary>
        /// 删除图层所有要素
        /// </summary>
        public void ClearLayer()
        {
            FeatureUtilities.DeleteFeatureLayerFeatrues(this.pFeatureLayer);
        }
        /// <summary>
        /// 刷新虚拟干扰源图层
        /// </summary>
        public bool ConstuctSelectPoints(string fromname)
        {
            Hashtable ht = new Hashtable();
            ht["fromName"] = fromname;
            DataTable virTable = IbatisHelper.ExecuteQueryForDataTable("GetSelectedPoint", ht);
            if (virTable.Rows.Count < 2)
            {
                return false;
            }
            IDataset dataset = (IDataset)pFeatureClass;
            IWorkspace workspace = dataset.Workspace;
            //Cast for an IWorkspaceEdit   
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;
            //start an edit session and operation               
            workspaceEdit.StartEditing(true);
            workspaceEdit.StartEditOperation();

            IFeatureCursor pFeatureCursor = pFeatureClass.Insert(true);
            IFeatureBuffer pFeatureBuffer;


            //循环添加
            double circRadius = 60;//扇区生成的半径大小
            double every = 1;


            foreach (DataRow vir in virTable.Rows)
            {

                int ci = Convert.ToInt32(vir["CI"]);
                string fromName = Convert.ToString(vir["fromName"]);
                double ReceivePW = Convert.ToDouble(vir["ReceivePW"]);
                double azimuth = Convert.ToDouble(vir["Azimuth"]);



                pFeatureBuffer = pFeatureClass.CreateFeatureBuffer();//创建图元缓冲


                double x = Convert.ToDouble(vir["x"]);
                double y = Convert.ToDouble(vir["y"]);

                IPoint startPoint = GeometryUtilities.ConstructPoint3D(x, y, 0);

                LTE.Geometric.Point p = new Geometric.Point(x,y,0);
                PointConvertByProj.Instance.GetGeoPoint(p);

                double Direction = (450 - azimuth) % 360;

                IPoint leftPoint = GeometryUtilities.ConstructPoint3D(circRadius * Math.Cos((Direction - 18 * every) * Math.PI / 180) + x, y + circRadius * Math.Sin((Direction - 18 * every) * Math.PI / 180), 0);
                IPoint rightPoint = GeometryUtilities.ConstructPoint3D(circRadius * Math.Cos((Direction + 18 * every) * Math.PI / 180) + x, y + circRadius * Math.Sin((Direction + 9 * every) * Math.PI / 180), 0);

                IGeometryCollection pGeometryColl = GeometryUtilities.ConstructPolygon(new IPoint[] { startPoint, leftPoint, rightPoint });
                GeometryUtilities.MakeZAware(pGeometryColl as IGeometry);

                ///暂时没添加经纬度

                pFeatureBuffer.Shape = pGeometryColl as IGeometry;
                pFeatureBuffer.set_Value(CIIndex, ci);
                pFeatureBuffer.set_Value(FromIndex, fromName);
                pFeatureBuffer.set_Value(AziIndex, azimuth);
                pFeatureBuffer.set_Value(LonIndex, p.X);
                pFeatureBuffer.set_Value(LatIndex, p.Y);
                pFeatureBuffer.set_Value(RecPwIndex, 10 * (Math.Log10(ReceivePW) + 3));
                pFeatureCursor.InsertFeature(pFeatureBuffer);

            }

            //一次性提交
            pFeatureCursor.Flush();

            //stop editing   
            workspaceEdit.StopEditOperation();
            workspaceEdit.StopEditing(true);

            GISMapApplication.Instance.RefreshLayer(pFeatureLayer);
            return true;
            //GISMapApplication.Instance.FullExtent(pFeatureLayer.AreaOfInterest);
        }


    }
}
