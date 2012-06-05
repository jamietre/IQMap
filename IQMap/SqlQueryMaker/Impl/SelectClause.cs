using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.SqlQueryBuilder.Impl
{
    /// <summary>
    /// THIS CODE IS UNFINISHED
    /// </summary>
    public class SelectClause: ISelectClause
    {
        #region constructor

        public SelectClause(string clause) {

        }
        public SelectClause(params ISelect[] components)
        {

        }

        #endregion

        #region private properties

        protected ParameterCollection _Parameters;

        #endregion

        #region public properties

        public ICollection<IDataParameter> Parameters
        {
            get
            {
                return IsEmpty ? 
                    QueryBuilder.EmptyParameterCollection :
                    (ICollection<IDataParameter>)_Parameters;
            }
        }

        #endregion

        public void Add(ISelectItem item)
        {
            throw new NotImplementedException();
        }

        public void Add(string clause)
        {
            throw new NotImplementedException();
        }

        public void Add(string field, string alias = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ISelectItem> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public ISelect Clone()
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ISqlQueryMaker Owner
        {
            get;
            set;
        }

        public string GetSql()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        ISqlClause ISqlClause.Clone()
        {
            throw new NotImplementedException();
        }
    }
}
