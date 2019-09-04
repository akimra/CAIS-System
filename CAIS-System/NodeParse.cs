using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace CAIS_System
{
    abstract class NodeParse
    {
        private System.Xml.XmlDocument requestDataXml = new System.Xml.XmlDocument();


        public string Serialize<TType>(TType sourceObject)
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
    }
}
