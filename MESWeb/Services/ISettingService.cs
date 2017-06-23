using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Configuration;
using MESWeb.Core;
using MESWeb.Models;
using System.Reflection;

namespace MESWeb.Services
{
    public interface ISettingService : IService
    {
        Setting Get(long id);
        Setting Get(string key);
        Setting GetValue(string key);
        string[] SourcePlanColumns();
        string[] ExportPlanColumns();
        StateModel Update(Setting model);
        StateModel Update(string key, string value);
    }

    public class SettingService : ServiceBase, ISettingService
    {
        public Setting Get(long id)
        {
            IEnumerable<Setting> query;
            Setting data = null;
            string cmdSingle = Select<Setting>().Where(a => new { a.ID }, Compare.Equal, id).ToString();

            QuickSQLite connMes = new QuickSQLite(SQLiteConnString.MESDB);
            connMes.Connect(_db =>
            {
                query = _db.Query<Setting>(cmdSingle);
                if (query.Count() == 1)
                    data = query.FirstOrDefault();
            });

            return data;
        }

        public Setting Get(string key)
        {
            IEnumerable<Setting> query;
            Setting data = null;
            string cmdSingle = Select<Setting>().Where(a => new { a.Key }, Compare.Equal, key).ToString();

            QuickSQLite connMes = new QuickSQLite(SQLiteConnString.MESDB);
            connMes.Connect(_db =>
            {
                query = _db.Query<Setting>(cmdSingle);
                if (query.Count() == 1)
                    data = query.FirstOrDefault();
            });

            return data;
        }

        public Setting GetValue(string key)
        {
            IEnumerable<Setting> query;
            Setting data = null;
            string cmdSingle = Select<Setting>(a => new { a.Value, a.Key, a.ID }).Where(a => new { a.Key }, Compare.Equal, key).ToString();

            QuickSQLite connMes = new QuickSQLite(SQLiteConnString.MESDB);
            connMes.Connect(_db =>
            {
                query = _db.Query<Setting>(cmdSingle);
                if (query.Count() == 1)
                    data = query.FirstOrDefault();
            });

            return data;
        }

        public string[] SourcePlanColumns()
        {
            return typeof(PlanData).GetProperties()
                .Where(a => !ExportExcelSetting.FixedColumn.Contains(a.Name))
                .Select(a => a.Name).ToArray();
        }

        public string[] ExportPlanColumns()
        {
            List<string> result = new List<string>();
            Setting exportColumns = GetValue(SettingKeys.ExcelExportColumn);
            if (exportColumns != null)
                result = exportColumns.Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();
            return result.ToArray();
        }

        public StateModel Update(Setting model)
        {
            StateModel state;
            string cmdUpdate = string.Format("update Setting set Value='{0}', DateModified=datetime('now') where ID={1}", model.Value, model.ID);
            QuickSQLite connMes = new QuickSQLite(SQLiteConnString.MESDB);
            state = connMes.Connect(_db =>
            {
                _db.Execute(cmdUpdate);
            });
            return state;
        }

        public StateModel Update(string key, string value)
        {
            StateModel state;
            string cmdUpdate = string.Format("update Setting set Value='{0}', DateModified=datetime('now') where Key='{1}'", value, key);
            QuickSQLite connMes = new QuickSQLite(SQLiteConnString.MESDB);
            state = connMes.Connect(_db =>
            {
                _db.Execute(cmdUpdate);
            });
            return state;
        }

    }
}