﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Analyst3D;

namespace LTE.GIS
{
    public static class FeatureLayerUtilities
    {
        public static void SetUniqueLayerSelectable(IScene pScene, string layerName)
        {
            IEnumLayer pEnumLayer = pScene.get_Layers(null, false);
            pEnumLayer.Reset();
            ILayer pLayer = pEnumLayer.Next();
            while (pLayer != null)
            {
                if (pLayer is IFeatureLayer)
                {
                    IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
                    if (pFeatureLayer.Name.Equals(layerName))
                        pFeatureLayer.Selectable = true;
                    else
                        pFeatureLayer.Selectable = false;
                }
                pLayer = pEnumLayer.Next();
            }
        }

        public static void SetSpecifiedLayersSelectable(IScene pScene, params string[] layerNames)
        {
            IEnumLayer pEnumLayer = pScene.get_Layers(null, false);
            pEnumLayer.Reset();
            ILayer pLayer = pEnumLayer.Next();
            while (pLayer != null)
            {
                if (pLayer is IFeatureLayer)
                {
                    IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
                    if (layerNames.Contains(pFeatureLayer.Name))
                        pFeatureLayer.Selectable = true;
                    else
                        pFeatureLayer.Selectable = false;
                }
                pLayer = pEnumLayer.Next();
            }
        }

        public static void SetAllLayersSelectable(IScene pScene)
        {
            IEnumLayer pEnumLayer = pScene.get_Layers(null, false);
            pEnumLayer.Reset();
            ILayer pLayer = pEnumLayer.Next();
            while (pLayer != null)
            {
                if (pLayer is IFeatureLayer)
                {
                    IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
                        pFeatureLayer.Selectable = true;
                }
                pLayer = pEnumLayer.Next();
            }
        }


        //public static void SetSpecifiedLayersSelectable(MapForm p_mapForm, params string[] layerNames)
        //{
        //    foreach (string var in layerNames)
        //    {
        //        ILayer pLayer = p_mapForm.GetLayer(var);
        //        IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
        //        pFeatureLayer.Selectable = true;
        //    }
        //}

                    

    }
}