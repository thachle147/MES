using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Configuration;
using MESWeb.Core;
using MESWeb.Models;

namespace MESWeb.Services
{
    public interface IPlanService : IService
    {
        IEnumerable<PlanData> GetPlanList(IEnumerable<long> idList);
        List<PlanData> TransferDeviceToPlan(List<Device> devices);
    }

    public class PlanService : ServiceBase, IPlanService
    {
        public IEnumerable<PlanData> GetPlanList(IEnumerable<long> idList)
        {
            IEnumerable<PlanData> result = new List<PlanData>();

            //string query = Select<T_PlanData>().In(a => new { a.ID }, idList).ToString();
            string query = "select * from T_PlanData where ID in " + string.Join(",", idList.ToArray());

            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                result = _db.Query<PlanData>(string.Format("select * from T_planData"));
            });
            return result;
        }

        public List<PlanData> TransferDeviceToPlan(List<Device> devices)
        {
            List<PlanData> plans = new List<PlanData>();
            foreach(var item in devices)
            {
                plans.Add(new PlanData()
                {
                    ID2 = item.ID2,
                    Cus_worker = item.Cus_worker,
                    Cus_machineid = item.Cus_machineid,
                    Cus_company = item.Cus_company,
                    Cus_factory = item.Cus_factory,
                    Cus_Atomid = item.Cus_Atomid,
                    Dv_MAC = item.Dv_MAC,
                    Cus_date = DateTime.Now
                });
            }
            return plans;
        }

    }
}