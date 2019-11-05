using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Transport;
using LTE.SeverImp;
using System.Threading;
using System.ServiceProcess;

namespace LTE
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            LTE.SeverImp.GisSever.start();
        }
    }
}
