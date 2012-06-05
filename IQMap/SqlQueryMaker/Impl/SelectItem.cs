using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.Impl;

namespace IQMap.SqlQueryBuilder.Impl
{
    public class SelectItem: ISelectItem
    {
        #region constructors

        private static ICollection<IDataParameter> EmptyCollection = new List<IDataParameter>();

        public SelectItem()
        {
         
        }
        public SelectItem(string clause)
        {
            Parse(clause);
        }
        public SelectItem(string field, string alias = null)
        {
            Field = field;
            Alias = alias;
        }

        #endregion

        protected Lazy<ICollection<IDataParameter>> InnerParameters 
            = new Lazy<ICollection<IDataParameter>>();
        
        #region public methods

        public ICollection<IDataParameter> Parameters 
        {
            get
            {
                return !InnerParameters.IsValueCreated ?
                    EmptyCollection :
                    InnerParameters.Value;
            }
        }

        public string Field
        {
            get;
            set;
        }

        public string Alias
        {
            get;
            set;
        }

        public bool IsEmpty
        {
            get {
                return String.IsNullOrEmpty(Field);
            }
        }

        public ISqlQueryMaker Owner
        {
            get;
            set;
        }

        public string GetSql()
        {
            return String.Format("{0}{1}{2}",
                Field ?? "",
                String.IsNullOrEmpty(Alias) ? "" : " AS ",
                Alias ?? "");

        }

        public void Clear()
        {
            Field = null;
            Alias = null;
        }
        public ISelectItem Clone()
        {
            return new SelectItem(Field,Alias);
            
        }
        public override int GetHashCode()
        {
            return (Field == null ? 0 : Field.GetHashCode()) +
                (Alias == null ? 0 : Alias.GetHashCode());
        }
        public override bool Equals(object obj)
        {
            SelectItem item = obj as SelectItem;
            return item != null &&
                item.Field == Field &&
                item.Alias == Alias;
        }

        #endregion

        #region private methods

        void Parse(string clause)
        {
            if (clause==null)
            {
                throw new ArgumentNullException("Null value is not a valid SelectItem constructor.");
            }
            string[] parts = clause.SplitClean(' ');

            bool fail = false;
            switch (parts.Length)
            {
                case 0:
                    return;
                case 1:
                    Field = parts[0];
                    break;
                case 2:
                    fail = true;
                    break;
                case 3:
                    if (!parts[1].EqualsIgnoreCase("as"))
                    {
                        fail = true;
                        break;
                    }
                    Field = parts[0];
                    Alias = parts[2];
                    break;
                default:
                    fail = true;
                    break;
            }
            if (fail)
            {
                throw new ArgumentException("Invalid string passed as a SelectItem constructor");
            }
        }

        public override string ToString()
        {
            return GetSql();
        }
        ISqlClause ISqlClause.Clone()
        {
            return Clone();
        }
        ISelect ISelect.Clone()
        {
            return Clone();
        }

        #endregion
    }
}
