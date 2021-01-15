using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Controller
{
    public class OutJsonHelper
    {
        public OutJsonHelper()
        {
            code = "0";
            message = new DataTable();
            errormsg = "";
        }
        public string code { get; set; }
        public DataTable message { get; set; }
        public string errormsg { get; set; }
        public string ToJsonStr()
        {
            if (message.Rows.Count > 0)
            {
                this.code = "1";
            }
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
