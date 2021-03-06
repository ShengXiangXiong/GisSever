using System.Collections.Generic;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using System;

namespace LTE.GIS
{
    public static class DrawUtilities
    {
        private static object _missing = Type.Missing;

        //public static void Drawline(IGraphicsContainer3D lineGraphicsContainer3D)
        //{
            //const esriSimple3DLineStyle lineStyle = esriSimple3DLineStyle.esriS3DLSStrip;
            //const double lineWidth = 0.25;
            //DrawLine(lineGraphicsContainer3D, GeometryUtilities.ConstructPoint3D(126663, 2493183, -20), GeometryUtilities.ConstructPoint3D(129791, 2496608, 100), ColorUtilities.GetColor(255, 0, 0), lineStyle, lineWidth);

            //DrawLine(lineGraphicsContainer3D, GeometryUtilities.ConstructPoint3D(-10, 0, 0), GeometryUtilities.ConstructPoint3D(10, 0, 0), ColorUtilities.GetColor(255, 0, 0), lineStyle, lineWidth);
            //DrawLine(lineGraphicsContainer3D, GeometryUtilities.ConstructPoint3D(0, -10, 0), GeometryUtilities.ConstructPoint3D(0, 10, 0), ColorUtilities.GetColor(0, 0, 255), lineStyle, lineWidth);
            //DrawLine(lineGraphicsContainer3D, GeometryUtilities.ConstructPoint3D(0, 0, -10), GeometryUtilities.ConstructPoint3D(0, 0, 10), ColorUtilities.GetColor(0, 255, 0), lineStyle, lineWidth);

            //DrawEnd(lineGraphicsContainer3D, GeometryUtilities.ConstructPoint3D(10, 0, 0), GeometryUtilities.ConstructVector3D(0, 10, 0), 90, ColorUtilities.GetColor(255, 0, 0), 0.2 * lineWidth);
            //DrawEnd(lineGraphicsContainer3D, GeometryUtilities.ConstructPoint3D(0, 10, 0), GeometryUtilities.ConstructVector3D(10, 0, 0), -90, ColorUtilities.GetColor(0, 0, 255), 0.2 * lineWidth);
            //DrawEnd(lineGraphicsContainer3D, GeometryUtilities.ConstructPoint3D(0, 0, 10), null, 0, ColorUtilities.GetColor(0, 255, 0), 0.2 * lineWidth);
        //}

        public static void DrawPoint(IGraphicsContainer3D lineGraphicsContainer3D, IPoint point)
        {
            const esriSimple3DMarkerStyle markerStyle = esriSimple3DMarkerStyle.esriS3DMSCube;
            const double size = 10;
            IColor markerColor = ColorUtilities.GetColor(255, 0, 0);
            GeometryUtilities.MakeZAware(point as IGeometry);

            GraphicsLayer3DUtilities.AddPointToGraphicsLayer3D(lineGraphicsContainer3D, point as IGeometry, markerColor, markerStyle, size);
        }
        public static void DrawPoint(IGraphicsContainer3D lineGraphicsContainer3D, IPoint point,IColor color)
        {
            const esriSimple3DMarkerStyle markerStyle = esriSimple3DMarkerStyle.esriS3DMSCube;
            const double size = 2;
            //IColor markerColor = ColorUtilities.GetColor(0, 255, 0);
            GeometryUtilities.MakeZAware(point as IGeometry);

            GraphicsLayer3DUtilities.AddPointToGraphicsLayer3D(lineGraphicsContainer3D, point as IGeometry, color, markerStyle, size);
        }

        public static void DrawPoint(IGraphicsContainer3D lineGraphicsContainer3D, IPoint point, IColor color, double size)
        {
            const esriSimple3DMarkerStyle markerStyle = esriSimple3DMarkerStyle.esriS3DMSCube;
            //const double size = 20;
            //IColor markerColor = ColorUtilities.GetColor(0, 255, 0);
            GeometryUtilities.MakeZAware(point as IGeometry);

            GraphicsLayer3DUtilities.AddPointToGraphicsLayer3D(lineGraphicsContainer3D, point as IGeometry, color, markerStyle, size);
        }

        public static void DrawLine(IGraphicsContainer3D lineGraphicsContainer3D, IPointCollection linePointCollection)
        {
            const esriSimple3DLineStyle lineStyle = esriSimple3DLineStyle.esriS3DLSTube;
            const double lineWidth = 0.25;
            IColor lineColor = ColorUtilities.GetColor(255, 0, 0);
            GeometryUtilities.MakeZAware(linePointCollection as IGeometry);

            GraphicsLayer3DUtilities.AddLineToGraphicsLayer3D(lineGraphicsContainer3D, linePointCollection as IGeometry, lineColor, lineStyle, lineWidth);
        }
        public static void DrawLine(IGraphicsContainer3D lineGraphicsContainer3D, List<IPoint> listPoints)
        {
            IPointCollection linePointColl = new PolylineClass();
            foreach (var point in listPoints)
            {
                linePointColl.AddPoint(point, ref _missing, ref _missing);
            }
            DrawUtilities.DrawLine(lineGraphicsContainer3D, linePointColl);
        }

        //private static void DrawLine(IGraphicsContainer3D lineGraphicsContainer3D, IPoint lineFromPoint, IPoint lineToPoint, IColor lineColor, esriSimple3DLineStyle lineStyle, double lineWidth)
        //{
        //    IPointCollection linePointCollection = new PolylineClass();

        //    linePointCollection.AddPoint(lineFromPoint, ref _missing, ref _missing);
        //    linePointCollection.AddPoint(lineToPoint, ref _missing, ref _missing);

        //    GeometryUtilities.MakeZAware(linePointCollection as IGeometry);

        //    GraphicsLayer3DUtilities.AddLineToGraphicsLayer3D(lineGraphicsContainer3D, linePointCollection as IGeometry, lineColor, lineStyle, lineWidth);
        //}

        private static void DrawEnd(IGraphicsContainer3D endGraphicsContainer3D, IPoint endPoint, IVector3D lineOfRotationVector3D, double degreesOfRotation, IColor endColor, double endRadius)
        {
            //IGeometry endGeometry = Vector3DExamples.GetExample2();

            //ITransform3D transform3D = endGeometry as ITransform3D;

            //IPoint originPoint = GeometryUtilities.ConstructPoint3D(0, 0, 0);

            //transform3D.Scale3D(originPoint, endRadius, endRadius, 2 * endRadius);

            //if (degreesOfRotation != 0)
            //{
            //    double angleOfRotationInRadians = GeometryUtilities.GetRadians(degreesOfRotation);

            //    transform3D.RotateVector3D(lineOfRotationVector3D, angleOfRotationInRadians);
            //}

            //transform3D.Move3D(endPoint.X - originPoint.X, endPoint.Y - originPoint.Y, endPoint.Z - originPoint.Z);

            //GraphicsLayer3DUtilities.AddMultiPatchToGraphicsLayer3D(endGraphicsContainer3D, endGeometry, endColor);
        }

        public static void DrawMultiPatch(IGraphicsContainer3D multiPatchGraphicsContainer3D, IGeometry geometry)
        {
            const int Yellow_R = 255;
            const int Yellow_G = 255;
            const int Yellow_B = 0;

            IColor multiPatchColor = ColorUtilities.GetColor(Yellow_R, Yellow_G, Yellow_B);

            multiPatchGraphicsContainer3D.DeleteAllElements();

            GraphicsLayer3DUtilities.AddMultiPatchToGraphicsLayer3D(multiPatchGraphicsContainer3D, geometry, multiPatchColor);
        }

        public static void DrawOutline(IGraphicsContainer3D outlineGraphicsContainer3D, IGeometry geometry)
        {
            const esriSimple3DLineStyle OutlineStyle = esriSimple3DLineStyle.esriS3DLSTube;
            const double OutlineWidth = 0.1;

            const int Black_R = 0;
            const int Black_G = 0;
            const int Black_B = 0;

            IColor outlineColor = ColorUtilities.GetColor(Black_R, Black_G, Black_B);

            outlineGraphicsContainer3D.DeleteAllElements();

            GraphicsLayer3DUtilities.AddOutlineToGraphicsLayer3D(outlineGraphicsContainer3D, GeometryUtilities.ConstructMultiPatchOutline(geometry), outlineColor, OutlineStyle, OutlineWidth);
        }

        //----------------------------------干扰源定位中的绘制

        // 绘制线段
        public static void DrawLine(IPoint p1, IPoint p2, int r, int g, int b)
        {
            IPolyline line = new PolylineClass();
            object _missing = Type.Missing;
            line.FromPoint = p1;
            line.ToPoint = p2;

            ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
            lineSymbol.Color = ColorUtilities.GetColor(r, g, b);
            lineSymbol.Width = 0.25;

            ILineElement lineElement = new LineElementClass();
            lineElement.Symbol = lineSymbol;

            IElement element = lineElement as IElement;
            element.Geometry = line as IGeometry;

            IGraphicsLayer pLayer = (GISMapApplication.Instance.Scene as IBasicMap).BasicGraphicsLayer;
            IGraphicsContainer3D pGC = pLayer as IGraphicsContainer3D;
            pGC.AddElement(element);
        }

        // 绘制矩形
        public static void DrawRect(IPoint pMin, IPoint pMax, int r, int g, int b)
        {
            IPoint p1 = GeometryUtilities.ConstructPoint3D(pMin.X, pMin.Y, 0);  
            IPoint p2 = GeometryUtilities.ConstructPoint3D(pMin.X, pMax.Y, 0); 
            IPoint p3 = GeometryUtilities.ConstructPoint3D(pMax.X, pMax.Y, 0);  
            IPoint p4 = GeometryUtilities.ConstructPoint3D(pMax.X, pMin.Y, 0);  
            DrawLine(p1, p2, r, g, b);
            DrawLine(p2, p3, r, g, b);
            DrawLine(p3, p4, r, g, b);
            DrawLine(p4, p1, r, g, b);
        }

        // 绘制矩形
        public static void DrawRect(double minX, double minY, double maxX, double maxY, int r, int g, int b)
        {
            IPoint p1 = GeometryUtilities.ConstructPoint3D(minX, minY, 0);
            IPoint p2 = GeometryUtilities.ConstructPoint3D(minX, maxY, 0);
            IPoint p3 = GeometryUtilities.ConstructPoint3D(maxX, maxY, 0);
            IPoint p4 = GeometryUtilities.ConstructPoint3D(maxX, minY, 0);
            DrawLine(p1, p2, r, g, b);
            DrawLine(p2, p3, r, g, b);
            DrawLine(p3, p4, r, g, b);
            DrawLine(p4, p1, r, g, b);
        }
    }
}