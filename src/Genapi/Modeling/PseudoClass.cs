using System.Collections.Generic;

namespace Tekcari.Genapi.Modeling
{
	public class PseudoClass
	{
		public int Visibility { get; set; }

		public string[] Modifiers { get; set; }

		public string Name { get; set; }

		public IEnumerable<PseudoMember> Members { get; set; }
	}
}