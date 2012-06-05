using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.SqlQueryBuilder;

namespace IQMap.SqlQueryBuilder.Impl
{


    //public class UpdateClause: IUpdateClause
    //{
    //    #region constructors
    //    public static implicit operator UpdateClause(string where)
    //    {
    //        var clause = new WhereClause();
    //        clause.Add(where);
    //        return clause;
    //    }
    //    public UpdateClause()
    //    {
    //        JoinType = JoinType.And;
    //    }
    //    public UpdateClause(JoinType joinType)
    //    {
    //        JoinType = joinType;
    //    }
    //    public UpdateClause(JoinType joinType, params IWhere[] criteria)
    //    {
    //        JoinType = joinType;

    //        Add(criteria);
    //    }

    //    #endregion

    //    #region private properties

    //    protected List<IWhere> _InnerList;
    //    protected ParameterList _Parameters;

    //    List<IWhere> InnerList {
    //        get{
    //            if (_InnerList == null)
    //            {
    //                _InnerList = new List<IWhere>();
    //            }
    //            return _InnerList;
    //        }
    //    }

    //    protected string JoinOperator()
    //    {
    //        switch (JoinType)
    //        {
    //            case JoinType.And:
    //                return "AND";
    //            case JoinType.Or:
    //                return "OR";
    //            default:
    //                throw new InvalidOperationException("Unhandled join operator");
    //        }
    //    }
    //    protected bool HasParameters
    //    {
    //        get
    //        {
    //            return _Parameters != null && _Parameters.Count > 0;
    //        }
    //    }
    //    #endregion

    //    #region public properties

    //    public IEnumerable<IWhere> Items
    //    {
    //        get
    //        {
    //            if (!IsEmpty())
    //            {
    //                foreach (var item in InnerList)
    //                {
    //                    yield return item;
    //                }
    //            }
    //        }
    //    }
    //    public IEnumerable<IDataParameter> Parameters
    //    {
    //        get
    //        {
    //            if (HasParameters)
    //            {
    //                foreach (var item in _Parameters)
    //                {
    //                    yield return item;
    //                }
    //            }

    //        }
    //    }

    //    public JoinType JoinType { get; set; }
    //    public ISqlQueryBuilder Owner
    //    {
    //        get
    //        {
    //            return _Owner;
    //        }
    //        set
    //        {
    //            foreach (IWhere item in InnerList)
    //            {
    //                item.Owner = value;
    //            }
    //            _Owner = value;
    //        }
    //    }
    //    protected ISqlQueryBuilder _Owner;

      
       
    //    public int Count
    //    {
    //        get
    //        {
    //            return InnerList.Count;
    //        }
    //    }
       
    //    #endregion

       

    //    #region public methods


    //    public virtual IWhereClause Add(IWhere condition)
    //    {

    //        MergeWhere(condition, JoinType.And);
    //        return this;
    //    }
    //    public IWhereClause Add(IEnumerable<IWhere> criteria)
    //    {
    //        if (criteria != null)
    //        {
    //            foreach (var item in criteria)
    //            {
    //                Add(item);
    //            }
    //        }
    //        return this;
    //    }

    //    /// <summary>
    //    /// Add a simple "field = value" condition
    //    /// </summary>
    //    /// <param name="condition"></param>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public virtual IWhereClause Add(string field, object value, bool parameterize=true)
    //    {
    //        return Add(field, value, ComparisonType.Equal);
    //    }
    //    public virtual IWhereClause Add(string field, object value, ComparisonType comparisonType, bool parameterize=true)
    //    {
    //        return Add(field, value, comparisonType, JoinType.And, parameterize);
    //    }
    //    public IWhereClause Add(string field, object value, ComparisonType comparisonType, JoinType joinType, bool parameterize = true)
    //    {
    //        IFieldValuePair crit;

    //        crit = new WhereItem(field, value, comparisonType);
    //        MergeWhere(crit, joinType);
    //        return this;
    //    }
    //    public virtual IWhereClause Add(string condition, IEnumerable<IDataParameter> parameters = null)
    //    {
    //        WhereString where = new WhereString(condition, parameters);
    //        MergeWhere(where, JoinType.And);
    //        return this;
    //    }

    //    public virtual IWhereClause AddOr(IWhere condition)
    //    {
    //        MergeWhere(condition, JoinType.Or);
    //        return this;
    //    }
    //    public virtual IWhereClause AddOr(string condition, IEnumerable<IDataParameter> parameters=null)
    //    {
    //        WhereString where = new WhereString(condition, parameters);
    //        MergeWhere(where, JoinType.Or);
    //        return this;
    //    }
      
    //    /// <summary>
    //    /// Returns true if any components of this query are of a different join type than the test
    //    /// </summary>
    //    public bool IsCompoundFor(JoinType joinType)
    //    {
    //        return Count > 1 &&
    //            (JoinType != joinType ||
    //            InnerList.Any(item => item is WhereClause && ((WhereClause)item).JoinType != JoinType));
    //    }
    //    public string GetSql()
    //    {
            
    //        string output = "";
    //        if (!IsEmpty())
    //        {
    //            if (InnerList.Count == 1)
    //            {
    //                return InnerList[0].GetSql();
    //            }
    //            else
    //            {
    //                foreach (var member in InnerList)
    //                {
    //                    string childSql = member.GetSql();
    //                    if (childSql != "")
    //                    {
    //                        output += (output == "" ? "" : " " + JoinOperator() + " ") +
    //                            (member.IsCompoundFor(JoinType) ? "(" + childSql + ")" :
    //                            childSql);
    //                    }
    //                }
    //            }
    //        }
    //        return output;
    //    }
       
       
    //    public void Clear()
    //    {
    //        _InnerList = null;
    //    }
    //    public WhereClause Clone()
    //    {
    //        WhereClause clone = new WhereClause();
    //        clone.JoinType = JoinType;
    //        foreach (var item in InnerList)
    //        {
    //            clone.Add(item.Clone());
    //        }
    //        return clone;
    //    }

    //    public string GetParameterName(string basedOn = "")
    //    {
    //        return _Parameters.GetParameterName();
    //    }
    //    public override int GetHashCode()
    //    {
    //        return GetSql().GetHashCode();
    //    }
    //    public override bool Equals(object obj)
    //    {
    //        return obj is IWhere &&
    //            ((IWhere)obj).GetSql() == GetSql();
    //    }

    //    public override string ToString()
    //    {
    //        return GetSql();
    //    }

    //    public void Touch()
    //    {
    //        if (Owner != null)
    //        {
    //            Owner.Touch();
    //        }
    //    }

    //    #endregion

    //    #region private methods

    //    public bool IsEmpty()
    //    {
    //        return _InnerList == null || InnerList.Count == 0;
    //    }

    //    /// <summary>
    //    /// Adds a condition to the "Where" property, and returns self for chainability
    //    /// </summary>
    //    /// <param name="condition"></param>
    //    /// <param name="conditionType"></param>
    //    /// <returns></returns>
    //    protected void MergeWhere(IWhere item, JoinType joinType)
    //    {
    //        if (ReferenceEquals(item, this))
    //        {
    //            throw new InvalidOperationException("You're trying to add a where clause to itself.");
    //        }
    //        if (joinType == JoinType.Or && IsEmpty())
    //        {
    //            throw new InvalidOperationException("There is no existing condition to OR");
    //        }
    //        if (JoinType == joinType)
    //        {
    //            InnerList.Add(item);
    //            item.Owner = this.Owner;
    //        }
    //        else
    //        {
    //            if (!IsEmpty())
    //            {
    //                // currently just a single criterion
    //                var clause = new UpdateClause(joinType, this, item);
    //                _InnerList = clause.InnerList;
    //            }
    //            else
    //            {
    //                InnerList.Add(item);
    //                item.Owner = this.Owner;
    //            }
    //        }
    //        Touch();
    //    }

    //    protected void RecurseList(IWhere list, Action<IFieldValuePair> callback)
    //    {
    //        IWhereClause clause = list as IWhereClause;
    //        if (clause!=null) {
    //            foreach (var item in clause.Items)
    //            {
    //                IFieldValuePair innerItem = item as IFieldValuePair;
    //                if (innerItem != null)
    //                {
    //                    callback(innerItem);
    //                }
    //                else
    //                {
    //                    RecurseList(innerItem,callback);
    //                }
    //            }
    //        }

    //    }

    //    #endregion

    //    #region private methods

    //    ISqlQueryBuilder ISqlClause.Owner
    //    {
    //        get
    //        {
    //            return Owner;
    //        }
    //        set
    //        {
    //            Owner = (SqlQueryComponent)value;
    //        }
    //    }

    //    ISqlClause ISqlClause.Clone()
    //    {
    //        return Clone();
    //    }
    //    IWhereClause IWhereClause.Clone()
    //    {
    //        return Clone();
    //    }
    //    IWhere IWhere.Clone()
    //    {
    //        return Clone();
    //    }

    //    #endregion

    // }
    
}
