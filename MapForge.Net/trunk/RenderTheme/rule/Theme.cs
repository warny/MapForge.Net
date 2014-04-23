/*
 * Copyright 2010, 2011, 2012 mapsforge.org
 *
 * This program is free software: you can redistribute it and/or modify it under the
 * terms of the GNU Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System.Collections.Generic;
using MapCore.Model;
using RenderTheme.RenderInstructions;
using MapCore.Util;
using System.Windows.Media;
namespace RenderTheme.Rule
{

	/**
	 * A RenderTheme defines how ways and nodes are drawn.
	 */
	public class Theme
	{
		private const int MatchingCacheSize = 512;

		private readonly float baseStrokeWidth;
		private readonly float baseTextSize;
		private readonly LRUCache<MatchingCacheKey, List<RenderInstruction>> matchingCache;
		private readonly List<Rule> rulesList;

		public Theme ( RenderThemeBuilder renderThemeBuilder )
		{
			this.baseStrokeWidth = renderThemeBuilder.baseStrokeWidth;
			this.baseTextSize = renderThemeBuilder.baseTextSize;
			this.MapBackground = renderThemeBuilder.mapBackground;
			this.rulesList = new List<Rule>();
			this.matchingCache = new LRUCache<MatchingCacheKey, List<RenderInstruction>>(MatchingCacheSize);
		}

		/**
		 * Must be called when this RenderTheme gets destroyed to clean up and free resources.
		 */
		public void Dispose ()
		{
			this.matchingCache.Clear();
		}

		/**
		 * @return the number of distinct drawing levels required by this RenderTheme.
		 */
		public int Levels { get; set; }

		/**
		 * @return the map background color of this RenderTheme.
		 */
		public Color MapBackground { get; private set; }

		/**
		 * Scales the stroke width of this RenderTheme by the given factor.
		 * 
		 * @param scaleFactor
		 *            the factor by which the stroke width should be scaled.
		 */
		public void ScaleStrokeWidth ( float scaleFactor )
		{
			for (int i = 0, n = this.rulesList.Count; i < n; ++i) {
				this.rulesList[i].ScaleStrokeWidth(scaleFactor * this.baseStrokeWidth);
			}
		}

		/**
		 * Scales the text size of this RenderTheme by the given factor.
		 * 
		 * @param scaleFactor
		 *            the factor by which the text size should be scaled.
		 */
		public void ScaleTextSize ( float scaleFactor )
		{
			for (int i = 0, n = this.rulesList.Count; i < n; ++i) {
				this.rulesList[i].ScaleTextSize(scaleFactor * this.baseTextSize);
			}
		}

		public void Match (MapElement mapElement) // ( RenderCallback renderCallback, List<Tag> tags, byte zoomLevel )
		{
			MatchingCacheKey matchingCacheKey = new MatchingCacheKey(mapElement.Tags, mapElement.Tile.ZoomFactor);

			List<RenderInstruction> matchingList;
			if (this.matchingCache.TryGetValue(matchingCacheKey, out matchingList)) {
				// cache hit
				for (int i = 0, n = matchingList.Count; i < n; ++i) {
					mapElement.RenderInstructions.AddRange(matchingList);
				}
				return;
			}

			// cache miss
			matchingList = new List<RenderInstruction>();
			for (int i = 0, n = this.rulesList.Count; i < n; ++i) {
				this.rulesList[i].Match(mapElement);
			}

			this.matchingCache[matchingCacheKey] = matchingList;
		}

		public void AddRule ( Rule rule )
		{
			this.rulesList.Add(rule);
		}

		public void Complete ()
		{
			this.rulesList.TrimExcess();
			for (int i = 0, n = this.rulesList.Count; i < n; ++i) {
				this.rulesList[i].OnComplete();
			}
		}
	}
}