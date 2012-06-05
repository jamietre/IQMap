using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Runtime.InteropServices;
using IQObjectMapper;

namespace IQMap.Impl
{

    public class DelegateInfo<T, U> : IQObjectMapper.Impl.DelegateInfo<T, U>,  IDelegateInfo where T : class
    {
        #region constructors

        public DelegateInfo(Func<T, object, U> getDel, Action<T, U, object> setDel)
            : base(getDel, setDel)
        {

        }

        public DelegateInfo(Func<T, U> getDel, Action<T, U> setDel)
            : base(getDel, setDel)
        {


        }
        public DelegateInfo(Func<object, object> getDel, Action<object, object> setDel)
            : base(getDel, setDel)
        {

        }
        #endregion

        #region private properties

        private string _SqlName;

        #endregion

        #region public properties

        public bool IsPrimaryKey { get; set; }

        public int Size {get;set;}

        public bool IsSqlReadOnly { get; set; }

        public bool IgnoreNull {get;set;}

        public string SqlName
        {
            get
            {
                return String.IsNullOrEmpty(_SqlName) ? 
                    Name :
                    _SqlName ;

            }
            set
            {
                _SqlName = value == "" ? null : value;
            }
        }

        #endregion

    }
}