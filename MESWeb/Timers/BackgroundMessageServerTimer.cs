using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using System.Web.Hosting;
using Microsoft.AspNet.SignalR;
using MESWeb.Models;
using MESWeb.Services;
using System.Data.SqlClient;

namespace MESWeb.Timers
{
    public class BackgroundMessageServerTimer : IRegisteredObject
    {
        private Timer _timer;
        private readonly IHubContext _notificationHub;
        ISettingService _settingService;

        public BackgroundMessageServerTimer()
        {
            _settingService = new SettingService();
            _notificationHub = GlobalHost.ConnectionManager.GetHubContext<MESWeb.Hubs.NotificationHub>();
            StartTimer();
        }

        private void StartTimer()
        {
            _timer = new Timer(15000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            StateModel state = new StateModel();
            SqlConnection sqlConnection;
            SqlCommand sqlCommand;
            SqlDataReader sqlReader;
            long lastID = 0;
            string getMsg = null;
            List<MessageServerModel> messageList = new List<MessageServerModel>();
            try
            {

                if (HttpContext.Current != null && HttpContext.Current.Session["LastMessageServerID"] != null)
                    lastID = (long)HttpContext.Current.Session["LastMessageServerID"];
                else
                {
                    Setting setting = _settingService.GetValue("LastMessageServerID");
                    if (setting != null)
                    {
                        lastID = long.Parse(setting.Value);
                        if (HttpContext.Current != null)
                            HttpContext.Current.Session["LastMessageServerID"] = lastID;
                    }
                }
                
                getMsg = "select * from Messager where ID > " + lastID;

                using (sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings["MESConnStr"]))
                {
                    sqlConnection.Open();
                    using (sqlCommand = new SqlCommand(getMsg, sqlConnection))
                    {
                        using (sqlReader = sqlCommand.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                MessageServerModel msg = new MessageServerModel();
                                msg.ID = long.Parse("0" + sqlReader[nameof(MessageServerModel.ID)]);
                                msg.Cus_company = sqlReader[nameof(MessageServerModel.Cus_company)] + "";
                                msg.Cus_factory = sqlReader[nameof(MessageServerModel.Cus_factory)] + "";
                                msg.Messager = sqlReader[nameof(MessageServerModel.Messager)] + "";
                                messageList.Add(msg);
                            }
                        }
                    }
                }

                if (messageList.Count > 0)
                {
                    lastID = messageList.Select(a => a.ID).Max();
                    StateModel updateState = _settingService.Update("LastMessageServerID", lastID.ToString());
                    if (updateState.Error == null)
                    {
                        if (HttpContext.Current != null)
                            HttpContext.Current.Session["LastMessageServerID"] = lastID;
                        var hubContext = GlobalHost.ConnectionManager.GetHubContext<MESWeb.Hubs.NotificationHub>();
                        foreach (var item in messageList)
                        {
                            hubContext.Clients.All.showMessageServer(item.Cus_company, item.Cus_factory, item.Messager);
                        }
                    }
                    else
                        state.Error = updateState.Error;
                }
                state.IsSuccess = true;
            }
            catch (Exception ex)
            {
                sqlReader = null;
                sqlCommand = null;
                sqlConnection = null;
                Stop(true);
                state.Error = ex;
            }
        }
        
        public void Stop(bool immediate)
        {
            _timer.Dispose();
            HostingEnvironment.UnregisterObject(this);
        }
    }
}