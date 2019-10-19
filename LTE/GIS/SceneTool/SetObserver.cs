// Copyright 2008 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See use restrictions at <your ArcGIS install location>/developerkit/userestrictions.txt.
// 

using System;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using System.Runtime.InteropServices;

namespace LTE.GIS
{
	[ClassInterface(ClassInterfaceType.None)]
	[Guid("834AC707-0088-4d08-BF74-A18EE28BCA8E")]

	public sealed class SetObserver : BaseTool
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

		public SetObserver()
		{
			base.m_category = "Sample_SceneControl(C#)";
			base.m_caption = "Set Observer";
			base.m_toolTip = "Set Observer";
			base.m_name = "Sample_SceneControl(C#)/SetObserver";
			base.m_message = "Set observer position to selected point";

			//Load resources
			string[] res = GetType().Assembly.GetManifestResourceNames();
			if(res.GetLength(0) > 0)
			{
				base.m_bitmap = new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("LTE.GIS.SceneTool.observer.bmp"));
			}
			m_pCursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream("LTE.GIS.SceneTool.observer.cur"));
		
			m_pSceneHookHelper = new SceneHookHelperClass ();
		}

		~SetObserver()
		{
			m_pSceneHookHelper = null;
			m_pCursor = null;
		}
	
		public override bool Enabled
		{
			get
			{
				if(m_pSceneHookHelper.Hook == null || m_pSceneHookHelper.Scene == null)
					return false;
				else
				{
					ICamera pCamera = (ICamera) m_pSceneHookHelper.Camera;

					//Disable if orthographic (2D) view
					if(pCamera.ProjectionType == esri3DProjectionType.esriOrthoProjection)
						return false;
					else
						return true;
				}
			}
		}
	
		public override void OnCreate(object hook)
		{
			m_pSceneHookHelper.Hook = hook;				
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
			ISceneGraph pSceneGraph = (ISceneGraph) m_pSceneHookHelper.SceneGraph;

			IPoint pNewObs;
			object pOwner, pObject;

			//Translate screen coordinates into a 3D point
			pSceneGraph.Locate(pSceneGraph.ActiveViewer, X, Y, esriScenePickMode.esriScenePickAll, true,
				out pNewObs, out pOwner, out pObject);

			if(pNewObs == null) return;

			//Get the scene viewer's camera
			ICamera pCamera = (ICamera) m_pSceneHookHelper.Camera;

			//Get the camera's old observer
			IPoint pOldObs = (IPoint) pCamera.Observer;

			//Get the duration in seconds of last redraw
			//and the average number of frames per second
			double dlastFrameDuration, dMeanFrameRate;
			pSceneGraph.GetDrawingTimeInfo(out dlastFrameDuration, out dMeanFrameRate);

			if(dlastFrameDuration < 0.01)
				dlastFrameDuration = 0.01;

			int iSteps;
			iSteps = (int) (2/dlastFrameDuration);
			if (iSteps < 1)
				iSteps = 1;
			
			if(iSteps > 60)
				iSteps = 60;

			double dxObs, dyObs, dzObs;

			dxObs = (pNewObs.X - pOldObs.X) / iSteps;
			dyObs = (pNewObs.Y - pOldObs.Y) / iSteps;
			dzObs = (pNewObs.Z - pOldObs.Z) / iSteps;

			//Loop through each step moving the camera's observer from the old
			//position to the new position, refreshing the scene viewer each time
			for(int i=0; i <= iSteps; i++)
			{
				pNewObs.X = pOldObs.X + (i * dxObs);
				pNewObs.Y = pOldObs.Y + (i * dyObs);
				pNewObs.Z = pOldObs.Z + (i * dzObs);
				pCamera.Observer = pNewObs;
				pSceneGraph.ActiveViewer.Redraw(true);
			}			
		}
	}
}
