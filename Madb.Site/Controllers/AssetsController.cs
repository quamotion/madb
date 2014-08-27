using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Madb.Site.Controllers {
	public class AssetsController : Controller {
		// GET: Assets
		public PartialViewResult Gist(string id) {
			return PartialView(model: id);
		}
	}
}