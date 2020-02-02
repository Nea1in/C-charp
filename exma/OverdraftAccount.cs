using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace exma
{
     [Serializable]
    public class OverdraftAccount:DefaultAccount
     {
         public int Limit { get; set; }
         public double Percents { get; set; }
         
         public OverdraftAccount(string name,Currency currency,double startBalance,int limit):base(name,currency,startBalance)
         {
             this.journal=new List<Transaction>();
             Limit = limit;
             Percents = 0;
         }
            
         public OverdraftAccount(NodeData nd,OverDraftData xd):base(nd)
         {
             this.journal=new List<Transaction>();
             Limit = xd.limit;
             Percents = xd.percents;
         }
         public void setPercents(double rate)
         {
             if(this.Balance>0){ throw new Exception("The account has no debt"); return;}
             Percents = Math.Abs(this.Balance) * rate;
         }
         public void payPercents(double payment)
         {
             if(this.Balance>0){ throw new Exception("The account has no debt"); return;}
             Percents -= payment;
         }

         public void payDebt(double payment)
         {
             if(this.Balance>0){ throw new Exception("The account has no debt"); return;}
             this.Balance += payment;
             
         }         
         //for xml
         public OverdraftAccount(){}

         public override string ToString()
         {
             return $"Overdraft Account. ID: {AccountId}, Name: {ClientName}, Balance: {Balance}, Currency: {currency.ToString()}, Limit: {Limit}, Percents: {Percents}";
         }
         
         public override void paymentslashtransaction(double money,string purpose,string receiverId,Currency currency=Currency.DOLLAR)
         {
             double defaultBalance = (Math.Abs(this.Balance)+this.Limit) * (double)CurrencyProcessor.convertCoeficient(this.currency);
             double defaultMoney = money * (double)CurrencyProcessor.convertCoeficient(currency);
             Transaction transaction;
             try
             {
                 //default currency is DOLLAR
                 if (defaultBalance < defaultMoney) throw new Exception("Your balance has not enough money even with overdraft for this transaction");
				
                 transaction = new Transaction(this.AccountId, money, purpose, receiverId, currency);
                 
                 if (!transaction.transact()) throw new Exception("Something went wrong");
             }
             catch (Exception ex)
             {
                 Console.WriteLine(ex.Message);
                 return;
             }
             if (this.currency == currency)
             {
                 this.Balance -= money;
             }
             else this.Balance -= defaultMoney/(double)CurrencyProcessor.convertCoeficient(this.currency);
             transaction.Verified = true;
	        
             this.journal.Add(transaction);
	        
             AccountXmlLoader.update(this);
         }
         
     }
}