using ESRI.ArcGIS.Geoprocessor;
using LTE.DB;
using LTE.GIS;
using LTE.InternalInterference;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTE.SeverImp
{
    class  OperateGisLayerImp : OpreateGisLayer.Iface
    {
        public Result cluster()
        {
            DataTable dt1 = DB.IbatisHelper.ExecuteQueryForDataTable("GetClusterPosition", null);  // Ibatis 数据访问,得到聚类图层文件位置
            string filepath = dt1.Rows[0][0].ToString();
            DataTable dt2 = DB.IbatisHelper.ExecuteQueryForDataTable("GetFishnetRange", null);  // Ibatis 数据访问，得到目标区域范围
            string minx_text = dt2.Rows[0][0].ToString(),
                   miny_text = dt2.Rows[0][1].ToString(),
                   maxx_text = dt2.Rows[0][2].ToString(),
                   maxy_text = dt2.Rows[0][3].ToString(),
                   gridsize_text = dt2.Rows[0][4].ToString();
            double minx = double.Parse(minx_text);
            double miny = double.Parse(miny_text);
            double maxx = double.Parse(maxx_text);
            double maxy = double.Parse(maxy_text);
            double cellsize = double.Parse(gridsize_text);
            int ilength = (int)((maxy - miny) / cellsize), jlength = (int)((maxx - minx) / cellsize);
            int xmax = ilength - 1, ymax = jlength - 1;//xmax是i的上界,ymax是j的上界
            Geoprocessor geoprocessor = new Geoprocessor();
            geoprocessor.OverwriteOutput = true;
            ESRI.ArcGIS.DataManagementTools.CreateFishnet CF = new ESRI.ArcGIS.DataManagementTools.CreateFishnet();
            //工具参数
            CF.out_feature_class = filepath;
            string oricord = minx_text + " " + miny_text;
            string cornercord = maxx_text + " " + maxy_text;
            string ycord = minx_text + " " + Convert.ToString(maxy + 10);
            CF.origin_coord = oricord;
            CF.corner_coord = cornercord;
            CF.y_axis_coord = ycord;
            CF.cell_height = cellsize;
            CF.cell_width = cellsize;
            CF.geometry_type = "POLYGON";
            try
            {
                geoprocessor.Execute(CF, null);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.ToString());
            }
            DataTable dt3;
            try
            {
                dt3 = DB.IbatisHelper.ExecuteQueryForDataTable("GetClusterResult", null);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.ToString());
            }

            int i;
            Dictionary<int, int> myDictionary = new Dictionary<int, int>();
            for (i = 0; i < dt3.Rows.Count; i++)
            {
                int gridID = (Convert.ToInt32(dt3.Rows[i][1].ToString())) * (ymax + 1) + (Convert.ToInt32(dt3.Rows[i][0].ToString()));
                myDictionary.Add(gridID, (Convert.ToInt32(dt3.Rows[i][2].ToString())));
            }
            //添加字段
            geoprocessor = new Geoprocessor();
            geoprocessor.OverwriteOutput = true;
            ESRI.ArcGIS.DataManagementTools.AddField addField = new ESRI.ArcGIS.DataManagementTools.AddField();
            addField.in_table = filepath;//ITable类型参数
            addField.field_name = "type";
            addField.field_type = "SHORT";
            geoprocessor.Execute(addField, null);
            string pFolder = System.IO.Path.GetDirectoryName(filepath);
            string pFileName = System.IO.Path.GetFileName(filepath);
            IWorkspaceFactory pWorkspacefactory = new ShapefileWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspacefactory.OpenFromFile(pFolder, 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            IFeatureClass pFC = pFeatureWorkspace.OpenFeatureClass(pFileName);
            ITable pTable = pFC as ITable;
            ICursor pCursor = pTable.Update(null, false);
            IRow pRow = pCursor.NextRow();
            int filedindex = pFC.Fields.FindField("type");
            int pType;
            while (pRow != null)
            {

                try
                {
                    pType = myDictionary[Convert.ToInt32(pRow.get_Value(0))];
                    pRow.set_Value(filedindex, pType);
                    pCursor.UpdateRow(pRow);
                    pRow = pCursor.NextRow();
                }
                catch (Exception ee)
                { return new Result(false, ee.ToString()); }
            }
            return new Result(true, "成功");

        }

        public Result makeFishnet()
        {
            try
            {
                DataTable dt1 = DB.IbatisHelper.ExecuteQueryForDataTable("GetFishnetPosition", null);  // Ibatis 数据访问,得到渔网图层文件位置
                string filepath = dt1.Rows[0][0].ToString();
                DataTable dt2 = DB.IbatisHelper.ExecuteQueryForDataTable("GetFishnetRange", null);  // Ibatis 数据访问，得到目标区域范围
                string minx_text = dt2.Rows[0][0].ToString(),
                       miny_text = dt2.Rows[0][1].ToString(),
                       maxx_text = dt2.Rows[0][2].ToString(),
                       maxy_text = dt2.Rows[0][3].ToString(),
                       gridsize_text = dt2.Rows[0][4].ToString();
                double minx = double.Parse(minx_text);
                double miny = double.Parse(miny_text);
                double maxx = double.Parse(maxx_text);
                double maxy = double.Parse(maxy_text);
                double cellsize = double.Parse(gridsize_text);
                Geoprocessor geoprocessor = new Geoprocessor();
                geoprocessor.OverwriteOutput = true;
                ESRI.ArcGIS.DataManagementTools.CreateFishnet CF = new ESRI.ArcGIS.DataManagementTools.CreateFishnet();
                //工具参数
                CF.out_feature_class = filepath;
                string oricord = minx_text + " " + miny_text;
                string cornercord = maxx_text + " " + maxy_text;
                string ycord = minx_text + " " + Convert.ToString(maxy + 10);
                CF.origin_coord = oricord;
                CF.corner_coord = cornercord;
                CF.y_axis_coord = ycord;
                CF.cell_height = cellsize;
                CF.cell_width = cellsize;
                CF.geometry_type = "POLYGON";
                //数据库网格
                DataTable dt = new DataTable();//入库
                dt.Columns.Add("x", Type.GetType("System.Int32"));
                dt.Columns.Add("y", Type.GetType("System.Int32"));
                dt.Columns.Add("z", Type.GetType("System.Int32"));
                int rownumber = (int)((maxy - miny) / cellsize), columnnumber = (int)((maxx - minx) / cellsize);//rownumber=xmax+1, columnnumber=ymax+1;
                for (int i = 0; i < rownumber; i++)//GYID
                {
                    for (int j = 0; j < columnnumber; j++)//GXID
                    {
                        dt.Rows.Add(new object[] { j.ToString(), i.ToString(), "1" });
                        dt.Rows.Add(new object[] { j.ToString(), i.ToString(), "2" });
                        dt.Rows.Add(new object[] { j.ToString(), i.ToString(), "3" });
                        if (dt.Rows.Count > 5000)
                        {
                            using (SqlBulkCopy bcp = new SqlBulkCopy(DataUtil.ConnectionString))
                            {
                                bcp.BatchSize = dt.Rows.Count;
                                bcp.BulkCopyTimeout = 1000;
                                bcp.DestinationTableName = "tbAccelerateGridScene1";
                                bcp.WriteToServer(dt);
                                bcp.Close();
                            }
                            dt.Clear();
                        }
                    }
                }
                using (SqlBulkCopy bcp = new SqlBulkCopy(DataUtil.ConnectionString))
                {
                    bcp.BatchSize = dt.Rows.Count;
                    bcp.BulkCopyTimeout = 1000;
                    bcp.DestinationTableName = "tbAccelerateGridScene1";
                    bcp.WriteToServer(dt);
                    bcp.Close();
                }
                dt.Clear();
                try
                {
                    geoprocessor.Execute(CF, null);
                    return new Result(true, "成功");
                }
                catch (Exception ex)
                {
                    string info = "";
                    for (int i = 0; i < geoprocessor.MessageCount; i++)
                    {
                        info = info + geoprocessor.GetMessage(i);
                    }
                    return new Result(false, info);
                }
            }
            catch (System.Data.SqlClient.SqlException err)
            {
                if (err.Message.IndexOf("连接超时") != -1)
                {
                    return new Result(false, "连接超时");
                }
                else if (err.Message.IndexOf("侦听") != -1)
                {
                    return new Result(false, "侦听");
                }
                else
                {
                    return new Result(false, err.ToString());
                }
            }
            catch (System.Exception err)
            {
                return new Result(false, err.ToString());
            }
        }

        public Result overlaybuilding()//渔网图层的名称尽可能短，如b.shp
        {
            try
            {
                //叠加分析
                DataTable dt1 = DB.IbatisHelper.ExecuteQueryForDataTable("GetFishnetPosition", null);  // Ibatis 数据访问,得到渔网图层文件位置
                string fishnetpath = dt1.Rows[0][0].ToString();
                DataTable dt2 = DB.IbatisHelper.ExecuteQueryForDataTable("GetBuildingPosition", null);  // Ibatis 数据访问,得到建筑物图层文件位置
                string buildingpath = dt2.Rows[0][0].ToString();
                DataTable dt3 = DB.IbatisHelper.ExecuteQueryForDataTable("GetBuildingOverlayPosition", null);  // Ibatis 数据访问,得到建筑物叠加结果图层文件位置
                string buildingoverlaypath = dt3.Rows[0][0].ToString();
                string out_feature = buildingoverlaypath;
                Geoprocessor gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                ESRI.ArcGIS.AnalysisTools.Intersect intersect = new ESRI.ArcGIS.AnalysisTools.Intersect();
                intersect.in_features = buildingpath + ";" + fishnetpath;
                intersect.out_feature_class = out_feature;
                intersect.join_attributes = "ALL";
                try
                {
                    gp.Execute(intersect, null);
                }
                catch (Exception ex)
                {
                    string info = "";
                    for (int i = 0; i < gp.MessageCount; i++)
                    {
                        info = info + gp.GetMessage(i);
                    }
                    return new Result(false, "111" + info);
                }
                Dictionary<int, double> myDictionary = new Dictionary<int, double>();
                //计算面积
                try
                {

                    ESRI.ArcGIS.esriSystem.IAoInitialize ao = new ESRI.ArcGIS.esriSystem.AoInitialize();
                    ao.Initialize(ESRI.ArcGIS.esriSystem.esriLicenseProductCode.esriLicenseProductCodeStandard);
                    IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
                    IWorkspaceFactoryLockControl pWorkspaceFactoryLockControl = pWorkspaceFactory as IWorkspaceFactoryLockControl;
                    if (pWorkspaceFactoryLockControl.SchemaLockingEnabled)
                    {
                        pWorkspaceFactoryLockControl.DisableSchemaLocking();
                    }
                    IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(out_feature), 0);
                    IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
                    IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(out_feature));
                    string fileName = System.IO.Path.GetFileName(fishnetpath);
                    string[] a = fileName.Split('.');
                    string fieldname = "FID_" + a[0];
                    int fieldindex = pFeatureClass.Fields.FindField(fieldname);//索引
                    IFeatureCursor pFeatureCursor;
                    pFeatureCursor = pFeatureClass.Search(null, false);
                    int areaindex = pFeatureClass.Fields.FindField("Shape_Area");//索引
                    IFeature pFeature;
                    pFeature = pFeatureCursor.NextFeature();
                    Stopwatch myWatch = new Stopwatch();
                    myWatch.Start();
                    while (pFeature != null)
                    {
                        int gridID = Convert.ToInt32(pFeature.get_Value(fieldindex));
                        IArea parea = pFeature.Shape as IArea;
                        double area = parea.Area;
                        area = Math.Round(area, 2);
                        if (myDictionary.ContainsKey(gridID) == false)
                        {
                            myDictionary.Add(gridID, area);
                        }
                        else
                        {
                            myDictionary[gridID] = myDictionary[gridID] + area;
                        }
                        pFeature = pFeatureCursor.NextFeature();
                    }
                    myWatch.Stop();
                }
                catch (Exception ex1)
                {
                    return new Result(false, "222," + ex1.ToString());
                }
                //结果入库
                DataTable dt4 = DB.IbatisHelper.ExecuteQueryForDataTable("GetFishnetRange", null);  // Ibatis 数据访问，得到目标区域范围
                string minx_text = dt4.Rows[0][0].ToString(),
                       miny_text = dt4.Rows[0][1].ToString(),
                       maxx_text = dt4.Rows[0][2].ToString(),
                       maxy_text = dt4.Rows[0][3].ToString(),
                       gridsize_text = dt4.Rows[0][4].ToString();
                double minx = double.Parse(minx_text);
                double miny = double.Parse(miny_text);
                double maxx = double.Parse(maxx_text);
                double maxy = double.Parse(maxy_text);
                double cellsize = double.Parse(gridsize_text);
                int rownumber = (int)((maxy - miny) / cellsize), columnnumber = (int)((maxx - minx) / cellsize);//rownumber=xmax+1, columnnumber=ymax+1;
                int xmax = rownumber - 1, ymax = columnnumber - 1;
                try
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("x", Type.GetType("System.Int32"));
                    dt.Columns.Add("y", Type.GetType("System.Int32"));
                    dt.Columns.Add("area", Type.GetType("System.Double"));
                    foreach (var item in myDictionary.Keys)
                    {
                        dt.Rows.Add(new object[] { (item % (ymax + 1)).ToString(), (item / (ymax + 1)).ToString(), Math.Round(myDictionary[item] / (cellsize * cellsize), 2).ToString() });
                        if (dt.Rows.Count > 5000)
                        {
                            using (SqlBulkCopy bcp = new SqlBulkCopy(DataUtil.ConnectionString))
                            {
                                bcp.BatchSize = dt.Rows.Count;
                                bcp.BulkCopyTimeout = 1000;
                                bcp.DestinationTableName = "tbAccelerateGridSceneTmpBuilding";
                                bcp.WriteToServer(dt);
                                bcp.Close();
                            }
                            dt.Clear();
                        }
                    }
                    using (SqlBulkCopy bcp = new SqlBulkCopy(DataUtil.ConnectionString))
                    {
                        bcp.BatchSize = dt.Rows.Count;
                        bcp.BulkCopyTimeout = 1000;
                        bcp.DestinationTableName = "tbAccelerateGridSceneTmpBuilding";
                        bcp.WriteToServer(dt);
                        bcp.Close();
                    }
                    dt.Clear();
                    IbatisHelper.ExecuteUpdate("UpdatetbAccelerateGridBuildingByTmp", null);
                    IbatisHelper.ExecuteDelete("DeletetbAccelerateGridSceneTmpBuilding", null);
                    return new Result(true, "建筑物叠加成功");
                }
                catch (Exception ex2)
                {
                    return new Result(false, "333," + ex2.ToString());
                }
            }
            catch (System.Data.SqlClient.SqlException err)
            {
                if (err.Message.IndexOf("连接超时") != -1)
                {
                    return new Result(false, "连接超时");
                }
                else if (err.Message.IndexOf("侦听") != -1)
                {
                    return new Result(false, "侦听");
                }
                else
                {
                    return new Result(false, err.ToString());
                }
            }
            catch (System.Exception err)
            {
                return new Result(false, err.ToString());
            }
        }
        public Result overlaygrass()
        {
            try
            {
                //叠加分析
                DataTable dt1 = DB.IbatisHelper.ExecuteQueryForDataTable("GetFishnetPosition", null);  // Ibatis 数据访问,得到渔网图层文件位置
                string fishnetpath = dt1.Rows[0][0].ToString();
                DataTable dt2 = DB.IbatisHelper.ExecuteQueryForDataTable("GetGrassPosition", null);  // Ibatis 数据访问,得到建筑物图层文件位置
                string grasspath = dt2.Rows[0][0].ToString();
                DataTable dt3 = DB.IbatisHelper.ExecuteQueryForDataTable("GetGrassOverlayPosition", null);  // Ibatis 数据访问,得到建筑物叠加结果图层文件位置
                string grassoverlaypath = dt3.Rows[0][0].ToString();
                string out_feature = grassoverlaypath;
                Geoprocessor gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                ESRI.ArcGIS.AnalysisTools.Intersect intersect = new ESRI.ArcGIS.AnalysisTools.Intersect();
                intersect.in_features = grasspath + ";" + fishnetpath;
                intersect.out_feature_class = out_feature;
                intersect.join_attributes = "ALL";
                try
                {
                    gp.Execute(intersect, null);
                }
                catch (Exception ex)
                {
                    string info = "";
                    for (int i = 0; i < gp.MessageCount; i++)
                    {
                        info = info + gp.GetMessage(i);
                    }
                    return new Result(false, "111" + info);
                }
                Dictionary<int, double> myDictionary = new Dictionary<int, double>();
                //计算面积
                try
                {
                    ESRI.ArcGIS.esriSystem.IAoInitialize ao = new ESRI.ArcGIS.esriSystem.AoInitialize();
                    ao.Initialize(ESRI.ArcGIS.esriSystem.esriLicenseProductCode.esriLicenseProductCodeStandard);
                    IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
                    IWorkspaceFactoryLockControl pWorkspaceFactoryLockControl = pWorkspaceFactory as IWorkspaceFactoryLockControl;
                    if (pWorkspaceFactoryLockControl.SchemaLockingEnabled)
                    {
                        pWorkspaceFactoryLockControl.DisableSchemaLocking();
                    }
                    IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(out_feature), 0);
                    IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
                    IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(out_feature));
                    //    IFeatureClass pFeatureClass = GetFeatureClass(out_feature);
                    string fileName = System.IO.Path.GetFileName(fishnetpath);
                    string[] a = fileName.Split('.');
                    string fieldname = "FID_" + a[0];
                    int fieldindex = pFeatureClass.Fields.FindField(fieldname);//索引
                    IFeatureCursor pFeatureCursor;
                    pFeatureCursor = pFeatureClass.Search(null, false);
                    int areaindex = pFeatureClass.Fields.FindField("Shape_Area");//索引
                    IFeature pFeature;
                    pFeature = pFeatureCursor.NextFeature();
                    Stopwatch myWatch = new Stopwatch();
                    myWatch.Start();
                    //      Dictionary<int, double> myDictionary = new Dictionary<int, double>();
                    while (pFeature != null)
                    {
                        int gridID = Convert.ToInt32(pFeature.get_Value(fieldindex));
                        IArea parea = pFeature.Shape as IArea;
                        double area = parea.Area;
                        area = Math.Round(area, 2);
                        if (myDictionary.ContainsKey(gridID) == false)
                        {
                            myDictionary.Add(gridID, area);
                        }
                        else
                        {
                            myDictionary[gridID] = myDictionary[gridID] + area;
                        }
                        pFeature = pFeatureCursor.NextFeature();
                    }
                    myWatch.Stop();
                    //     long myUseTime = myWatch.ElapsedMilliseconds;
                    //     return new Result(true, (myUseTime / 1000).ToString() + "s," + myDictionary.Count.ToString());
                }
                catch (Exception ex1)
                {
                    return new Result(false, "222," + ex1.ToString());
                }
                //结果入库
                DataTable dt4 = DB.IbatisHelper.ExecuteQueryForDataTable("GetFishnetRange", null);  // Ibatis 数据访问，得到目标区域范围
                string minx_text = dt4.Rows[0][0].ToString(),
                       miny_text = dt4.Rows[0][1].ToString(),
                       maxx_text = dt4.Rows[0][2].ToString(),
                       maxy_text = dt4.Rows[0][3].ToString(),
                       gridsize_text = dt4.Rows[0][4].ToString();
                double minx = double.Parse(minx_text);
                double miny = double.Parse(miny_text);
                double maxx = double.Parse(maxx_text);
                double maxy = double.Parse(maxy_text);
                double cellsize = double.Parse(gridsize_text);
                int rownumber = (int)((maxy - miny) / cellsize), columnnumber = (int)((maxx - minx) / cellsize);//rownumber=xmax+1, columnnumber=ymax+1;
                int xmax = rownumber - 1, ymax = columnnumber - 1;
                try
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("x", Type.GetType("System.Int32"));
                    dt.Columns.Add("y", Type.GetType("System.Int32"));
                    dt.Columns.Add("area", Type.GetType("System.Double"));
                    foreach (var item in myDictionary.Keys)
                    {
                        dt.Rows.Add(new object[] { (item % (ymax + 1)).ToString(), (item / (ymax + 1)).ToString(), Math.Round(myDictionary[item] / (cellsize * cellsize), 2).ToString() });
                        if (dt.Rows.Count > 5000)
                        {
                            using (SqlBulkCopy bcp = new SqlBulkCopy(DataUtil.ConnectionString))
                            {
                                bcp.BatchSize = dt.Rows.Count;
                                bcp.BulkCopyTimeout = 1000;
                                bcp.DestinationTableName = "tbAccelerateGridSceneTmpGrass";
                                bcp.WriteToServer(dt);
                                bcp.Close();
                            }
                            dt.Clear();
                        }
                    }
                    using (SqlBulkCopy bcp = new SqlBulkCopy(DataUtil.ConnectionString))
                    {
                        bcp.BatchSize = dt.Rows.Count;
                        bcp.BulkCopyTimeout = 1000;
                        bcp.DestinationTableName = "tbAccelerateGridSceneTmpGrass";
                        bcp.WriteToServer(dt);
                        bcp.Close();
                    }
                    dt.Clear();
                    IbatisHelper.ExecuteUpdate("UpdatetbAccelerateGridGrassByTmp", null);
                    IbatisHelper.ExecuteDelete("DeletetbAccelerateGridSceneTmpGrass", null);
                    return new Result(true, "草地叠加成功");
                }
                catch (Exception ex2)
                {
                    return new Result(false, "333," + ex2.ToString());
                }
            }
            catch (System.Data.SqlClient.SqlException err)
            {
                if (err.Message.IndexOf("连接超时") != -1)
                {
                    return new Result(false, "连接超时");
                }
                else if (err.Message.IndexOf("侦听") != -1)
                {
                    return new Result(false, "侦听");
                }
                else
                {
                    return new Result(false, err.ToString());
                }
            }
            catch (System.Exception err)
            {
                return new Result(false, err.ToString());
            }
        }

        public Result overlaywater()
        {
            try
            {
                //叠加分析
                DataTable dt1 = DB.IbatisHelper.ExecuteQueryForDataTable("GetFishnetPosition", null);  // Ibatis 数据访问,得到渔网图层文件位置
                string fishnetpath = dt1.Rows[0][0].ToString();
                DataTable dt2 = DB.IbatisHelper.ExecuteQueryForDataTable("GetWaterPosition", null);  // Ibatis 数据访问,得到建筑物图层文件位置
                string waterpath = dt2.Rows[0][0].ToString();
                DataTable dt3 = DB.IbatisHelper.ExecuteQueryForDataTable("GetWaterOverlayPosition", null);  // Ibatis 数据访问,得到建筑物叠加结果图层文件位置
                string wateroverlaypath = dt3.Rows[0][0].ToString();
                string out_feature = wateroverlaypath;
                Geoprocessor gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                ESRI.ArcGIS.AnalysisTools.Intersect intersect = new ESRI.ArcGIS.AnalysisTools.Intersect();
                intersect.in_features = waterpath + ";" + fishnetpath;
                intersect.out_feature_class = out_feature;
                intersect.join_attributes = "ALL";
                try
                {
                    gp.Execute(intersect, null);
                }
                catch (Exception ex)
                {
                    string info = "";
                    for (int i = 0; i < gp.MessageCount; i++)
                    {
                        info = info + gp.GetMessage(i);
                    }
                    return new Result(false, "111" + info);
                }
                Dictionary<int, double> myDictionary = new Dictionary<int, double>();
                //计算面积
                try
                {
                    ESRI.ArcGIS.esriSystem.IAoInitialize ao = new ESRI.ArcGIS.esriSystem.AoInitialize();
                    ao.Initialize(ESRI.ArcGIS.esriSystem.esriLicenseProductCode.esriLicenseProductCodeStandard);
                    IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
                    IWorkspaceFactoryLockControl pWorkspaceFactoryLockControl = pWorkspaceFactory as IWorkspaceFactoryLockControl;
                    if (pWorkspaceFactoryLockControl.SchemaLockingEnabled)
                    {
                        pWorkspaceFactoryLockControl.DisableSchemaLocking();
                    }
                    IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(out_feature), 0);
                    IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
                    IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(out_feature));
                    //    IFeatureClass pFeatureClass = GetFeatureClass(out_feature);
                    string fileName = System.IO.Path.GetFileName(fishnetpath);
                    string[] a = fileName.Split('.');
                    string fieldname = "FID_" + a[0];
                    int fieldindex = pFeatureClass.Fields.FindField(fieldname);//索引
                    IFeatureCursor pFeatureCursor;
                    pFeatureCursor = pFeatureClass.Search(null, false);
                    int areaindex = pFeatureClass.Fields.FindField("Shape_Area");//索引
                    IFeature pFeature;
                    pFeature = pFeatureCursor.NextFeature();
                    Stopwatch myWatch = new Stopwatch();
                    myWatch.Start();
                    //      Dictionary<int, double> myDictionary = new Dictionary<int, double>();
                    while (pFeature != null)
                    {
                        int gridID = Convert.ToInt32(pFeature.get_Value(fieldindex));
                        IArea parea = pFeature.Shape as IArea;
                        double area = parea.Area;
                        area = Math.Round(area, 2);
                        if (myDictionary.ContainsKey(gridID) == false)
                        {
                            myDictionary.Add(gridID, area);
                        }
                        else
                        {
                            myDictionary[gridID] = myDictionary[gridID] + area;
                        }
                        pFeature = pFeatureCursor.NextFeature();
                    }
                    myWatch.Stop();
                    //     long myUseTime = myWatch.ElapsedMilliseconds;
                    //     return new Result(true, (myUseTime / 1000).ToString() + "s," + myDictionary.Count.ToString());
                }
                catch (Exception ex1)
                {
                    return new Result(false, "222," + ex1.ToString());
                }
                //结果入库
                DataTable dt4 = DB.IbatisHelper.ExecuteQueryForDataTable("GetFishnetRange", null);  // Ibatis 数据访问，得到目标区域范围
                string minx_text = dt4.Rows[0][0].ToString(),
                       miny_text = dt4.Rows[0][1].ToString(),
                       maxx_text = dt4.Rows[0][2].ToString(),
                       maxy_text = dt4.Rows[0][3].ToString(),
                       gridsize_text = dt4.Rows[0][4].ToString();
                double minx = double.Parse(minx_text);
                double miny = double.Parse(miny_text);
                double maxx = double.Parse(maxx_text);
                double maxy = double.Parse(maxy_text);
                double cellsize = double.Parse(gridsize_text);
                int rownumber = (int)((maxy - miny) / cellsize), columnnumber = (int)((maxx - minx) / cellsize);//rownumber=xmax+1, columnnumber=ymax+1;
                int xmax = rownumber - 1, ymax = columnnumber - 1;
                try
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("x", Type.GetType("System.Int32"));
                    dt.Columns.Add("y", Type.GetType("System.Int32"));
                    dt.Columns.Add("area", Type.GetType("System.Double"));
                    foreach (var item in myDictionary.Keys)
                    {
                        dt.Rows.Add(new object[] { (item % (ymax + 1)).ToString(), (item / (ymax + 1)).ToString(), Math.Round(myDictionary[item] / (cellsize * cellsize), 2).ToString() });
                        if (dt.Rows.Count > 5000)
                        {
                            using (SqlBulkCopy bcp = new SqlBulkCopy(DataUtil.ConnectionString))
                            {
                                bcp.BatchSize = dt.Rows.Count;
                                bcp.BulkCopyTimeout = 1000;
                                bcp.DestinationTableName = "tbAccelerateGridSceneTmpWater";
                                bcp.WriteToServer(dt);
                                bcp.Close();
                            }
                            dt.Clear();
                        }
                    }
                    using (SqlBulkCopy bcp = new SqlBulkCopy(DataUtil.ConnectionString))
                    {
                        bcp.BatchSize = dt.Rows.Count;
                        bcp.BulkCopyTimeout = 1000;
                        bcp.DestinationTableName = "tbAccelerateGridSceneTmpWater";
                        bcp.WriteToServer(dt);
                        bcp.Close();
                    }
                    dt.Clear();
                    IbatisHelper.ExecuteUpdate("UpdatetbAccelerateGridWaterByTmp", null);
                    IbatisHelper.ExecuteDelete("DeletetbAccelerateGridSceneTmpWater", null);
                    return new Result(true, "水面叠加成功");
                }
                catch (Exception ex2)
                {
                    return new Result(false, "333," + ex2.ToString());
                }
            }
            catch (System.Data.SqlClient.SqlException err)
            {
                if (err.Message.IndexOf("连接超时") != -1)
                {
                    return new Result(false, "连接超时");
                }
                else if (err.Message.IndexOf("侦听") != -1)
                {
                    return new Result(false, "侦听");
                }
                else
                {
                    return new Result(false, err.ToString());
                }
            }
            catch (System.Exception err)
            {
                return new Result(false, err.ToString());
            }
        }



        public Result refresh3DCover(string cellName)
        {
            CellInfo cellInfo = new CellInfo();
            cellInfo.SourceName = cellName;
            Utils.validate.validateCell(ref cellInfo);

            if (!AnalysisEntry.Display3DAnalysis(cellInfo))
            {
                return new Result(false, "请先进行小区覆盖计算");
            }

            return new Result(true, "立体覆盖刷新成功");
        }

        public Result refresh3DCoverLayer(int minXid, int minYid, int maxXid, int maxYid)
        {
            string areaRange = String.Format("{0}_{1}_{2}_{3}", minXid, minYid, maxXid, maxYid);
            OperateCoverGird3DLayer operateGrid = new OperateCoverGird3DLayer(LayerNames.AreaCoverGrid3Ds+areaRange);
            operateGrid.ClearLayer();
            if (!operateGrid.constuctAreaGrid3Ds(minXid, minYid, maxXid, maxYid))
                return new Result(false, "请先对区域内的小区进行覆盖计算");
            return new Result(true, "区域立体覆盖刷新成功");
        }

        public Result refreshBuildingLayer()
        {
            OperateBuildingLayer layer = new OperateBuildingLayer(LayerNames.Building);
            layer.ClearLayer();
            if (!layer.constuctBuilding())
                return new Result(false, "无建筑物数据");

            OperateBuildingLayer layer1 = new OperateBuildingLayer(LayerNames.Building1);
            layer1.ClearLayer();
            if (!layer1.constuctBuilding1())
                return new Result(false, "无建筑物数据");

            return new Result(true,"建筑物图层刷新成功");
        }

        public Result refreshBuildingSmoothLayer()
        {
            OperateSmoothBuildingLayer layer = new OperateSmoothBuildingLayer();
            layer.ClearLayer();
            if (!layer.constuctBuildingVertex())
                return new Result(false, "无建筑物数据");

            return new Result(true,"建筑物底边平滑图层刷新成功");
        }

        public Result RefreshCell()
        {
            LTE.GIS.OperateCellLayer cellLayer = new LTE.GIS.OperateCellLayer();
            if (!cellLayer.RefreshCellLayer())
                return new Result(false, "小区数据为空");
            return new Result(true, "小区图层刷新成功");
        }

        public Result refreshDefectLayer(int minXid, int minYid, int maxXid, int maxYid, DefectType type)
        {
            string areaRange = String.Format("{0}_{1}_{2}_{3}", minXid, minYid, maxXid, maxYid);
            string layerName = "";
            switch (type)
            {
                case DefectType.Weak:
                    layerName += LayerNames.Weak;
                    break;
                case DefectType.Excessive:
                    layerName += LayerNames.Excessive;
                    break;
                case DefectType.Overlapped:
                    layerName += LayerNames.Overlapped;
                    break;
                case DefectType.PCIconflict:
                    layerName += LayerNames.PCIconflict;
                    break;
                case DefectType.PCIconfusion:
                    layerName += LayerNames.PCIconfusion;
                    break;
                case DefectType.PCImod3:
                    layerName += LayerNames.PCImod3;
                    break;
                default:
                    break;
            }
            OperateDefectLayer operateGrid3d = new OperateDefectLayer(layerName+areaRange);
            operateGrid3d.ClearLayer();
            if (!operateGrid3d.constuctGrid3Ds(minXid, minYid, maxXid, maxYid, (short)type))
                return new Result(false, "数据为空");
            return new Result(true,"网内干扰刷新成功");
        }

        public Result refreshDTLayer(string bts, int dis, double minx, double miny, double maxx, double maxy)
        {
            OperateDTLayer layer = new OperateDTLayer();
            layer.ClearLayer();
            if (!layer.constuctDTGrids())
                return new Result(false, "路测数据不存在");
            return new Result(true, "路测图层刷新成功");
        }

        public Result refreshGroundCover(string cellName)
        {
            //new Test().getBuilding();
            CellInfo cellInfo = new CellInfo();
            cellInfo.SourceName = cellName;
            Utils.validate.validateCell(ref cellInfo);
            string layerName = "小区" + cellInfo.CI + "地面覆盖";
            if (!AnalysisEntry.DisplayAnalysis(cellInfo,layerName))
            {
                return new Result(false, "请先进行小区覆盖计算");
            }
            return new Result {Ok=true,Msg="刷新成功",ShpName=layerName};
        }

        public Result refreshGroundCoverLayer(int minXid, int minYid, int maxXid, int maxYid)
        {
            string areaRange = String.Format("{0}_{1}_{2}_{3}", minXid, minYid, maxXid, maxYid);
            OperateCoverGirdLayer operateGrid = new OperateCoverGirdLayer(LayerNames.AreaCoverGrids + areaRange);
            operateGrid.ClearLayer();
            if (!operateGrid.constuctAreaGrids(minXid, minYid, maxXid, maxYid))
                return new Result(false, "请先对区域内的小区进行覆盖计算");
            return new Result(true,"区域覆盖刷新成功");
        }

        public Result refreshInfLayer()
        {
            OperateInterferenceLocLayer layer = new OperateInterferenceLocLayer();
            layer.ClearLayer();
            if (!layer.constuctGrid3Ds())
                return new Result(false, "无干扰源");
            return new Result(true,"网外干扰刷新成功");
        }

        public Result refreshTINLayer()
        {
            OperateTINLayer layer = new OperateTINLayer(LayerNames.TIN);
            layer.ClearLayer();
            if (!layer.constuctTIN())
                return new Result(false, "无TIN");

            OperateTINLayer layer1 = new OperateTINLayer(LayerNames.TIN1);
            layer1.ClearLayer();
            if (!layer1.constuctTIN())
                return new Result(false, "无TIN");

            return new Result(true,"Tin图层刷新成功");
        }
    }
}
