using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Madb.Site.Models.Documentation.Xml {
	public class Simple {
		[XmlIgnore]
		public string Value { get; set; }

		[XmlText]
		public XmlNode[] CDataContent {
			get {
				var dummy = new XmlDocument();
				return new XmlNode[] { dummy.CreateCDataSection(Value) };
			}
			set {
				if(value == null) {
					Value = null;
					return;
				}
				Value = string.Join(" ", value.Select(a => a.OuterXml)).Trim();
			}
		}

		public override string ToString() {
			return Value;
		}
	}
}