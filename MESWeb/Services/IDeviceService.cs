using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Configuration;
using MESWeb.Core;
using MESWeb.Models;

namespace MESWeb.Services
{
    public interface IDeviceService : IService
    {
        IEnumerable<Device> GetDeviceList();
        IEnumerable<Device> GetDeviceList(int online);
        Device GetDevice(string mac);
        Device GetDevice(string columnName, string value);
    }

    public class DeviceService : ServiceBase, IDeviceService
    {
        public IEnumerable<Device> GetDeviceList()
        {
            List<Device> result = new List<Device>();
            List<Device> resultOnline = new List<Device>();

            //get all
            string cmd = "select * from Device";// Select<Device>().ToString();

            //get online only
            //string getOnline = Select<Device>(a => new { a.Dv_MAC, a.ID2 })
            //    .Where("(strftime('%s','now','localtime') - strftime('%s', LastAskUnitID)) < {0}", 60).ToString();
            string getOnline = "select * from Device where (strftime('%s','now','localtime') - strftime('%s', LastAskUnitID)) < 60";

            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                result = _db.Query<Device>(cmd).ToList();
                resultOnline = _db.Query<Device>(getOnline).ToList();
            });
            foreach (var item in result)
            {
                item.IsOnline = resultOnline.SingleOrDefault(a => a.Dv_MAC == item.Dv_MAC && a.ID2 == item.ID2) != null;
            }
            return result;
        }

        public IEnumerable<Device> GetDeviceList(int online)
        {
            //string getOnline = Select<Device>()
            //    .Where("(strftime('%s','now','localtime') - strftime('%s', LastAskUnitID)) < {0}", online).ToString();

            string getOnline = "select * from Device where (strftime('%s','now','localtime') - strftime('%s', LastAskUnitID)) < " + online;

            IEnumerable<Device> result = new List<Device>();
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                result = _db.Query<Device>(getOnline);
            });
            return result;
        }

        public Device GetDevice(string mac)
        {
            IEnumerable<Device> query;
            Device data = null;
            string cmdSingle = Select<Device>().Where(a => new { a.Dv_MAC }, Compare.Equal, mac).ToString();

            QuickSQLite connMes = new QuickSQLite(SQLiteConnString.Basic);
            connMes.Connect(_db =>
            {
                query = _db.Query<Device>(cmdSingle);
                if (query.Count() == 1)
                    data = query.FirstOrDefault();
            });

            return data;
        }

        public Device GetDevice(string columnName, string value)
        {
            IEnumerable<Device> query;
            Device data = null;
            //string cmdSingle = Select<Device>().Where(a => new { columnName = columnName }, Compare.Equal, value).ToString();

            string cmdSingle = string.Format("select * from Device where {0}='{1}'", columnName, value);

            QuickSQLite connMes = new QuickSQLite(SQLiteConnString.Basic);
            connMes.Connect(_db =>
            {
                query = _db.Query<Device>(cmdSingle);
                if (query.Count() == 1)
                    data = query.FirstOrDefault();
            });

            return data;
        }
    }
}