using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
namespace exma
{
    
    public static class AccountXmlLoader
    {
        
        
        private const string FILENAME = "Accounts.xml";
        public static bool push(Account account)
        {
            XmlSerializer xmlSerializer = null;
            Type accountType = account.GetType();
            try
            {
                if(account.checkDefaultBalance()<100) throw new Exception("Not enough start deposit");
                if (checkIfExists(account.AccountId)) throw new Exception($"User: {account.ClientName} already exists");
                switch (accountType.Name)
                {
                    case "DefaultAccount"   : { xmlSerializer=new XmlSerializer(typeof(DefaultAccount)); }
                        break;
                    case "OverdraftAccount" : {xmlSerializer=new XmlSerializer(typeof(OverdraftAccount));}
                        break;
                    default: throw new Exception("Wrong Type");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            //To avoid namespace creation
            var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            string xml=String.Empty;

            using (StringWriter stream = new StringWriter())
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                xmlSerializer.Serialize(writer, account, emptyNamepsaces);
                xml = stream.ToString();
            }

            if (!File.Exists(FILENAME))
            {
                using (FileStream fs = File.Create(FILENAME)){}
                using (XmlWriter writer = XmlWriter.Create(FILENAME, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("accounts");
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
            return true;
        }
        public static Account pull(string id)
        {
            Account account=null;

            try
            {
                if (!File.Exists(FILENAME))
                {
                    throw new FileNotFoundException();
                }
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(FILENAME);

                XmlNode res = xmlDocument.SelectSingleNode($"//*[@id='{id}']");
                if (res == null)
                {
                    return account;
                }
                Currency currency;
                double Balance;

                NodeData nd = parseDefaultAccount(res);
                
//                if (!Enum.TryParse<Currency>(res["currency"].InnerText, out currency))
//                {
//                    throw new Exception("Invalid currency");
//                }
//                if (!double.TryParse(res["Balance"].InnerText, out Balance))
//                {
//                    throw new Exception("Invalid balance type");
//                }
//                
                switch (res.Name)
                {
                    case "DefaultAccount"   : {
                        account = new DefaultAccount(nd);
                        } break;
                    case "OverdraftAccount" :
                    {
                        OverDraftData extra = parseOverdraftData(res);
                             account = new OverdraftAccount(nd,extra);
                        }
                        break;
                    default: throw new Exception("Invalid account");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            

            return account;
        }
        public static bool checkIfExists(string id)
        {
            return AccountXmlLoader.pull(id) != null ? true : false;
        }

        private static NodeData  parseDefaultAccount(XmlNode xmlNode)
        {
            try
            {
                string id = xmlNode.Attributes[0].InnerText;
                string name = xmlNode["ClientName"].InnerText;
                double balance = double.Parse(xmlNode["Balance"].InnerText);
                Currency cur;
                switch (xmlNode["currency"].InnerText)
                {
                    case "DOLLAR":
                        cur = Currency.DOLLAR;
                        break;
                    case "EURO":
                        cur = Currency.EURO;
                        break;
                    case "GRIVNA":
                        cur = Currency.GRIVNA;
                        break;
                    default:
                        cur = Currency.DOLLAR;
                        break;
                }
                return new NodeData(id, name, balance, cur);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new NodeData("","",0,Currency.DOLLAR);
            }   
        }
        private static OverDraftData parseOverdraftData(XmlNode xmlNode)
        {
            double percent = double.Parse(xmlNode["Percents"].InnerText);
            int limit = int.Parse(xmlNode["Limit"].InnerText);
            return new OverDraftData(limit,percent);
        }
        public static List<Account> pullAll()
        {
            List<Account> res=new List<Account>();
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(FILENAME);
                foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes)
                {
                    Account account;
                    
                    NodeData nd = parseDefaultAccount(xmlNode);
                    
                    switch (xmlNode.Name)
                    {
                        case "DefaultAccount": account=new DefaultAccount(nd); 
                            break;
                        case "OverdraftAccount":
                            OverDraftData extra = parseOverdraftData(xmlNode);    
                            account = new OverdraftAccount(nd,extra);
                        break;
                            default: account=new DefaultAccount(nd);
                                break;
                    }
                    res.Add(account);
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            
            return res;
        }

        public static void update(Account ac)
        {
            
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(FILENAME);
            
            if (xmlDocument.SelectSingleNode($"//*[@id='{ac.AccountId}']") == null) throw new Exception("Account not found");
            XmlNode root = xmlDocument.DocumentElement;
            XmlNode xmlNode;
            //string balance = xmlDocument.SelectSingleNode($"//*[@id='{id}']")["Balance"].InnerText;
            List<Transaction> list = TransactionXmlLoader.pull(ac.AccountId);
            
            
            xmlNode = root.SelectSingleNode($"//*[@id='{ac.AccountId}']")["Balance"];
            xmlNode.InnerText= ac.Balance.ToString();

            xmlNode = root.SelectSingleNode($"//*[@id='{ac.AccountId}']")["currency"];
            xmlNode.InnerText = ac.currency.ToString();

            xmlNode = root.SelectSingleNode($"//*[@id='{ac.AccountId}']")["ClientName"];
            xmlNode.InnerText = ac.ClientName.ToString();
            if (list != null)
            {
                xmlNode = root.SelectSingleNode($"//*[@id='{ac.AccountId}']")["journal"];
                xmlNode.InnerText = "";
                foreach (Transaction transaction in list)
                {
                    XmlNode child=xmlDocument.CreateElement("transaction");
                    child.InnerText = transaction.ToString();
                    xmlNode.AppendChild(child);
                }
            }
            xmlDocument.Save(FILENAME);
        }
        
        
    }
}