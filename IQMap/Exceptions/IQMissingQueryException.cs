﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Implementation
{
    public class IQMissingQueryException : IQException
    {
        public IQMissingQueryException(): base("No query was found.")
        {

        }
        public IQMissingQueryException(string message) : base(message)
        {

        }
    }
}
