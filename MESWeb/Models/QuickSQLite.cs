using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;
using Dapper;
using System.Data;

namespace MESWeb.Models
{
    public class QuickSQLite
    {
        private string _connection;
        private SQLiteConnection _sqliteConnection;

        public string ConnectionString { get { return _connection; } }

        public QuickSQLite(string connection)
        {
            _connection = connection;
        }

        public StateModel Connect(Action<SQLiteConnection> action)
        {
            StateModel state = new StateModel();
            try
            {
                using (_sqliteConnection = new SQLiteConnection(_connection))
                {
                    if (_sqliteConnection.State == ConnectionState.Closed)
                        _sqliteConnection.Open();
                    action(_sqliteConnection);
                }
                state.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _sqliteConnection = null;
                state.Error = ex;
                //throw ex;
            }
            return state;
        }
    }
}