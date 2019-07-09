using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows;
using System.Net;

namespace CAIS_System
{
    class NodeSmev
    {
        private static Uri address = new Uri("http://smev3-n0.test.gosuslugi.ru:7500/smev/v1.2/ws");
        private static BasicHttpBinding binding = new BasicHttpBinding("SMEVMessageExchangeSoap11Binding");
        private static EndpointAddress endpoint = new EndpointAddress(address);
        private static ChannelFactory<TestSmevService.SMEVMessageExchangePortTypeChannel> port = new ChannelFactory<TestSmevService.SMEVMessageExchangePortTypeChannel>(binding, endpoint);
        private static TestSmevService.SMEVMessageExchangePortTypeChannel channel = port.CreateChannel();
        private static TestSmevService.SendRequestRequest req = new TestSmevService.SendRequestRequest();
        public NodeSmev()
        {
            channel.Open();
        }
        public void SendMessage(string messageId)
        {
            string st;
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load("Request0.xml");
            req.SenderProvidedRequestData = new TestSmevService.SenderProvidedRequestData();
            req.SenderProvidedRequestData.MessagePrimaryContent = doc.DocumentElement;
            req.SenderProvidedRequestData.TestMessage = new TestSmevService.Void();
            req.SenderProvidedRequestData.AttachmentHeaderList = new TestSmevService.AttachmentHeaderList();
            
            req.SenderProvidedRequestData.MessageID = messageId;
            st = Serialize(req);
            
            TestSmevService.SendRequestResponse sendreqresp = new TestSmevService.SendRequestResponse();
            //sendreqresp = channel.SendRequest(req);
            MessageBox.Show(st);
        }
        public string Serialize<TType> (TType sourceObject)
        {
            if (sourceObject == null)
            {
                return string.Empty;
            }
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TType));
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stringWriter, new System.Xml.XmlWriterSettings() { Indent = true }))
            {
                serializer.Serialize(writer, sourceObject);
                return stringWriter.ToString();
            }
        }
    }
}
