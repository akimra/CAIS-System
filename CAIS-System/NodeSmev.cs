using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

namespace CAIS_System
{
    class NodeSmev
    {
        private Uri address;
        private BasicHttpBinding binding;
        private EndpointAddress endpoint;
        private ChannelFactory<SmevExchange.SMEVMessageExchangePortTypeChannel> port;
        private SmevExchange.SMEVMessageExchangePortTypeChannel channel;
        
        public NodeSmev(bool isTest = true)
        {
            if (isTest)
            {
                address = new Uri("http://smev3-n0.test.gosuslugi.ru:7500/smev/v1.2/ws");
                binding = new BasicHttpBinding("SMEVMessageExchangeSoap11Binding");
                endpoint = new EndpointAddress(address);
            }

            port = new ChannelFactory<SmevExchange.SMEVMessageExchangePortTypeChannel>(binding, endpoint);
            channel = port.CreateChannel();
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
        private async Task<string> GetMessageId()
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
            XmlDocument doc = new XmlDocument();
            doc.Load("Request0.xml");                                      //Импорт сгенерированного xml запроса (пока тест)
            req.SenderProvidedRequestData = new SmevExchange.SenderProvidedRequestData
            {
                MessagePrimaryContent = doc.DocumentElement, //TODO: приложение данных запроса в xml из nodeparse
                //TestMessage = new SmevExchange.Void(),
                //AttachmentHeaderList = new SmevExchange.AttachmentHeaderList()
            };

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
            }

            req.SenderProvidedRequestData.AttachmentHeaderList = new SmevExchange.AttachmentHeaderList();
            req.SenderProvidedRequestData.TestMessage = new SmevExchange.Void();

            XmlDocument toSign = new XmlDocument();
            toSign.LoadXml(Serialize(req));

            XmlDocument signed = NodeCrypto.Demo(toSign);

            if (signed.GetElementsByTagName("CallerInformationSystemSignature").Count > 1)
                throw new Exception("Более одного блока подписи недопустимо");

            XmlDocument childsSignature = new XmlDocument();
            childsSignature.LoadXml(signed.GetElementsByTagName("CallerInformationSystemSignature")[0].InnerXml);
            req.CallerInformationSystemSignature = childsSignature.DocumentElement;

            //try
            //{
            //    messageId = await GetMessageId();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
            //finally
            //{
            //    req.SenderProvidedRequestData.MessageID = messageId;
            //}

            //req.SenderProvidedRequestData.AttachmentHeaderList = new SmevExchange.AttachmentHeaderList();
            //req.SenderProvidedRequestData.TestMessage = new SmevExchange.Void();

            MessageBox.Show(Serialize(req));

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
            var serializer = new XmlSerializer(typeof(TType));
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            using (XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings() { Indent = true }))
            {
                serializer.Serialize(writer, sourceObject);
                return stringWriter.ToString();
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------//
        
    }
}
