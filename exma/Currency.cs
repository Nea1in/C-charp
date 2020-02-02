using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exma
{
    public class CurrencyProcessor
    {
        public static decimal grivna = 0.03841625M;
        public static decimal euro = 1.1842153123M;

        public static decimal convertCoeficient(Currency from)
        {
            decimal koef=1M;
            switch (from)
            {
                case Currency.EURO:   koef = euro; break;
                case Currency.GRIVNA: koef = grivna;break;
                case Currency.DOLLAR: koef = 1;break;
                
            }
            return koef;
        }
    }
}
