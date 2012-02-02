using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMapTest
{

        public class Dog
        {
            public int? Age { get; set; }
            public Guid Id { get; set; }
            public string Name { get; set; }
            public float? Weight { get; set; }

            public int IgnoredProperty { get { return 1; } }
        }

    
}
