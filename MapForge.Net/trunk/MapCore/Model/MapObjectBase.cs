using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapCore.Model
{
	public abstract class MapObjectBase
	{
		/**
		 * The layer of this way + 5 (to avoid negative values).
		 */
		public byte Layer { get; private set; }

		/**
		 * The tags of this way.
		 */
		public TagList Tags { get; private set; }

		protected MapObjectBase ( byte layer, IEnumerable<Tag> tags )
		{
			this.Layer = layer;
			this.Tags = tags as TagList ?? new TagList(tags);
		}

		public override string ToString ()
		{
			return string.Join(" ; ", Tags.Select(t=>t.ToString()));
		}
	}
}
