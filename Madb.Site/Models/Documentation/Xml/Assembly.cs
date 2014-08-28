using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Madb.Site.Models.Documentation.Xml {
	public class Assembly {
		[XmlElement("name")]
		public string Name { get; set; }
	}
}