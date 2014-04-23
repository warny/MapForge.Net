using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MapCore;
using MapCore.Util;
using RenderTheme;

namespace RenderTheme.Rule
{
	public interface IRuleTest
	{
		bool Test ( MapElement mapElement );
		bool IsCovered(IRuleTest ruleTest);
	}

	public class ZoomTest : IRuleTest
	{
		private readonly double zoomMin;
		private readonly double zoomMax;

		public static ZoomTest Build ( RuleBuilder rule )
		{
			if (rule.zoomMin == 0 && rule.zoomMax == 255) return null;
			return new ZoomTest(rule.zoomMin, rule.zoomMax);
		}

		#region IRuleTest Membres

		private ZoomTest(double zoomMin, double zoomMax)
		{
			this.zoomMin = zoomMin;
			this.zoomMax = zoomMax;
		}

		public bool Test ( MapElement mapElement )
		{
			return !(mapElement.Tile.ZoomFactor < zoomMin) && !(mapElement.Tile.ZoomFactor > zoomMax);
		}

		public bool IsCovered ( IRuleTest ruleTest )
		{
			if (!(ruleTest is ZoomTest)) return false;
			var rule = (ZoomTest) ruleTest;
			if (rule.zoomMin < this.zoomMin) return false;
			if (rule.zoomMax > this.zoomMax) return false;
			return true;
		}

		#endregion
	}

	public class KeyNoValueTest : IRuleTest
	{
		private readonly List<string> keys;

		public static KeyNoValueTest Build(RuleBuilder rule)
		{
			if (!rule.valueList.Contains("~")) return null;
			List<string> keyList = new List<string>(rule.keyList);
			if (!keyList.Any()) keyList.Add("*");

			return new KeyNoValueTest(keyList);
		}

		private KeyNoValueTest ( List<string> keys )
		{
			this.keys = keys;
		}

		#region IRuleTest Membres
		public bool Test ( MapElement mapElement )
		{
			return !mapElement.Tags.Match(keys);
		}

		public bool IsCovered ( IRuleTest ruleTest )
		{
			if (ruleTest is KeyNoValueTest) {
				var rule = (KeyNoValueTest) ruleTest;
				keys.RemoveAll(k => rule.keys.Any(key => Wildcard.Compare(k, key)));

				if (!keys.Any()) keys.Add("*");

				if (keys[0] == "*") return true;
			}
			return false;
		}

		#endregion
	}

	public class KeyValueTest : IRuleTest
	{
		public static KeyValueTest Build ( RuleBuilder rule )
		{
			List<string> keyList = new List<string>(rule.keyList);
			List<string> valueList = new List<string>(rule.valueList);
			if ((!keyList.Any() || keyList.Contains("*"))) {
				keyList.Clear();
				keyList.Add("*");
			}
			valueList.Remove("~");
			if ((!valueList.Any() || valueList.Contains("*"))) {
				valueList.Clear();
				valueList.Add("*");
			}

			if (keyList[0] == "*" && valueList[0] == "*") return null;
			return new KeyValueTest(keyList, valueList);
		}
		
		private readonly List<string> keys;
		private readonly List<string> values;

		private KeyValueTest ( List<string> keys, List<string> values )
		{
			this.keys = keys;
			this.values = values;

			Wildcard.TrimPatterns(keys);
			Wildcard.TrimPatterns(values);
		}

		#region IRuleTest Membres

		public bool Test ( MapElement mapElement )
		{
			return mapElement.Tags.Match(keys, values);
		}

		public bool IsCovered ( IRuleTest ruleTest )
		{
			if (!(ruleTest is KeyValueTest)) return false;
			var rule = (KeyValueTest)ruleTest;
			keys.RemoveAll(k => rule.keys.Any(key => (Wildcard) key == k));
			values.RemoveAll(v => rule.values.Any(value => (Wildcard)value == v));

			if (!keys.Any()) keys.Add("*");
			if (!values.Any()) values.Add("*");

			if (keys[0] == "*" && values[0] == "*") return true;

			return false;
		}

		#endregion
	}

	public class ClosedWayTest : IRuleTest
	{
		#region singletons
		public static readonly ClosedWayTest ClosedWayTester = new ClosedWayTest(true);
		public static readonly ClosedWayTest OpenedWayTester = new ClosedWayTest(false);

		public static ClosedWayTest Build ( RuleBuilder rule )
		{
			switch (rule.Closed) {
				case "yes":
					return ClosedWayTester;
				case "no":
					return OpenedWayTester;
				default:
					return null;
			}
		}

		#endregion
		private readonly bool opened;

		private ClosedWayTest ( bool closed )
		{
			this.opened = !closed;
		}

		#region IRuleTest Membres

		public bool Test ( MapElement mapElement )
		{
			return mapElement.IsClosed != null && mapElement.IsClosed.Value ^ opened;
		}

		public bool IsCovered ( IRuleTest ruleTest )
		{
 			if (!(ruleTest is ClosedWayTest)) return false;
			var rule = (ClosedWayTest)ruleTest;
			return this.opened == rule.opened;
		}

		#endregion
	}

	public class ClassTester : IRuleTest
	{
		#region singletons
		public static readonly ClassTester WayTester = new ClassTester(ClassEnum.Way);
		public static readonly ClassTester NodeTester = new ClassTester(ClassEnum.Node);

		public static ClassTester Build ( RuleBuilder rule )
		{
			switch (rule.ElementClass) {
				case "way" :
					return WayTester;
				case "node":
				case "poi":
					return NodeTester;
				default:
					return null;
			}
		}

		#endregion

		private readonly ClassEnum @class;

		private ClassTester(ClassEnum @class)
		{
			this.@class = @class;
		}

		#region IRuleTest Membres

		public bool Test(MapElement mapElement)
		{
			switch (@class) {
				case ClassEnum.Node:
					return mapElement.WayGeometry == null;
				case ClassEnum.Way:
					return mapElement.WayGeometry != null;
				default:
					return true;
			}
		}

		public bool IsCovered ( IRuleTest ruleTest )
		{
			if (!(ruleTest is ClassTester)) return false;
			var rule = (ClassTester)ruleTest;
			return this.@class == rule.@class;
		}

		#endregion
	}

}
