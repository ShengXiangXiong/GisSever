using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Server;
using Thrift.Transport;

namespace LTE.SeverImp
{
    class GisSever
    {
        public static TServer server;
        public static void start()
        {
            if(server is null)
            {
                ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);
                //new OperateGisLayerImp().refreshGroundCover("汊河变中兴宏基站-扇区1");

                //Create a processor
                OpreateGisLayer.Processor processor = new OpreateGisLayer.Processor(new OperateGisLayerImp());
                TServerTransport transport = new TServerSocket(8800);//监听8800端口
                server = new TSimpleServer(processor, transport);
            }
            server.Serve();
        }
        public static void stop()
        {
            server.Stop();
        }
    }
}
