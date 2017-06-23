namespace MESWeb.Models
{
    using System;
    using System.Collections.Generic;
    
    public class ResultData
    {
        public string Dv_date { get; set; }
        public string Dv_time { get; set; }
        public string Dv_company { get; set; }
        public string Dv_type { get; set; }
        public string Dv_factory { get; set; }
        public string Dv_machineid { get; set; }
        public string Dv_processname { get; set; }
        public Nullable<int> Dv_target { get; set; }
        public string Dv_worker { get; set; }
        public Nullable<int> Dv_counting { get; set; }
        public Nullable<int> Dv_workercount { get; set; }
        public string Dv_punchingno { get; set; }
        public Nullable<int> Dv_rangrex { get; set; }
        public Nullable<int> Dv_rangey { get; set; }
        public Nullable<int> Dv_maxrpm { get; set; }
        public Nullable<int> Dv_avrrpm { get; set; }
        public string ID2 { get; set; }
        public string Dv_lineno { get; set; }
        public string Dv_module { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public long ID { get; set; }
        public long Seconds { get; set; }
    }
}
