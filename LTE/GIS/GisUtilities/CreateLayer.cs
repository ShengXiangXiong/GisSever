using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTE.GIS
{
    class CreateLayer
    {
        private string workspaceDirectory;
        private string fileName;
        public CreateLayer(string wd,string fn)
        {
            this.workspaceDirectory = wd;
            this.fileName = fn;
        }
        public ILayer GetLayer(string workspaceDirectory, string fileName, IFields pFields)
        {
            //Open the folder to contain the shapefile as a workspace
            IFeatureWorkspace pFWS;
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            pFWS = pWorkspaceFactory.OpenFromFile(workspaceDirectory, 0) as IFeatureWorkspace;

            //Create the shapefile
            IFeatureClass featureClass = pFWS.CreateFeatureClass(fileName, pFields, null, null, esriFeatureType.esriFTSimple, "Shape", null);
            IFeatureLayer featurelayer = new FeatureLayerClass();
            featurelayer.Name = fileName;
            featurelayer.FeatureClass = featureClass;
            ILayer layer = featurelayer as ILayer;
            return layer;
        }
        public void addFiled(string fieldName, esriFieldType esriFieldType,ref IFieldsEdit pFieldsEdit)
        {
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = fieldName;
            pFieldEdit.Type_2 = esriFieldType;
            pFieldEdit.Length_2 = 50;
            pFieldsEdit.AddField(pField);
        }
        public void Test()
        {
            //定义一个几何字段，类型为点类型
            ISpatialReferenceFactory2 originalSpatialReferenceFactory = new SpatialReferenceEnvironmentClass();
            ISpatialReference originalSpatialReference = originalSpatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            IGeometryDefEdit pGeoDef = new GeometryDefClass();
            IGeometryDefEdit pGeoDefEdit = pGeoDef as IGeometryDefEdit;
            pGeoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;

            pGeoDefEdit.SpatialReference_2 = originalSpatialReference;
            //定义一个字段集合对象
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;

            //定义shape字段
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "SHAPE";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFieldsEdit.AddField(pField);
            pFieldEdit.GeometryDef_2 = pGeoDef;

            //定义单个的字段，并添加到字段集合中
            pField = new FieldClass();
            pFieldEdit = (IFieldEdit) pField;
            pFieldEdit.Name_2 = "STCD";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            pFieldsEdit.AddField(pField);

            //定义单个的字段，并添加到字段集合中
            pField = new FieldClass();
            pFieldEdit = (IFieldEdit) pField;
            pFieldEdit.Name_2 = "SLM10";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            pFieldsEdit.AddField(pField);

            //定义单个的字段，并添加到字段集合中
            pField = new FieldClass();
            pFieldEdit = (IFieldEdit) pField;
            pFieldEdit.Name_2 = "SLM20";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            pFieldsEdit.AddField(pField);

            //定义单个的字段，并添加到字段集合中
            pField = new FieldClass();
            pFieldEdit = (IFieldEdit) pField;
            pFieldEdit.Name_2 = "SLM40";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            pFieldsEdit.AddField(pField);

            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IFeatureWorkspace pFWS = pWorkspaceFactory.OpenFromFile(workspaceDirectory, 0) as IFeatureWorkspace;
            IFeatureClass pFtClass = pFWS.CreateFeatureClass("Test", pFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", null);
        }
        public ILayer CreateCoverLayer()
        {
            //定义一个几何字段，类型为多边形类型
            ISpatialReferenceFactory2 originalSpatialReferenceFactory = new SpatialReferenceEnvironmentClass();
            //ISpatialReference originalSpatialReference1 = originalSpatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            ISpatialReference originalSpatialReference = originalSpatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_WGS1984UTM_50N);
            IGeometryDefEdit pGeoDef = new GeometryDefClass();
            IGeometryDefEdit pGeoDefEdit = pGeoDef as IGeometryDefEdit;
            pGeoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            pGeoDefEdit.SpatialReference_2 = originalSpatialReference;
            //定义一个字段集合对象
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;

            //单独处理shape字段
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "Shape";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFieldsEdit.AddField(pField);
            pFieldEdit.GeometryDef_2 = pGeoDef;
            //添加其它字段
            //addFiled("Shape", esriFieldType.esriFieldTypeGeometry, ref pFieldsEdit);
            addFiled("Id", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("GXID", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("GYID", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("eNodeB", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("Longitude", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("Latitude", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("CI", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("CellName", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("RecePower", esriFieldType.esriFieldTypeDouble, ref pFieldsEdit);
            addFiled("PathLoss", esriFieldType.esriFieldTypeDouble, ref pFieldsEdit);
            return GetLayer(workspaceDirectory, fileName, pFields);
        }
        public ILayer Create3DCoverLayer()
        {

            //定义一个几何字段，类型为多边形类型
            ISpatialReferenceFactory2 originalSpatialReferenceFactory = new SpatialReferenceEnvironmentClass();
            ISpatialReference originalSpatialReference = originalSpatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_WGS1984UTM_50N);
            IGeometryDefEdit pGeoDef = new GeometryDefClass();
            IGeometryDefEdit pGeoDefEdit = pGeoDef as IGeometryDefEdit;
            pGeoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            pGeoDefEdit.SpatialReference_2 = originalSpatialReference;
            
            //定义一个字段集合对象
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;

            //添加字段
            addFiled("Shape", esriFieldType.esriFieldTypeGeometry, ref pFieldsEdit);
            addFiled("Id", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("GXID", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("GYID", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("eNodeB", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("CI", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("CellName", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("RecePower", esriFieldType.esriFieldTypeDouble, ref pFieldsEdit);
            addFiled("PathLoss", esriFieldType.esriFieldTypeDouble, ref pFieldsEdit);
            addFiled("Level", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("Longitude", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            addFiled("Latitude", esriFieldType.esriFieldTypeString, ref pFieldsEdit);
            return GetLayer(workspaceDirectory, fileName, pFields);
        }
    }
}
