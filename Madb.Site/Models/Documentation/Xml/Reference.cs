using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Madb.Site.Models.Documentation.Xml {
	public class Reference {
		[XmlAttribute("cref")]
		public string Link { get; set; }
		[XmlText]
		public string Description { get; set; }
	}
}
