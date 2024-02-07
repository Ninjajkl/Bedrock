using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Empty : Biome
{
    public override byte[] GenerateChunkData(Vector2Int vecPos, float[] biomeMerge)
    {
        byte[] chunkData = new byte[Constants.CHUNK_BYTES];
        for (int z = 0; z < Constants.CHUNK_VERTEX_SIZE; z++)
        {
            for (int x = 0; x < Constants.CHUNK_VERTEX_SIZE; x++)
            {
                // Get surface height of the x,z position 
                float height = 1f;

                int heightY = Mathf.CeilToInt(height);//Vertex Y where surface start
                int lastVertexWeigh = (int)((255 - isoLevel) * (height % 1) + isoLevel);//Weigh of the last vertex

                for (int y = 0; y < Constants.CHUNK_VERTEX_HEIGHT; y++)
                {
                    int index = (x + z * Constants.CHUNK_VERTEX_SIZE + y * Constants.CHUNK_VERTEX_AREA) * Constants.CHUNK_POINT_BYTE;
                    chunkData[index] = 0;
                    chunkData[index + 1] = 7;//BedRock
                }
            }
        }
        return chunkData;
    }
}
