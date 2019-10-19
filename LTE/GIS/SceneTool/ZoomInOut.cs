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
	[Guid("C0624E23-6085-4e46-9ED1-25CE20A972E9")]

	public sealed class ZoomInOut : BaseTool
	{
		[DllImport("user32")] public static extern int SetCapture(int hwnd); 
		[DllImport("user32")] public static extern int GetCapture(int fuFlags);
		[DllImport("user32")] public static extern int ReleaseCapture(int hwnd);

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
		private long m_lMouseX, m_lMouseY;
		private bool m_bInUse;

		public ZoomInOut()
		{
			base.m_category = "Sample_SceneControl(C#)";
			base.m_caption = "Zoom In/Out";
			base.m_toolTip = "Zoom In/Out";
			base.m_name = "Sample_SceneControl(C#)/Zoom In/Out";
			base.m_message = "Dynamically zooms in and out on the scene";

			//Load resources
			string[] res = GetType().Assembly.GetManifestResourceNames();
			if(res.GetLength(0) > 0)
			{
				base.m_bitmap = new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("LTE.GIS.SceneTool.ZoomInOut.bmp"));
			}
			m_pCursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream("LTE.GIS.SceneTool.ZOOMINOUT.CUR"));
		
			m_pSceneHookHelper = new SceneHookHelperClass ();
		}

		~ZoomInOut()
		{
			m_pSceneHookHelper = null;
			m_pCursor = null;
		}
	
		public override bool Enabled
		{
			get
			{
				if(m_pSceneHookHelper.Scene == null)
					return false;
				else
					return true;
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
	
		public override void OnMouseDown(int Button, int Shift, int X, int Y)
		{
			//Initialize mouse coordinates
			m_bInUse = true;
			m_lMouseX = X;
			m_lMouseY = Y;

			SetCapture(m_pSceneHookHelper.ActiveViewer.hWnd);
		}
	
		public override void OnMouseMove(int Button, int Shift, int X, int Y)
		{
			if(!m_bInUse) return;

			//Differences between previous and current mouse coordinates
			long dx, dy;
			dx = X - m_lMouseX;
			dy = Y - m_lMouseY;

			if(dx == 0 && dy == 0) return;

			//Get the scene viewer's camera
			ICamera pCamera = (ICamera) m_pSceneHookHelper.Camera;

			//Zoom camera
			if(dy < 0)
				pCamera.Zoom(1.1);
			else if(dy > 0)
				pCamera.Zoom(0.9);

			//Set mouse coordinates for the next OnMouseMove event
			m_lMouseX = X;
			m_lMouseY = Y;

			//Redraw the scene viewer
			ISceneViewer pSceneViewer = (ISceneViewer) m_pSceneHookHelper.ActiveViewer;
			pSceneViewer.Redraw(true);
		}
	
		public override void OnMouseUp(int Button, int Shift, int X, int Y)
		{
			if(!m_bInUse) return;
	
			if(GetCapture(m_pSceneHookHelper.ActiveViewer.hWnd) != 0)
				ReleaseCapture(m_pSceneHookHelper.ActiveViewer.hWnd);

			m_bInUse = false;
		}
	}
}
