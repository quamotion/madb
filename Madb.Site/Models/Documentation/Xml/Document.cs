using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Madb.Site.Models.Documentation.Xml {
	[XmlRoot("doc")]
	public class Document {
		[XmlArrayItem("member")]
		[XmlArray("members")]
		public IList<Member> Members { get; set; }
	}
}