using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Configuration;
using MESWeb.Core;
using MESWeb.Models;

namespace MESWeb.Services
{
    public interface IPlanResultService : IService
    {
        IEnumerable<ResultData> GetHistory(string unitId);
    }

    public class PlanResultService : ServiceBase, IPlanResultService
    {
        public IEnumerable<ResultData> GetHistory(string id2)
        {
            IEnumerable<ResultData> result = new List<ResultData>();
            
            string query = string.Format(@"
            WITH _temHistory AS
            (
                SELECT _history.ID, _history.Dv_machineid, _history.ID2, strftime('%Y-%m-%d %H:%M:%S', _history.dv_date || _history.dv_time) AS DateCreated,
                    (SELECT COUNT(*) FROM T_ResultData AS _count WHERE _count.ID <= _history.ID ORDER BY _count.Dv_time) AS RownNmber
                FROM T_ResultData AS _history
                WHERE _history.Dv_date = date('now') AND _history.ID2 = '{0}'
                ORDER BY _history.Dv_time
            )
            SELECT _cur.*, strftime('%S', _next.DateCreated) - strftime('%S', _cur.DateCreated) AS Seconds
            FROM _temHistory AS _cur
            INNER JOIN _temHistory AS _next
            ON _next.RownNmber = _cur.RownNmber + 1", id2);

            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                result = _db.Query<ResultData>(query);
            });
            return result;
        }

    }
}