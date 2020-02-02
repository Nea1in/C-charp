using System.Xml;
using System.Xml.Serialization;
using System;
using System.IO;
using System.Collections.Generic;
namespace exma
{
    public static class TransactionXmlLoader
    {
        public const string FILENAME = "Transactions.xml";
        public static void push(Transaction transaction)
        {
            XmlSerializer xmlSerializer = null;

            //To avoid namespace creation
            var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            string xml=String.Empty;
            
            
            xmlSerializer=new XmlSerializer(typeof(Transaction));
            using (StringWriter stream = new StringWriter())
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                xmlSerializer.Serialize(writer, transaction, emptyNamepsaces);
                xml = stream.ToString();
            }

            if (!File.Exists(FILENAME))
            {
                using (FileStream fs = File.Create(FILENAME)){}
                using (XmlWriter writer = XmlWriter.Create(FILENAME, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("transactions");
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            } 
            
            XmlDocument xmlDocument=new XmlDocument();
            xmlDocument.Load(FILENAME);

            XmlTextReader textReader = new XmlTextReader(new StringReader(xml));
            XmlNode newNode = xmlDocument.ReadNode(textReader);
            xmlDocument.DocumentElement.AppendChild(newNode);
            xmlDocument.Save(FILENAME);
            textReader.Close();
        }

        public static List<Transaction> pull(string accountId)
        {
            XmlSerializer xmlSerializer = null;
//            Account account = AccountXmlLoader.pull(accountId);
            List<Transaction> result=new List<Transaction>();
            
            try
            {
                XmlDocument doc=new XmlDocument();
                doc.Load(FILENAME);
                XmlNodeList list= doc.SelectNodes($"//*[ReceiverID='{accountId}' or SenderID='{accountId}']");
                foreach (XmlNode node in list)
                {
                    Currency cur;
                    Currency.TryParse(node["currency"].InnerText, out cur);
                    result.Add(new Transaction(node["SenderID"].InnerText,
                        double.Parse(node["Sum"].InnerText),
                        node["purpose"].InnerText,
                        node["ReceiverID"].InnerText,
                        cur));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }
    }
}