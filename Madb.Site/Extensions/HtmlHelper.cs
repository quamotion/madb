using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc.Html;
using System.Web.Mvc;
using Camalot.Common.Extensions;

namespace Madb.Site.Extensions {
	public static partial class MadbSiteExtensions {
		public static IHtmlString SiteName(this HtmlHelper helper) {
			var value = ConfigurationManager.AppSettings["site:Name"].Or("");
			return new MvcHtmlString(value);
		}

		public static IHtmlString DirectLink(this HtmlHelper helper, string id) {
			return helper.Partial("_DirectLink", id);
		}
	}
}