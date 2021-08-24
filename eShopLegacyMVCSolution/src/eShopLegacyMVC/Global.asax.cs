using log4net;
using System;
using System.Diagnostics;

namespace eShopLegacyMVC
{
    public class MvcApplication //: System.Web.HttpApplication
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //set the property to our new object
            LogicalThreadContext.Properties["activityid"] = new ActivityIdHelper();

            LogicalThreadContext.Properties["requestinfo"] = new WebRequestInfo();

            _log.Debug("WebApplication_BeginRequest");
        }
    }

    public class ActivityIdHelper
    {
        public override string ToString()
        {
            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
            {
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            }

            return Trace.CorrelationManager.ActivityId.ToString();
        }
    }

    public class WebRequestInfo
    {
        public override string ToString()
        {
            return HttpContextHelper.Current?.Request?.RawUrl + ", " + HttpContextHelper.Current?.Request?.UserAgent;
        }
    }
}
