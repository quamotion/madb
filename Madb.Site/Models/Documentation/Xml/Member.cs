using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Madb.Site.Models.Documentation.Xml {
	[XmlRoot("member")]
	public class Member {
		[XmlAttribute("name")]
		public string Name { get; set; }
		[XmlElement("summary")]
		public List<Simple> Summary { get; set; }
		[XmlElement("typeparam")]
		public List<Param> TypeParams { get; set; }
		[XmlElement("param")]
		public List<Param> Params { get; set; }
		[XmlElement("returns")]
		public List<Simple> Returns { get; set; }
		[XmlElement("gist")]
		public List<Gist> Gists { get; set; }
		[XmlElement("remarks")]
		public List<Simple> Remarks { get; set; }
		[XmlElement("example")]
		public List<Simple> Examples { get; set; }
		[XmlElement("exception")]
		public List<Reference> Exceptions { get; set; }
		[XmlElement("permission")]
		public List<Reference> Permissions { get; set; }
		[XmlElement("see")]
		public List<Reference> Sees { get; set; }
		[XmlElement("seealso")]
		public List<Reference> SeeAlsos { get; set; }
		[XmlElement("value")]
		public List<Simple> Values { get; set; }
		[XmlElement("ignore")]
		[DefaultValue(false)]
		public bool Ignore { get; set; }
	}
}