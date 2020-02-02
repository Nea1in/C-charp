using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace exma
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Menu menu = new Menu("Create", "GetInfo", "ShowAllAccounts","Transcation","Select Account");
            menu.Start();
        }
    }
}