using System;
using System.IO;
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
namespace MapsUtilities.MapGenerator
{

/**
 * A MapGeneratorJob holds all immutable rendering parameters for a single map image together with a mutable priority
 * field, which indicates the importance of this job.
 */
public class MapGeneratorJob : IComparable<MapGeneratorJob> {
	private static long serialVersionUID = 1L;

	/**
	 * The debug settings for this job.
	 */
	public DebugSettings debugSettings;

	/**
	 * The rendering parameters for this job.
	 */
	public JobParameters jobParameters;

	/**
	 * The tile which should be generated.
	 */
	public Tile tile;

	private int hashCodeValue;
	private FileInfo mapFile;
	private double priority;

	/**
	 * Creates a new job for a MapGenerator with the given parameters.
	 * 
	 * @param tile
	 *            the tile which should be generated.
	 * @param mapFile
	 *            the map file for this job.
	 * @param jobParameters
	 *            the rendering parameters for this job.
	 * @param debugSettings
	 *            the debug settings for this job.
	 */
	public MapGeneratorJob(Tile tile, FileInfo mapFile, JobParameters jobParameters, DebugSettings debugSettings) {
		this.tile = tile;
		this.mapFile = mapFile;
		this.jobParameters = jobParameters;
		this.debugSettings = debugSettings;
		calculateTransientValues();
	}

	public int CompareTo(MapGeneratorJob otherMapGeneratorJob) {
		if (this.priority < otherMapGeneratorJob.priority) {
			return -1;
		} else if (this.priority > otherMapGeneratorJob.priority) {
			return 1;
		}
		return 0;
	}

	public bool Equals(object obj) {
		if (this == obj) {
			return true;
		}
		if (!(obj is MapGeneratorJob)) {
			return false;
		}
		MapGeneratorJob other = (MapGeneratorJob) obj;
		if (this.debugSettings == null) {
			if (other.debugSettings != null) {
				return false;
			}
		} else if (!this.debugSettings.Equals(other.debugSettings)) {
			return false;
		}
		if (this.jobParameters == null) {
			if (other.jobParameters != null) {
				return false;
			}
		} else if (!this.jobParameters.Equals(other.jobParameters)) {
			return false;
		}
		if (this.mapFile == null) {
			if (other.mapFile != null) {
				return false;
			}
		} else if (!this.mapFile.Equals(other.mapFile)) {
			return false;
		}
		if (this.tile == null) {
			if (other.tile != null) {
				return false;
			}
		} else if (!this.tile.Equals(other.tile)) {
			return false;
		}
		return true;
	}

	public override int GetHashCode() {
		return this.hashCodeValue;
	}

	/**
	 * @return the hash code of this object.
	 */
	private int calculateHashCode() {
		int result = 1;
		result = 31 * result + ((this.debugSettings == null) ? 0 : this.debugSettings.GetHashCode());
		result = 31 * result + ((this.jobParameters == null) ? 0 : this.jobParameters.GetHashCode());
		result = 31 * result + ((this.mapFile == null) ? 0 : this.mapFile.GetHashCode());
		result = 31 * result + ((this.tile == null) ? 0 : this.tile.GetHashCode());
		return result;
	}

	/**
	 * Calculates the values of some transient variables.
	 */
	private void calculateTransientValues() {
		this.hashCodeValue = calculateHashCode();
	}

	void setPriority(double priority) {
		this.priority = priority;
	}
}
}
