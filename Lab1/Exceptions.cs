using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    public class TimestampException : Exception
    {
        public TimestampException() : base("Message timestamp less than LBTS") { }
    }
}
