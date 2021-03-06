using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Analyst3D;
using System.IO;

namespace LTE.GIS
{
    class DefineLayer
    {
        internal static ILayer CreateLayer(string workspaceDirectory,string fileName,IFields pFields)
        {
            //Open the folder to contain the shapefile as a workspace
            IFeatureWorkspace pFWS;
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            pFWS = pWorkspaceFactory.OpenFromFile(workspaceDirectory, 0) as IFeatureWorkspace;

            //Create the shapefile
            IFeatureClass featureClass = pFWS.CreateFeatureClass(fileName, pFields, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            IFeatureLayer featurelayer = new FeatureLayerClass();
            featurelayer.Name = fileName;
            featurelayer.FeatureClass = featureClass;
            ILayer layer = featurelayer as ILayer;
            return layer;
        }

        internal static bool findLayer(string workSpaceDir,string layerName)
        {
            if (Directory.Exists(workSpaceDir))
            {
                string filename = workSpaceDir + "\\" + layerName;
                if (File.Exists(filename))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal static IFields CreateBaseStationFields(ISpatialReference spatialReference)
        {
            //Set up a simple fields collection
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;

            //Make the shape field 
            //it will need a geometry definition, with a spatial reference
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = "Shape";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;

            IGeometryDef pGeometryDef = new GeometryDef();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            pGeometryDefEdit.SpatialReference_2 = spatialReference;

            pFieldEdit.GeometryDef_2 = pGeometryDef;
            pFieldsEdit.AddField(pField);

            //Add OID field
            pField = new FieldClass();
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Length_2 = 30;
            pFieldEdit.Name_2 = "OID";
            //pFieldEdit.AliasName_2 = "AliasName";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            pFieldsEdit.AddField(pField);

            return pFields;
        }

    }
}
