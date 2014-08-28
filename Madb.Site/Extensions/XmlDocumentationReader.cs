using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Camalot.Common.Extensions;

namespace Madb.Site.Extensions {
	public static partial class MadbSiteExtensions {

		public static string GetXmlDocumentationName(this MethodInfo mi) {
			var mparams = mi.GetParameters();
			var gcount = mi.GetGenericArguments().Count();
			var generics = mi.GetGenericArguments().Select((generic, index) => new { generic, index });
			var paramItems = mparams.Select((p, index) => new {
				p,
				index = generics.Where(x => x.generic == p.ParameterType).Count() > 0 ? generics.Where(x => x.generic == p.ParameterType).First().index : index
			}).Select(x => x.p.ParameterType.XmlDocumentParameterSafeName(x.index));

			return "M:{0}.{1}{2}{4}{3}{5}".With(

				mi.ReflectedType.XmlDocumentTypeSafeName(),
				mi.Name,
				gcount > 0 ? "``{0}".With(gcount) : "",
				string.Join(",", paramItems),
				mparams.Count() > 0 ? "(" : "",
				mparams.Count() > 0 ? ")" : ""
				);
		}

		public static string GetXmlDocumentationName(this PropertyInfo propertyInfo) {
			return "P:{0}.{1}".With(propertyInfo.ReflectedType.XmlDocumentTypeSafeName(), propertyInfo.Name);
		}

		public static string GetXmlDocumentationName(this Type type) {
			return "T:{0}.{1}".With(type.Namespace,type.Name);
		}

		private static string XmlDocumentTypeSafeName(this Type type) {
			return "{1}.{0}".With(type.Name.Replace("`", "``"), type.Namespace);
		}

		private static string XmlDocumentParameterSafeName(this Type type, int index) {
			var gparams = type.GetGenericArguments();
			var s = new StringBuilder();
			s.Append("{");
			for(var i = 0; i < gparams.Length; ++i) {
				if(gparams.Length > 1) {
					s.Append("``{0},".With(i));
				} else {
					s.Append(gparams[i].GetXmlParameterTypeSaveName(i));
				}
			}
			if(s.Length > 1 && s[s.Length - 1] == ',') {
				s.Remove(s.Length - 1, 1);
			}
			s.Append("}");
			if(type.IsGenericType) {
				return "{1}.{0}{2}".With(type.Name.Substring(0, type.Name.LastIndexOf("`")), type.Namespace, s);
			} else if(type.IsGenericParameter) {
				return "``{0}".With(index);
			} else if(type.IsGenericTypeDefinition) {
				return "GTD";
			} else {
				return "{1}.{0}".With(type.Name, type.Namespace);
			}
		}

		private static string GetXmlParameterTypeSaveName(this Type type, int index) {
			if(type.IsGenericParameter) {
				return "``{0}".With(index);
			} else {
				return "{1}.{0}".With(type.Name, type.Namespace);
			}
		}

		public static Models.Documentation.Xml.Member GetDocumenation(this XmlDocument doc, MethodInfo mi) {
			var itemName = mi.GetXmlDocumentationName();
			var tdoc = new XmlDocument();

			var node = doc.SelectSingleNode("//member[@name=\"{0}\"]".With(itemName));
			if(node == null) {
				return null;
			}
			var nnode = tdoc.ImportNode(node, true);
			tdoc.AppendChild(nnode);
			var serializer = new XmlSerializer(typeof(Models.Documentation.Xml.Member));
			using(var reader = new StringReader(tdoc.OuterXml)) {
				using(var xreader = XmlReader.Create(reader)) {
					var member = serializer.Deserialize(xreader) as Models.Documentation.Xml.Member;
					if(mi.IsExtension()) {
						if(member.Params.Count > 0) {
							member.Params.RemoveAt(0);
						}
					}
					return member;
				}
			}
		}

		public static Models.Documentation.Xml.Member GetDocumenation(this XmlDocument doc, PropertyInfo pi) {
			var itemName = pi.GetXmlDocumentationName();
			var tdoc = new XmlDocument();

			var node = doc.SelectSingleNode("//member[@name=\"{0}\"]".With(itemName));
			if(node == null) {
				return null;
			}
			var nnode = tdoc.ImportNode(node, true);
			tdoc.AppendChild(nnode);
			var serializer = new XmlSerializer(typeof(Models.Documentation.Xml.Member));
			using(var reader = new StringReader(tdoc.OuterXml)) {
				using(var xreader = XmlReader.Create(reader)) {
					var member = serializer.Deserialize(xreader) as Models.Documentation.Xml.Member;
					return member;
				}
			}
		}

		public static Models.Documentation.Xml.Member GetDocumenation(this XmlDocument doc, Type type) {
			var itemName = type.GetXmlDocumentationName();
			var tdoc = new XmlDocument();

			var node = doc.SelectSingleNode("//member[@name=\"{0}\"]".With(itemName));
			if(node == null) {
				return null;
			}
			var nnode = tdoc.ImportNode(node, true);
			tdoc.AppendChild(nnode);
			var serializer = new XmlSerializer(typeof(Models.Documentation.Xml.Member));
			using(var reader = new StringReader(tdoc.OuterXml)) {
				using(var xreader = XmlReader.Create(reader)) {
					var member = serializer.Deserialize(xreader) as Models.Documentation.Xml.Member;
					return member;
				}
			}
		}
	}
}