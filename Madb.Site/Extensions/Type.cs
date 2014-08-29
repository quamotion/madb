using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camalot.Common.Extensions;

namespace Madb.Site.Extensions {
	public  static partial class MadbSiteExtensions {
		public static bool IsInNamespace(this Type type, string @namespace) {
			return type != null && !string.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.Equals(@namespace, StringComparison.InvariantCulture);
		}

		public static bool IsInChildNamespace(this Type type, string rootNamespace) {
			return type != null && !string.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.StartsWith(rootNamespace, StringComparison.InvariantCulture) && !type.Namespace.Equals(rootNamespace, StringComparison.InvariantCulture);
		}

		public static string ToSafeName(this Type type) {
			if(type.IsGenericType) {
				var gparams = type.GetGenericArguments();
				var len = type.Name.IndexOf("`");
				if(len <= 0) {
					len = type.Name.Length;
				}
				return "{0}{1}".With(gparams.Count() > 0 ? type.Name.Substring(0,len) : type.Name, gparams.Count() > 0 ? "<{0}>".With(String.Join(", ", gparams.Select(g => g.ToSafeName()))) : "");
			} else {
				return type.Name;
			}
		}



		public static string ToSafeFullName(this Type type) {
				return "{1}.{0}".With(ToSafeName(type), type.Namespace);
		}

		public static string ToSafeUrlFullName(this Type type) {
			return "{1}.{0}".With(type.Name.REReplace(@"\[|\]",""), type.Namespace);
		}
		
	}
}