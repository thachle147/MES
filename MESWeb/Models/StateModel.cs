using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MESWeb.Models
{
    public class StateModel
    {
        public bool IsSuccess { get; set; }
        public string Value { get; set; }
        public int Value1 { get; set; }
        public string Message { get; set; }
        public object Extend { get; set; }
        public object Extend1 { get; set; }
        public object Extend2 { get; set; }
        public Exception Error { get; set; }
    }
}