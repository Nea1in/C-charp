using System;
using System.Xml.Serialization;

namespace exma
{
    [Serializable]
    [XmlRoot("transaction")]
    

    public class Transaction
    {
        public DateTime date;
        public double Sum;
        public string purpose;
        public string SenderID;
        public string ReceiverID;
        public Currency currency;

        public bool Verified { get; set; }

        public Transaction(string SenderID,double Sum,string purpose,string receiverId,Currency currency)
        {
            try {
                
                if(Sum<=0) throw new Exception("Invalid sum of transaction");
                if (!AccountXmlLoader.checkIfExists(SenderID) || !AccountXmlLoader.checkIfExists(SenderID))
                {
                    throw new Exception("Account not found");
                    
                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            this.SenderID = SenderID;
            this.ReceiverID = receiverId;
            this.Sum = Sum;
            this.purpose = purpose;
            this.date=DateTime.Now;
            this.currency = currency;
        }

        public bool transact()
        {

            try
            {
                if (!AccountXmlLoader.checkIfExists(this.ReceiverID)) throw new Exception("Receiver not found");

                Account sender = AccountXmlLoader.pull(this.SenderID);
                Account receiver = AccountXmlLoader.pull(this.ReceiverID);


                //no commision
                if (receiver.currency == this.currency)
                {
                    receiver.Balance += this.Sum;
                }
                else
                {
                    double defaultvalue = this.Sum * (double) CurrencyProcessor.convertCoeficient(this.currency);
                    double receiverCurrencyValue =
                        defaultvalue / (double) CurrencyProcessor.convertCoeficient(receiver.currency);
                    receiver.Balance += receiverCurrencyValue;
                }
                AccountXmlLoader.update(receiver);
                this.Verified = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.Verified = false;
            }
            finally
            {
                TransactionXmlLoader.push(this);
            }
            return this.Verified;
        }
        public Transaction(){}
        
        public override string ToString()
        {
            return String.Format($"Transaction. Date: {date}, SenderID: {SenderID}, ReceiverID: {ReceiverID}, Currency: {currency}, Sum: {Sum}");
        }
    }
}