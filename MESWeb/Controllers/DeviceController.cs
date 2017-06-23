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
    public class DeviceController : Controller
    {
        IDeviceService _deviceService;

        public DeviceController()
        {
            _deviceService = new DeviceService();
        }

        public ActionResult Index()
        {
            IEnumerable<Device> plans = _deviceService.GetDeviceList();

            return View(plans);
        }

        
    }
}