using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Camalot.Common.Extensions;
using Madb.Site.Extensions;

namespace Madb.Site.Models.Documentation {
	public class PropertyModel {
		public string Name { get; set; }
		public Xml.Member Documentation { get; set; }
		public string XmlName { get; set; }
		public bool Ignore { get { return Documentation != null && Documentation.Ignore; } }
		public Type ReturnType { get; set; }
		public Type Parent { get; set; }
		public bool IsStatic { get; set; }
		public bool IsReadOnly { get; set; }
		public string Id {
			get {
				return "{0}.{1}".With(Parent.ToSafeFullName(), Name).Slug();
			}
		}

		public override string ToString() {
			return Name;
		}
	}
}
