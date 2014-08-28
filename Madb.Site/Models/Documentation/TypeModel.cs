using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Camalot.Common.Extensions;
using Madb.Site.Extensions;

namespace Madb.Site.Models.Documentation {
	public class TypeModel {
		public Type BaseType { get; set; }
		public string Name { get; set; }
		public override string ToString() {
			return BaseType.ToSafeName();
		}
	}
}
