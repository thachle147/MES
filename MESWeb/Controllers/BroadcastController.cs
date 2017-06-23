using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using MESWeb.Models;
using MESWeb.Services;
using System.Data.SqlClient;

namespace MESWeb.Controllers
{
    public class BroadcastController : Controller
    {
        IDeviceService _deviceService;
        ISettingService _settingService;

        public BroadcastController()
        {
            _deviceService = new DeviceService();
            _settingService = new SettingService();
        }

        public JsonResult Send(string id, string message)
        {
            StateModel status = new StateModel();
            Device device = _deviceService.GetDevice(nameof(Device.ID2), id);
            if (device != null)
            {
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<MESWeb.Hubs.NotificationHub>();
                hubContext.Clients.All.addNewMessageToPage(device.Dv_MAC, device.Cus_machineid, device.Cus_Atomid, device.Cus_worker, device.Dv_state, message);

                status.IsSuccess = true;
                status.Extend = device;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }

    }
}