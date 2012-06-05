using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Impl.Support
{
    public class LocalStringComparer: IEqualityComparer<string>
    {

        public LocalStringComparer(bool caseSensitive)
        {
            CaseSensitive = caseSensitive;
        }
        protected bool CaseSensitive;
        public bool Equals(string x, string y)
        {
            return CaseSensitive ? x.Equals(y, StringComparison.CurrentCulture) :
            x.Equals(y, StringComparison.CurrentCultureIgnoreCase);

        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
        
    }
}
