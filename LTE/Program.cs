using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Transport;
using LTE.SeverImp;
using System.Threading;

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
            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //new OperateGisLayerImp().refreshGroundCover("汊河变中兴宏基站-扇区1");

            //TMultiplexedProcessor processor = new TMultiplexedProcessor();//定义多服务接口，此TMultiplexedProcessor能够处理多个服务的输入输出流
            //processor.RegisterProcessor("UserService", new UserService.Processor(new UserServiceImp()));
            //processor.RegisterProcessor("HelloWorldService", new HelloWorldService.Processor(new HelloWorldServiceImp()));

            //Create a processor
            OpreateGisLayer.Processor processor = new OpreateGisLayer.Processor(new OperateGisLayerImp());
            TServerTransport transport = new TServerSocket(8800);//监听8800端口
            TServer server = new TSimpleServer(processor, transport);

            TServerTransport transport2 = new TServerSocket(8800);
            server.Serve();
        }
    }
}
