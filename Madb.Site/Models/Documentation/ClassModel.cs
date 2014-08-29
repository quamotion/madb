using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camalot.Common.Extensions;

namespace Madb.Site.Models.Documentation {
	public class ClassModel {
		public ClassModel() {
			Methods = new List<MethodModel>();
			Properties = new List<PropertyModel>();
		}
		public string Name { get; set; }
		public string Description { get; set; }
		public string Namespace { get; set; }
		public string Assembly { get; set; }
		public Xml.Member Documentation { get; set; }
		public string XmlName { get; set; }
		public bool Ignore { get { return Documentation != null && Documentation.Ignore; } }
		public bool IsStatic { get; set; }

		public IList<MethodModel> Methods {get;set;}

		public IList<PropertyModel> Properties { get; set; }

		public string Id {
			get {
				return "{0}.{1}".With(Namespace, Name).Slug();
			}
		}

		public override string ToString() {
			return "{0}.{1}".With(Namespace, Name);
		}
	}
}