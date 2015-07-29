/*
---------------------- Terrain Water Erosion ----------------------
-- TerrainWaterErosion.cs
--
-- Code and algorithm by Dmitry Soldatenkov
-- Based on Terrain Toolkit by Sándor Moldán. 
--
-------------------------------------------------------------------
*/
using UnityEngine;
using System;
using System.IO;

[ExecuteInEditMode()]
[AddComponentMenu("Terrain/Terrain Water Erosion")]

public class TerrainWaterErosion : MonoBehaviour {

    public delegate void ErosionProgressDelegate (string titleString,string displayString,int iteration,int nIterations,float percentComplete);

    public int waterErosionIterations = 15;
    public float waterErosionRainfall = 1.0f;
    public float waterErosionConeThreshold = 2.0f;
    float[,] heightMap;
    float[,] waterMap;
    float[,] sedimentMap;
    int Tx, Ty;

    public void processTerrain (ErosionProgressDelegate erosionProgressDelegate) {
        Terrain ter = (Terrain)GetComponent (typeof(Terrain));
        if (ter == null) {
            return;
        }
        try {
            // Pass the height array to the erosion script
            TerrainData terData = ter.terrainData;
            Tx = terData.heightmapWidth;
            Ty = terData.heightmapHeight;
            heightMap = terData.GetHeights (0, 0, Tx, Ty);
            waterMap = new float[Tx, Ty];
            sedimentMap = new float[Tx, Ty];
            waterErosion (waterErosionIterations, erosionProgressDelegate);
            // Apply it to the terrain object
            terData.SetHeights (0, 0, heightMap);
        } catch (Exception e) {
            Debug.LogError ("An error occurred: " + e);
        }
    }


    struct neighbourVertex {
        public float decline;
        public int x;
        public int y;
    }
    neighbourVertex[] neighbourP = new neighbourVertex[8];
    int nNeighbourP = 0;
    bool waterExists = true;

    // Clamp - if you need a tiled terrain just change this two metods
    bool clampXInd (int x, out int newX) {
        if (x < 0) {
			newX = 0;
			return false;
		}
		else if (x >= Tx) {
			newX = Tx - 1;
			return false;
		}
		else
			newX = x;
		return true;
	}

	bool clampYInd (int x, out int newX) {
		if (x < 0) {
			newX = 0;
			return false;
		}
		else if (x >= Ty) {
			newX = Ty - 1;
			return false;
		}
		else
			newX = x;
		return true;
	}

    void moveWater (int x, int y) {
        if (waterMap [x, y] <= 0.0f)
            return;
        waterExists = true;
        nNeighbourP = 0;

		int checkX, checkY;
        for (int i = -1; i <= 1; i++) {
            checkY = y + i;
            if (checkY < 0 || checkY >= Ty) continue;
            for (int j = -1; j <= 1; j++) {
                checkX = x + j;
                if (checkX < 0 || checkX >= Tx) continue;
                float decline = heightMap [x, y] - heightMap [checkX, checkY];
                float lenCoeff = 0.714f; // for dialonals
                if (i == 0 || j == 0)
                    lenCoeff = 1.0f; //for straights
                if (decline > 0) {
                    neighbourP [nNeighbourP].decline = decline * lenCoeff;
                    neighbourP [nNeighbourP].x = checkX;
                    neighbourP [nNeighbourP].y = checkY;
                    nNeighbourP++;
                }
            }
        }

        if (nNeighbourP > 0) { // water flows
            float summDecline = 0.0f;
            for (int i = 0; i < nNeighbourP; i++) {
                summDecline += neighbourP [i].decline;
            }
            if (summDecline <= 0.00001f)
                summDecline = 0.00001f;
            float waterNorm = 1.0f / summDecline;
            float waterHere = waterMap [x, y];
            for (int i = 0; i < nNeighbourP; i++) {
                float waterFlow = waterHere * neighbourP [i].decline * waterNorm;
                float sandAmount = 0.05f * waterFlow * neighbourP [i].decline;
                waterMap [neighbourP [i].x, neighbourP [i].y] += waterFlow;
                sedimentMap [x, y] -= sandAmount;
                sedimentMap [neighbourP [i].x, neighbourP [i].y] += sandAmount;
            }
        } // water stays
        waterMap [x, y] = 0.0f;
    }

    void addSediment (int x, int y) {
        float val = sedimentMap [x, y];
        float sign = 1.0f;
        if (val < 0)
            sign = -1.0f;
        float absVal = val * sign;
        float threshold = 2e-4f * waterErosionConeThreshold;

        if (absVal < threshold) {
            heightMap [x, y] += sedimentMap [x, y];
        } else {
            float radius = absVal / threshold;
            radius = Mathf.Sqrt (radius);
            int intR = Mathf.CeilToInt (radius) - 1;

			int checkX, checkY;
			for (int i = -intR; i <= intR; i++) {
                checkY = y + i;
                if (checkY < 0 || checkY >= Ty) continue;
                for (int j = -intR; j <= intR; j++) {
                    checkX = x + j;
                    if (checkX < 0 || checkX >= Tx) continue;
                    float normDiff = 1.0f - Mathf.Sqrt (i * i + j * j) / radius;
                    if (normDiff < 0)
                        normDiff = 0;
                    if (normDiff > 1.0f) {
                        normDiff = 1.0f;
                    }
                    float diff = threshold * sign * normDiff;
                    heightMap [checkX, checkY] += diff;
                }
            }
        }
    }

    void waterErosion (int iterations, ErosionProgressDelegate erosionProgressDelegate) {
        for (int iter = 0; iter < iterations; iter++) {
            float percentComplete = (float)iter / (float)iterations;
            erosionProgressDelegate ("Applying Water Erosion", "Applying water erosion.", iter, iterations, percentComplete);
            for (int y = 0; y < Ty; y++) {
                for (int x = 0; x < Tx; x++) {
                    waterMap [x, y] = waterErosionRainfall;
                    sedimentMap [x, y] = 0.0f;
                }
            }

            int waterCounter = 100;
            do {
                waterExists = false;
                for (int y = 0; y < Ty; y++) {
                    for (int x = 0; x < Tx; x++) {
                        moveWater (x, y);
                    }
                }
                waterCounter--;
            } while (waterExists && (waterCounter > 0));

            for (int y = 0; y < Ty; y++) {
                for (int x = 0; x < Tx; x++) {
                    addSediment (x, y);
                }
            }
        }
    }

}
