using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.SqlQueryBuilder.Impl
{
    /// <summary>
    /// Legacy - to support unstructured where stuff
    /// </summary>
    public class WhereString: IWhere
    {
        #region constructor

        public WhereString()
        {

        }
        public WhereString(string clause, params IDataParameter[] parameters)
        {
            Where = clause;
            if (parameters != null)
            {
                InnerParameterCollection.Add(parameters);
            }
        }
        #endregion

        #region private properties

        protected IParameterCollection _InnerParameterCollection;
        protected IParameterCollection InnerParameterCollection
        {
            get
            {
                if (_InnerParameterCollection == null)
                {
                    _InnerParameterCollection = new ParameterCollection();
                }
                return _InnerParameterCollection;
            }
        }
        protected bool HasParameters
        {
            get
            {
                return _InnerParameterCollection != null;
            }
        }
        #endregion

        #region public properties

        public ICollection<IDataParameter> Parameters
        {
            get
            {
                return HasParameters ?
                    (ICollection<IDataParameter>)InnerParameterCollection :
                    QueryBuilder.EmptyParameterCollection;
            }
        }


        public ISqlQueryMaker Owner
        {
            get;
            set;
        }
        public string Where { get; set; }
     
        #endregion

        #region public methods

        public IWhere Clone()
        {
            WhereString clone = new WhereString();
            clone.Where = Where;
            if (HasParameters)
            {
                clone._InnerParameterCollection = _InnerParameterCollection.Clone();
            }
            return clone;
        }
        public string GetSql()
        {
            return Where;
        }
        // Always compoundNever because we don't know how it was constructed
        public bool MustParenthesizeFor(JoinType joinType)
        {
            return true;
        }
        public override int GetHashCode()
        {
            return GetSql().GetHashCode() + Parameters.Sum(item => item.Value.GetHashCode());
        }
        public override bool Equals(object obj)
        {
            IWhere other = obj as IWhere;
            if (other==null || GetSql()!=other.GetSql()) {
                return false; 
            }
            SetComparer<IDataParameter> comparer = new SetComparer<IDataParameter>(Parameters,other.Parameters);
            return comparer.AreEqual;
        }
        public override string ToString()
        {
            return GetSql();
        }

        public void Clear()
        {

        }

        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(Where);
            }
        }

        ISqlClause ISqlClause.Clone()
        {
            return Clone();
        }

        #endregion
        
    }
}
