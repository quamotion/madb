using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camalot.Common.Extensions;
using Madb.Site.Extensions;
namespace Madb.Site.Models.Documentation {
	public class MethodModel {
		public MethodModel() {
			Parameters = new List<ParameterModel>();
			GenericParameters = new List<TypeModel>();
		}
		public string Name { get; set; }
		public Xml.Member Documentation { get; set; }
		public string XmlName { get; set; }
		public bool Ignore { get { return Documentation != null && Documentation.Ignore; } }
		public Type ReturnType { get; set; }
		public Type Parent { get; set; }
		public IList<ParameterModel> Parameters { get; set; }
		public IList<TypeModel> GenericParameters { get; set; }
		public bool IsStatic { get; set; }
		public bool IsExtension {
			get {
				return ExtensionOf != null;
			}
		}
		public Type ExtensionOf { get; set; }

		public string Id {
			get {
				return "{0}.{1}".With(Parent.ToSafeFullName(), Name).Slug();
			}
		}

		public override string ToString() {
			if(ExtensionOf == null) {
				return "{0}{2} ( {1} )".With(Name, String.Join(", ", Parameters.Select(p => p.ToString())), GenericParameters != null && GenericParameters.Count > 0 ? "<{0}>".With(String.Join(", ", GenericParameters.Select(g => g.ToString()))) : "");
			} else {
				return "{0}{2} ( {1} )".With(
					Name,
					String.Join(", ", Parameters.Skip(1).Select(p => p.ToString())),
					GenericParameters != null && GenericParameters.Count > 0 ? "<{0}>".With(String.Join(", ", GenericParameters.Select(g => g.ToString()))) : ""
				);
			}
		}
	}
}