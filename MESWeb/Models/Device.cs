namespace MESWeb.Models
{
    using System;
    using System.Collections.Generic;
    
    public class Device
    {
        public long ID { get; set; }
        public string Dv_MAC { get; set; }
        public string ID2 { get; set; }
        public string Cus_worker { get; set; }
        public string Cus_machineid { get; set; }
        public string Cus_company { get; set; }
        public string Cus_factory { get; set; }
        public Nullable<System.DateTime> LastAskUnitID { get; set; }
        public string Dv_state { get; set; }
        public string Dv_version { get; set; }
        public string Dv_type { get; set; }
        public string Cus_Atomid { get; set; }
        public bool IsOnline { get; set; }
        public string Cus_position { get; set; }
    }
}
