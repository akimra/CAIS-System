using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

using CryptoPro.Sharpei;
using CryptoPro.Sharpei.Xml;


namespace CAIS_System
{
    class NodeCrypto
    {
        private const string CallerInformationSystemSignatureTag = "CallerInformationSystemSignature";
        private const string CallerInformationSystemSignatureTagNamespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/1.2";
        private const string AckTargetMessageTag = "ns1:AckTargetMessage";
        private const string MessagePrimaryContentTag = "basic:MessagePrimaryContent";
        private const string PersonalSignatureTag = "PersonalSignature";
        private const string PersonalSignatureTagNamespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/1.2";
        private const string SenderProvidedRequestDataTag = "tns:SenderProvidedRequestData";

        [STAThread]
        static void Demo(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("<doc_to_sign> <signed_doc>");
                return;
            }
            X509Certificate2Collection certs = null;
            X509Store certStore = new X509Store(StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);
            // "01020389ef09"  - Серийный номер сертификата ЭЦП   
            certs = certStore.Certificates.Find(X509FindType.FindBySerialNumber, "01020389ef09", false);
            SignXmlFile2(args[0], args[1], certs[0]);
        }

        static XmlElement SignElement2(string xml, X509Certificate2 Certificate, string signUri)
        {
            XmlDocument xmlObj = new XmlDocument();
            xmlObj.PreserveWhitespace = true;
            XmlDeclaration xmldecl;
            xmldecl = xmlObj.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlObj.LoadXml(xml);

            SignedXml signedXml = new SignedXml(xmlObj);

            signedXml.SigningKey = Certificate.PrivateKey;
            Reference reference = new Reference();
            reference.Uri = signUri.Length > 0 ? "#" + signUri : "";
            #pragma warning disable 612
            signedXml.SignedInfo.SignatureMethod = CPSignedXml.XmlDsigGost3410_2012_256Url;
            #pragma warning restore 612

            XmlDsigExcC14NTransform c14 = new XmlDsigExcC14NTransform();
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

            CryptoPro.Sharpei.Xml.XmlDsigSmevTransform smev = new XmlDsigSmevTransform();
            //signedXml.SafeCanonicalizationMethods.Add("urn://smev-gov-ru/xmldsig/transform");
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(c14);
            reference.AddTransform(smev);
            signedXml.AddReference(reference);
            #pragma warning disable 612
            signedXml.SignedInfo.SignatureMethod = CPSignedXml.XmlDsigGost3410_2012_256Url;
            #pragma warning restore 612
            signedXml.KeyInfo = new KeyInfo();
            signedXml.KeyInfo.AddClause(new KeyInfoX509Data(Certificate));
            signedXml.Signature.Id = "SignId_" + Certificate.SerialNumber;
            signedXml.ComputeSignature();
            return signedXml.GetXml();
        }

        static void SignXmlFile2(string FileName, string SignedFileName, X509Certificate2 Certificate)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(new XmlTextReader(FileName));
            doc.PreserveWhitespace = true;

            XmlElement xNode2Change;
            XmlNodeList nlElmnts;
            XmlElement xNodeToSign;
            XmlElement xSign;
            XmlNode xInsert;
            XmlElement xMain;
            string sUri;

            nlElmnts = doc.GetElementsByTagName(MessagePrimaryContentTag);
            if (nlElmnts.Count == 1)
            {
                xNode2Change = (XmlElement)nlElmnts[0];
                if (xNode2Change.ChildNodes.Count != 1) throw new XmlException(string.Format("Количество потомков не равно 1 у {0}", MessagePrimaryContentTag));

                if (xNode2Change.GetAttribute("xmlns").Length > 0)
                {
                    xNode2Change.Prefix = "mpc";
                    xNode2Change.SetAttribute("xmlns:" + xNode2Change.Prefix, xNode2Change.GetAttribute("xmlns"));
                    xNode2Change.Attributes.Remove(xNode2Change.Attributes[0]);
                }

                xMain = xNode2Change;
                // Любой дочерний узел у MessagePrimaryContent
                xNodeToSign = (XmlElement)xMain.ChildNodes[0];
                sUri = xNodeToSign.GetAttribute("Id");

                xSign = SignElement2(doc.OuterXml, Certificate, sUri);

                nlElmnts = doc.GetElementsByTagName(PersonalSignatureTag, PersonalSignatureTagNamespace);
                if (nlElmnts.Count == 1)
                {
                    xInsert = nlElmnts[0];
                    xInsert.RemoveAll();
                }
                else if (nlElmnts.Count == 0)
                {
                    xInsert = doc.CreateNode(XmlNodeType.Element, PersonalSignatureTag, PersonalSignatureTagNamespace);
                    xNodeToSign.ParentNode.ParentNode.InsertAfter(xInsert, xMain);
                }
                else throw new XmlException(string.Format("Количество потомков не равно 1 у {0}", PersonalSignatureTag));

                xInsert.AppendChild(doc.ImportNode(xSign, true));
            }

            // CallerInformationSystemSignature
            nlElmnts = doc.GetElementsByTagName(SenderProvidedRequestDataTag);
            if (nlElmnts.Count != 1)
                nlElmnts = doc.GetElementsByTagName(AckTargetMessageTag);
            if (nlElmnts.Count != 1) throw new XmlException(string.Format("Количество потомков не равно 1 у {0}", SenderProvidedRequestDataTag));
            xNodeToSign = (XmlElement)nlElmnts[0];

            sUri = xNodeToSign.GetAttribute("Id");

            xSign = SignElement2(doc.OuterXml, Certificate, sUri);

            nlElmnts = doc.GetElementsByTagName(CallerInformationSystemSignatureTag, CallerInformationSystemSignatureTagNamespace);
            if (nlElmnts.Count == 1)
            {
                xInsert = nlElmnts[0];
                xInsert.RemoveAll();
                xInsert.ParentNode.RemoveChild(xInsert);
            }
            else if (nlElmnts.Count == 0) xInsert = doc.CreateNode(XmlNodeType.Element, CallerInformationSystemSignatureTag, CallerInformationSystemSignatureTagNamespace);
            else throw new XmlException(string.Format("Количество потомков не равно 1 у {0}", CallerInformationSystemSignatureTag));

            xInsert.AppendChild(doc.ImportNode(xSign, true));
            xNodeToSign.ParentNode.AppendChild(xInsert);

            if (doc.FirstChild is XmlDeclaration)
            {
                doc.RemoveChild(doc.FirstChild);
            }
            // Сохраняем подписанный документ в файл.
            using (XmlTextWriter xmltw = new XmlTextWriter(SignedFileName, new UTF8Encoding(false)))
            {
                doc.WriteTo(xmltw);
            }
        }
    }
}
