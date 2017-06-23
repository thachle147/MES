using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MESWeb.Services;
using MESWeb.Models;

namespace MESWeb.Controllers
{
    public class SettingsController : Controller
    {
        ISettingService _settingService;
        public SettingsController()
        {
            _settingService = new SettingService();
        }

        public ActionResult Index()
        {
            ViewData["ExportColumn"] = _settingService.ExportPlanColumns().ToList();
            ViewData["SourcePlanColumns"] = _settingService.SourcePlanColumns().ToList();
            ViewData["TimeReloadChart"] = _settingService.Get(SettingKeys.PlanChartIntervalReload);
            ViewData["PlanChartColor"] = _settingService.Get(SettingKeys.PlanChartColors);
            ViewData["PlanChartThreshold"] = _settingService.Get(SettingKeys.PlanChartThreshold);

            return View();
        }

        public ActionResult UpdateValue(long id, string value)
        {
            Setting model = new Setting() { ID = id, Value = value };
            StateModel state = _settingService.Update(model);
            if (state.Error != null)
                throw state.Error;
            return Json(state, JsonRequestBehavior.AllowGet);
        }
    }
}