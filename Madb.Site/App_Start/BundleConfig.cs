using System.Web;
using System.Web.Optimization;
using Camalot.Common.Extensions;
using Camalot.Common.Mvc.Extensions;

namespace Madb.Site {
	public class BundleConfig {
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles) {
			bundles.LoadFromWebConfiguration();
		}
	}
}
