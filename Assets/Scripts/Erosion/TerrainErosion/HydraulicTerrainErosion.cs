using UnityEngine;
using System.Collections;

public class HydraulicTerrainErosion {
	// How much rain to add each turn.
	public float rainfallAmount = 0.01f; // kr

	// Percentage of rain that evaporates every turn.
	public float evaporationRatio = 0.5f; // ke

	// Amount of soil that can be carried with water.
	public float sedimentCapacity = 0.01f; // kc

	// How easily soil get dissolved in water.
	public float soilSolubility = 0.01f; // ks

	// Height above which rain starts to taper off.
	public float rainAltitude = 0.4f;

	// How fast rain tapers off above the cutoff.
	public float rainFalloff = 1.0f;

	public int iterations = 1;

	public void Erode(float[,] heightmap) {
		var width = heightmap.GetLength(0);
		var height = heightmap.GetLength(1);

		float[,] currentHeight = (float[,])heightmap.Clone();
		float[,] nextHeight = new float[height, width];
		float[,] currentWater = new float[height, width];
		float[,] nextWater = new float[height, width];
		float[,] currentSediment = new float[height, width];
		float[,] nextSediment = new float[height, width];

		float [,] temp;

		// wew lads, here we go
		for (int i = 0; i < iterations; i++) {
			// Do one initial pass to add rain and blow away the old
			// garbage values in the next* maps.
			for (int z = 0; z < height; z++) {
				for (int x = 0; x < width; x++) {
					float rainRatio;
					float h = currentHeight[z, x];

					if (h < rainAltitude) {
						rainRatio = 1.0f;
					} else {
						rainRatio = Mathf.Lerp(1.0f, 0.0f,
								((h - rainAltitude) / (1.0f - rainAltitude)));
					}

					// add rain
					currentWater[z, x] += rainfallAmount * Mathf.Pow(rainRatio, rainFalloff);
					nextWater[z, x] = currentWater[z, x];

					// dissolve sediment
					float soilDissolved = (soilSolubility * currentWater[z, x]);
					currentSediment[z, x] += soilDissolved;
					currentHeight[z, x] -= soilDissolved;

					nextHeight[z, x] = currentHeight[z, x];
					nextSediment[z, x] = currentSediment[z, x];
				}
			}

			// The second pass is the crazy one.  We need to spill the water
			// and sediment between neighbors.
			for (int z = 0; z < height; z++) {
				for (int x = 0; x < width; x++) {
					// Level out water with neighboring squares.
					//
					// The goal is to make (height + water) equal for neighboring cells.
					// Note that we're using nextHeight here because we've already dissolved
					// some of the rock at this location into sediment.
					//
					// We need to do a bit of precalculation across the neighbors first.
					float altitude = currentWater[z, x] + nextHeight[z, x];
					float totalAltitude = altitude;
					float neighborhoodCount = 1;
					float totalDifference = 0.0f;

					for (int dx = -1; dx <= 1; dx += 1) {
						for (int dz = -1; dz <= 1; dz += 1) {
							int nx = x + dx;
							int nz = z + dz;

							if (nx < 0 || nx >= width || nz < 0 || nz >= height || (nx == 0 && nz == 0)) {
								// Skip neighbor cells that fall off the map.
								continue;
							}

							float neighborAltitude = currentWater[nz, nx] + currentHeight[nz, nx];
							float difference = altitude - neighborAltitude;

							// We only move water to neighbors that are LOWER.
							if (difference <= 0.0) continue;

							totalAltitude += neighborAltitude;
							neighborhoodCount += 1;
							totalDifference += difference;
						}
					}

					float averageAltitude = totalAltitude / neighborhoodCount;
					float altitudeToMove = altitude - averageAltitude;

					// If we try to move more altitude than we have water, we just
					// move as much water as we have.
					float waterToMove = (altitudeToMove > currentWater[z, x]
						? currentWater[z, x]
						: altitudeToMove);

					// Now that we have the bookkeeping out of the way, we can actually move the water.
					for (int dx = -1; dx <= 1; dx += 1) {
						for (int dz = -1; dz <= 1; dz += 1) {
							int nx = x + dx;
							int nz = z + dz;

							if (nx < 0 || nx >= width || nz < 0 || nz >= height || (nx == 0 && nz == 0)) {
								// Skip neighbor cells that fall off the map.
								continue;
							}

							// Recalculate :(
							float neighborAltitude = currentWater[nz, nx] + currentHeight[nz, nx];
							float difference = altitude - neighborAltitude;

							// We only move water to neighbors that are LOWER.
							if (difference <= 0.0) continue;

							// Water gets distributed according to to the difference in altitudes.
							// Lower neighbors will get a bigger share of the runoff.
							float proportion = difference / totalDifference;
							float waterForNeighbor = waterToMove * proportion;

							nextWater[z, x] -= waterForNeighbor;
							nextWater[nz, nx] += waterForNeighbor;

							// Dissolved sediment flows with water.
							//
							// We assume sediment is uniformly distributed in the water, so we
							// move an amount of sediment proportional to how much water we
							// just moved.
							float sedimentForNeighbor = currentSediment[z, x] * (waterForNeighbor / waterToMove);

							nextSediment[z, x] -= sedimentForNeighbor;
							nextSediment[nz, nx] += sedimentForNeighbor;
						} // oh
					} // my
				} // good
			} // god
				
			// Now we've done everything that involves interaction between cells,
			// so it's safe to swap the maps.
			temp = currentWater;
			currentWater = nextWater;
			nextWater = temp;

			temp = currentSediment; // my
			currentSediment = nextSediment; // kingdom
			nextSediment = temp;

			temp = currentHeight; // for
			currentHeight = nextHeight; // (rotatef ...)
			nextHeight = temp;

			// One final pass to evaporate and deposit sediment.
			for (int z = 0; z < height; z++) {
				for (int x = 0; x < width; x++) {
					float waterToEvaporate = currentWater[z, x] * evaporationRatio;
					currentWater[z, x] -= waterToEvaporate;

					float maxSediment = sedimentCapacity * currentWater[z, x];
					float excessSediment = currentSediment[z, x] - maxSediment;
					if (excessSediment > 0) {
						currentSediment[z, x] -= excessSediment;
						currentHeight[z, x] += excessSediment;
					}
				}
			}
		}

		System.Array.Copy(currentHeight, heightmap, width * height);
	}
}
