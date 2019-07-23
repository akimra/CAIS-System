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
        private static readonly Uri address = new Uri("http://smev3-n0.test.gosuslugi.ru:7500/smev/v1.2/ws");
        private static readonly BasicHttpBinding binding = new BasicHttpBinding("SMEVMessageExchangeSoap11Binding");
        private static readonly EndpointAddress endpoint = new EndpointAddress(address);
        private static ChannelFactory<TestSmevService.SMEVMessageExchangePortTypeChannel> port = new ChannelFactory<TestSmevService.SMEVMessageExchangePortTypeChannel>(binding, endpoint);
        private static TestSmevService.SMEVMessageExchangePortTypeChannel channel = port.CreateChannel();
        private static TestSmevService.SendRequestRequest req = new TestSmevService.SendRequestRequest();
        public NodeSmev()
        {
            
        }
        public void OpenChannel()
        {
            channel.Open();
        }
        public void CloseChannel()
        {
            channel.Close();
        }
        public string GetMessageId()
        {
            // ----------------------------Блок получения Message ID-------------------------------//
            // ЧЕРЕЗ API!!!!!!!
            string url = "https://www.uuidgenerator.net/api/version1";
            string messageId;

            using (var webClient = new WebClient())
            {

                var response = webClient.DownloadString(url);
                messageId = response.Substring(0, 36);
            }
            return messageId;
            //-----------------------------Блок получения Message ID-------------------------------//
        }

        public async Task<TestSmevService.SendRequestResponse> SendMessage()
        {
            string st, messageId="";
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load("Request0.xml");                                                                //Импорт сгенерированного xml запроса (пока тест)
            req.SenderProvidedRequestData = new TestSmevService.SenderProvidedRequestData
            {
                MessagePrimaryContent = doc.DocumentElement, //TODO: приложение данных запроса в xml из nodeparse
                TestMessage = new TestSmevService.Void(),
                AttachmentHeaderList = new TestSmevService.AttachmentHeaderList()
            };
            System.Xml.XmlDocument signature = new System.Xml.XmlDocument();
            signature.Load("CallerSignature.xml");
            req.CallerInformationSystemSignature = signature.DocumentElement;
            try
            {
                messageId = GetMessageId();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            req.SenderProvidedRequestData.MessageID = messageId.ToString();
            st = Serialize(req);
            
            TestSmevService.SendRequestResponse sendreqresp = new TestSmevService.SendRequestResponse();
            try
            {
                sendreqresp = await Task.Run(() => channel.SendRequestAsync(req));
            }
            
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                return null;
            }
            return sendreqresp;
        }
        //--------------------------Пока вспопогательная функция, возможно это будет в классе NodeParse-------------------------------//
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
        //------------------------------------------------------------------------------------------------------------------------------//
        protected void ThrowErrorMessageBox(string message)
        {

        }
    }
}
