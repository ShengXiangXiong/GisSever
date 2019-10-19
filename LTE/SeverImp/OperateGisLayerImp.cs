using LTE.GIS;
using LTE.InternalInterference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTE.SeverImp
{
    class  OperateGisLayerImp : OpreateGisLayer.Iface
    {

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

        public Result refreshDTLayer()
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
            if (!AnalysisEntry.DisplayAnalysis(cellInfo))
            {
                return new Result(false, "请先进行小区覆盖计算");
            }
            return new Result(true, "地面覆盖刷新成功");
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
