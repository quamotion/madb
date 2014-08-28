using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camalot.Common.Extensions;

namespace Madb.Site.Models.Documentation {
	public class NamespaceModel {
		public NamespaceModel() {
			Classes = new List<ClassModel>();
			Namespaces = new List<NamespaceModel>();
		}
		public string Name { get; set; }
		public IList<NamespaceModel> Namespaces { get; set; }
		public IList<ClassModel> Classes { get; set; }
		public bool Ignore { get { return Classes.Where(c => !c.Ignore).Count() == 0 && Namespaces.Where(c => !c.Ignore).Count() == 0; } }
		public String AssemblyVersion { get; set; }

		public string Id {
			get {
				return Name.Slug();
			}
		}
	}
}