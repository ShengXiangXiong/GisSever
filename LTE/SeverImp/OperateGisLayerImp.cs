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
using System.Collections;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Xml;
using System.Diagnostics;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.IO;
using LTE.Model;
using System.Threading;
using LTE.InternalInterference.Grid;

namespace LTE.SeverImp
{
    class  OperateGisLayerImp : OpreateGisLayer.Iface
    {
        public Result cluster()
        {
            ESRI.ArcGIS.esriSystem.IAoInitialize ao = new ESRI.ArcGIS.esriSystem.AoInitialize();
            ao.Initialize(ESRI.ArcGIS.esriSystem.esriLicenseProductCode.esriLicenseProductCodeStandard);
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
            DataTable dt3;//查询场景信息
            Dictionary<int, int> myDictionary = new Dictionary<int, int>();
            int a, b;
            Hashtable ht = new Hashtable();//分行读取
            int i = -1, pagesize, gridID = -1;
            int ci = 0;
            if (ymax > 8000)
            { pagesize = 1; }
            else
            { pagesize = 8000 / ymax; }
            a = 0; b = pagesize - 1;
            try
            {
                while (a <= xmax)
                {
                    ht["minGYID"] = a;
                    ht["maxGYID"] = b;
                    dt3 = DB.IbatisHelper.ExecuteQueryForDataTable("GetClusterResult", ht);
                    for (i = 0; i < dt3.Rows.Count; i++)
                    {
                        gridID = (Convert.ToInt32(dt3.Rows[i][1].ToString())) * (ymax + 1) + (Convert.ToInt32(dt3.Rows[i][0].ToString()));
                        //  myDictionary[gridID] = Convert.ToInt32(dt3.Rows[i][2].ToString());
                        try
                        {
                            myDictionary.Add(gridID, (Convert.ToInt32(dt3.Rows[i][2].ToString())));
                        }
                        catch (Exception ex1)
                        {
                            return new Result(false, ex1.ToString());
                            //return new Result(false, a.ToString()+","+b.ToString() + "," + ci.ToString() + "," + i.ToString() +","+ gridID.ToString()+","+myDictionary.Count.ToString());
                        }
                    }
                    dt3.Clear();
                    a = a + pagesize;
                    b = b + pagesize;
                    ci++;
                }
            }
            catch (Exception ex)
            {
                return new Result(false, ex.ToString());
            }
            //添加场景字段
            geoprocessor = new Geoprocessor();
            geoprocessor.OverwriteOutput = true;
            ESRI.ArcGIS.DataManagementTools.AddField addField = new ESRI.ArcGIS.DataManagementTools.AddField();
            addField.in_table = filepath;//ITable类型参数
            addField.field_name = "scene";
            addField.field_type = "SHORT";//short(-32768,32767)
            geoprocessor.Execute(addField, null);
            string pFolder = System.IO.Path.GetDirectoryName(filepath);
            string pFileName = System.IO.Path.GetFileName(filepath);
            IWorkspaceFactory pWorkspacefactory = new ShapefileWorkspaceFactory();
            IWorkspaceFactoryLockControl pWorkspaceFactoryLockControl = pWorkspacefactory as IWorkspaceFactoryLockControl;
            if (pWorkspaceFactoryLockControl.SchemaLockingEnabled)
            {
                pWorkspaceFactoryLockControl.DisableSchemaLocking();
            }
            IWorkspace pWorkspace = pWorkspacefactory.OpenFromFile(pFolder, 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            IFeatureClass pFC = pFeatureWorkspace.OpenFeatureClass(pFileName);
            ITable pTable = pFC as ITable;
            ICursor pCursor = pTable.Update(null, false);
            IRow pRow = pCursor.NextRow();
            int filedindex = pFC.Fields.FindField("scene");
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
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspacefactory);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactoryLockControl);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureWorkspace);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFC);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pTable);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
            DataTable dt4;//查询场景所属的簇序号
            myDictionary.Clear();
            ht.Clear();//分行读取
            i = -1; gridID = -1; ci = 0;
            if (ymax > 8000)
            { pagesize = 1; }
            else
            { pagesize = 8000 / ymax; }
            a = 0; b = pagesize - 1;
            try
            {
                while (a <= xmax)
                {
                    ht["minGYID"] = a;
                    ht["maxGYID"] = b;
                    dt4 = DB.IbatisHelper.ExecuteQueryForDataTable("GetClusterResultNumber", ht);
                    for (i = 0; i < dt4.Rows.Count; i++)
                    {
                        gridID = (Convert.ToInt32(dt4.Rows[i][1].ToString())) * (ymax + 1) + (Convert.ToInt32(dt4.Rows[i][0].ToString()));
                        try
                        {
                            myDictionary.Add(gridID, (Convert.ToInt32(dt4.Rows[i][2].ToString())));
                        }
                        catch (Exception ex1)
                        {
                            return new Result(false, ex1.ToString());
                        }
                    }
                    dt4.Clear();
                    a = a + pagesize;
                    b = b + pagesize;
                    ci++;
                }
            }
            catch (Exception ex)
            {
                return new Result(false, ex.ToString());
            }
            //添加簇序号字段
            geoprocessor = new Geoprocessor();
            geoprocessor.OverwriteOutput = true;
            addField = new ESRI.ArcGIS.DataManagementTools.AddField();
            addField.in_table = filepath;//ITable类型参数
            addField.field_name = "clusterid";
            addField.field_type = "SHORT";//short（-32768，32767）
            geoprocessor.Execute(addField, null);
            pFolder = System.IO.Path.GetDirectoryName(filepath);
            pFileName = System.IO.Path.GetFileName(filepath);
            IWorkspaceFactory pWorkspacefactory1 = new ShapefileWorkspaceFactory();
            IWorkspaceFactoryLockControl pWorkspaceFactoryLockControl1 = pWorkspacefactory1 as IWorkspaceFactoryLockControl;
            if (pWorkspaceFactoryLockControl1.SchemaLockingEnabled)
            {
                pWorkspaceFactoryLockControl1.DisableSchemaLocking();
            }
            IWorkspace pWorkspace1 = pWorkspacefactory1.OpenFromFile(pFolder, 0);
            IFeatureWorkspace pFeatureWorkspace1 = pWorkspace1 as IFeatureWorkspace;
            IFeatureClass pFC1 = pFeatureWorkspace1.OpenFeatureClass(pFileName);
            ITable pTable1 = pFC1 as ITable;
            ICursor pCursor1 = pTable1.Update(null, false);
            IRow pRow1 = pCursor1.NextRow();
            filedindex = pFC1.Fields.FindField("clusterid");
            while (pRow1 != null)
            {
                try
                {
                    pType = myDictionary[Convert.ToInt32(pRow1.get_Value(0))];
                    pRow1.set_Value(filedindex, pType);
                    pCursor1.UpdateRow(pRow1);
                    pRow1 = pCursor1.NextRow();
                }
                catch (Exception ee)
                { return new Result(false, ee.ToString()); }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspacefactory1);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactoryLockControl1);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace1);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureWorkspace1);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFC1);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pTable1);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor1);
            myDictionary.Clear();
            DataTable dt5;//查询经度信息
            Dictionary<int, double> myDictionary1 = new Dictionary<int, double>();
            ht.Clear();//分行读取
            i = -1; gridID = -1;
            ci = 0;
            if (ymax > 8000)
            { pagesize = 1; }
            else
            { pagesize = 8000 / ymax; }
            a = 0; b = pagesize - 1;
            try
            {
                while (a <= xmax)
                {
                    ht["minGYID"] = a;
                    ht["maxGYID"] = b;
                    dt5 = DB.IbatisHelper.ExecuteQueryForDataTable("GetGridLongitude", ht);
                    for (i = 0; i < dt5.Rows.Count; i++)
                    {
                        gridID = (Convert.ToInt32(dt5.Rows[i][1].ToString())) * (ymax + 1) + (Convert.ToInt32(dt5.Rows[i][0].ToString()));
                        try
                        {
                            myDictionary1.Add(gridID, (Convert.ToDouble(dt5.Rows[i][2].ToString())));
                        }
                        catch (Exception ex1)
                        {
                            return new Result(false, ex1.ToString());
                        }
                    }
                    dt5.Clear();
                    a = a + pagesize;
                    b = b + pagesize;
                    ci++;
                }
            }
            catch (Exception ex)
            {
                return new Result(false, ex.ToString());
            }
            //添加经度字段
            geoprocessor = new Geoprocessor();
            geoprocessor.OverwriteOutput = true;
            addField = new ESRI.ArcGIS.DataManagementTools.AddField();
            addField.in_table = filepath;//ITable类型参数
            addField.field_name = "longitude";
            addField.field_type = "DOUBLE";
            geoprocessor.Execute(addField, null);
            pFolder = System.IO.Path.GetDirectoryName(filepath);
            pFileName = System.IO.Path.GetFileName(filepath);
            IWorkspaceFactory pWorkspacefactory2 = new ShapefileWorkspaceFactory();
            IWorkspaceFactoryLockControl pWorkspaceFactoryLockControl2 = pWorkspacefactory2 as IWorkspaceFactoryLockControl;
            if (pWorkspaceFactoryLockControl2.SchemaLockingEnabled)
            {
                pWorkspaceFactoryLockControl2.DisableSchemaLocking();
            }
            IWorkspace pWorkspace2 = pWorkspacefactory2.OpenFromFile(pFolder, 0);
            IFeatureWorkspace pFeatureWorkspace2 = pWorkspace2 as IFeatureWorkspace;
            IFeatureClass pFC2 = pFeatureWorkspace2.OpenFeatureClass(pFileName);
            ITable pTable2 = pFC2 as ITable;
            ICursor pCursor2 = pTable2.Update(null, false);
            IRow pRow2 = pCursor2.NextRow();
            filedindex = pFC2.Fields.FindField("longitude");
            double pType1;
            while (pRow2 != null)
            {
                try
                {
                    pType1 = myDictionary1[Convert.ToInt32(pRow2.get_Value(0))];
                    pRow2.set_Value(filedindex, pType1);
                    pCursor2.UpdateRow(pRow2);
                    pRow2 = pCursor2.NextRow();
                }
                catch (Exception ee)
                { return new Result(false, ee.ToString()); }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspacefactory2);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactoryLockControl2);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace2);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureWorkspace2);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFC2);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pTable2);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor2);
            DataTable dt6;//查询纬度信息
            myDictionary1.Clear();
            ht.Clear();//分行读取
            i = -1; gridID = -1;
            ci = 0;
            if (ymax > 8000)
            { pagesize = 1; }
            else
            { pagesize = 8000 / ymax; }
            a = 0; b = pagesize - 1;
            try
            {
                while (a <= xmax)
                {
                    ht["minGYID"] = a;
                    ht["maxGYID"] = b;
                    dt6 = DB.IbatisHelper.ExecuteQueryForDataTable("GetGridLatitude", ht);
                    for (i = 0; i < dt6.Rows.Count; i++)
                    {
                        gridID = (Convert.ToInt32(dt6.Rows[i][1].ToString())) * (ymax + 1) + (Convert.ToInt32(dt6.Rows[i][0].ToString()));
                        try
                        {
                            myDictionary1.Add(gridID, (Convert.ToDouble(dt6.Rows[i][2].ToString())));
                        }
                        catch (Exception ex1)
                        {
                            return new Result(false, ex1.ToString());
                        }
                    }
                    dt6.Clear();
                    a = a + pagesize;
                    b = b + pagesize;
                    ci++;
                }
            }
            catch (Exception ex)
            {
                return new Result(false, ex.ToString());
            }
            //添加纬度字段
            geoprocessor = new Geoprocessor();
            geoprocessor.OverwriteOutput = true;
            addField = new ESRI.ArcGIS.DataManagementTools.AddField();
            addField.in_table = filepath;//ITable类型参数
            addField.field_name = "latitude";
            addField.field_type = "DOUBLE";
            geoprocessor.Execute(addField, null);
            pFolder = System.IO.Path.GetDirectoryName(filepath);
            pFileName = System.IO.Path.GetFileName(filepath);
            IWorkspaceFactory pWorkspacefactory3 = new ShapefileWorkspaceFactory();
            IWorkspaceFactoryLockControl pWorkspaceFactoryLockControl3 = pWorkspacefactory3 as IWorkspaceFactoryLockControl;
            if (pWorkspaceFactoryLockControl3.SchemaLockingEnabled)
            {
                pWorkspaceFactoryLockControl3.DisableSchemaLocking();
            }
            IWorkspace pWorkspace3 = pWorkspacefactory3.OpenFromFile(pFolder, 0);
            IFeatureWorkspace pFeatureWorkspace3 = pWorkspace3 as IFeatureWorkspace;
            IFeatureClass pFC3 = pFeatureWorkspace3.OpenFeatureClass(pFileName);
            ITable pTable3 = pFC3 as ITable;
            ICursor pCursor3 = pTable3.Update(null, false);
            IRow pRow3 = pCursor3.NextRow();
            filedindex = pFC3.Fields.FindField("latitude");
            while (pRow3 != null)
            {
                try
                {
                    pType1 = myDictionary1[Convert.ToInt32(pRow3.get_Value(0))];
                    pRow3.set_Value(filedindex, pType1);
                    pCursor3.UpdateRow(pRow3);
                    pRow3 = pCursor3.NextRow();
                }
                catch (Exception ee)
                { return new Result(false, ee.ToString()); }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspacefactory3);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactoryLockControl3);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace3);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureWorkspace3);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFC3);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pTable3);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor3);
            try//添加坐标系.prj文件
            {
                string path = @"D:\test10.8\building.prj";//后期需要更改
                string fileName = System.IO.Path.GetFileName(filepath);
                string[] a1 = fileName.Split('.');
                string name = a1[0];
                string b1 = "\\" + name + ".shp";
                filepath = filepath.Replace(b1, "");
                string path2 = filepath + "\\" + name + ".prj";
                File.Copy(path, path2, true);//允许覆盖目的地的同名文件
            }
            catch (Exception ex)
            {
                return new Result(false, ex.ToString());
            }
            IbatisHelper.ExecuteUpdate("UpdatetbDependTableClusterShp", null);
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
                       gridsize_text = dt2.Rows[0][4].ToString(),
                       longitude1_text = dt2.Rows[0][5].ToString(),
                       latitude1_text = dt2.Rows[0][6].ToString(),
                       longitude2_text = dt2.Rows[0][7].ToString(),
                       latitude2_text = dt2.Rows[0][8].ToString();
                double minx = double.Parse(minx_text);
                double miny = double.Parse(miny_text);
                double maxx = double.Parse(maxx_text);
                double maxy = double.Parse(maxy_text);
                double cellsize = double.Parse(gridsize_text);
                double longitude1 = double.Parse(longitude1_text);
                double latitude1 = double.Parse(latitude1_text);
                double longitude2 = double.Parse(longitude2_text);
                double latitude2 = double.Parse(latitude2_text);
                int ilength = (int)((maxy - miny) / cellsize), jlength = (int)((maxx - minx) / cellsize);
                int xmax = ilength - 1, ymax = jlength - 1;//xmax是i的上界,ymax是j的上界
                double k1 = (longitude2 - longitude1) / (double)(jlength);
                double k2 = (latitude2 - latitude1) / (double)(ilength);
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
                    //数据库网格
                    DataTable dt = new DataTable();//入库
                    dt.Columns.Add("x", Type.GetType("System.Int32"));
                    dt.Columns.Add("y", Type.GetType("System.Int32"));
                    dt.Columns.Add("z", Type.GetType("System.Int32"));
                    dt.Columns.Add("CenterLong", Type.GetType("System.Double"));
                    dt.Columns.Add("CenterLati", Type.GetType("System.Double"));
                    int rownumber = (int)((maxy - miny) / cellsize), columnnumber = (int)((maxx - minx) / cellsize);//rownumber=xmax+1, columnnumber=ymax+1;
                    for (int i = 0; i < rownumber; i++)//GYID
                    {
                        for (int j = 0; j < columnnumber; j++)//GXID
                        {
                            dt.Rows.Add(new object[] { j.ToString(), i.ToString(), "1", (longitude1 + j * k1 + k1 / 2.0).ToString(), (latitude1 + i * k2 + k2 / 2.0).ToString() });
                            dt.Rows.Add(new object[] { j.ToString(), i.ToString(), "2", (longitude1 + j * k1 + k1 / 2.0).ToString(), (latitude1 + i * k2 + k2 / 2.0).ToString() });
                            dt.Rows.Add(new object[] { j.ToString(), i.ToString(), "3", (longitude1 + j * k1 + k1 / 2.0).ToString(), (latitude1 + i * k2 + k2 / 2.0).ToString() });
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
                }
                catch (Exception ex)
                { return new Result(false, ex.ToString()); }

                try
                {
                    geoprocessor.Execute(CF, null);
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
                return new Result(true, "渔网生成结束");
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
            ESRI.ArcGIS.esriSystem.IAoInitialize ao = new ESRI.ArcGIS.esriSystem.AoInitialize();
            ao.Initialize(ESRI.ArcGIS.esriSystem.esriLicenseProductCode.esriLicenseProductCodeStandard);
            try
            {
                DataTable dt1 = DB.IbatisHelper.ExecuteQueryForDataTable("GetFishnetPosition", null);  // Ibatis 数据访问,得到渔网图层文件位置
                string fishnetpath = dt1.Rows[0][0].ToString();
                DataTable dt2 = DB.IbatisHelper.ExecuteQueryForDataTable("GetBuildingPosition", null);  // Ibatis 数据访问,得到建筑物图层文件位置
                string buildingpath = dt2.Rows[0][0].ToString();
                DataTable dt3 = DB.IbatisHelper.ExecuteQueryForDataTable("GetBuildingOverlayPosition", null);  // Ibatis 数据访问,得到建筑物叠加结果图层文件位置
                string buildingoverlaypath = dt3.Rows[0][0].ToString();
                string out_feature = buildingoverlaypath;
                try
                {
                    //叠加分析
                    Geoprocessor gp = new Geoprocessor();
                    gp.OverwriteOutput = true;
                    ESRI.ArcGIS.AnalysisTools.Intersect intersect = new ESRI.ArcGIS.AnalysisTools.Intersect();
                    intersect.in_features = buildingpath + ";" + fishnetpath;
                    intersect.out_feature_class = out_feature;
                    intersect.join_attributes = "ALL";
                    IGPProcess gPProcess1 = intersect;
                    gp.Execute(gPProcess1, null);

                }
                catch (Exception ex)
                {
                    return new Result(false, "111" + ex.ToString());
                }
                Dictionary<int, double> myDictionary = new Dictionary<int, double>();
                //计算面积
                try
                {
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
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactory);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactoryLockControl);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureWorkspace);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureClass);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
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
                        dt.Rows.Add(new object[] { (item % (ymax + 1)).ToString(), (item / (ymax + 1)).ToString(), Math.Round(myDictionary[item] / (cellsize * cellsize), 4).ToString() });
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
            ESRI.ArcGIS.esriSystem.IAoInitialize ao = new ESRI.ArcGIS.esriSystem.AoInitialize();
            ao.Initialize(ESRI.ArcGIS.esriSystem.esriLicenseProductCode.esriLicenseProductCodeStandard);
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
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactory);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactoryLockControl);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureWorkspace);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureClass);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
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
                        dt.Rows.Add(new object[] { (item % (ymax + 1)).ToString(), (item / (ymax + 1)).ToString(), Math.Round(myDictionary[item] / (cellsize * cellsize), 4).ToString() });
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
            ESRI.ArcGIS.esriSystem.IAoInitialize ao = new ESRI.ArcGIS.esriSystem.AoInitialize();
            ao.Initialize(ESRI.ArcGIS.esriSystem.esriLicenseProductCode.esriLicenseProductCodeStandard);
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
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactory);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactoryLockControl);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureWorkspace);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureClass);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
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
                        dt.Rows.Add(new object[] { (item % (ymax + 1)).ToString(), (item / (ymax + 1)).ToString(), Math.Round(myDictionary[item] / (cellsize * cellsize), 4).ToString() });
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
            string layerName = "小区" + cellInfo.SourceName + "立体覆盖.shp";
            if (!AnalysisEntry.Display3DAnalysis(cellInfo))
            {
                return new Result(false, "请先进行小区覆盖计算");
            }
            return new Result { Ok = true, Msg = "刷新成功", ShpName = layerName };
        }

        public Result refresh3DCoverLayer(int minXid, int minYid, int maxXid, int maxYid)
        {
            //图层切片 .xsx
            int shpSize = 50;
            for (int i = minXid; i <= maxXid; )
            {
                for (int j = minYid; j <= maxYid; )
                {
                    string areaRange = String.Format("{0}_{1}_{2}_{3}", i, j, i + shpSize, j + shpSize);
                    string layerName = LayerNames.AreaCoverGrid3Ds + areaRange + ".shp";
                    OperateCoverGird3DLayer operateGrid = new OperateCoverGird3DLayer(layerName);
                    operateGrid.ClearLayer();
                    if (!operateGrid.constuctAreaGrid3Ds(i, j, i + shpSize, j + shpSize))
                        return new Result(false, "请先对区域内的小区进行覆盖计算");

                    LTE.Geometric.Point MinP = GridHelper.getInstance().GridToLeftDownGeo(i, j);
                    LTE.Geometric.Point MaxP = GridHelper.getInstance().GridToRightUpGeo(i+shpSize, j+shpSize);
                    Hashtable ht = new Hashtable();
                    ht["MinLongitude"] = MinP.X;
                    ht["MinLatitude"] = MinP.Y;
                    ht["MaxLongitude"] = MaxP.X;
                    ht["MaxLatitude"] = MaxP.Y;
                    ht["ShpName"] = layerName;
                    ht["Type"] = "Area3DCover";
                    ht["DateTime"] = DateTime.Now;
                    IbatisHelper.ExecuteInsert("insAreaShp", ht);

                    j = Math.Min(maxYid, j + shpSize);
                }
                i = Math.Min(maxXid, i + shpSize);
            }

            //string areaRange = String.Format("{0}_{1}_{2}_{3}", minXid, minYid, maxXid, maxYid);
            //string layerName = LayerNames.AreaCoverGrid3Ds + areaRange+".shp";
            //OperateCoverGird3DLayer operateGrid = new OperateCoverGird3DLayer(layerName);
            //operateGrid.ClearLayer();
            //if (!operateGrid.constuctAreaGrid3Ds(minXid, minYid, maxXid, maxYid))
            //    return new Result(false, "请先对区域内的小区进行覆盖计算");
            return new Result { Ok = true, Msg = "刷新成功" };
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
            string layerName = "基站图层.shp";
            if (!cellLayer.RefreshCellLayer(layerName))
                return new Result(false, "小区数据为空");
            return new Result { Ok = true, Msg = "刷新成功", ShpName = layerName };
        }

        public Result refreshDefectLayer(int minXid, int minYid, int maxXid, int maxYid, DefectType type)
        {
            //图层切片 .xsx
            int shpSize = 50;
            for (int i = minXid; i <= maxXid;)
            {
                for (int j = minYid; j <= maxYid;)
                {
                    string layerName = String.Format("{0}_{1}_{2}_{3}", i, j, i + shpSize, j + shpSize);
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
                    layerName += ".shp";

                    OperateDefectLayer operateGrid3d = new OperateDefectLayer(layerName);
                    operateGrid3d.ClearLayer();
                    if (!operateGrid3d.constuctGrid3Ds(i, j, i + shpSize, j + shpSize, (short)type))
                        return new Result(false, "请先对区域进行网内干扰分析计算");

                    LTE.Geometric.Point MinP = GridHelper.getInstance().GridToLeftDownGeo(i, j);
                    LTE.Geometric.Point MaxP = GridHelper.getInstance().GridToRightUpGeo(i + shpSize, j + shpSize);
                    Hashtable ht = new Hashtable();
                    ht["MinLongitude"] = MinP.X;
                    ht["MinLatitude"] = MinP.Y;
                    ht["MaxLongitude"] = MaxP.X;
                    ht["MaxLatitude"] = MaxP.Y;
                    ht["ShpName"] = layerName;
                    ht["Type"] = type;
                    ht["DateTime"] = DateTime.Now;
                    IbatisHelper.ExecuteInsert("insAreaShp", ht);

                    j = Math.Min(maxYid, j + shpSize);
                }
                i = Math.Min(maxXid, i + shpSize);
            }

            //string layerName = String.Format("{0}_{1}_{2}_{3}", minXid, minYid, maxXid, maxYid);
            //switch (type)
            //{
            //    case DefectType.Weak:
            //        layerName += LayerNames.Weak;
            //        break;
            //    case DefectType.Excessive:
            //        layerName += LayerNames.Excessive;
            //        break;
            //    case DefectType.Overlapped:
            //        layerName += LayerNames.Overlapped;
            //        break;
            //    case DefectType.PCIconflict:
            //        layerName += LayerNames.PCIconflict;
            //        break;
            //    case DefectType.PCIconfusion:
            //        layerName += LayerNames.PCIconfusion;
            //        break;
            //    case DefectType.PCImod3:
            //        layerName += LayerNames.PCImod3;
            //        break;
            //    default:
            //        break;
            //}
            //layerName += ".shp";
            //OperateDefectLayer operateGrid3d = new OperateDefectLayer(layerName);
            //operateGrid3d.ClearLayer();
            //if (!operateGrid3d.constuctGrid3Ds(minXid, minYid, maxXid, maxYid, (short)type))
            //    return new Result(false, "数据为空");
            return new Result { Ok = true, Msg = "刷新成功"};
        }

        public Result refreshDTLayer(string bts, int dis, double minx, double miny, double maxx, double maxy)
        {
            try
            {

                OperateDTLayer dtlayer = new OperateDTLayer("TD路测.shp");
                
                dtlayer.ClearLayer();
                if (dtlayer.constuctDTGrids(bts, dis, minx, miny, maxx, maxy))
                {
                    return new Result { Ok = true, Msg = "DT图层刷新成功", ShpName = "路测点" };
                }
                else
                {
                    return new Result(false, "无数据");
                }
            }
            catch (Exception ex)
            {
                return new Result(false, string.Format("出现异常：{0}", ex.ToString()));
            }
        }

        public Result refreshGroundCover(string cellName)
        {
            //new Test().getBuilding();
            CellInfo cellInfo = new CellInfo();
            cellInfo.SourceName = cellName;
            Utils.validate.validateCell(ref cellInfo);
            string layerName = "小区" + cellInfo.SourceName + "地面覆盖.shp";
            if (!AnalysisEntry.DisplayAnalysis(cellInfo,layerName))
            {
                return new Result(false, "请先进行小区覆盖计算");
            }
            return new Result {Ok=true,Msg="刷新成功",ShpName=layerName};
        }

        public Result refreshGroundCoverLayer(int minXid, int minYid, int maxXid, int maxYid)
        {
            //图层切片 .xsx
            int shpSize = 50;
            for (int i = minXid; i <= maxXid;)
            {
                for (int j = minYid; j <= maxYid;)
                {
                    string areaRange = String.Format("{0}_{1}_{2}_{3}", i, j, i + shpSize, j + shpSize);
                    string layerName = LayerNames.AreaCoverGrid3Ds + areaRange + ".shp";
                    OperateCoverGirdLayer operateGrid = new OperateCoverGirdLayer(layerName);
                    operateGrid.ClearLayer();
                    if (!operateGrid.constuctAreaGrids(i, j, i + shpSize, j + shpSize))
                        return new Result(false, "请先对区域内的小区进行覆盖计算");

                    LTE.Geometric.Point MinP = GridHelper.getInstance().GridToLeftDownGeo(i, j);
                    LTE.Geometric.Point MaxP = GridHelper.getInstance().GridToRightUpGeo(i + shpSize, j + shpSize);
                    Hashtable ht = new Hashtable();
                    ht["MinLongitude"] = MinP.X;
                    ht["MinLatitude"] = MinP.Y;
                    ht["MaxLongitude"] = MaxP.X;
                    ht["MaxLatitude"] = MaxP.Y;
                    ht["ShpName"] = layerName;
                    ht["Type"] = "AreaGroundCover";
                    ht["DateTime"] = DateTime.Now;
                    IbatisHelper.ExecuteInsert("insAreaShp", ht);

                    j = Math.Min(maxYid, j + shpSize);
                }
                i = Math.Min(maxXid, i + shpSize);
            }
            //string areaRange = String.Format("{0}_{1}_{2}_{3}", minXid, minYid, maxXid, maxYid);
            //string layerName = areaRange + LayerNames.AreaCoverGrids;
            //layerName += ".shp";
            //OperateCoverGirdLayer operateGrid = new OperateCoverGirdLayer(layerName);
            //operateGrid.ClearLayer();
            //if (!operateGrid.constuctAreaGrids(minXid, minYid, maxXid, maxYid))
            //    return new Result(false, "请先对区域内的小区进行覆盖计算");
            return new Result { Ok = true, Msg = "刷新成功" };
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
            string areaName = "南京";
            string layerName = areaName + LayerNames.TIN1 + ".shp";
            OperateTINLayer layer = new OperateTINLayer(layerName);
            layer.ClearLayer();
            if (!layer.constuctTIN())
                return new Result(false, "无TIN");

            //OperateTINLayer layer1 = new OperateTINLayer(layerName);
            //layer1.ClearLayer();
            //if (!layer1.constuctTIN())
            //    return new Result(false, "无TIN");

            //return new Result(true,"Tin图层刷新成功");
            return new Result { Ok = true, Msg = "刷新成功", ShpName = layerName };
        }

        public Result setLoadInfo(int userId, string taskName)
        {
            LoadInfo.UserId.Value = userId;
            LoadInfo.taskName.Value = taskName;
            return new Result(true, "任务进度设置成功");
        }
    }
}
