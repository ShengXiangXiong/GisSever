using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace LTE.GIS
{
    public static class FeatureUtilities
    {
        /// <summary>
        /// 清空FeatureLayer内容
        /// </summary>
        /// <param name="pFeatureLayer"></param>
        public static void DeleteFeatureLayerFeatrues(IFeatureLayer pFeatureLayer)
        {
            if (pFeatureLayer == null) return;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            ITable pTable = pFeatureClass as ITable;
            pTable.DeleteSearchedRows(null);
        }

        /// <summary>
        /// 删除指定名称的Field
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="fieldname"></param>
        public static void DeletField(IFeatureClass featureClass, string fieldname)
        {
            IFields fields = featureClass.Fields;
            IField field = fields.get_Field(fields.FindField(fieldname));
            featureClass.DeleteField(field);
        }
    }
}
