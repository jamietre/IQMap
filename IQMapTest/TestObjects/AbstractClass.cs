using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMapTest
{
 
    public abstract class OrderAbstract
    {
        internal int Internal { get; set; }
        public int ProtectedSet { get; protected set; }
        public int Public { get; set; }

        public int ProtectedVal { get { return ProtectedSet; } }
    }

    public class ConcreteOrder : OrderAbstract
    {
        public int Concrete { get; set; }
        private int PrivateGetSet { get; set; }
        public int ProtectedGet { protected get; set; }

        public int PrivateGetSetVal
        {
            get
            {
                return PrivateGetSet;
            }
        }
        public int ProtectedGetVal
        {
            get
            {
                return ProtectedGet;
            }
        }
    }

    
}
