using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MESWeb.Models;
using MESWeb.Services;

namespace MESWeb.Controllers
{
    public class StatisticsController : Controller
    {
        private IPlanService _planService;
        private IPlanResultService _historyService;

        public StatisticsController()
        {
            _planService = new PlanService();
            _historyService = new PlanResultService();
        }

        public ActionResult Index()
        {
            return View();
        }

        #region Plan Chart
        public ActionResult Search(DateTime? date, string worker, string style, string lineNo, string processName)
        {
            List<PlanData> result = Utilities.Search(date, worker.Trim(), style.Trim(), lineNo.Trim(), processName.Trim());
            ViewData["SearchResult"] = result;
            return PartialView("Work", result);
        }

        private List<string> CalcChartData(IEnumerable<long> idList)
        {
            string _date = DateTime.Now.ToString("yyyy-MM-dd");
            List<string> chartData = new List<string>();
            List<PlanData> planList = Utilities.GetChartData(idList);

            //testing data
            ////////////////////////////////////////////////////////////////////////////////////////////////
            //List<PlanData> planList = new List<PlanData>();
            //for (int i = 1; i <= 84; i++)
            //{
            //    PlanData test = new PlanData();
            //    test.Cus_machineid = "M" + i;
            //    test.Cus_targetqty = i * 20;
            //    test.Counting = i * 10;
            //    planList.Add(test);
            //}
            ////////////////////////////////////////////////////////////////////////////////////////////////

            int percent = 0;
            string countingColor = ChartSettings.Lv1Color;
            foreach (var _plan in planList)
            {
                percent = 0;
                countingColor = ChartSettings.Lv1Color;
                if (_plan.Counting.HasValue && _plan.Cus_targetqty.HasValue && _plan.Counting.Value > 0 && _plan.Cus_targetqty.Value > 0)
                {
                    percent = (_plan.Counting.Value * 100) / _plan.Cus_targetqty.Value;
                    if (percent >= ChartSettings.CountingThreshold && percent < 100)
                        countingColor = ChartSettings.Lv2Color;
                    else if (percent >= 100)
                        countingColor = ChartSettings.Lv3Color;
                }

                chartData.Add(string.Format(@"[""{0}"", {1}, ""color: {2}"", {3}, ""color: {4}""]",
                    string.IsNullOrEmpty(_plan.Cus_machineid) ? _plan.Cus_Atomid : _plan.Cus_machineid,
                    _plan.Cus_targetqty, ChartSettings.TargetColor,
                    _plan.Counting, countingColor));
            }
            return chartData;
        }

        //Call before redirect to Show-Plan-Chart
        public JsonResult SetupChartObject(long[] list)
        {
            string chartId = DateTime.Now.Ticks.ToString();
            Session[chartId] = list;
            return Json(chartId, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowPlanChart(string chartId, int pageNum)
        {
            List<string> item1, item2;
            int totalItem;

            //Session[chartId] = new long[] { 1 }; //TEST HERE...

            PagingPlanChart(chartId, pageNum, out item1, out item2, out totalItem);
            ViewData["ChartData1"] = item1;
            ViewData["ChartData2"] = item2;
            ViewData["CurrentPage"] = pageNum;
            ViewData["Total"] = totalItem;
            ViewData["ChartId"] = chartId;
            return View("PlanChartSplit");
        }

        private void PagingPlanChart(string chartId, int pageNum, out List<string> item1, out List<string> item2, out int total)
        {
            List<string> items = new List<string>();
            List<string> _item1 = new List<string>();
            List<string> _item2 = new List<string>();
            long[] list;
            total = 0;
            if (Session[chartId] != null)
            {
                list = (long[])Session[chartId];
                List<string> chartData = CalcChartData(list); //Get chart data from database

                total = chartData.Count;
                pageNum = pageNum - 1;
                if (pageNum < 1)
                    pageNum = 0;
                int _pageSize = 64;
                int _halfPage = _pageSize / 2;
                int _skip = pageNum * _pageSize;
                int _take = _pageSize;

                items = chartData.Skip(_skip).Take(_take).ToList();
                _item1 = items.Skip(0).Take(_halfPage - 1).ToList();
                _item2 = items.Skip(_halfPage).Take(_halfPage - 1).ToList();
            }
            item1 = _item1;
            item2 = _item2;
        }

        public JsonResult ReloadPlanChart(string chartId, int pageNum)
        {
            List<string> item1, item2;
            int total;
            PagingPlanChart(chartId, pageNum, out item1, out item2, out total);
            StateModel state = new StateModel();
            state.Extend1 = item1;
            state.Extend2 = item2;
            state.Extend = new { CurrentPage = pageNum, Total = total };
            return Json(state, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Effort History
        public ActionResult Effort()
        {
            return Content("");
        }



        #endregion
    }
}