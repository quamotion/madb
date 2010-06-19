#if DOTNET35
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace Managed.Adb.Utilities.Xml.Linq.Extensions
{
    /// <summary>
    /// Extensions to System.Object for LINQ to XML purposes.
    /// </summary>
    public static class ObjectExt
    {
        /// <summary>
        /// Returns the properties of the given object as XElements.
        /// Properties with null values are still returned, but as empty
        /// elements. Underscores in property names are replaces with hyphens.
        /// </summary>
        public static IEnumerable<XElement> AsXElements(this object source)
        {
            foreach (PropertyInfo prop in source.GetType().GetProperties())
            {
                object value = prop.GetValue(source, null);
                yield return new XElement(prop.Name.Replace("_", "-"), value);
            }
        }

        /// <summary>
        /// Returns the properties of the given object as XElements.
        /// Properties with null values are returned as empty attributes.
        /// Underscores in property names are replaces with hyphens.
        /// </summary>
        public static IEnumerable<XAttribute> AsXAttributes(this object source)
        {
            foreach (PropertyInfo prop in source.GetType().GetProperties())
            {
                object value = prop.GetValue(source, null);
                yield return new XAttribute(prop.Name.Replace("_", "-"), value ?? "");
            }
        }
    }
}
#endif