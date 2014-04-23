using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapForgeDb.Reader
{
	public class ZoomTable : Dictionary<int, ZoomTable.ZoomTableEntry>
	{
		public struct ZoomTableEntry
		{
			public int NodesCount { get; private set; }
			public int WaysCount { get; private set; }

			public ZoomTableEntry ( int nodesCount, int waysCount ) : this()
			{
				this.NodesCount = nodesCount;
				this.WaysCount = waysCount;
			}


		}
		public ZoomTable ()
			: base()
		{
		}

		public ZoomTable (int capacity)
			: base(capacity)
		{
		}
	}
}
