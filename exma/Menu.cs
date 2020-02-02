using System;
using System.Collections.Generic;

namespace exma
{
    public class Menu
    {
        public Dictionary<int, string> categories;
        private Account currentAcc;
        
        public Menu(params string[] lines)
        {
            categories=new Dictionary<int, string>();
            int i = 1;
            foreach (var VARIABLE in lines)
            {
                categories.Add(i++,VARIABLE);                
            }
        }
        
        public void showLines()
        {
            foreach (string cat in categories.Values)
            {
                Console.WriteLine(cat);
            }
        } 
        public void Start()
        {
            while (true)
            {
                showLines(); 
                int key = int.Parse(Console.ReadLine());
                switch (key)
                {
                    case 1:  create();
                        break;
                        case 2: getInfo();
                            break;
                            case 3: showAll();
                                break;
                                case 4: transact();
                                    break;
                                    case 5: selectAccount();
                                        break;
                }
                Console.Clear();
            }
        }

        public bool showAll()
        {
            bool emptyList = false;
            List<Account> list=AccountXmlLoader.pullAll();
            if (list.Count<=0)
            {
                emptyList = true;
                Console.WriteLine("Empty list");
            }
            int i = 1;
            foreach (Account account in list)
            {
                Console.WriteLine($"{i++}: {account}");
            }
            Console.ReadKey();
            return emptyList;

        }

        public void selectAccount()
        {
            Console.WriteLine("Choose account:");
            List<Account> list = AccountXmlLoader.pullAll();
            int i = 1;
            foreach (Account account in list)
            {
                Console.WriteLine($"{i++}: {account}");
            }
            int key = int.Parse(Console.ReadLine());
            try
            {
                if(key<=0 || key> list.Count) throw new Exception("Wrong input");
                if(list[key-1]==null) throw new Exception("This account is not available");
                currentAcc = list[key-1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        public bool getInfo()
        {
            try
            {
                if (this.currentAcc == null) throw new Exception("Account not set");
                Console.WriteLine($"Your account {currentAcc}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            finally
            {
                Console.ReadKey();
            }
            return true;
        }
        
        public void create()
        {
            
            string name;
            Currency cur;
            double balance;
            bool type;
            int key;
           
            Console.WriteLine("Enter your name: ");
            name = Console.ReadLine();
            Console.WriteLine("Choose your currency:\n1:GRIVNA\n2:DOLLAR\n3:EURO");
            switch (int.Parse(Console.ReadLine()))
            {
                case 1: cur=Currency.GRIVNA;
                    break;
                    case 2: cur=Currency.DOLLAR;
                        break;
                        case 3: cur=Currency.EURO;
                            break;
                            default: cur=Currency.DOLLAR;
                                break;
            }
            Console.WriteLine("Enter start balance(100$):");
            balance = double.Parse(Console.ReadLine());
            
            Console.WriteLine("What account you need:\n1:Default\n2:Overdraft");
            
            Console.WriteLine("Your base limit is 100");
            if (int.Parse(Console.ReadLine())==2)
            {
                currentAcc=new OverdraftAccount(name,cur,balance,100);
            }
            else currentAcc=new DefaultAccount(name,cur,balance);
            if (!AccountXmlLoader.push(currentAcc)) currentAcc = null;
            Console.ReadKey();
        }

        public void transact()
        {
            List<Account> list = AccountXmlLoader.pullAll();
            if(!getInfo()) return;
            int i = 1;
            Console.WriteLine("\n\n");
            foreach (Account account in list)
            {
                Console.WriteLine($"{i++}: {account}");
            }
            if (list.Count <= 0)
            {
                Console.WriteLine("List is empty");
                return;
            }
            int key;
            Console.WriteLine("Set receiver");
            do
            {
                 key = int.Parse(Console.ReadLine());
            } while (key > list.Count || key<=0);
            
            Account receiver = list[key-1];
            
            Console.WriteLine("Money: ");
            double money = double.Parse(Console.ReadLine());
            string receiverId = receiver.AccountId;
            string senderId = currentAcc.AccountId;
            Console.WriteLine("Enter purpose: ");
            string purpose = Console.ReadLine();
            Currency cur;
            Console.WriteLine("Choose currency:\n1:GRIVNA\n2:DOLLAR\n3:EURO");
            switch (int.Parse(Console.ReadLine()))
            {
                case 1: cur=Currency.GRIVNA;
                    break;
                case 2: cur=Currency.DOLLAR;
                    break;
                case 3: cur=Currency.EURO;
                    break;
                default: cur=Currency.DOLLAR;
                    break;
            }
            
                if (currentAcc.GetType() == typeof(DefaultAccount))
                {
                     (currentAcc as DefaultAccount).paymentslashtransaction(money,purpose,receiverId,cur);                   
                }
                if (currentAcc.GetType() == typeof(OverdraftAccount))
                {
                    (currentAcc as OverdraftAccount).paymentslashtransaction(money,purpose,receiverId,cur);                   
                }
            Console.ReadKey();

        }
        
    }
}