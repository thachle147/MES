using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace MESWeb.Core
{
    public class QueryStringBuilder
    {
        public IQueryBuilder<TEntity> Select<TEntity>()
        {
            QueryBuilder<TEntity> result = new QueryBuilder<TEntity>();
            result.Select = "*";
            return result;
        }

        public IQueryBuilder<TEntity> Select<TEntity>(Func<TEntity, object> selectFn)
        {
            QueryBuilder<TEntity> result = new QueryBuilder<TEntity>();
            result.Select = result._columns(selectFn);
            return result;
        }
    }

    public static class ExtendQueryBuilder
    {
        public static IQueryBuilder<TEntity> Where<TEntity>(this IQueryBuilder<TEntity> select, string comparison, params object[] parameters)
        {
            QueryBuilder<TEntity> result = (QueryBuilder<TEntity>)select;
            result.Where = string.Format(comparison, parameters);
            return result;
        }

        public static IQueryBuilder<TEntity> Where<TEntity>(this IQueryBuilder<TEntity> select, Func<TEntity, object> fn, Compare compare, object value)
        {
            QueryBuilder<TEntity> result = (QueryBuilder<TEntity>)select;
            result.Where = string.Format("{0} {1} '{2}'", result._column(fn), result._compare(compare), value);
            return result;
        }

        public static IQueryBuilder<TEntity> In<TEntity, T>(this IQueryBuilder<TEntity> select, IEnumerable<T> list, Func<TEntity, object> columnObj)
        {
            QueryBuilder<TEntity> result = (QueryBuilder<TEntity>)select;
            result.Where = string.Format("{0} IN ({1})", result._column(columnObj), string.Join(",", list.ToArray()));
            return result;
        }
    }

    public interface IQueryBuilder<T>
    {
    }

    public class QueryBuilder<T> : IQueryBuilder<T>
    {
        public string Select { get; set; }
        public string From { get { return typeof(T).Name; } }
        public string Where { get; set; }

        public string _compare(Compare type)
        {
            string _type = "";
            switch (type)
            {
                case Compare.Equal:
                    _type = "=";
                    break;
                case Compare.GreaterThan:
                    _type = ">";
                    break;
                case Compare.GreaterThanOrEqual:
                    _type = ">=";
                    break;
                case Compare.LessThan:
                    _type = "<";
                    break;
                case Compare.LessThanOrEqual:
                    _type = "<=";
                    break;
                case Compare.NotEqual:
                    _type = "<>";
                    break;
            }
            return _type;
        }

        public string _columns<TEntity>(Func<TEntity, object> selectFn)
        {
            TEntity _default = (TEntity)Activator.CreateInstance(typeof(TEntity));
            var columnObj = selectFn(_default);
            string[] columns = columnObj.GetType().GetProperties().Select(a => a.Name).ToArray();
            return string.Join(", ", columns);
        }

        public string _column<TEntity>(Func<TEntity, object> selectFn)
        {
            TEntity _default = (TEntity)Activator.CreateInstance(typeof(TEntity));
            var columnObj = selectFn(_default);
            var column = columnObj.GetType().GetProperties().FirstOrDefault();
            return column != null ? column.Name : "";
        }

        private string _where
        {
            get
            {
                if (!string.IsNullOrEmpty(Where))
                {
                    return "WHERE " + Where;
                }
                return "";
            }
        }

        public override string ToString()
        {
            return string.Format("SELECT {0} FROM {1} {2}", Select, From, _where);
        }
    }

    public class Comparer
    {
        public Comparer()
        {
        }

        public Comparer(object left, object logic, object right)
        {

        }

        public Comparer(Operator Operation, object left, object logic, object right)
        {

        }

        public Operator Operation { get; set; }
        public object Left { get; set; }
        public object Logic { get; set; }
        public object Right { get; set; }
        public List<Comparer> Childs { get; set; }
    }

    public enum Operator { And, Or, In }

    public enum Compare
    {
        /// <summary>
        /// <para>=</para>
        /// <para>Equal to</para>
        /// </summary>
        Equal,
        /// <summary>
        /// >
        /// </summary>
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        NotEqual
    }
}