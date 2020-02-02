using System.Collections.Generic;
namespace exma
{
    public struct NodeData
    {
        public string AccountId;
        public string clientName;
        public double balance;
        public Currency currency;
        public List<Transaction> JournalList;

        public NodeData(string id, string name, double bal, Currency cur,List<Transaction> list=null)
        {
            AccountId = id;
            clientName = name;
            balance = bal;
            currency = cur;
            JournalList = list;
        }
    }
    public struct OverDraftData
    {
        public int limit;
        public double percents;

        public OverDraftData(int limit, double p)
        {
            this.limit = limit;
            this.percents = p;
        }
    }
}