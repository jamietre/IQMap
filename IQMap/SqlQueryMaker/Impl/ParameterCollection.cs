using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.SqlQueryBuilder;

namespace IQMap.SqlQueryBuilder.Impl
{
    public class ParameterCollection: IParameterCollection, ICollection<IDataParameter>
    {
        #region private properties


        /// <summary>
        /// Parameters names MUST include the @ in this list
        /// </summary>
        protected HashSet<string> _ParameterNames;
        private List<IDataParameter> _InnerList;

        protected List<IDataParameter> InnerList
        {
            get
            {
                if (_InnerList == null)
                {
                    _InnerList = new List<IDataParameter>();
                    _ParameterNames  = new HashSet<string>();
                }
                // Only allow adding from methods
                return _InnerList;
            }
        }
        #endregion

        #region public properties

        public SqlQueryMaker Owner
        {
            get;
            set;
        }

        public int AutoParameterIndex { get; set; }
        public static bool OptimizeParameterNames { get; set; }
        public int Count
        {
            get
            {
                return InnerList.Count;
            }
        }

        #endregion

        #region public methods

        public void Clear()
        {
            _InnerList = null;
            _ParameterNames = null;
            AutoParameterIndex = 0;
            Touch();
        }
        public void Touch()
        {
            if (Owner != null)
            {
                Owner.Touch();
            }
        }

        public bool ContainsKey(string parameterName)
        {
            return IsEmpty ?
                false :
                _ParameterNames.Contains(FormatParameterName(parameterName));

        }
        /// <summary>
        /// Return an unused parameter name WITHOUT the @
        /// </summary>
        /// <returns></returns>
        public string GetNewParameterName(string basedOn = "")
        {
            string name, test;
            if (OptimizeParameterNames && basedOn.Length > 3)
            {
                name = "@p";
                test = "@p" + AutoParameterIndex.ToString();
            }
            else
            {
                name = FormatParameterName(basedOn);
                test = name;
            }


            while (ContainsKey(test))
            {
                AutoParameterIndex++;
                test = name + AutoParameterIndex.ToString();
            }
            return test;
        }

        /// <summary>
        /// Adds an SQL parameter to the query
        /// </summary>
        /// <param name="parm"></param>
        public void Add(IDataParameter parm)
        {

            if (ContainsKey(parm.ParameterName))
            {
                IDataParameter existing = InnerList.FirstOrDefault(item => item.ParameterName == parm.ParameterName);
                if (existing.Value != parm.Value)
                {
                    throw new ArgumentException("A parameter named \"" + parm.ParameterName + "\" with a different value has already been added to this query.");
                }
            }
            else
            {
                // touch it - this simply forces list to be created. We want creation code in the property so that methods can freely
                // check its members, but we also want it to be a read-only list so you must add using the innner property. this
                // makse sure this code block is the only place that can add to it (since it takes special effort to do so) as we
                // must keep the name list synchronized
                InnerList.FirstOrDefault();
                _InnerList.Add(parm);
                _ParameterNames.Add(parm.ParameterName);
                Touch();
            }

        }
        /// <summary>
        /// Adds a list of parameters to the query
        /// </summary>
        /// <param name="parameterList"></param>
        public void Add(IEnumerable<IDataParameter> parameterList)
        {
            foreach (var parm in parameterList)
            {
                Add(parm);
            }

        }
        /// <summary>
        /// Add a paremeter to the list, @ will be added to name automatically if not present
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, object value)
        {
            SqlValue val = new SqlValue(value);
            Add(new QueryParameter(FormatParameterName(name), val.Value));
        }

        public IParameterCollection Clone()
        {
            ParameterCollection list = new ParameterCollection();
            foreach (var item in this)
            {

                var parm = new QueryParameter(item.ParameterName, item.Value);
                parm.Direction = item.Direction;
                list.Add(parm);
            }
            return list;
        }
        public override int GetHashCode()
        {
            return InnerList.OrderBy(item => item.ParameterName).Sum(item => item.GetHashCode());
        }
        public override bool Equals(object obj)
        {
            ParameterCollection list = obj as ParameterCollection;


            if (list == null ||
                list.InnerList.Count != InnerList.Count)
            {
                return false;
            };
            HashSet<IDataParameter> test = new HashSet<IDataParameter>(InnerList);
            test.ExceptWith(list);
            return test.Count == 0;
        }
        #endregion

        #region private methods

        /// <summary>
        /// There are no parameters in the collection
        /// </summary>
        protected bool IsEmpty
        {
            get
            {
                return _InnerList == null || InnerList.Count == 0;
            }
        }

        private string FormatParameterName(string name)
        {
            return (name.Substring(0, 1) == "@" ? "" : "@") + name;
        }


        ISqlQueryMaker IParameterCollection.Owner
        {
            get;
            set;
        }


        #endregion

        public IEnumerator<IDataParameter> GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public bool Contains(IDataParameter item)
        {
            return InnerList.Contains(item);
        }

        public void CopyTo(IDataParameter[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IDataParameter item)
        {
            return Remove(item);
        }
    }
}
