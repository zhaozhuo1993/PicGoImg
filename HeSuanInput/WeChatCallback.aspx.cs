using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using WeChatCoreHandle;

namespace WeChatInternetHospital
{
    /// <summary>
    /// 异步接收微信支付结果通知的回调地址，通知url必须为外网可访问的url，不能携带参数。
    /// </summary>
    public partial class WeChatCallback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ResultNotify resultNotify = new ResultNotify(this);
            resultNotify.ProcessNotify();
        }
    }
}