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
        private static ChannelFactory<SmevExchange.SMEVMessageExchangePortTypeChannel> port =
            new ChannelFactory<SmevExchange.SMEVMessageExchangePortTypeChannel>(binding, endpoint);
        private static SmevExchange.SMEVMessageExchangePortTypeChannel channel = port.CreateChannel();
        
        public NodeSmev()
        {

        }
        ~NodeSmev()
        {
            CloseChannel();
        }
        public void OpenChannel()
        {
            channel.Open();
        }
        public void CloseChannel()
        {
            channel.Close();
        }
        public async Task<string> GetMessageId()
        {
            // ----------------------------Блок получения Message ID-------------------------------//
            // ЧЕРЕЗ API!!!!!!!
            string url = "https://www.uuidgenerator.net/api/version1";
            string messageId;

            using (var webClient = new WebClient())
            {

                var response = await Task.Run(() => webClient.DownloadString(url));
                messageId = response.Substring(0, 36);
            }
            return messageId;
            //-----------------------------Блок получения Message ID-------------------------------//
        }

        public async Task<SmevExchange.SendRequestResponse> SendMessage()
        {
            SmevExchange.SendRequestRequest req = new SmevExchange.SendRequestRequest();
            string st, messageId="";
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load("Request0.xml");                                                                //Импорт сгенерированного xml запроса (пока тест)
            req.SenderProvidedRequestData = new SmevExchange.SenderProvidedRequestData
            {
                MessagePrimaryContent = doc.DocumentElement, //TODO: приложение данных запроса в xml из nodeparse
                TestMessage = new SmevExchange.Void(),
                AttachmentHeaderList = new SmevExchange.AttachmentHeaderList()
            };

            //  тут заглушка под подпись
            System.Xml.XmlDocument signature = new System.Xml.XmlDocument();
            signature.Load("CallerSignature.xml");
            req.CallerInformationSystemSignature = signature.DocumentElement;

            NodeCrypto Cryp = new NodeCrypto();
            try
            {
                messageId = await GetMessageId();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                req.SenderProvidedRequestData.MessageID = messageId;
                st = Serialize(req);
            }
            
            
            SmevExchange.SendRequestResponse sendreqresp = new SmevExchange.SendRequestResponse();
            try
            {
                sendreqresp = await Task.Run(() => channel.SendRequestAsync(req));
            }
            
            catch (Exception ex)
            {
                ErrorHandler.ErrorHandling(ex);
                return sendreqresp;
            }
            return sendreqresp;
        }
        //--------------------------Пока вспомогательная функция, возможно это будет в классе NodeParse-------------------------------//
        public string Serialize<TType> (TType sourceObject)
        {
            if ((object)sourceObject == null)
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
        
    }
}
