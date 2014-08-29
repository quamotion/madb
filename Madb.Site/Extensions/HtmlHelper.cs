using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc.Html;
using System.Web.Mvc;
using Camalot.Common.Extensions;
using Madb.Site.Models;

namespace Madb.Site.Extensions {
	public static partial class MadbSiteExtensions {
		public static IHtmlString SiteName(this HtmlHelper helper) {
			var value = ConfigurationManager.AppSettings["site:Name"].Or("");
			return new MvcHtmlString(value);
		}

		public static IHtmlString DirectLink(this HtmlHelper helper, string id) {
			return helper.Partial("_DirectLink", id);
		}

		public static IHtmlString StaticIcon(this HtmlHelper helper, bool isStatic) {
			return helper.Icon("fa-flag", isStatic, "Static");
		}

		public static IHtmlString ReadonlyIcon(this HtmlHelper helper, bool isReadonly) {
			return helper.Icon("fa-lock", isReadonly, "Readonly");
		}

		public static IHtmlString ExtensionIcon(this HtmlHelper helper, bool isExtension) {
			return helper.Icon("fa-plug", isExtension, "Extension");
		}

		public static IHtmlString Icon(this HtmlHelper helper, string icon, bool show = true, string title = "") {
			return helper.Partial("_IconPartial", new IconHelperModel {
				Show = show,
				Title = title,
				Icon = icon
			});
		}
	}
}