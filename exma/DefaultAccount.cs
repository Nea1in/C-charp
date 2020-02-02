using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace exma
{   /*1.2.(2) клас "Поточний рахунок" (звичайний рахунок, який дозволяє здійснювати розрахунки між контрагентами)
	Поля: журнал операцій(транзакцій) по рахунку. Запис журналу повинен містити такі дані: дата транзакції, сума транзакції, призначення платежу, номер рахунку контрагента;
	Методи: метод списання(переведення) коштів з рахунку на інший рахунок - кошти списуються за допомогою методу, а надходять на рахунок контрагента через подію "банківська транзакція".
	Передбачити можливість розрахунків між рахунками в різних валютах. Для визначення курсу валюти на поточну дату, наприклад гривні, можна використовувати 
*/




    
    //TODO: event
    [Serializable]
	[XmlRoot("DefaultAccount")]
	[XmlInclude(typeof(Account))]
	public class DefaultAccount:Account
	{
        public event BankTransaction bt;
        public delegate bool BankTransaction();


        [XmlArray]
		public List<Transaction> journal;
		
		public Transaction this[int index]{get { return journal[index]; }}
				
		public DefaultAccount(string name,Currency currency,
			double startBalance):base(name,currency,startBalance)
		{
			this.journal = new List<Transaction>();
			
		}
		public DefaultAccount(NodeData nd):base(nd.AccountId,nd.clientName,nd.currency,nd.balance)
		{
			if (nd.JournalList == null)
			{
				this.journal=new List<Transaction>();
			}
		}
        //for xml serializing
		public DefaultAccount()
		{
			
		}


        public virtual void paymentslashtransaction(double money,string purpose,string receiverId,Currency currency=Currency.DOLLAR)
        {
            double defaultBalance = this.Balance * (double)CurrencyProcessor.convertCoeficient(this.currency);
            double defaultMoney = money * (double)CurrencyProcessor.convertCoeficient(currency);
	        Transaction transaction;
            try
            {
                //default currency is DOLLAR
                if (defaultBalance < defaultMoney) throw new Exception("Your balance has not money for this transaction");
				
	            
                transaction = new Transaction(this.AccountId, money, purpose, receiverId, currency);
                this.bt += transaction.transact;
                if (!bt.Invoke()) throw new Exception("Something went wrong");
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
		public override string ToString()
		{
			return String.Format($"Default account: {AccountId}" +
			                     $"ClientName: {ClientName}" +
			                     $"Currency:{currency.ToString()}" +
			                     $"Balance: {Balance}");
		}
	}
}