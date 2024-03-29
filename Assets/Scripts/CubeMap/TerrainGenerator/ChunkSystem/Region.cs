﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Region
{
    private readonly string worldpath;
    /*REGIONS DATA = lookTable (REGION_LOOKTABLE_POS_BYTE * number of chunks in a region) + chunks data (2 byte per vertex in each chunk)
    lookTable: Indicate the start position of the chunk data in the region Data byte list +REGION_LOOKTABLE_POS_BYTE. Because the 0 it's reserved for indicate empty chunk.
    chunks data: Contains the data of all chunks saved in the region*/
    private List<byte> regionData;
    private int regionX;
    private int regionZ;
    private bool modified = false;

    /// <summary>
    /// Load the data of a region from a file.
    /// </summary>
    public Region(int x, int z)
    {
        worldpath = WorldManager.GetSelectedWorldDir();
        regionX = x;
        regionZ = z;
        if (File.Exists(DirectionChunkFile()) && !ChunkManager.Instance.editorMode)
        {
            if (TerrainConstants.REGION_SAVE_COMPRESSED)
            {
                byte[] compressedRegion = File.ReadAllBytes(DirectionChunkFile());//Load compressed region
                regionData = new List<byte>(CompressHelper.Decompress(compressedRegion));//Decompress the region data
            }
        }
        else if (File.Exists(StartingChunkFile()))
        {
            if (TerrainConstants.REGION_SAVE_COMPRESSED)
            {
                byte[] compressedRegion = File.ReadAllBytes(StartingChunkFile());//Load compressed region
                regionData = new List<byte>(CompressHelper.Decompress(compressedRegion));//Decompress the region data
            }
        }
        else
        {
            regionData = new List<byte>(new byte[TerrainConstants.REGION_LOOKTABLE_BYTES]);//Look table initialized, all 0
        }
    }
    
    /// <summary>
    /// Return the byte[] from a chunk in the region.
    /// </summary>
    public byte[] GetChunkData(int index)
    {
        int startPos = TerrainConstants.REGION_LOOKTABLE_BYTES + (index-1)*TerrainConstants.CHUNK_BYTES; // index-1 because the lookTable start at 1. LookTable position 10 = chunk data position 9.
        byte[] chunk = new byte[TerrainConstants.CHUNK_BYTES];

        for (int i = startPos, j = 0; i < (startPos + TerrainConstants.CHUNK_BYTES); i ++,j++)
        {
            chunk[j] = regionData[i];
        }

        return chunk;
    }

    /// <summary>
    /// Get the index from the lookTable, the first section of the regionData list<byte>.
    /// </summary>
    public int GetChunkIndex(int x, int z)
    {

        int startPos = (x + z * TerrainConstants.REGION_SIZE) * TerrainConstants.REGION_LOOKTABLE_POS_BYTE + TerrainConstants.REGION_LOOKTABLE_POS_BYTE;
        int index = 0;

        for (int i = 0; i< TerrainConstants.REGION_LOOKTABLE_POS_BYTE; i++)
        {
   
            index |= regionData[startPos+ TerrainConstants.REGION_LOOKTABLE_POS_BYTE-i-1] << 8*i;
        }

        return index;


    }

    /// <summary>
    /// save a chunk byte[] data in the regionData list<byte> of the Chunk class.
    /// </summary>
    public void saveChunkData(byte[] chunkData, int x, int z)
    {
        int chunksDataStartPos = GetChunkIndex(x,z); 
        if(chunksDataStartPos == 0)//Chunk no saved, assign a new position in the chunks data for the lookTable.
        {
            int lookTablePos = (x + z * TerrainConstants.REGION_SIZE) * TerrainConstants.REGION_LOOKTABLE_POS_BYTE + TerrainConstants.REGION_LOOKTABLE_POS_BYTE ;
            int increasePos = TerrainConstants.REGION_LOOKTABLE_POS_BYTE - 1;
            for (int i = TerrainConstants.REGION_LOOKTABLE_POS_BYTE-1; i >= 0; i--)
            {
                //First done the increase because the 0 is reserved for empty, the lookTable start at 1.
                if (i == increasePos)
                {
                    if (regionData[i] == 255)//Reach 255, need return to 0 and change the previous byte.
                    {
                        regionData[i] = 0;
                        increasePos--;
                    }
                    else
                        regionData[i]++;
                }
                //Save the position of chunk in the chunks data inside the lookTable
                regionData[lookTablePos + i] = regionData[i];
            }
            //Write chunks bytes in the regionData list<byte>
            regionData.AddRange(chunkData);
        }
        else
        {
            int startPos = (chunksDataStartPos - 1) * TerrainConstants.CHUNK_BYTES + TerrainConstants.REGION_LOOKTABLE_BYTES;
            // Write chunks bytes in the regionData list<byte>
            for(int i = 0; i < TerrainConstants.CHUNK_BYTES; i++)
            {
                regionData[startPos + i] = chunkData[i];
            }
        }
        modified = true;
    }

    /// <summary>
    /// Save the region data in a file.
    /// </summary>
    public void SaveRegionData()
    {
        if(!modified)
            return;

        if(TerrainConstants.REGION_SAVE_COMPRESSED)
        {
            if(ChunkManager.Instance.editorMode) 
            {
                byte[] Compressed = CompressHelper.Compress(regionData.ToArray());
                File.WriteAllBytes(StartingChunkFile(), Compressed);
            }
            else
            {
                byte[] Compressed = CompressHelper.Compress(regionData.ToArray());
                File.WriteAllBytes(DirectionChunkFile(), Compressed);
            }
        }
        //else
            //File.WriteAllBytes(DirectionChunkFile(), regionData.ToArray());

    }


    //Help function, get chunk file direction.
    private string DirectionChunkFile()
    {
        return worldpath + "/"+ regionX + "."+ regionZ + ".reg";
    }

    private string StartingChunkFile()
    {
        return Application.streamingAssetsPath + "/StartingTerrain/" + regionX + "." + regionZ + ".reg";
    }
}
