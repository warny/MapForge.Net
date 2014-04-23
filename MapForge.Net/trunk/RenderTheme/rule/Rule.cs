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
using System.Linq;
using System.Windows.Media;
using MapCore.Model;
using RenderTheme.RenderInstructions;
namespace RenderTheme.Rule
{

	public sealed class Rule
	{
		private readonly List<RenderInstruction> renderInstructions;
		private readonly List<Rule> subRules;
		public List<IRuleTest> RuleTests { get; private set; }

		public Rule ( RuleBuilder ruleBuilder )
		{
			this.RuleTests = ruleBuilder.RuleTests;
			this.renderInstructions = new List<RenderInstruction>(4);
			this.subRules = new List<Rule>(4);
		}

		public void AddRenderingInstruction ( RenderInstruction renderInstruction )
		{
			this.renderInstructions.Add(renderInstruction);
		}

		public void AddSubRule ( Rule rule )
		{
			this.subRules.Add(rule);
		}

		public bool Matches ( MapElement mapElement )
		{
			return !RuleTests.Any() || RuleTests.All(rt=>rt.Test(mapElement));
		}

		public void Match (MapElement mapElement)
		{
			if (Matches(mapElement)) {
				mapElement.RenderInstructions.AddRange(this.renderInstructions);

				for (int i = 0, n = this.subRules.Count; i < n; ++i) {
					this.subRules[i].Match(mapElement);
				}
			}
		}

		public void OnComplete ()
		{
			this.renderInstructions.TrimExcess();
			this.subRules.TrimExcess();
			for (int i = 0, n = this.subRules.Count; i < n; ++i) {
				this.subRules[i].OnComplete();
			}
		}

		public void ScaleStrokeWidth ( float scaleFactor )
		{
			for (int i = 0, n = this.renderInstructions.Count; i < n; ++i) {
				this.renderInstructions[i].ScaleStrokeWidth(scaleFactor);
			}
			for (int i = 0, n = this.subRules.Count; i < n; ++i) {
				this.subRules[i].ScaleStrokeWidth(scaleFactor);
			}
		}

		public void ScaleTextSize ( float scaleFactor )
		{
			for (int i = 0, n = this.renderInstructions.Count; i < n; ++i) {
				this.renderInstructions[i].ScaleTextSize(scaleFactor);
			}
			for (int i = 0, n = this.subRules.Count; i < n; ++i) {
				this.subRules[i].ScaleTextSize(scaleFactor);
			}
		}
	}
}