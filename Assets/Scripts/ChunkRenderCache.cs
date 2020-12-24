﻿using System;
using UnityEngine;

public struct ChunkRenderCache : IBlockDisplayReader
{
    public readonly ClientWorld World;
    public readonly int ChunkStartX;
    public readonly int ChunkStartZ;
    public readonly Vector3Int CacheStartPos;
    public readonly int CacheSizeX;
    public readonly int CacheSizeY;
    public readonly int CacheSizeZ;
    public readonly Chunk[,] Chunks;
    public readonly Tile[,,] Tiles;
    // protected readonly BlockState[] blockStates;
    // protected readonly FluidState[] fluidStates;
    
    private ChunkRenderCache(ClientWorld world, int chunkStartX, int chunkStartZ, Vector3Int startPos, Vector3Int endPos, Chunk[,] chunks)
    {
        World = world;
        ChunkStartX = chunkStartX;
        ChunkStartZ = chunkStartZ;
        Chunks = chunks;
        CacheStartPos = startPos;
        CacheSizeX = endPos.x - startPos.x + 1;
        CacheSizeY = endPos.y - startPos.y + 1;
        CacheSizeZ = endPos.z - startPos.z + 1;
        Tiles = new Tile[CacheSizeX, CacheSizeY, CacheSizeZ];
        // blockStates = new BlockState[CacheSizeX * CacheSizeY * CacheSizeZ];
        // fluidStates = new FluidState[CacheSizeX * CacheSizeY * CacheSizeZ];

        for (int x = startPos.x; x < endPos.x; x++)
        {
            for (int z = startPos.z; z < endPos.z; z++)
            {
                var chunk = chunks[(x >> 4) - chunkStartX, (z >> 4) - chunkStartZ];
                
                for (int y = startPos.y; y < endPos.y; y++)
                {
                    Tiles[x - startPos.x, y - startPos.y, z - startPos.z] = chunk.GetTile(x, y, z);
                }
            }
        }
    }

    public static bool ContainsGaps(Vector3Int from, Vector3Int to, int cx, int cy, Chunk[,] chunks)
    {
        for (int x = from.x >> 4; x <= to.x >> 4; ++x)
        {
            for (int z = from.z >> 4; z <= to.z >> 4; ++z)
            {
                var chunk = chunks[x - cx, z - cy];
                if (chunk == null) return true;

                // if (!chunk.IsEmptyBetween(from.y, to.y))
                // {
                    // return false;
                // }
            }
        }

        return false;
    }

    public Tile GetTile(int x, int y, int z)
    {
        try
        {
            return Tiles[x - CacheStartPos.x, y - CacheStartPos.y, z - CacheStartPos.z];
        }
        catch (Exception e)
        {
            Debug.LogError($"{x - CacheStartPos.x}, {y - CacheStartPos.y}, {z - CacheStartPos.z}");
            return Tile.Air;
        }
    }

    public static ChunkRenderCache? GenerateCache(ClientWorld world, Vector3Int from, Vector3Int to, int padding)
    {
        int startX = from.x - padding >> 4;
        int startZ = from.z - padding >> 4;
        int lastX = to.x + padding >> 4;
        int lastZ = to.z + padding >> 4;

        var chunks = new Chunk[lastX - startX + 1, lastZ - startZ + 1];

        for (int x = startX; x <= lastX; ++x)
        {
            for (int z = startZ; z <= lastZ; ++z)
            {
                chunks[x - startX, z - startZ] = world.GetChunk(x, z);
            }
        }

        var startPos = from - Vector3Int.one;
        var endPos = to + Vector3Int.one;
        
        if (ContainsGaps(startPos, endPos, startX, startZ, chunks))
        {
            return null;
        }

        return new ChunkRenderCache(world, startX, startZ, startPos, endPos, chunks);
    }
}