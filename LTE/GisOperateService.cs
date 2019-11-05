using LTE.SeverImp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Thrift.Server;
using Thrift.Transport;

namespace LTE
{
    partial class GisOperateService : ServiceBase
    {
        public GisOperateService()
        {
            InitializeComponent();
            base.ServiceName = "GisSever";
        }

        protected override void OnStart(string[] args)
        {
            GisSever.start();
        }

        protected override void OnStop()
        {
            GisSever.stop();
        }
    }
}
