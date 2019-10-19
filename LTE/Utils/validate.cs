using LTE.DB;
using LTE.InternalInterference;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTE.Utils
{
    class validate
    {
        public static Result validateCell(ref CellInfo cellInfo)
        {
            if (cellInfo.SourceName == string.Empty)
            {
                return new Result(false, "请输入小区名称");
            }
            DataTable dt = IbatisHelper.ExecuteQueryForDataTable("SingleGetCellType", cellInfo.SourceName);
            if (dt.Rows.Count == 0)
            {
                return new Result(false, "您输入的小区名称有误，请重新输入！");
            }
            cellInfo.eNodeB = Convert.ToInt32(dt.Rows[0]["eNodeB"]);
            cellInfo.CI = Convert.ToInt32(dt.Rows[0]["CI"]);
            return new Result(true,"");
        }
    }
}
