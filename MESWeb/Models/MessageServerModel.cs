using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MESWeb.Models
{
    public class MessageServerModel
    {
        public long ID { get; set; }
        public string Cus_company { get; set; }
        public string Cus_factory { get; set; }
        public string Messager { get; set; }
        public bool Cus_Readed { get; set; }
        public DateTime? DateAdd { get; set; }
    }
}