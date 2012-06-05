using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.SqlQueryBuilder.Impl
{
    public class SetComparer<T>
    {
        HashSet<T> Set1;
        public SetComparer(IEnumerable<T> set1, IEnumerable<T> set2)
        {
            if (set1.IsNullOrEmpty() || set2.IsNullOrEmpty())
            {
                AreEqual = true;
            }
            else
            {
                Set1 = new HashSet<T>(set1);
                if (Set1.Count != set2.Count())
                {
                    AreEqual = false;
                }
                else
                {
                    Set1.ExceptWith(set2);
                    AreEqual = Set1.Count == 0;
                }
            }
        }
        public bool AreEqual
        {
            get;
            protected set;
        }
    }
}
