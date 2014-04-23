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
using MapCore.Model;
using System.Collections.Generic;
using RenderTheme.Graphics;
using System.Windows.Media.Imaging;
using MapCore;
using System.Linq;

namespace MapsUtilities.MapGenerator.DatabaseRenderer
{

	/**
	 * This class process the methods for the Dependency Cache. It's connected with the LabelPlacement class. The main goal
	 * is, to remove double labels and symbols that are already rendered, from the actual tile. Labels and symbols that,
	 * would be rendered on an already drawn Tile, will be deleted too.
	 */
	public class DependencyCache
	{
		/**
		 * The class holds the data for a symbol with dependencies on other tiles.
		 * 
		 * @param <Type>
		 *            only two types are reasonable. The DependencySymbol or DependencyText class.
		 */
		private class Dependency<T>
		{
			public MapCore.Model.MapPoint point;
			public T value;

			public Dependency ( T value, MapCore.Model.MapPoint point )
			{
				this.value = value;
				this.point = point;
			}
		}

		/**
		 * This class holds all the information off the possible dependencies on a tile.
		 */
		private class DependencyOnTile
		{
			public bool drawn;
			public List<Dependency<DependencyText>> labels;
			public List<Dependency<DependencySymbol>> symbols;

			/**
			 * Initialize label, symbol and drawn.
			 */
			public DependencyOnTile ()
			{
				this.labels = null;
				this.symbols = null;
				this.drawn = false;
			}

			/**
			 * @param toAdd
			 *            a dependency Symbol
			 */
			public void AddSymbol ( Dependency<DependencySymbol> toAdd )
			{
				if (this.symbols == null) {
					this.symbols = new List<Dependency<DependencySymbol>>();
				}
				this.symbols.Add(toAdd);
			}

			/**
			 * @param toAdd
			 *            a Dependency Text
			 */
			public void AddText ( Dependency<DependencyText> toAdd )
			{
				if (this.labels == null) {
					this.labels = new List<Dependency<DependencyText>>();
				}
				this.labels.Add(toAdd);
			}
		}

		/**
		 * The class holds the data for a symbol with dependencies on other tiles.
		 */
		private class DependencySymbol
		{
			private List<Tile> tiles;
			public BitmapImage symbol;

			/**
			 * Creates a symbol dependency element for the dependency cache.
			 * 
			 * @param symbol
			 *            reference on the dependency symbol.
			 * @param tile
			 *            dependency tile.
			 */
			public DependencySymbol ( BitmapImage symbol, Tile tile )
			{
				this.symbol = symbol;
				this.tiles = new List<Tile>();
				this.tiles.Add(tile);
			}

			/**
			 * Adds an additional tile, which has an dependency with this symbol.
			 * 
			 * @param tile
			 *            additional tile.
			 */
			public void addTile ( Tile tile )
			{
				this.tiles.Add(tile);
			}
		}

		/**
		 * The class holds the data for a label with dependencies on other tiles.
		 */
		private class DependencyText
		{
			public Rect boundary;
			public Paint paintBack;
			public Paint paintFront;
			public string text;
			public List<Tile> tiles;

			/**
			 * Creates a text dependency in the dependency cache.
			 * 
			 * @param paintFront
			 *            paint element from the front.
			 * @param paintBack
			 *            paint element form the background of the text.
			 * @param text
			 *            the text of the element.
			 * @param boundary
			 *            the fixed boundary with width and height.
			 * @param tile
			 *            all tile in where the element has an influence.
			 */
			public DependencyText ( Paint paintFront, Paint paintBack, string text, Rect boundary, Tile tile )
			{
				this.paintFront = paintFront;
				this.paintBack = paintBack;
				this.text = text;
				this.tiles = new List<Tile>();
				this.tiles.Add(tile);
				this.boundary = boundary;
			}

			public void addTile ( Tile tile )
			{
				this.tiles.Add(tile);
			}
		}

		private DependencyOnTile currentDependencyOnTile;
		private Tile currentTile;

		/**
		 * Hash table, that connects the Tiles with their entries in the dependency cache.
		 */
		Dictionary<Tile, DependencyOnTile> dependencyTable;
		Dependency<DependencyText> depLabel;
		Rect rect1;
		Rect rect2;
		SymbolContainer smb;
		DependencyOnTile tmp;

		/**
		 * Constructor for this class, that creates a hashtable for the dependencies.
		 */
		public DependencyCache ()
		{
			this.dependencyTable = new Dictionary<Tile, DependencyOnTile>(60);
		}

		private void addLabelsFromDependencyOnTile ( List<pointTextContainer> labels )
		{
			for (int i = 0; i < this.currentDependencyOnTile.labels.Count; i++) {
				this.depLabel = this.currentDependencyOnTile.labels[i];
				if (this.depLabel.value.paintBack != null) {
					labels.Add(new PointTextContainer(this.depLabel.value.text, this.depLabel.point.X,
							this.depLabel.point.Y, this.depLabel.value.paintFront, this.depLabel.value.paintBack));
				} else {
					labels.Add(new PointTextContainer(this.depLabel.value.text, this.depLabel.point.X,
							this.depLabel.point.Y, this.depLabel.value.paintFront));
				}
			}
		}

		private void addSymbolsFromDependencyOnTile ( List<SymbolContainer> symbols )
		{
			foreach (Dependency<DependencySymbol> depSmb in this.currentDependencyOnTile.symbols) {
				symbols.Add(new SymbolContainer(depSmb.value.symbol, depSmb.point));
			}
		}

		/**
		 * Fills the dependency entry from the tile and the neighbor tiles with the dependency information, that are
		 * necessary for drawing. To do that every label and symbol that will be drawn, will be checked if it produces
		 * dependencies with other tiles.
		 * 
		 * @param pTC
		 *            list of the labels
		 */
		private void fillDependencyLabels ( List<pointTextContainer> pTC )
		{
			Tile left = new Tile(this.currentTile.TileX - 1, this.currentTile.TileY, this.currentTile.ZoomFactor);
			Tile right = new Tile(this.currentTile.TileX + 1, this.currentTile.TileY, this.currentTile.ZoomFactor);
			Tile up = new Tile(this.currentTile.TileX, this.currentTile.TileY - 1, this.currentTile.ZoomFactor);
			Tile down = new Tile(this.currentTile.TileX, this.currentTile.TileY + 1, this.currentTile.ZoomFactor);

			Tile leftup = new Tile(this.currentTile.TileX - 1, this.currentTile.TileY - 1, this.currentTile.ZoomFactor);
			Tile leftdown = new Tile(this.currentTile.TileX - 1, this.currentTile.TileY + 1, this.currentTile.ZoomFactor);
			Tile rightup = new Tile(this.currentTile.TileX + 1, this.currentTile.TileY - 1, this.currentTile.ZoomFactor);
			Tile rightdown = new Tile(this.currentTile.TileX + 1, this.currentTile.TileY + 1, this.currentTile.ZoomFactor);

			pointTextContainer label;
			DependencyOnTile linkedDep;
			DependencyText toAdd;

			for (int i = 0; i < pTC.Count; i++) {
				label = pTC[i];

				toAdd = null;

				// up
				if ((label.y - label.boundary.Height < 0.0f) && (!this.dependencyTable[up].drawn)) {
					linkedDep = this.dependencyTable[up];

					toAdd = new DependencyText(label.paintFront, label.paintBack, label.text, label.boundary, this.currentTile);

					this.currentDependencyOnTile
							.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x, label.y)));

					linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x, label.y + Tile.TileSize)));

					toAdd.addTile(up);

					if ((label.x < 0.0f) && (!this.dependencyTable[leftup].drawn)) {
						linkedDep = this.dependencyTable[leftup];

						linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x + Tile.TileSize, label.y + Tile.TileSize)));

						toAdd.addTile(leftup);
					}

					if ((label.x + label.boundary.Width > Tile.TileSize) && (!this.dependencyTable[rightup].drawn)) {
						linkedDep = this.dependencyTable[rightup];

						linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x - Tile.TileSize, label.y + Tile.TileSize)));

						toAdd.addTile(rightup);
					}
				}

				// down
				if ((label.y > Tile.TileSize) && (!this.dependencyTable[down].drawn)) {
					linkedDep = this.dependencyTable[down];

					if (toAdd == null) {
						toAdd = new DependencyText(label.paintFront, label.paintBack, label.text, label.boundary, this.currentTile);

						this.currentDependencyOnTile.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x, label.y)));

					}

					linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x, label.y - Tile.TileSize)));

					toAdd.addTile(down);

					if ((label.x < 0.0f) && (!this.dependencyTable[leftdown].drawn)) {
						linkedDep = this.dependencyTable[leftdown];

						linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x + Tile.TileSize, label.y - Tile.TileSize)));

						toAdd.addTile(leftdown);
					}

					if ((label.x + label.boundary.Width > Tile.TileSize) && (!this.dependencyTable[rightdown].drawn)) {
						linkedDep = this.dependencyTable[rightdown];

						linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x - Tile.TileSize, label.y - Tile.TileSize)));

						toAdd.addTile(rightdown);
					}
				}
				// left

				if ((label.x < 0.0f) && (!this.dependencyTable[left].drawn)) {
					linkedDep = this.dependencyTable[left];

					if (toAdd == null) {
						toAdd = new DependencyText(label.paintFront, label.paintBack, label.text, label.boundary,
								this.currentTile);

						this.currentDependencyOnTile.AddText(new Dependency<DependencyText>(toAdd, new MapPoint(label.x,
								label.y)));
					}

					linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapPoint(label.x + Tile.TileSize, label.y)));

					toAdd.addTile(left);
				}
				// right
				if ((label.x + label.boundary.Width > Tile.TileSize) && (!this.dependencyTable[right].drawn)) {
					linkedDep = this.dependencyTable[right];

					if (toAdd == null) {
						toAdd = new DependencyText(label.paintFront, label.paintBack, label.text, label.boundary,
								this.currentTile);

						this.currentDependencyOnTile.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x, label.y)));
					}

					linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x - Tile.TileSize, label.y)));

					toAdd.addTile(right);
				}

				// check symbols

				if ((label.symbol != null) && (toAdd == null)) {
					if ((label.symbol.point.Y <= 0.0f) && (!this.dependencyTable[up].drawn)) {
						linkedDep = this.dependencyTable[up];

						toAdd = new DependencyText(label.paintFront, label.paintBack, label.text, label.boundary,
								this.currentTile);

						this.currentDependencyOnTile.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x,
								label.y)));

						linkedDep.AddText(new Dependency<DependencyText>(toAdd,
								new MapCore.Model.MapPoint(label.x, label.y + Tile.TileSize)));

						toAdd.addTile(up);

						if ((label.symbol.point.X < 0.0f) && (!this.dependencyTable[leftup].drawn)) {
							linkedDep = this.dependencyTable[leftup];

							linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x + Tile.TileSize,
									label.y + Tile.TileSize)));

							toAdd.addTile(leftup);
						}

						if ((label.symbol.point.X + label.symbol.symbol.Width > Tile.TileSize)
								&& (!this.dependencyTable[rightup].drawn)) {
							linkedDep = this.dependencyTable[rightup];

							linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x - Tile.TileSize,
									label.y + Tile.TileSize)));

							toAdd.addTile(rightup);
						}
					}

					if ((label.symbol.point.Y + label.symbol.symbol.Height >= Tile.TileSize)
							&& (!this.dependencyTable[down].drawn)) {
						linkedDep = this.dependencyTable[down];

						if (toAdd == null) {
							toAdd = new DependencyText(label.paintFront, label.paintBack, label.text, label.boundary,
									this.currentTile);

							this.currentDependencyOnTile.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x,
									label.y)));
						}

						linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x, label.y + Tile.TileSize)));

						toAdd.addTile(up);

						if ((label.symbol.point.X < 0.0f) && (!this.dependencyTable[leftdown].drawn)) {
							linkedDep = this.dependencyTable[leftdown];

							linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x + Tile.TileSize, label.y - Tile.TileSize)));

							toAdd.addTile(leftdown);
						}

						if ((label.symbol.point.X + label.symbol.symbol.Width > Tile.TileSize)
								&& (!this.dependencyTable[rightdown].drawn)) {
							linkedDep = this.dependencyTable[rightdown];

							linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x - Tile.TileSize, label.y - Tile.TileSize)));

							toAdd.addTile(rightdown);
						}
					}

					if ((label.symbol.point.X <= 0.0f) && (!this.dependencyTable[left].drawn)) {
						linkedDep = this.dependencyTable[left];

						if (toAdd == null) {
							toAdd = new DependencyText(label.paintFront, label.paintBack, label.text, label.boundary,
									this.currentTile);

							this.currentDependencyOnTile.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x, label.y)));
						}

						linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x - Tile.TileSize, label.y)));

						toAdd.addTile(left);
					}

					if ((label.symbol.point.X + label.symbol.symbol.Width >= Tile.TileSize)
							&& (!this.dependencyTable[right].drawn)) {
						linkedDep = this.dependencyTable[right];

						if (toAdd == null) {
							toAdd = new DependencyText(label.paintFront, label.paintBack, label.text, label.boundary,
									this.currentTile);

							this.currentDependencyOnTile.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x, label.y)));
						}

						linkedDep.AddText(new Dependency<DependencyText>(toAdd, new MapCore.Model.MapPoint(label.x + Tile.TileSize, label.y)));

						toAdd.addTile(right);
					}
				}
			}
		}

		private void fillDependencyOnTile2 ( List<pointTextContainer> labels, List<SymbolContainer> symbols,
				List<pointTextContainer> areaLabels )
		{
			Tile left = new Tile(this.currentTile.TileX - 1, this.currentTile.TileY, this.currentTile.ZoomFactor);
			Tile right = new Tile(this.currentTile.TileX + 1, this.currentTile.TileY, this.currentTile.ZoomFactor);
			Tile up = new Tile(this.currentTile.TileX, this.currentTile.TileY - 1, this.currentTile.ZoomFactor);
			Tile down = new Tile(this.currentTile.TileX, this.currentTile.TileY + 1, this.currentTile.ZoomFactor);

			Tile leftup = new Tile(this.currentTile.TileX - 1, this.currentTile.TileY - 1, this.currentTile.ZoomFactor);
			Tile leftdown = new Tile(this.currentTile.TileX - 1, this.currentTile.TileY + 1, this.currentTile.ZoomFactor);
			Tile rightup = new Tile(this.currentTile.TileX + 1, this.currentTile.TileY - 1, this.currentTile.ZoomFactor);
			Tile rightdown = new Tile(this.currentTile.TileX + 1, this.currentTile.TileY + 1, this.currentTile.ZoomFactor);

			if (!this.dependencyTable.ContainsKey(up)) {
				this.dependencyTable[up] = new DependencyOnTile();
			}
			if (!this.dependencyTable.ContainsKey(down)) {
				this.dependencyTable[down] = new DependencyOnTile();
			}
			if (!this.dependencyTable.ContainsKey(left)) {
				this.dependencyTable[left] = new DependencyOnTile();
			}
			if (!this.dependencyTable.ContainsKey(right)) {
				this.dependencyTable[right] = new DependencyOnTile();
			}
			if (!this.dependencyTable.ContainsKey(leftdown)) {
				this.dependencyTable[leftdown] = new DependencyOnTile();
			}
			if (!this.dependencyTable.ContainsKey(rightup)) {
				this.dependencyTable[rightup] = new DependencyOnTile();
			}
			if (!this.dependencyTable.ContainsKey(leftup)) {
				this.dependencyTable[leftup] = new DependencyOnTile();
			}
			if (!this.dependencyTable.ContainsKey(rightdown)) {
				this.dependencyTable[rightdown] = new DependencyOnTile();
			}

			fillDependencyLabels(labels);
			fillDependencyLabels(areaLabels);

			DependencyOnTile linkedDep;
			DependencySymbol addSmb;

			foreach (SymbolContainer symbol in symbols) {
				addSmb = null;

				// up
				if ((symbol.point.Y < 0.0f) && (!this.dependencyTable[up].drawn)) {
					linkedDep = this.dependencyTable[up];

					addSmb = new DependencySymbol(symbol.symbol, this.currentTile);
					this.currentDependencyOnTile.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapPoint(symbol.point.X, symbol.point.Y)));

					linkedDep.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapPoint(symbol.point.X, symbol.point.Y + Tile.TileSize)));
					addSmb.addTile(up);

					if ((symbol.point.X < 0.0f) && (!this.dependencyTable[leftup].drawn)) {
						linkedDep = this.dependencyTable[leftup];

						linkedDep.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapPoint(symbol.point.X + Tile.TileSize, symbol.point.Y + Tile.TileSize)));
						addSmb.addTile(leftup);
					}

					if ((symbol.point.X + symbol.symbol.Width > Tile.TileSize)
							&& (!this.dependencyTable[rightup].drawn)) {
						linkedDep = this.dependencyTable[rightup];

						linkedDep.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapPoint(symbol.point.X - Tile.TileSize, symbol.point.Y + Tile.TileSize)));
						addSmb.addTile(rightup);
					}
				}

				// down
				if ((symbol.point.Y + symbol.symbol.Height > Tile.TileSize)
						&& (!this.dependencyTable[down].drawn)) {
					linkedDep = this.dependencyTable[down];

					if (addSmb == null) {
						addSmb = new DependencySymbol(symbol.symbol, this.currentTile);
						this.currentDependencyOnTile.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapCore.Model.MapPoint(symbol.point.X, symbol.point.Y)));
					}

					linkedDep.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapCore.Model.MapPoint(symbol.point.X, symbol.point.Y - Tile.TileSize)));
					addSmb.addTile(down);

					if ((symbol.point.X < 0.0f) && (!this.dependencyTable[leftdown].drawn)) {
						linkedDep = this.dependencyTable[leftdown];

						linkedDep.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapCore.Model.MapPoint(symbol.point.X + Tile.TileSize, symbol.point.Y - Tile.TileSize)));
						addSmb.addTile(leftdown);
					}

					if ((symbol.point.X + symbol.symbol.Width > Tile.TileSize)
							&& (!this.dependencyTable[rightdown].drawn)) {
						linkedDep = this.dependencyTable[rightdown];

						linkedDep.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapCore.Model.MapPoint(symbol.point.X - Tile.TileSize, symbol.point.Y - Tile.TileSize)));
						addSmb.addTile(rightdown);
					}
				}

				// left
				if ((symbol.point.X < 0.0f) && (!this.dependencyTable[left].drawn)) {
					linkedDep = this.dependencyTable[left];

					if (addSmb == null) {
						addSmb = new DependencySymbol(symbol.symbol, this.currentTile);
						this.currentDependencyOnTile.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapCore.Model.MapPoint(symbol.point.X, symbol.point.Y)));
					}

					linkedDep.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapCore.Model.MapPoint(symbol.point.X + Tile.TileSize, symbol.point.Y)));
					addSmb.addTile(left);
				}

				// right
				if ((symbol.point.X + symbol.symbol.Width > Tile.TileSize)
						&& (!this.dependencyTable[right].drawn)) {
					linkedDep = this.dependencyTable[right];
					if (addSmb == null) {
						addSmb = new DependencySymbol(symbol.symbol, this.currentTile);
						this.currentDependencyOnTile.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapPoint(
								symbol.point.X, symbol.point.Y)));
					}

					linkedDep.AddSymbol(new Dependency<DependencySymbol>(addSmb, new MapPoint(symbol.point.X - Tile.TileSize,
							symbol.point.Y)));
					addSmb.addTile(right);
				}
			}
		}

		private void removeOverlappingAreaLabelsWithDependencyLabels ( List<pointTextContainer> areaLabels )
		{
			pointTextContainer pTC;

			for (int i = 0; i < this.currentDependencyOnTile.labels.Count; i++) {
				this.depLabel = this.currentDependencyOnTile.labels[i];
				this.rect1 = new Rect((int)(this.depLabel.point.X),
						(int)(this.depLabel.point.Y - this.depLabel.value.boundary.Height),
						(int)(this.depLabel.point.X + this.depLabel.value.boundary.Width), (int)(this.depLabel.point.Y));

				for (int x = 0; x < areaLabels.Count; x++) {
					pTC = areaLabels[x];

					this.rect2 = new Rect((int)pTC.x, (int)pTC.y - pTC.boundary.Height, (int)pTC.x
							+ pTC.boundary.Width, (int)pTC.y);

					if (Utils.Intersects(this.rect2, this.rect1)) {
						areaLabels.Remove(x);
						x--;
					}
				}
			}
		}

		private void removeOverlappingAreaLabelsWithDependencySymbols ( List<pointTextContainer> areaLabels )
		{
			pointTextContainer label;

			foreach (Dependency<DependencySymbol> depSmb in this.currentDependencyOnTile.symbols) {
				this.rect1 = new Rect((int)depSmb.point.X, (int)depSmb.point.Y, (int)depSmb.point.X
						+ depSmb.value.symbol.Width, (int)depSmb.point.Y + depSmb.value.symbol.Height);

				for (int x = 0; x < areaLabels.Count; x++) {
					label = areaLabels[x];

					this.rect2 = new Rect((int)(label.x), (int)(label.y - label.boundary.Height),
							(int)(label.x + label.boundary.Width), (int)(label.y));

					if (Utils.Intersects(this.rect2, this.rect1)) {
						areaLabels.Remove(x);
						x--;
					}
				}
			}
		}

		private void removeOverlappingLabelsWithDependencyLabels ( List<pointTextContainer> labels )
		{
			for (int i = 0; i < this.currentDependencyOnTile.labels.Count; i++) {
				for (int x = 0; x < labels.Count; x++) {
					if ((labels[x].text.Equals(this.currentDependencyOnTile.labels[i].value.text))
							&& (labels[x].paintFront
									.Equals(this.currentDependencyOnTile.labels[i].value.paintFront))
							&& (labels[x].paintBack.Equals(this.currentDependencyOnTile.labels[i].value.paintBack))) {
						labels.Remove(x);
						i--;
						break;
					}
				}
			}
		}

		private void removeOverlappingSymbolsWithDepencySymbols ( List<SymbolContainer> symbols, int dis )
		{
			SymbolContainer symbolContainer;
			Dependency<DependencySymbol> sym2;

			for (int x = 0; x < this.currentDependencyOnTile.symbols.Count; x++) {
				sym2 = this.currentDependencyOnTile.symbols[x];
				this.rect1 = new Rect((int)sym2.point.X - dis, (int)sym2.point.Y - dis, (int)sym2.point.X
						+ sym2.value.symbol.Width + dis, (int)sym2.point.Y + sym2.value.symbol.Height + dis);

				for (int y = 0; y < symbols.Count; y++) {
					symbolContainer = symbols[y];
					this.rect2 = new Rect((int)symbolContainer.point.X, (int)symbolContainer.point.Y,
							(int)symbolContainer.point.X + symbolContainer.symbol.Width,
							(int)symbolContainer.point.Y + symbolContainer.symbol.Height);

					if (Utils.Intersects(this.rect2, this.rect1)) {
						symbols.Remove(y);
						y--;
					}
				}
			}
		}

		private void removeOverlappingSymbolsWithDependencyLabels ( List<SymbolContainer> symbols )
		{
			for (int i = 0; i < this.currentDependencyOnTile.labels.Count; i++) {
				this.depLabel = this.currentDependencyOnTile.labels[i];
				this.rect1 = new Rect((int)(this.depLabel.point.X),
						(int)(this.depLabel.point.Y - this.depLabel.value.boundary.Height),
						(int)(this.depLabel.point.X + this.depLabel.value.boundary.Width), (int)(this.depLabel.point.Y));

				for (int x = 0; x < symbols.Count; x++) {
					this.smb = symbols[x];

					this.rect2 = new Rect((int)this.smb.point.X, (int)this.smb.point.Y, (int)this.smb.point.X
							+ this.smb.symbol.Width, (int)this.smb.point.Y + this.smb.symbol.Height);

					if (Utils.Intersects(this.rect2, this.rect1)) {
						symbols.Remove(x);
						x--;
					}
				}
			}
		}

		/**
		 * This method fills the entries in the dependency cache of the tiles, if their dependencies.
		 * 
		 * @param labels
		 *            current labels, that will be displayed.
		 * @param symbols
		 *            current symbols, that will be displayed.
		 * @param areaLabels
		 *            current areaLabels, that will be displayed.
		 */
		public void fillDependencyOnTile ( List<pointTextContainer> labels, List<SymbolContainer> symbols,
				List<pointTextContainer> areaLabels )
		{
			this.currentDependencyOnTile.drawn = true;

			if ((labels.Any()) || (symbols.Any()) || (areaLabels.Any())) {
				fillDependencyOnTile2(labels, symbols, areaLabels);
			}

			if (this.currentDependencyOnTile.labels != null) {
				addLabelsFromDependencyOnTile(labels);
			}
			if (this.currentDependencyOnTile.symbols != null) {
				addSymbolsFromDependencyOnTile(symbols);
			}
		}

		/**
		 * This method must be called, before the dependencies will be handled correctly. Because it sets the actual Tile
		 * and looks if it has already dependencies.
		 * 
		 * @param tile
		 *            the current Tile
		 */
		public void generateTileAndDependencyOnTile ( Tile tile )
		{
			this.currentTile = new Tile(tile.TileX, tile.TileY, tile.ZoomFactor);
			this.currentDependencyOnTile = this.dependencyTable[this.currentTile];

			if (this.currentDependencyOnTile == null) {
				this.dependencyTable[this.currentTile] = new DependencyOnTile();
				this.currentDependencyOnTile = this.dependencyTable[this.currentTile];
			}
		}

		/**
		 * Removes the are labels from the actual list, that would be rendered in a Tile that has already be drawn.
		 * 
		 * @param areaLabels
		 *            current area Labels, that will be displayed
		 */
		public void removeAreaLabelsInAlreadyDrawnAreas ( List<pointTextContainer> areaLabels )
		{
			Tile lefttmp = new Tile(this.currentTile.TileX - 1, this.currentTile.TileY, this.currentTile.ZoomFactor);
			Tile righttmp = new Tile(this.currentTile.TileX + 1, this.currentTile.TileY, this.currentTile.ZoomFactor);
			Tile uptmp = new Tile(this.currentTile.TileX, this.currentTile.TileY - 1, this.currentTile.ZoomFactor);
			Tile downtmp = new Tile(this.currentTile.TileX, this.currentTile.TileY + 1, this.currentTile.ZoomFactor);

			bool up;
			bool left;
			bool right;
			bool down;

			this.tmp = this.dependencyTable[lefttmp];
			left = this.tmp == null ? false : this.tmp.drawn;

			this.tmp = this.dependencyTable[righttmp];
			right = this.tmp == null ? false : this.tmp.drawn;

			this.tmp = this.dependencyTable[uptmp];
			up = this.tmp == null ? false : this.tmp.drawn;

			this.tmp = this.dependencyTable[downtmp];
			down = this.tmp == null ? false : this.tmp.drawn;

			pointTextContainer label;

			for (int i = 0; i < areaLabels.Count; i++) {
				label = areaLabels[i];

				if (up && label.y - label.boundary.Height < 0.0f) {
					areaLabels.Remove(i);
					i--;
					continue;
				}

				if (down && label.y > Tile.TileSize) {
					areaLabels.Remove(i);
					i--;
					continue;
				}
				if (left && label.x < 0.0f) {
					areaLabels.Remove(i);
					i--;
					continue;
				}
				if (right && label.x + label.boundary.Width > Tile.TileSize) {
					areaLabels.Remove(i);
					i--;
					continue;
				}
			}
		}

		/**
		 * Removes all objects that overlaps with the objects from the dependency cache.
		 * 
		 * @param labels
		 *            labels from the current tile
		 * @param areaLabels
		 *            area labels from the current tile
		 * @param symbols
		 *            symbols from the current tile
		 */
		public void removeOverlappingObjectsWithDependencyOnTile ( List<pointTextContainer> labels,
				List<pointTextContainer> areaLabels, List<SymbolContainer> symbols )
		{
			if (this.currentDependencyOnTile.labels != null && this.currentDependencyOnTile.labels.Count != 0) {
				removeOverlappingLabelsWithDependencyLabels(labels);
				removeOverlappingSymbolsWithDependencyLabels(symbols);
				removeOverlappingAreaLabelsWithDependencyLabels(areaLabels);
			}

			if (this.currentDependencyOnTile.symbols != null && this.currentDependencyOnTile.symbols.Count != 0) {
				removeOverlappingSymbolsWithDepencySymbols(symbols, 2);
				removeOverlappingAreaLabelsWithDependencySymbols(areaLabels);
			}
		}

		/**
		 * When the LabelPlacement class generates potential label positions for an NODE, there should be no possible
		 * positions, that collide with existing symbols or labels in the dependency Cache. This class : this
		 * functionality.
		 * 
		 * @param refPos
		 *            possible label positions form the two or four point Greedy
		 */
		public void removeReferencepointsFromDependencyCache ( LabelPlacement.ReferencePosition[] refPos )
		{
			Tile lefttmp = new Tile(this.currentTile.TileX - 1, this.currentTile.TileY, this.currentTile.ZoomFactor);
			Tile righttmp = new Tile(this.currentTile.TileX + 1, this.currentTile.TileY, this.currentTile.ZoomFactor);
			Tile uptmp = new Tile(this.currentTile.TileX, this.currentTile.TileY - 1, this.currentTile.ZoomFactor);
			Tile downtmp = new Tile(this.currentTile.TileX, this.currentTile.TileY + 1, this.currentTile.ZoomFactor);

			bool up;
			bool left;
			bool right;
			bool down;

			this.tmp = this.dependencyTable[lefttmp];
			left = this.tmp == null ? false : this.tmp.drawn;

			this.tmp = this.dependencyTable[righttmp];
			right = this.tmp == null ? false : this.tmp.drawn;

			this.tmp = this.dependencyTable[uptmp];
			up = this.tmp == null ? false : this.tmp.drawn;

			this.tmp = this.dependencyTable[downtmp];
			down = this.tmp == null ? false : this.tmp.drawn;

			LabelPlacement.ReferencePosition reference;

			for (int i = 0; i < refPos.Length; i++) {
				reference = refPos[i];

				if (reference == null) {
					continue;
				}

				if (up && reference.y - reference.height < 0) {
					refPos[i] = null;
					continue;
				}

				if (down && reference.y >= Tile.TileSize) {
					refPos[i] = null;
					continue;
				}

				if (left && reference.x < 0) {
					refPos[i] = null;
					continue;
				}

				if (right && reference.x + reference.width > Tile.TileSize) {
					refPos[i] = null;
				}
			}

			// removes all Reverence points that intersects with Labels from the Dependency Cache

			int dis = 2;
			if (this.currentDependencyOnTile != null) {
				if (this.currentDependencyOnTile.labels != null) {
					for (int i = 0; i < this.currentDependencyOnTile.labels.Count; i++) {
						this.depLabel = this.currentDependencyOnTile.labels[i];
						this.rect1 = new Rect((int)this.depLabel.point.X - dis,
								(int)(this.depLabel.point.Y - this.depLabel.value.boundary.Height) - dis,
								(int)(this.depLabel.point.X + this.depLabel.value.boundary.Width + dis),
								(int)(this.depLabel.point.Y + dis));

						for (int y = 0; y < refPos.Length; y++) {
							if (refPos[y] != null) {
								this.rect2 = new Rect((int)refPos[y].x, (int)(refPos[y].y - refPos[y].height),
										(int)(refPos[y].x + refPos[y].width), (int)(refPos[y].y));

								if (Utils.Intersects(this.rect2, this.rect1)) {
									refPos[y] = null;
								}
							}
						}
					}
				}
				if (this.currentDependencyOnTile.symbols != null) {
					foreach (Dependency<DependencySymbol> symbols2 in this.currentDependencyOnTile.symbols) {
						this.rect1 = new Rect((int)symbols2.point.X, (int)(symbols2.point.Y),
								(int)(symbols2.point.X + symbols2.value.symbol.Width),
								(int)(symbols2.point.Y + symbols2.value.symbol.Height));

						for (int y = 0; y < refPos.Length; y++) {
							if (refPos[y] != null) {
								this.rect2 = new Rect((int)refPos[y].x, (int)(refPos[y].y - refPos[y].height),
										(int)(refPos[y].x + refPos[y].width), (int)(refPos[y].y));

								if (Utils.Intersects(this.rect2, this.rect1)) {
									refPos[y] = null;
								}
							}
						}
					}
				}
			}
		}

		public void removeSymbolsFromDrawnAreas ( List<SymbolContainer> symbols )
		{
			Tile lefttmp = new Tile(this.currentTile.TileX - 1, this.currentTile.TileY, this.currentTile.ZoomFactor);
			Tile righttmp = new Tile(this.currentTile.TileX + 1, this.currentTile.TileY, this.currentTile.ZoomFactor);
			Tile uptmp = new Tile(this.currentTile.TileX, this.currentTile.TileY - 1, this.currentTile.ZoomFactor);
			Tile downtmp = new Tile(this.currentTile.TileX, this.currentTile.TileY + 1, this.currentTile.ZoomFactor);

			bool up;
			bool left;
			bool right;
			bool down;

			this.tmp = this.dependencyTable[lefttmp];
			left = this.tmp == null ? false : this.tmp.drawn;

			this.tmp = this.dependencyTable[righttmp];
			right = this.tmp == null ? false : this.tmp.drawn;

			this.tmp = this.dependencyTable[uptmp];
			up = this.tmp == null ? false : this.tmp.drawn;

			this.tmp = this.dependencyTable[downtmp];
			down = this.tmp == null ? false : this.tmp.drawn;

			SymbolContainer reference;

			for (int i = 0; i < symbols.Count; i++) {
				reference = symbols[i];

				if (up && reference.point.Y < 0) {
					symbols.Remove(i);
					i--;
					continue;
				}

				if (down && reference.point.Y + reference.symbol.Height > Tile.TileSize) {
					symbols.Remove(i);
					i--;
					continue;
				}
				if (left && reference.point.X < 0) {
					symbols.Remove(i);
					i--;
					continue;
				}
				if (right && reference.point.X + reference.symbol.Width > Tile.TileSize) {
					symbols.Remove(i);
					i--;
					continue;
				}
			}
		}
	}
}