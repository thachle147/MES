namespace MESWeb.Models
{
    using System;
    using System.Collections.Generic;

    public class PlanData
    {
        public string Cus_authenkey { get; set; }
        public System.DateTime Cus_date { get; set; }
        public string Cus_company { get; set; }
        public string Cus_factory { get; set; }
        public string Cus_machineid { get; set; }
        public string Cus_type { get; set; }
        public string Cus_lineno { get; set; }
        public string Cus_processname { get; set; }
        public string Cus_module { get; set; }
        public Nullable<int> Cus_dailyseq { get; set; }
        public string Cus_stylename { get; set; }
        public Nullable<int> Cus_targetqty { get; set; }
        public string Cus_worker { get; set; }
        public string Cus_seatseq { get; set; }
        public Nullable<int> Cus_trimcount { get; set; }
        public string Cus_punchingno { get; set; }
        public Nullable<int> Cus_rpm { get; set; }
        public Nullable<int> IsSend { get; set; }
        public Nullable<int> IsReset { get; set; }
        public Nullable<int> Result { get; set; }
        public Nullable<int> FlagMachine { get; set; }
        public Nullable<int> Counting { get; set; }
        public Nullable<System.DateTime> AskTargetTime { get; set; }
        public Nullable<System.DateTime> Inserttime { get; set; }
        public string ID2 { get; set; }
        public string DeviceType { get; set; }
        public Nullable<int> Onl_Offl { get; set; }
        public Nullable<System.DateTime> ChangeTime { get; set; }
        public long ID { get; set; }
        public string Cus_Atomid { get; set; }
        public string Dv_MAC { get; set; }
        public string Cus_position { get; set; }
    }
}
