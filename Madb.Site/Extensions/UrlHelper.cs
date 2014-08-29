using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Camalot.Common.Extensions;
using Camalot.Common.Mvc.Extensions;

namespace Madb.Site.Extensions {
	public static partial class MadbSiteExtensions {
		public static string ParseReferenceUrl(this UrlHelper helper, string url ) {
			if(url.IsMatch("^!:")) {
				return url.Substring(2);
			} else if ( url.IsMatch("^(T|M|P):")) {
				var skipped = url.Substring(2);
				// is it msdn link?
				if(url.IsMatch(@"^T\:System\.")) {
					var baseUrl = "http://msdn.microsoft.com/en-us/library/{0}.aspx";
					return baseUrl.With(skipped.ToLowerInvariant());
				}
				return "#{0}".With(skipped.Slug());
			} else {
				return url;
			}
		}

		public static string ParseReferenceAsText(this UrlHelper helper, string url) {
			if(url.IsMatch("^(!|T|M|P):")) {
				return "{0}".With(url.Substring(2));
			} else {
				return url;
			}
		}

		public static string UrlForType(this UrlHelper helper, Type type) {
			var baseDocumentationNamespace = helper.RequestContext.RouteData.GetRequiredString("id");
			if(type.IsInNamespace(baseDocumentationNamespace) || type.IsInChildNamespace(baseDocumentationNamespace)) {
				// local type
				return "#{0}".With(type.ToSafeFullName().Slug());
			} else {
				// remote type
				if(type.IsGenericType) {
					return "http://social.msdn.microsoft.com/Search/en-US?query={0}&ac=3".With(type.ToSafeFullName());
				} else {
					return "http://msdn.microsoft.com/en-us/library/{0}.aspx".With(type.ToSafeUrlFullName().ToLowerInvariant());
				}
			}
		}
	}
}