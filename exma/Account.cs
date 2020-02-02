using System;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace exma
{	
    public enum Currency {DOLLAR=1,GRIVNA=2,EURO=3}
    [Serializable]
    public abstract class Account
    {
        [XmlAttribute("id")]            
        public string AccountId {get;set;}
    public const double START_BALANCE = 100;

        public string ClientName { get; set; }
        public Currency currency { get; set; }
        public double Balance { get; set; }

        public double checkDefaultBalance()
        {            
            return Balance*(double)CurrencyProcessor.convertCoeficient(currency);
        }

        public Account()
        {
            
        }
        protected Account(string name,Currency currency,double startBalance)
        {
            //default dollars
            
            this.ClientName = name;
            this.currency = currency;
            
            try
            {
                MD5 md5 = MD5.Create();
                
                this.AccountId = CustomMD5.GetMd5Hash(md5, String.Format($"User:{name}"));
                //Start balance 100 dollars
                //TODO: converter
                double balance = startBalance * (double) CurrencyProcessor.convertCoeficient(this.currency);
                
                if(balance<START_BALANCE) 
                    throw new Exception("Invalid starting deposit");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
            this.Balance = startBalance;    
        }
        //call only if exists
        protected Account(string id,string name, Currency currency,double balance)
        {
            const double START_BALANCE = 100;
            
            this.ClientName = name;
            this.currency = currency;
            this.AccountId = id;
            this.Balance = balance;  
        }
        
        

        
       
        
        
    }
}