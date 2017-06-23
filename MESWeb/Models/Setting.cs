using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MESWeb.Models
{
    public class Setting
    {
        public long ID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}