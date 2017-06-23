using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Dapper;
using System.Configuration;

namespace MESWeb.Models
{
    public class SQLiteConnString
    {
        public static string MESDB { get { return string.Format(@"{0}", ConfigurationManager.AppSettings["SQLiteDBMESConnStr"]); } }
        public static string Basic { get { return ConfigurationManager.AppSettings["SQLiteDBBasicConnStr"]; } }
        public static string History { get { return ConfigurationManager.AppSettings["SQLiteDBResultConnStr"]; } }
    }

    public class ChartSettings
    {
        private static string[] colors = ConfigurationManager.AppSettings["Chart.Color"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        public static string TargetColor { get { return colors[0]; } }
        public static string Lv1Color { get { return colors[1]; } }
        public static string Lv2Color { get { return colors[2]; } }
        public static string Lv3Color { get { return colors[3]; } }
        public static int CountingThreshold { get { return int.Parse("0" + ConfigurationManager.AppSettings["Chart.CountingThreshold"]); } }
    }

    public class SettingKeys
    {
        public static string ExcelExportColumn { get { return "Excel.ExportColumn"; } }
        public static string PlanChartIntervalReload { get { return "PlanChart.IntervalReload"; } }
        public static string PlanChartColors { get { return "PlanChart.Colors"; } }
        public static string PlanChartThreshold { get { return "PlanChart.Threshold"; } }
    }

    public class ExportExcelSetting
    {
        public static List<string> FixedColumn { get { return new List<string>() { "ID", "Dv_MAC", "Cus_company" }; } }
    }

    /// <summary>
    /// Do not use class Utilities for query data anymore. All methods about Plan will be move to PlanService
    /// </summary>
    public class Utilities
    {
        public static int SendTargetToDevice(string idList)
        {
            int recordAffected = 0;
            string cmd = string.Format("update PlanData set IsSend = 1, ChangeTime = datetime('now', 'localtime') where id in ({0})", idList);
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                recordAffected = _db.Execute(cmd);
            });
            return recordAffected;
        }

        public static List<PlanData> GetChartData(IEnumerable<long> idList)
        {
            string getPlans = "select * from PlanData where ID in (" + string.Join(",", idList.ToArray()) + ")";
            List<PlanData> _plans = new List<PlanData>();

            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                _plans = _db.Query<PlanData>(getPlans).ToList();
            });

            return _plans;
        }


        public static List<ResultData> GetChartData(IEnumerable<long> idList, string _date)
        {
            List<ResultData> results = new List<ResultData>();
            string getPlans = "select * from PlanData where ID in (" + string.Join(",", idList.ToArray()) + ")";
            List<PlanData> _plans = new List<PlanData>();
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                _plans = _db.Query<PlanData>(getPlans).ToList();
            });

            results = _plans.Select(a => new ResultData() { Dv_counting = a.Counting, Dv_target = a.Cus_targetqty, Dv_machineid = a.Cus_machineid }).ToList();

            return results.ToList();

        }

        public static List<PlanData> GetPlanList(IEnumerable<long> idList)
        {
            List<PlanData> result = new List<PlanData>();
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                result = _db.Query<PlanData>(string.Format("select * from PlanData")).ToList();
            });
            return result;
        }

        public static List<PlanData> GetPlanToday()
        {
            IEnumerable<PlanData> plans = new List<PlanData>();
            List<PlanData> plans2 = new List<PlanData>();
            string planToday = string.Format(@"select * from PlanData where Cus_date = date('now') order by ID desc");
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                plans = _db.Query<PlanData>(planToday);
            });

            return plans.ToList();
        }

        public static List<PlanData> GetPlanList(string _day)
        {
            IEnumerable<PlanData> plans = new List<PlanData>();
            List<PlanData> plans2 = new List<PlanData>();
            string planToday = string.Format(@"select * from PlanData where (strftime('%d','now','localtime') - strftime('%d', Cus_date)) = {0} order by ID desc", _day);
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                plans = _db.Query<PlanData>(planToday);
            });

            return plans.ToList();
        }

        public static IEnumerable<string> GetCompanyList()
        {
            IEnumerable<string> result = new List<string>();
            string query = string.Format("select distinct Cus_company from Device");
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                result = _db.Query<string>(query);
            });
            return result;
        }

        public static IEnumerable<string> GetFactoryList()
        {
            string query = string.Format("select distinct Cus_factory from Device");
            IEnumerable<string> result = new List<string>();
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                result = _db.Query<string>(query);
            });
            return result;
        }

        public static void TransferDeviceToPlan(PlanData planTemplate, int deviceStatus)
        {
            string getDevice = "select * from Device";
            if (deviceStatus == 0) //get online
                getDevice = getDevice + " where (strftime('%s','now','localtime') - strftime('%s', LastAskUnitID))  < 60";
            else if (deviceStatus == 1)//get offline
                getDevice = getDevice + " where (strftime('%s','now','localtime') - strftime('%s', LastAskUnitID))  > 60";

            List<Device> devices;
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                devices = _db.Query<Device>(getDevice).ToList();
                //.Where(a => a.Cus_company == planTemplate.Cus_company && a.Cus_factory == planTemplate.Cus_factory);
                foreach (var _device in devices)
                {
                    if (IsPlanExist(_device.Dv_MAC))
                        continue;
                    PlanData _plan = planTemplate;
                    _plan.Cus_Atomid = _device.Cus_Atomid;
                    _plan.Dv_MAC = _device.Dv_MAC;

                    string addNew = string.Format(@"INSERT INTO PlanData 
                        (Cus_authenkey, Cus_date, Cus_company, Cus_factory, Cus_machineid, Cus_type, Cus_lineno, 
                        Cus_processname, Cus_module, Cus_dailyseq, Cus_stylename, Cus_targetqty, Cus_worker, 
                        Cus_seatseq, Cus_trimcount, Cus_punchingno, Cus_rpm, ID, Cus_Atomid, ChangeTime, Dv_MAC
                        ) 
                        VALUES ('{0}', date('now'),'{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}',
                        '{14}','{15}','{16}', (SELECT CASE WHEN (MAX(ID) + 1) is NULL THEN 1 ELSE (MAX(ID) + 1) END FROM PlanData), 
                        '{17}', datetime('now', 'localtime'), '{18}'
                        )",
                        _plan.Cus_authenkey, _plan.Cus_date.ToString("yyyy-MM-dd"), _plan.Cus_company, _plan.Cus_factory, _plan.Cus_machineid, 
                        _plan.Cus_type, _plan.Cus_lineno, _plan.Cus_processname, _plan.Cus_module, _plan.Cus_dailyseq, _plan.Cus_stylename, 
                        _plan.Cus_targetqty, _plan.Cus_worker, _plan.Cus_seatseq, _plan.Cus_trimcount, _plan.Cus_punchingno, _plan.Cus_rpm, 
                        _plan.Cus_Atomid, _plan.Dv_MAC);

                    _db.Execute(addNew);
                }
            });
        }

        public static void SavePlanList(List<PlanData> data)
        {
            string getPlan = "";
            string update = "";
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                foreach (var item in data)
                {
                    getPlan = string.Format("select * from PlanData where id={0}", item.ID);
                    IEnumerable<PlanData> _plans = _db.Query<PlanData>(getPlan);
                    if (_plans.Count() == 1) //Exist in database, do update
                    {
                        update = string.Format(@"update PlanData set Cus_type = '{1}',
                        Cus_lineno = '{2}',
                        Cus_processname = '{3}',
                        Cus_module = '{4}',
                        Cus_dailyseq = '{5}',
                        Cus_stylename = '{6}',
                        Cus_targetqty = '{7}',
                        Cus_worker = '{8}',
                        Cus_seatseq = '{9}',
                        Cus_trimcount = '{10}',
                        Cus_punchingno = '{11}', 
                        Cus_machineid = '{12}',
                        Cus_company = '{13}',
                        Cus_factory = '{14}',
                        Cus_Atomid = '{15}',
                        Cus_position = '{16}',
                        ChangeTime = datetime('now', 'localtime') where id={0}", item.ID, 
                        item.Cus_type, item.Cus_lineno, item.Cus_processname,
                        item.Cus_module, item.Cus_dailyseq, item.Cus_stylename, item.Cus_targetqty, item.Cus_worker, item.Cus_seatseq,
                        item.Cus_trimcount, item.Cus_punchingno, item.Cus_machineid, item.Cus_company, item.Cus_factory, item.Cus_Atomid, item.Cus_position);

                        _db.Execute(update);
                    }
                }
            });
        }

        public static int UpdatePlanList(string updateCmd)
        {
            int result = 0;//Number of rows affected
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                result = _db.Execute(updateCmd);
            });
            return result;
        }

        public static List<PlanData> Search(DateTime? date, string worker, string style, string lineNo, string processName)
        {
            IEnumerable<PlanData> result = new List<PlanData>();
            string search = "";
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            if (date.HasValue)
            {
                search = string.Format(@"select distinct * from PlanData where cus_date='{0}' and (Cus_worker like '%{1}%') 
                and Cus_stylename like '%{2}%' and Cus_lineno like '%{3}%'
                and Cus_processname like '%{4}%'", date.Value.ToString("yyyy-MM-dd"), worker, style, lineNo, processName);
            }
            else
            {
                search = string.Format(@"select distinct * from PlanData where cus_date=date('now') and (Cus_worker like '%{0}%') 
                and Cus_stylename like '%{1}%' and Cus_lineno like '%{2}%' and Cus_processname like '%{3}%'",
                worker, style, lineNo, processName);
            }
            planDB.Connect(_db =>
            {
                result = _db.Query<PlanData>(search);
            });

            return result.Distinct().ToList();
        }

        public static bool IsPlanExist(string mac)
        {
            bool isExist = false;
            string query = string.Format("select id from PlanData where Dv_MAC='{0}' and cus_date=date('now')", mac);
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                var result = _db.Query<long>(query).ToArray();
                isExist = result.Length > 0;
            });

            return isExist;
        }

    }
}