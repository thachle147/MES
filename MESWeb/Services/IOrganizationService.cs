using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Configuration;
using MESWeb.Core;
using MESWeb.Models;

namespace MESWeb.Services
{
    public interface IOrganizationService : IService
    {
        StateModel SaveFactory(string currentName, string newName);
        StateModel SaveCompany(string currentName, string newName);
        Worker GetWorker(string workerNo);
        StateModel SaveWorker(Worker worker);
        StateModel DeleteWorker(string workerNo);
        IEnumerable<Worker> GetWorkerList();
        IEnumerable<string> GetCompanyList();
        IEnumerable<string> GetFactoryList();
    }

    public class OrganizationService : ServiceBase, IOrganizationService
    {
        public StateModel SaveWorker(Worker worker)
        {
            StateModel state = new StateModel();
            IEnumerable<Worker> query = new List<Worker>();
            string select = string.Format("select * from Worker where Cus_worker='{0}'", worker.Cus_worker);
            string insert = string.Format("insert into Worker (Cus_worker, Cus_name, Cus_skill, LastChange) values ('{0}', '{1}', '{2}', datetime('now', 'localtime'))",
                worker.Cus_worker, worker.Cus_name, worker.Cus_skill);
            string update = string.Format("update Worker set Cus_worker='{0}', Cus_name='{1}', Cus_skill='{2}', LastChange=datetime('now', 'localtime') where Cus_worker='{0}'",
                worker.Cus_worker, worker.Cus_name, worker.Cus_skill);
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            state = planDB.Connect(_db =>
            {
                query = _db.Query<Worker>(select);
                if (query.Count() == 0) //insert
                    _db.Execute(insert);
                else //update
                    _db.Execute(update);
            });
            
            return state;
        }

        public StateModel DeleteWorker(string workerNo)
        {
            StateModel state = new StateModel();
            string cmdDelete = string.Format("delete from Worker where Cus_worker='{0}'", workerNo);
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            state = planDB.Connect(_db =>
            {
                _db.Execute(cmdDelete);
            });
            return state;
        }

        public StateModel SaveCompany(string currentName, string newName)
        {
            StateModel state = new StateModel();
            string cmdUpdate = string.Format("update PlanData set Cus_company='{0}', ChangeTime = datetime('now', 'localtime') where Cus_company='{1}'", newName, currentName);
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                state.Value1 = _db.Execute(cmdUpdate);
            });
            state.IsSuccess = true;
            return state;
        }

        public StateModel SaveFactory(string currentName, string newName)
        {
            StateModel state = new StateModel();
            string cmdUpdate = string.Format("update PlanData set Cus_factory='{0}', ChangeTime = datetime('now', 'localtime') where Cus_factory='{1}'", newName, currentName);
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                state.Value1 = _db.Execute(cmdUpdate);
            });
            state.IsSuccess = true;
            return state;
        }

        public IEnumerable<Worker> GetWorkerList()
        {
            IEnumerable<Worker> result = new List<Worker>();
            string query = string.Format("select * from Worker");
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                result = _db.Query<Worker>(query);
            });
            return result;
        }

        public Worker GetWorker(string workerNo)
        {
            Worker worker = new Worker();
            IEnumerable<Worker> query = new List<Worker>();
            string select = string.Format("select * from Worker where Cus_worker='{0}'", workerNo);
            QuickSQLite planDB = new QuickSQLite(SQLiteConnString.Basic);
            planDB.Connect(_db =>
            {
                query = _db.Query<Worker>(select);
            });
            if (query != null && query.Count() == 1)
                worker = query.FirstOrDefault();
            return worker;
        }

        public IEnumerable<string> GetCompanyList()
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

        public IEnumerable<string> GetFactoryList()
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

    }
}