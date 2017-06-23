using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using MESWeb.Models;
using MESWeb.Services;

namespace MESWeb.Controllers
{
    public class OrganizationController : Controller
    {
        IOrganizationService _organizationService;

        public OrganizationController()
        {
            _organizationService = new OrganizationService();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Company()
        {
            IEnumerable<string> companyList = _organizationService.GetCompanyList();
            return View(companyList);
        }

        public ActionResult Factory()
        {
            IEnumerable<string> factoryList = _organizationService.GetFactoryList();
            return View(factoryList);
        }

        public ActionResult Worker()
        {
            IEnumerable<Worker> workerList = _organizationService.GetWorkerList();
            return View(workerList);
        }

        public ActionResult WorkerDetail(string workerNo)
        {
            Worker worker = new Worker();
            if(!string.IsNullOrEmpty(workerNo))
            {
                worker = _organizationService.GetWorker(workerNo);
            }
            return PartialView("WorkerFormPartial", worker);
        }

        public ActionResult SaveWorker(Worker worker)
        {
            StateModel state = _organizationService.SaveWorker(worker);
            if (state.Error != null)
                throw state.Error;
            return Json(state, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteWorker(string workerNo)
        {
            StateModel state = _organizationService.DeleteWorker(workerNo);
            if (state.Error != null)
                throw state.Error;
            return Json(state, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveCompany(string current, string name)
        {
            StateModel state = new StateModel();
            state = _organizationService.SaveCompany(current, name);
            return Json(state, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveFactory(string current, string name)
        {
            StateModel state = new StateModel();
            state = _organizationService.SaveFactory(current, name);
            return Json(state, JsonRequestBehavior.AllowGet);
        }
    }
}