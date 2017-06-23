using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using MESWeb.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Reflection;
using MESWeb.Services;

namespace MESWeb.Controllers
{
    public class HomeController : Controller
    {
        private IPlanService _planService;
        private IDeviceService _deviceService;
        private ISettingService _settingService;

        public HomeController()
        {
            _planService = new PlanService();
            _deviceService = new DeviceService();
            _settingService = new SettingService();
        }

        public ActionResult Index()
        {
            List<PlanData> plans = Utilities.GetPlanToday();
            return View(plans);
        }

        public ActionResult ExportExcel(string _day)
        {
            // _day =
            //      0: today
            //      1: yesterday
            //      2: 2 days ago
            //     -1: get by online devices
            StateModel state = new StateModel();
            byte[] fileContents = null;
            string name = string.Format("Plan_{0}.xlsx", DateTime.Now.ToString("yyyy-MM-dd"));

            List<PlanData> plans = new List<PlanData>();
            if (_day != "-1")
                plans = Utilities.GetPlanList(_day);
            else
            {
                List<Device> devices = _deviceService.GetDeviceList(60).ToList();
                plans = _planService.TransferDeviceToPlan(devices);
            }
            fileContents = CreateExcelFile(plans);
            return File(fileContents, System.Net.Mime.MediaTypeNames.Application.Octet, name);
        }

        private byte[] CreateExcelFile(List<PlanData> plans)
        {
            byte[] fileContents;
            IWorkbook wb = new XSSFWorkbook();
            ISheet sheet = wb.CreateSheet("Plan");
            ICreationHelper cH = wb.GetCreationHelper();
            int indexApplyMachineValidation = -1;
            int irow = 0;
            IRow rowHeader = sheet.CreateRow(irow);
            string[] columnExport = _settingService.ExportPlanColumns();
            List<string> columnExportList = ExportExcelSetting.FixedColumn;
            columnExportList.AddRange(_settingService.ExportPlanColumns());
            columnExport = columnExportList.ToArray();

            //Create header
            for (int i = 0; i < columnExport.Length; i++)
            {
                ICell cell = rowHeader.CreateCell(i);
                cell.SetCellValue(cH.CreateRichTextString(columnExport[i]));
                sheet.AutoSizeColumn(i);
                if (columnExport[i] == "Cus_machineid")
                    indexApplyMachineValidation = i;
            }

            foreach (var item in plans)
            {
                irow++;
                IRow row = sheet.CreateRow(irow);
                PropertyInfo[] _cells = item.GetType().GetProperties().Where(a => columnExport.Contains(a.Name)).ToArray();
                for (int i = 0; i < columnExport.Length; i++)
                {
                    PropertyInfo propCell = _cells.SingleOrDefault(a => a.Name == columnExport[i]);
                    if (propCell == null)
                        continue;
                    string _value = propCell.GetValue(item, null) + "";
                    ICell cell = row.CreateCell(i);
                    if (columnExport[i].ToLower().IndexOf("time") >= 0 || columnExport[i].ToLower().IndexOf("date") >= 0)
                    {
                        DateTime _dateValue = DateTime.Now;
                        if (DateTime.TryParse(_value, out _dateValue))
                            _value = _dateValue.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    cell.SetCellValue(cH.CreateRichTextString(_value));
                    sheet.AutoSizeColumn(i);
                }
            }

            //Add validation (drop-down-list) for MachineID column
            MachineValidationList(wb, sheet, indexApplyMachineValidation);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                wb.Write(memoryStream);
                fileContents = memoryStream.ToArray();
            }

            wb.Close();
            return fileContents;
        }

        private void MachineValidationList(IWorkbook workbook, ISheet sheet, int col)
        {
            string[] deviceList = _deviceService.GetDeviceList().Select(a => a.Cus_machineid).Distinct().ToArray();
            if (deviceList.Length > 0)
            {
                ISheet sheetMachine = workbook.CreateSheet("Machine");
                for (int i = 0; i < deviceList.Length; i++)
                {
                    IRow row = sheetMachine.CreateRow(i);
                    ICell cell = row.CreateCell(0);
                    cell.SetCellValue(deviceList[i]);
                }
                XSSFName nameCell = (XSSFName)workbook.CreateName();
                nameCell.NameName = "hiddenMachine";
                nameCell.RefersToFormula = sheetMachine.SheetName + "!$A$1:$A$" + deviceList.Length;

                NPOI.SS.Util.CellRangeAddressList markColumn = new NPOI.SS.Util.CellRangeAddressList(1, 1000, col, col);
                XSSFDataValidationHelper _validationHelper = new XSSFDataValidationHelper((XSSFSheet)sheet);
                //IDataValidationConstraint _validationConstraint = _validationHelper.CreateExplicitListConstraint(deviceList);
                IDataValidationConstraint _validationConstraint = _validationHelper.CreateFormulaListConstraint("hiddenMachine");
                IDataValidation _validation = _validationHelper.CreateValidation(_validationConstraint, markColumn);
                _validation.EmptyCellAllowed = true;
                _validation.SuppressDropDownArrow = true;
                _validation.ShowErrorBox = true;
                workbook.SetSheetHidden(1, 1);
                sheet.AddValidationData(_validation);
            }
        }

        public ActionResult ImportExcel()
        {
            int recordAffected = 0;
            int rowCount = 0;
            string batchCommand = "";
            string insertOrUpdate = "";
            string colName = "";
            string recordID = "";
            string[] skipUpdateColumns = new string[] { nameof(PlanData.Dv_MAC), nameof(PlanData.Cus_authenkey), nameof(PlanData.Cus_date), nameof(PlanData.ChangeTime) };
            StateModel state = new StateModel();
            List<string> coubleUpdate = new List<string>();
            List<string> machinesAdded = new List<string>();
            var file = Request.Files["fileExcel"]; //Get excel file submitted

            //Must add a reference: 'NPOI.OpenXml4Net, Version=2.2.1.0, Culture=neutral, PublicKeyToken=0df73ec7942b34e1'.
            IWorkbook wb = new XSSFWorkbook(file.InputStream);

            ISheet _sheet = wb.GetSheetAt(0);
            if (_sheet == null) state.Message = "Sheet not found!";

            IRow header = _sheet.GetRow(0);
            if (header == null) state.Message = "Header column name not found!";
            if (string.IsNullOrEmpty(state.Message))
            {
                List<ICell> _columns = header.Cells;
                rowCount = _sheet.PhysicalNumberOfRows - 1;
                for (int rowNum = 1; rowNum <= rowCount; rowNum++) //Loop all rows
                {
                    IRow row = _sheet.GetRow(rowNum);
                    if (row == null) break; //Break if row null
                    recordID = row.GetCell(0) + ""; //Get ID value

                    if (string.IsNullOrEmpty(recordID) || recordID == "0")
                    {
                        //Insert new record
                        insertOrUpdate = ImportNewPlan(_columns, row, ref machinesAdded);
                    }
                    else //Start Update
                    {
                        for (int colNum = 1; colNum < _columns.Count; colNum++) //Loop in columns, start [1] to skip ID column
                        {
                            colName = _columns[colNum].StringCellValue;
                            if (skipUpdateColumns.Contains(colName))
                                continue;//coubleUpdate.Add("Cus_date = datetime('now', 'localtime')");
                            else if (colName.ToLower().IndexOf("time") > 0 && colName != nameof(PlanData.ChangeTime))
                            {
                                string _timeValue = row.GetCell(colNum) + "";
                                DateTime _dt = DateTime.Now;
                                if (DateTime.TryParse(_timeValue, out _dt))
                                    coubleUpdate.Add(string.Format("{0} = '{1}'", colName, _dt.ToString("yyyy-MM-dd HH:mm:ss") + ""));
                            }
                            else if (colName == nameof(PlanData.ChangeTime))
                                coubleUpdate.Add(string.Format("{0} = datetime('now', 'localtime')", nameof(PlanData.ChangeTime)));
                            else
                                coubleUpdate.Add(string.Format("{0} = '{1}'", colName, row.GetCell(colNum) + ""));
                        }

                        insertOrUpdate = string.Format("update PlanData set {0} where ID={1};", string.Join(",", coubleUpdate.ToArray()), recordID);
                    }

                    if (!string.IsNullOrEmpty(insertOrUpdate))
                        batchCommand = string.IsNullOrEmpty(batchCommand) ? insertOrUpdate : string.Format("{0} {1} {2}", batchCommand, Environment.NewLine, insertOrUpdate);
                }
            }
            wb.Close();
            wb = null;

            if (!string.IsNullOrEmpty(batchCommand))
            {
                recordAffected = Utilities.UpdatePlanList(batchCommand);
                state.Message = string.Format("Rows affected: {0}/{1}", recordAffected, rowCount);
                state.IsSuccess = true;
            }
            else
                state.Message = string.Format("Not found data");
            TempData["ResultMessage"] = state;
            return RedirectToAction("ImportExcelForm", "Home");
        }

        private string ImportNewPlan(List<ICell> cells, IRow row, ref List<string> machinesAdded)
        {
            string cmdInsert = "";
            string colName = "";
            string cellValue = "";
            string[] sameColumns = new string[] { nameof(Device.Cus_position), nameof(Device.Cus_worker), nameof(Device.Cus_machineid), nameof(Device.Cus_company), nameof(Device.Cus_factory), nameof(Device.Cus_Atomid) };
            List<string> colNameList = new List<string>();
            List<string> valueList = new List<string>();

            //Add insert ID
            colNameList.Add("ID");
            valueList.Add("(SELECT MAX(ID) + 1 FROM PlanData)");

            //Get device from Dv_MAC cell
            Device device = _deviceService.GetDevice(row.GetCell(1) + "") ?? new Device();
            if (string.IsNullOrEmpty(device.Dv_MAC) || Utilities.IsPlanExist(device.Dv_MAC) || machinesAdded.IndexOf(device.Dv_MAC) >= 0)
            {
                return null;
            }
            else
            {
                //Add insert Dv_MAC
                colNameList.Add(nameof(Device.Dv_MAC));
                valueList.Add("'" + device.Dv_MAC + "'");

                colNameList.Add(nameof(Device.ID2));
                valueList.Add("'" + device.ID2 + "'");
            }

            //Loop in columns, start index from [2] to skip columns: ID; Dv_MAC
            for (int index = 2; index < cells.Count; index++)
            {
                colName = cells[index].StringCellValue;
                cellValue = row.GetCell(index) + "";

                if (colName == nameof(Device.ID2))
                    continue;

                //Add column name to insert list
                colNameList.Add(colName);

                //Add value to insert list
                ///
                if (sameColumns.Contains(colName))
                {
                    if (string.IsNullOrEmpty(cellValue))
                    {
                        valueList.Add("'" + GetValueFromDevice(device, colName) + "'");
                    }
                    else
                        valueList.Add("'" + cellValue + "'");
                }
                else if (colName == nameof(PlanData.Cus_date)) // Cus_date
                {
                    valueList.Add("date('now')");
                }
                else if (colName.ToLower().IndexOf("time") > 0) // type: date time
                {
                    DateTime _dt = DateTime.Now;
                    if (DateTime.TryParse(cellValue, out _dt))
                        valueList.Add("'" + _dt.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                    else
                        valueList.Add("datetime('now', 'localtime')");
                }
                else
                {
                    valueList.Add("'" + cellValue + "'");
                }

            }

            foreach (var item in sameColumns)
            {
                if (!colNameList.Contains(item))
                {
                    colNameList.Add(item);
                    valueList.Add("'" + GetValueFromDevice(device, item) + "'");
                }
            }

            if (!colNameList.Contains(nameof(PlanData.Cus_date))) //Specific Cus_date
            {
                colNameList.Add(nameof(PlanData.Cus_date));
                valueList.Add("date('now')");
            }

            if (!colNameList.Contains(nameof(PlanData.ChangeTime))) //Specific ChangeTime
            {
                colNameList.Add(nameof(PlanData.ChangeTime));
                valueList.Add("datetime('now', 'localtime')");
            }

            if (!colNameList.Contains(nameof(PlanData.Cus_authenkey))) //Specific Cus_authenkey
            {
                colNameList.Add(nameof(PlanData.Cus_authenkey));
                valueList.Add("'NA'");
            }

            cmdInsert = string.Format("INSERT INTO PlanData({0}) VALUES ({1});",
                string.Join(",", colNameList), string.Join(",", valueList));

            machinesAdded.Add(device.Dv_MAC); //add Dv_MAC to check duplicate row in excel

            return cmdInsert;
        }

        private string GetValueFromDevice(Device device, string colName)
        {
            PropertyInfo _prop = device.GetType().GetProperty(colName);
            return _prop.GetValue(device) + "";
        }

        public ActionResult ImportExcelForm()
        {
            return View();
        }

        public ActionResult PlanForm()
        {
            PlanData model = new PlanData();
            model.Cus_date = DateTime.Now;
            return View(model);
        }

        public JsonResult SavePlan(PlanData model)
        {
            int _deviceStatus = 0;
            string aa = Request["selectStatusDevice"] + "";
            int.TryParse(aa, out _deviceStatus);
            StateModel state = new StateModel();
            try
            {
                if (string.IsNullOrEmpty(model.Cus_punchingno))
                    model.Cus_punchingno = "";
                Utilities.TransferDeviceToPlan(model, _deviceStatus);
                state.IsSuccess = true;
            }
            catch (Exception ex)
            {
                state.Message = ex.ToString();
            }

            return Json(state, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SavePlanList(List<PlanData> models)
        {
            StateModel state = new StateModel();
            try
            {
                Utilities.SavePlanList(models);
                state.IsSuccess = true;
            }
            catch (Exception ex)
            {
                state.Message = ex.ToString();
            }

            return Json(state, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Statistics()
        {
            return View();
        }

        public ActionResult Search(DateTime? date, string worker, string style, string lineNo, string processName)
        {
            List<PlanData> result = Utilities.Search(date, worker.Trim(), style.Trim(), lineNo.Trim(), processName.Trim());
            ViewData["SearchResult"] = result;
            return PartialView("Work", result);
        }

        public ActionResult SendTargetToDevice(string ids)
        {
            StateModel state = new StateModel();
            state.Extend = Utilities.SendTargetToDevice(ids);
            state.Value = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Count().ToString();
            return Json(state, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Error()
        {
            Exception ex = null;
            if (Session["LastException"] != null)
                ex = (Exception)Session["LastException"];
            return View("CustomError", ex);
        }

    }
}