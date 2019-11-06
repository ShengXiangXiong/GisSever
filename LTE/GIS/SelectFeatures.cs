﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using System.Runtime.InteropServices;

namespace LTE.GIS
{
    public sealed class SelectFeatures : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsCommands.Unregister(regKey);

        }

        #endregion
        #endregion

        private System.Windows.Forms.Cursor m_pCursor;
        private ISceneHookHelper m_pSceneHookHelper;

        public SelectFeatures()
        {
            base.m_category = "Sample_SceneControl(C#)";
            base.m_caption = "Select Features";
            base.m_toolTip = "Select Features";
            base.m_name = "Sample_SceneControl(C#)/SelectFeatures";
            base.m_message = "Select features by clicking";

            //Load resources
            string[] res = GetType().Assembly.GetManifestResourceNames();
            if (res.GetLength(0) > 0)
            {
                base.m_bitmap = new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("LTE.GIS.SceneTool.SelectFeatures.bmp"));
            }
            m_pCursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream("LTE.GIS.SceneTool.SelectFeatures.cur"));

            m_pSceneHookHelper = new SceneHookHelperClass();
        }

        ~SelectFeatures()
        {
            m_pSceneHookHelper = null;
            m_pCursor = null;
        }

        public override void OnCreate(object hook)
        {
            m_pSceneHookHelper.Hook = hook;
        }

        public override bool Enabled
        {
            get
            {
                if (m_pSceneHookHelper.Hook == null || m_pSceneHookHelper.Scene == null)
                    return false;
                else
                {
                    IScene pScene = (IScene)m_pSceneHookHelper.Scene;

                    //Disable if no layer
                    if (pScene.LayerCount == 0)
                        return false;

                    //Enable if any selectable layers
                    bool bSelectable = false;

                    IEnumLayer pEnumLayer;
                    pEnumLayer = pScene.get_Layers(null, true);
                    pEnumLayer.Reset();

                    ILayer pLayer = (ILayer)pEnumLayer.Next();

                    //Loop through the scene layers
                    do
                    {
                        //Determine if there is a selectable feature layer
                        if (pLayer is IFeatureLayer)
                        {
                            IFeatureLayer pFeatureLayer = (IFeatureLayer)pLayer;
                            if (pFeatureLayer.Selectable == true)
                            {
                                bSelectable = true;
                                break;
                            }
                        }
                        pLayer = pEnumLayer.Next();
                    }
                    while (pLayer != null);

                    return bSelectable;
                }
            }
        }

        public override int Cursor
        {
            get
            {
                return m_pCursor.Handle.ToInt32();
            }
        }

        public override bool Deactivate()
        {
            return true;
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            //Get the scene graph
            ISceneGraph pSceneGraph = m_pSceneHookHelper.SceneGraph;

            //Get the scene
            IScene pScene = (IScene)m_pSceneHookHelper.Scene;

            IPoint pPoint;
            object pOwner, pObject;

            //Translate screen coordinates into a 3D point
            pSceneGraph.Locate(pSceneGraph.ActiveViewer, X, Y, esriScenePickMode.esriScenePickGeography, true, out pPoint, out pOwner, out pObject);

            //Get a selection environment
            ISelectionEnvironment pSelectionEnv;
            pSelectionEnv = new SelectionEnvironmentClass();

            if (Shift == 0)
            {
                pSelectionEnv.CombinationMethod = ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultNew;

                //Clear previous selection
                if (pOwner == null)
                {
                    pScene.ClearSelection();
                    return;
                }
            }
            else
                pSelectionEnv.CombinationMethod = ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultAdd;

            //If the layer is a selectable feature layer
            if (pOwner is IFeatureLayer)
            {
                IFeatureLayer pFeatureLayer = (IFeatureLayer)pOwner;

                if (pFeatureLayer.Selectable == true)
                {
                    //Select by Shape
                    pScene.SelectByShape(pPoint, pSelectionEnv, false);
                }
            }

            //Refresh the scene viewer
            pSceneGraph.RefreshViewers();
        }
    }
}