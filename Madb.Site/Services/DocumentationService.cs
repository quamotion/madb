using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Madb.Site.Models.Documentation;
using MoreLinq;
using Camalot.Common.Extensions;
using Madb.Site.Extensions;
using System.Configuration;
using System.Reflection;
using System.Web.Hosting;
using System.IO;
using System.Security.AccessControl;
using System.ComponentModel;

namespace Madb.Site.Services {
	public class DocumentationService {

		/// <summary>
		/// You should call GetDocumentationDomain()
		/// </summary>
		/// <value>
		/// The documentation domain.
		/// </value>
		private AppDomain DocumentationDomain { get; set; }
		private IEnumerable<Assembly> DocumentationAssemblies {
			get {
				return GetDocumentationDomain().GetAssemblies().Where(a => !a.IsDynamic);
			}
		}
		/// <summary>
		/// Builds the specified assembly name.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <returns></returns>
		public NamespaceModel Build(string assemblyName) {
			var alternateName = GetAlternateName(assemblyName);
			var nm = new NamespaceModel {
				Name = assemblyName,
			};
			var exclude = this.GetType().Assembly;

			var xml = LoadXml(assemblyName);
			var NSIgnoreList = ConfigurationManager.AppSettings["site:NamespacesIgnoreList"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

			DocumentationAssemblies.Where(a => a != exclude &&
					(a.GetName().Name.Equals(alternateName, StringComparison.InvariantCultureIgnoreCase) || a.GetName().Name.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
				).ForEach(a => {
				a.GetTypes()
					.Where(t => ((t.IsInNamespace(alternateName) || t.IsInNamespace(assemblyName)) || (t.IsInChildNamespace(alternateName) || t.IsInChildNamespace(assemblyName))) && !NSIgnoreList.Contains(t.Namespace))
					.Select(t => t.Namespace)
					.Distinct()
					.ForEach(ns => {
						if(string.IsNullOrWhiteSpace(nm.AssemblyVersion)) {
							nm.AssemblyVersion = a.GetName().Version.ToString();
						}
						var processedNS = ProcessNamespace(ns, xml);
						if(processedNS != null && processedNS.Classes.Count > 0) {
							nm.Namespaces.Add(processedNS);
						}
					});
			});
			return nm;
		}

		/// <summary>
		/// Gets the documentation domain.
		/// </summary>
		/// <returns></returns>
		private AppDomain GetDocumentationDomain() {
			if(DocumentationDomain == null) {
				// create a new app domain for processing.
				DocumentationDomain = AppDomain.CreateDomain("Documentation");

				var dir = new DirectoryInfo(HostingEnvironment.MapPath("~/app_data/"));
				dir.GetFiles("*.dll").ForEach(a => {
					DocumentationDomain.Load(AssemblyName.GetAssemblyName(a.FullName));
				});
			}
			return DocumentationDomain;
		}

		/// <summary>
		/// Loads the XML.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <returns></returns>
		/// <exception cref="System.IO.FileNotFoundException"></exception>
		private XmlDocument LoadXml(string assemblyName) {
			var file = DocumentationPathFromAssemblyName(assemblyName);
			if(file.Exists) {
				var doc = new XmlDocument();
				using(var fs = new FileStream(file.FullName, FileMode.Open, FileSystemRights.Read, FileShare.Read, 2048, FileOptions.None)) {
					doc.Load(fs);
				}
				return doc;
			} else {
				throw new FileNotFoundException();
			}
		}


		private FileInfo DocumentationPathFromAssemblyName(string assemblyName) {

			var overrideName = GetAlternateName(assemblyName);

			var fn = "{0}.xml".With(overrideName.Or(assemblyName));
			var dir = HostingEnvironment.MapPath("~/app_data/");
			var file = new FileInfo(Path.Combine(dir, fn));
			if(file.Exists) {
				return file;
			} else {
				throw new FileNotFoundException();
			}
		}

		private string GetAlternateName(string namespaceName) {
			var overrideName = ConfigurationManager.AppSettings[namespaceName];
			return overrideName.Or(namespaceName);
		}

		/// <summary>
		/// Processes the namespace.
		/// </summary>
		/// <param name="namespace">The namespace.</param>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		private NamespaceModel ProcessNamespace(string @namespace, XmlDocument xml) {
			var nm = new NamespaceModel {
				Name = @namespace
			};
			DocumentationAssemblies.ForEach(a => {
				var classes = new List<ClassModel>();
				a.GetTypes().Where(t => t.IsPublic && t.IsClass && t.IsInNamespace(@namespace)).ForEach(t => {
					var pclass = ProcessType(t, xml);
					nm.Classes.Add(pclass);
				});
			});

			return nm;
		}

		/// <summary>
		/// Processes the type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		private ClassModel ProcessType(Type type, XmlDocument xml) {
			return new ClassModel {
				Name = type.ToSafeName(),
				Namespace = type.Namespace,
				Assembly = type.Assembly.GetName().Name,
				Description = type.GetCustomAttributeValue<DescriptionAttribute, String>(x => x.Description).Or(""),
				XmlName = type.GetXmlDocumentationName(),
				Documentation = xml.GetDocumenation(type),
				Methods = ProcessMethods(type, xml),
				Properties = ProcessProperties(type,xml)
			};
		}

		private IList<PropertyModel> ProcessProperties(Type type, XmlDocument xml) {
			return type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
				.Where(m => true).Select(m => new PropertyModel {
					Name = m.Name,
					Documentation = xml.GetDocumenation(m),
					XmlName = m.GetXmlDocumentationName(),
					//GenericParameters = ProcessMethodGenericParameters(m),
					//Parameters = ProcessParams(m),
					//ExtensionOf = m.ExtensionOf(),
					Parent = type,
					ReturnType = m.PropertyType
				}).OrderBy(x => x.Name).ToList();
		}

		/// <summary>
		/// Processes the methods.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		private IList<MethodModel> ProcessMethods(Type type, XmlDocument xml) {

			return type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly).Where(m =>
				!m.IsConstructor &&
				!m.Name.StartsWith("get_", StringComparison.CurrentCulture) &&
				!m.Name.StartsWith("set_", StringComparison.CurrentCulture) &&
				!m.Name.StartsWith("add_", StringComparison.CurrentCulture) &&
				!m.Name.StartsWith("remove_", StringComparison.CurrentCulture) &&
					// exclude overrides because I don't care about them. Unless, base definition is in this assembly.
				(m.GetBaseDefinition() == null || m.GetBaseDefinition() == m || m.GetBaseDefinition().DeclaringType.Assembly == type.Assembly)
				).Select(m => new MethodModel {
					Name = m.Name,
					Documentation = xml.GetDocumenation(m),
					XmlName = m.GetXmlDocumentationName(),
					GenericParameters = ProcessMethodGenericParameters(m),
					Parameters = ProcessParams(m),
					ExtensionOf = m.ExtensionOf(),
					Parent = type,
					ReturnType = m.ReturnType
				}).OrderBy(x => x.Name).ThenBy(x => x.ExtensionOf == null ? "" : x.ExtensionOf.ToSafeFullName()).ThenBy(x => x.Parameters.Count).ToList();
		}

		/// <summary>
		/// Processes the method generic parameters.
		/// </summary>
		/// <param name="m">The m.</param>
		/// <returns></returns>
		private IList<TypeModel> ProcessMethodGenericParameters(System.Reflection.MethodInfo m) {
			if(m.IsGenericMethod) {
				return m.GetGenericArguments().Select(t => new TypeModel { BaseType = t, Name = t.ToSafeName() }).ToList();
			} else {
				return default(IList<TypeModel>);
			}
		}

		/// <summary>
		/// Processes the parameters.
		/// </summary>
		/// <param name="m">The m.</param>
		/// <returns></returns>
		private IList<ParameterModel> ProcessParams(System.Reflection.MethodInfo m) {
			return m.GetParameters().Select(p => new ParameterModel {
				Type = new TypeModel { BaseType = p.ParameterType, Name = p.ParameterType.ToSafeName() },
				Name = p.Name,
				IsOut = p.IsOut,
				IsIn = p.IsIn,
				IsOptional = p.IsOptional
			}).ToList();
		}
	}
}