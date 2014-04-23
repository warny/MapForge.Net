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
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using MapCore;
using MapCore.Model;
namespace MapsUtilities.MapGenerator.DatabaseRenderer
{

	/**
	 * This class place the labels form nodes, area labels and normal labels. The main target is avoiding collisions of these
	 * different labels.
	 */
	public class LabelPlacement
	{
		/**
		 * This class holds the reference positions for the two and four point greedy algorithms.
		 */
		public class ReferencePosition
		{
			public double height;
			public int nodeNumber;
			public SymbolContainer symbol;
			public double width;
			public double x;
			public double y;

			public ReferencePosition ( double x, double y, int nodeNumber, double width, double height, SymbolContainer symbol )
			{
				this.x = x;
				this.y = y;
				this.nodeNumber = nodeNumber;
				this.width = width;
				this.height = height;
				this.symbol = symbol;
			}
		}

		public class ReferencePositionHeightComparator : IComparer<ReferencePosition>
		{
			private const long serialVersionUID = 1L;
			public static readonly ReferencePositionHeightComparator INSTANCE = new ReferencePositionHeightComparator();

			private ReferencePositionHeightComparator ()
			{
				// do nothing
			}

			//@Override
			public int Compare ( ReferencePosition x, ReferencePosition y )
			{
				if (x.y - x.height < y.y - y.height) {
					return -1;
				}

				if (x.y - x.height > y.y - y.height) {
					return 1;
				}
				return 0;
			}
		}

		public class ReferencePositionWidthComparator : IComparer<ReferencePosition>
		{
			private const long serialVersionUID = 1L;
			public static readonly ReferencePositionWidthComparator INSTANCE = new ReferencePositionWidthComparator();

			private ReferencePositionWidthComparator ()
			{
				// do nothing
			}

			public int Compare ( ReferencePosition x, ReferencePosition y )
			{
				if (x.x + x.width < y.x + y.width) {
					return -1;
				}

				if (x.x + x.width > y.x + y.width) {
					return 1;
				}

				return 0;
			}
		}

		public class ReferencePositionXComparator : IComparer<ReferencePosition>
		{
			private const long serialVersionUID = 1L;
			public static readonly ReferencePositionXComparator INSTANCE = new ReferencePositionXComparator();

			private ReferencePositionXComparator ()
			{
				// do nothing
			}

			public int Compare ( ReferencePosition x, ReferencePosition y )
			{
				if (x.x < y.x) {
					return -1;
				}

				if (x.x > y.x) {
					return 1;
				}

				return 0;
			}

		}

		public class ReferencePositionYComparator : IComparer<ReferencePosition>
		{
			private const long serialVersionUID = 1L;
			public static readonly ReferencePositionYComparator INSTANCE = new ReferencePositionYComparator();

			private ReferencePositionYComparator ()
			{
				// do nothing
			}

			public int Compare ( ReferencePosition x, ReferencePosition y )
			{
				if (x.y < y.y) {
					return -1;
				}

				if (x.y > y.y) {
					return 1;
				}

				return 0;
			}
		}

		private const int LABEL_DISTANCE_TO_LABEL = 2;
		private const int LABEL_DISTANCE_TO_SYMBOL = 2;
		private const int PLACEMENT_MODEL = 1;
		private const int START_DISTANCE_TO_SYMBOLS = 4;
		private const int SYMBOL_DISTANCE_TO_SYMBOL = 2;

		public DependencyCache dependencyCache;
		public PointTextContainer label;
		public Rect rect1;
		public Rect rect2;
		public ReferencePosition referencePosition;
		public SymbolContainer symbolContainer;

		public LabelPlacement ()
		{
			this.dependencyCache = new DependencyCache();
		}

		/**
		 * Centers the labels.
		 * 
		 * @param labels
		 *            labels to center
		 */
		private void centerLabels ( List<pointTextContainer> labels )
		{
			for (int i = 0; i < labels.Count; i++) {
				this.label = labels[i];
				this.label.x = this.label.x - this.label.boundary.Width / 2;
			}
		}

		private void preprocessAreaLabels ( List<pointTextContainer> areaLabels )
		{
			centerLabels(areaLabels);

			removeOutOfTileAreaLabels(areaLabels);

			removeOverlappingAreaLabels(areaLabels);

			if (areaLabels.Any()) {
				this.dependencyCache.removeAreaLabelsInAlreadyDrawnAreas(areaLabels);
			}
		}

		private void preprocessLabels ( List<pointTextContainer> labels )
		{
			removeOutOfTileLabels(labels);
		}

		private void preprocessSymbols ( List<SymbolContainer> symbols )
		{
			removeOutOfTileSymbols(symbols);
			removeOverlappingSymbols(symbols);
			this.dependencyCache.removeSymbolsFromDrawnAreas(symbols);
		}

		/**
		 * This method uses an adapted greedy strategy for the fixed four position model, above, under left and right form
		 * the point of interest. It uses no priority search tree, because it will not function with symbols only with
		 * points. Instead it uses two minimum heaps. They work similar to a sweep line algorithm but have not a O(n log n
		 * +k) runtime. To find the rectangle that has the top edge, I use also a minimum Heap. The rectangles are sorted by
		 * their y coordinates.
		 * 
		 * @param labels
		 *            label positions and text
		 * @param symbols
		 *            symbol positions
		 * @param areaLabels
		 *            area label positions and text
		 * @return list of labels without overlaps with symbols and other labels by the four fixed position greedy strategy
		 */
		private List<pointTextContainer> processFourpointGreedy ( List<pointTextContainer> labels,
				List<SymbolContainer> symbols, List<pointTextContainer> areaLabels )
		{
			List<pointTextContainer> resolutionSet = new List<pointTextContainer>();

			// Array for the generated reference positions around the points of interests
			ReferencePosition[] refPos = new ReferencePosition[(labels.Count) * 4];

			// lists that sorts the reference points after the minimum top edge y position
			PriorityQueue<ReferencePosition> priorUp = new PriorityQueue<ReferencePosition>(labels.Count * 4 * 2
					+ labels.Count / 10 * 2, ReferencePositionYComparator.INSTANCE);
			// lists that sorts the reference points after the minimum bottom edge y position
			PriorityQueue<ReferencePosition> priorDown = new PriorityQueue<ReferencePosition>(labels.Count * 4 * 2
					+ labels.Count / 10 * 2, ReferencePositionHeightComparator.INSTANCE);

			pointTextContainer tmp;
			int dis = START_DISTANCE_TO_SYMBOLS;

			// creates the reference positions
			for (int z = 0; z < labels.Count; z++) {
				if (labels[z] != null) {
					if (labels[z].symbol != null) {
						tmp = labels[z];

						// up
						refPos[z * 4] = new ReferencePosition(tmp.x - tmp.boundary.Width / 2, tmp.y
								- tmp.symbol.symbol.Height / 2 - dis, z, tmp.boundary.Width, tmp.boundary.Height,
								tmp.symbol);
						// down
						refPos[z * 4 + 1] = new ReferencePosition(tmp.x - tmp.boundary.Width / 2, tmp.y
								+ tmp.symbol.symbol.Height / 2 + tmp.boundary.Height + dis, z, tmp.boundary.Width,
								tmp.boundary.Height, tmp.symbol);
						// left
						refPos[z * 4 + 2] = new ReferencePosition(tmp.x - tmp.symbol.symbol.Width / 2
								- tmp.boundary.Width - dis, tmp.y + tmp.boundary.Height / 2, z, tmp.boundary.Width,
								tmp.boundary.Height, tmp.symbol);
						// right
						refPos[z * 4 + 3] = new ReferencePosition(tmp.x + tmp.symbol.symbol.Width / 2 + dis, tmp.y
								+ tmp.boundary.Height / 2 - 0.1f, z, tmp.boundary.Width, tmp.boundary.Height,
								tmp.symbol);
					} else {
						refPos[z * 4] = new ReferencePosition(labels[z].x - ((labels[z].boundary.Width) / 2),
								labels[z].y, z, labels[z].boundary.Width, labels[z].boundary.Height, null);
						refPos[z * 4 + 1] = null;
						refPos[z * 4 + 2] = null;
						refPos[z * 4 + 3] = null;
					}
				}
			}

			removeNonValidateReferencePosition(refPos, symbols, areaLabels);

			// do while it gives reference positions
			for (int i = 0; i < refPos.Length; i++) {
				this.referencePosition = refPos[i];
				if (this.referencePosition != null) {
					priorUp.Add(this.referencePosition);
					priorDown.Add(this.referencePosition);
				}
			}

			while (priorUp.Count != 0) {
				this.referencePosition = priorUp.Pool();

				this.label = labels[this.referencePosition.nodeNumber];

				resolutionSet.Add(new PointTextContainer(this.label.text, this.referencePosition.x,
						this.referencePosition.y, this.label.paintFront, this.label.paintBack, this.label.symbol));

				if (priorUp.Count == 0) {
					return resolutionSet;
				}

				priorUp.Remove(refPos[this.referencePosition.nodeNumber * 4 + 0]);
				priorUp.Remove(refPos[this.referencePosition.nodeNumber * 4 + 1]);
				priorUp.Remove(refPos[this.referencePosition.nodeNumber * 4 + 2]);
				priorUp.Remove(refPos[this.referencePosition.nodeNumber * 4 + 3]);

				priorDown.Remove(refPos[this.referencePosition.nodeNumber * 4 + 0]);
				priorDown.Remove(refPos[this.referencePosition.nodeNumber * 4 + 1]);
				priorDown.Remove(refPos[this.referencePosition.nodeNumber * 4 + 2]);
				priorDown.Remove(refPos[this.referencePosition.nodeNumber * 4 + 3]);

				LinkedList<ReferencePosition> linkedRef = new LinkedList<ReferencePosition>();

				while (priorDown.Count != 0) {
					if (priorDown.Peek().x < this.referencePosition.x + this.referencePosition.width) {
						linkedRef.AddLast(priorDown.Pool());
					} else {
						break;
					}
				}
				// brute Force collision test (faster then sweep line for a small amount of
				// objects)
				linkedRef.RemoveAll(e=>(e.x <= this.referencePosition.x + this.referencePosition.width)
							&& (e.y >= this.referencePosition.y - e.height)
							&& (e.y <= this.referencePosition.y + e.height));

				priorDown.AddRange(linkedRef);
			}

			return resolutionSet;
		}

		/**
		 * This method uses an adapted greedy strategy for the fixed two position model, above and under. It uses no
		 * priority search tree, because it will not function with symbols only with points. Instead it uses two minimum
		 * heaps. They work similar to a sweep line algorithm but have not a O(n log n +k) runtime. To find the rectangle
		 * that has the leftest edge, I use also a minimum Heap. The rectangles are sorted by their x coordinates.
		 * 
		 * @param labels
		 *            label positions and text
		 * @param symbols
		 *            symbol positions
		 * @param areaLabels
		 *            area label positions and text
		 * @return list of labels without overlaps with symbols and other labels by the two fixed position greedy strategy
		 */
		private List<pointTextContainer> processTwopointGreedy ( List<pointTextContainer> labels,
				List<SymbolContainer> symbols, List<pointTextContainer> areaLabels )
		{
			List<pointTextContainer> resolutionSet = new List<pointTextContainer>();
			// Array for the generated reference positions around the points of interests
			ReferencePosition[] refPos = new ReferencePosition[labels.Count * 2];

			// lists that sorts the reference points after the minimum right edge x position
			PriorityQueue<ReferencePosition> priorRight = new PriorityQueue<ReferencePosition>(labels.Count * 2
					+ labels.Count / 10 * 2, ReferencePositionWidthComparator.INSTANCE);
			// lists that sorts the reference points after the minimum left edge x position
			PriorityQueue<ReferencePosition> priorLeft = new PriorityQueue<ReferencePosition>(labels.Count * 2
					+ labels.Count / 10 * 2, ReferencePositionXComparator.INSTANCE);

			// creates the reference positions
			for (int z = 0; z < labels.Count; z++) {
				this.label = labels[z];

				if (this.label.symbol != null) {
					refPos[z * 2] = new ReferencePosition(this.label.x - (this.label.boundary.Width / 2) - 0.1f,
							this.label.y - this.label.boundary.Height - LabelPlacement.START_DISTANCE_TO_SYMBOLS, z,
							this.label.boundary.Width, this.label.boundary.Height, this.label.symbol);
					refPos[z * 2 + 1] = new ReferencePosition(this.label.x - (this.label.boundary.Width / 2),
							this.label.y + this.label.symbol.symbol.Height + LabelPlacement.START_DISTANCE_TO_SYMBOLS,
							z, this.label.boundary.Width, this.label.boundary.Height, this.label.symbol);
				} else {
					refPos[z * 2] = new ReferencePosition(this.label.x - (this.label.boundary.Width / 2) - 0.1f,
							this.label.y, z, this.label.boundary.Width, this.label.boundary.Height, null);
					refPos[z * 2 + 1] = null;
				}
			}

			// removes reference positions that overlaps with other symbols or dependency objects
			removeNonValidateReferencePosition(refPos, symbols, areaLabels);

			for (int i = 0; i < refPos.Length; i++) {
				this.referencePosition = refPos[i];
				if (this.referencePosition != null) {
					priorLeft.Add(this.referencePosition);
					priorRight.Add(this.referencePosition);
				}
			}

			while (priorRight.Count != 0) {
				this.referencePosition = priorRight.Pool();

				this.label = labels[this.referencePosition.nodeNumber];

				resolutionSet.Add(new PointTextContainer(this.label.text, this.referencePosition.x,
						this.referencePosition.y, this.label.paintFront, this.label.paintBack,
						this.referencePosition.symbol));

				// Removes the other position that is a possible position for the label of one point
				// of interest

				priorRight.Remove(refPos[this.referencePosition.nodeNumber * 2 + 1]);

				if (priorRight.Count == 0) {
					return resolutionSet;
				}

				priorLeft.Remove(this.referencePosition);
				priorLeft.Remove(refPos[this.referencePosition.nodeNumber * 2 + 1]);

				// find overlapping labels and deletes the reference points and delete them
				LinkedList<ReferencePosition> linkedRef = new LinkedList<ReferencePosition>();

				while (priorLeft.Count != 0) {
					if (priorLeft.Peek().x < this.referencePosition.x + this.referencePosition.width) {
						linkedRef.AddLast(priorLeft.Pool());
					} else {
						break;
					}
				}

				// brute Force collision test (faster then sweep line for a small amount of
				// objects)
				linkedRef.RemoveAll(e=>(e.x <= this.referencePosition.x + this.referencePosition.width)
							&& (e.y >= this.referencePosition.y - e.height)
							&& (e.y <= this.referencePosition.y + e.height));
				priorLeft.AddRange(linkedRef);
			}

			return resolutionSet;
		}

		private void removeEmptySymbolReferences ( List<pointTextContainer> nodes, List<SymbolContainer> symbols )
		{
			for (int i = 0; i < nodes.Count; i++) {
				this.label = nodes[i];
				if (!symbols.Contains(this.label.symbol)) {
					this.label.symbol = null;
				}
			}
		}

		/**
		 * The greedy algorithms need possible label positions, to choose the best among them. This method removes the
		 * reference points, that are not validate. Not validate means, that the Reference overlap with another symbol or
		 * label or is outside of the tile.
		 * 
		 * @param refPos
		 *            list of the potential positions
		 * @param symbols
		 *            actual list of the symbols
		 * @param areaLabels
		 *            actual list of the area labels
		 */
		private void removeNonValidateReferencePosition ( ReferencePosition[] refPos, List<SymbolContainer> symbols,
				List<pointTextContainer> areaLabels ) {
		int distance = LABEL_DISTANCE_TO_SYMBOL;

		for (int i = 0; i < symbols.Count; i++) {
			this.symbolContainer = symbols[i];
			this.rect1 = new Rect((int) this.symbolContainer.point.X - distance, (int) this.symbolContainer.point.Y
					- distance, (int) this.symbolContainer.point.X + this.symbolContainer.symbol.Width + distance,
					(int) this.symbolContainer.point.Y + this.symbolContainer.symbol.Height + distance);

			for (int y = 0; y < refPos.Length; y++) {
				if (refPos[y] != null) {
					this.rect2 = new Rect((int) refPos[y].x, (int) (refPos[y].y - refPos[y].height),
							(int) (refPos[y].x + refPos[y].width), (int) (refPos[y].y));

					if (Utils.Intersects(this.rect2, this.rect1)) {
						refPos[y] = null;
					}
				}
			}
		}

		distance = LABEL_DISTANCE_TO_LABEL;

		foreach (pointTextContainer areaLabel in areaLabels) {
			this.rect1 = new Rect((int) areaLabel.x - distance, (int) areaLabel.y - areaLabel.boundary.Height
					- distance, (int) areaLabel.x + areaLabel.boundary.Width + distance, (int) areaLabel.y + distance);

			for (int y = 0; y < refPos.Length; y++) {
				if (refPos[y] != null) {
					this.rect2 = new Rect((int) refPos[y].x, (int) (refPos[y].y - refPos[y].height),
							(int) (refPos[y].x + refPos[y].width), (int) (refPos[y].y));

					if (Utils.Intersects(this.rect2, this.rect1)) {
						refPos[y] = null;
					}
				}
			}
		}

		this.dependencyCache.removeReferencepointsFromDependencyCache(refPos);
	}

		/**
		 * This method removes the area labels, that are not visible in the actual tile.
		 * 
		 * @param areaLabels
		 *            area Labels from the actual tile
		 */
		private void removeOutOfTileAreaLabels ( List<pointTextContainer> areaLabels )
		{
			for (int i = 0; i < areaLabels.Count; i++) {
				this.label = areaLabels[i];

				if (this.label.x > Tile.TileSize) {
					areaLabels.Remove(this.label);

					i--;
				} else if (this.label.y - this.label.boundary.Height > Tile.TileSize) {
					areaLabels.Remove(this.label);

					i--;
				} else if (this.label.x + this.label.boundary.Width < 0.0f) {
					areaLabels.Remove(this.label);

					i--;
				} else if (this.label.y + this.label.boundary.Height < 0.0f) {
					areaLabels.Remove(this.label);

					i--;
				}
			}
		}

		/**
		 * This method removes the labels, that are not visible in the actual tile.
		 * 
		 * @param labels
		 *            Labels from the actual tile
		 */
		private void removeOutOfTileLabels ( List<pointTextContainer> labels )
		{
			for (int i = 0; i < labels.Count; ) {
				this.label = labels[i];

				if (this.label.x - this.label.boundary.Width / 2 > Tile.TileSize) {
					labels.Remove(this.label);
					this.label = null;
				} else if (this.label.y - this.label.boundary.Height > Tile.TileSize) {
					labels.Remove(this.label);
					this.label = null;
				} else if ((this.label.x - this.label.boundary.Width / 2 + this.label.boundary.Width) < 0.0f) {
					labels.Remove(this.label);
					this.label = null;
				} else if (this.label.y < 0.0f) {
					labels.Remove(this.label);
					this.label = null;
				} else {
					i++;
				}
			}
		}

		/**
		 * This method removes the Symbols, that are not visible in the actual tile.
		 * 
		 * @param symbols
		 *            Symbols from the actual tile
		 */
		private void removeOutOfTileSymbols ( List<SymbolContainer> symbols )
		{
			for (int i = 0; i < symbols.Count; ) {
				this.symbolContainer = symbols[i];

				if (this.symbolContainer.point.X > Tile.TileSize) {
					symbols.Remove(this.symbolContainer);
				} else if (this.symbolContainer.point.Y > Tile.TileSize) {
					symbols.Remove(this.symbolContainer);
				} else if (this.symbolContainer.point.X + this.symbolContainer.symbol.Width < 0.0f) {
					symbols.Remove(this.symbolContainer);
				} else if (this.symbolContainer.point.Y + this.symbolContainer.symbol.Height < 0.0f) {
					symbols.Remove(this.symbolContainer);
				} else {
					i++;
				}
			}
		}

		/**
		 * This method removes all the area labels, that overlap each other. So that the output is collision free
		 * 
		 * @param areaLabels
		 *            area labels from the actual tile
		 */
		private void removeOverlappingAreaLabels ( List<pointTextContainer> areaLabels )
		{
			int dis = LABEL_DISTANCE_TO_LABEL;

			for (int x = 0; x < areaLabels.Count; x++) {
				this.label = areaLabels[x];
				this.rect1 = new Rect((int)this.label.x - dis, (int)this.label.y - dis,
						(int)(this.label.x + this.label.boundary.Width) + dis, (int)(this.label.y
								+ this.label.boundary.Height + dis));

				for (int y = x + 1; y < areaLabels.Count; y++) {
					if (y != x) {
						this.label = areaLabels[y];
						this.rect2 = new Rect((int)this.label.x, (int)this.label.y,
								(int)(this.label.x + this.label.boundary.Width),
								(int)(this.label.y + this.label.boundary.Height));

						if (Utils.Intersects(this.rect1, this.rect2)) {
							areaLabels.Remove(this.label);

							y--;
						}
					}
				}
			}
		}

		/**
		 * Removes the the symbols that overlap with area labels.
		 * 
		 * @param symbols
		 *            list of symbols
		 * @param pTC
		 *            list of labels
		 */
		private void removeOverlappingSymbolsWithAreaLabels ( List<SymbolContainer> symbols, List<pointTextContainer> pTC )
		{
			int dis = LABEL_DISTANCE_TO_SYMBOL;

			for (int x = 0; x < pTC.Count; x++) {
				this.label = pTC[x];

				this.rect1 = new Rect((int)this.label.x - dis, (int)(this.label.y - this.label.boundary.Height) - dis,
						(int)(this.label.x + this.label.boundary.Width + dis), (int)(this.label.y + dis));

				for (int y = 0; y < symbols.Count; y++) {
					this.symbolContainer = symbols[y];

					this.rect2 = new Rect((int)this.symbolContainer.point.X, (int)this.symbolContainer.point.Y,
							(int)(this.symbolContainer.point.X + this.symbolContainer.symbol.Width),
							(int)(this.symbolContainer.point.Y + this.symbolContainer.symbol.Height));

					if (Utils.Intersects(this.rect1, this.rect2)) {
						symbols.Remove(this.symbolContainer);
						y--;
					}
				}
			}
		}

		/**
		 * The inputs are all the label and symbol objects of the current tile. The output is overlap free label and symbol
		 * placement with the greedy strategy. The placement model is either the two fixed point or the four fixed point
		 * model.
		 * 
		 * @param labels
		 *            labels from the current tile.
		 * @param symbols
		 *            symbols of the current tile.
		 * @param areaLabels
		 *            area labels from the current tile.
		 * @param cT
		 *            current tile with the x,y- coordinates and the zoom level.
		 * @return the processed list of labels.
		 */
		List<pointTextContainer> placeLabels ( List<pointTextContainer> labels, List<SymbolContainer> symbols,
				List<pointTextContainer> areaLabels, Tile cT )
		{
			List<pointTextContainer> returnLabels = labels;
			this.dependencyCache.generateTileAndDependencyOnTile(cT);

			preprocessAreaLabels(areaLabels);

			preprocessLabels(returnLabels);

			preprocessSymbols(symbols);

			removeEmptySymbolReferences(returnLabels, symbols);

			removeOverlappingSymbolsWithAreaLabels(symbols, areaLabels);

			this.dependencyCache.removeOverlappingObjectsWithDependencyOnTile(returnLabels, areaLabels, symbols);

			if (returnLabels.Any()) {
				switch (PLACEMENT_MODEL) {
					case 0:
						returnLabels = processTwopointGreedy(returnLabels, symbols, areaLabels);
						break;
					case 1:
						returnLabels = processFourpointGreedy(returnLabels, symbols, areaLabels);
						break;
					default:
						break;
				}
			}

			this.dependencyCache.fillDependencyOnTile(returnLabels, symbols, areaLabels);

			return returnLabels;
		}

		/**
		 * This method removes all the Symbols, that overlap each other. So that the output is collision free.
		 * 
		 * @param symbols
		 *            symbols from the actual tile
		 */
		void removeOverlappingSymbols ( List<SymbolContainer> symbols )
		{
			int dis = SYMBOL_DISTANCE_TO_SYMBOL;

			for (int x = 0; x < symbols.Count; x++) {
				this.symbolContainer = symbols[x];
				this.rect1 = new Rect((int)this.symbolContainer.point.X - dis, (int)this.symbolContainer.point.Y - dis,
						(int)this.symbolContainer.point.X + this.symbolContainer.symbol.Width + dis,
						(int)this.symbolContainer.point.Y + this.symbolContainer.symbol.Height + dis);

				for (int y = x + 1; y < symbols.Count; y++) {
					if (y != x) {
						this.symbolContainer = symbols[y];
						this.rect2 = new Rect((int)this.symbolContainer.point.X, (int)this.symbolContainer.point.Y,
								(int)this.symbolContainer.point.X + this.symbolContainer.symbol.Width,
								(int)this.symbolContainer.point.Y + this.symbolContainer.symbol.Height);

						if (Utils.Intersects(this.rect2, this.rect1)) {
							symbols.Remove(this.symbolContainer);
							y--;
						}
					}
				}
			}
		}
	}
}