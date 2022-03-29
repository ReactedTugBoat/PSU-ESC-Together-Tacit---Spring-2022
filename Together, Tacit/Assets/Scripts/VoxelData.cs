using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData
{
    public static int resolution = 50;
    public int[,,] voxelData = new int[resolution, resolution, resolution];

    public int Width {
        get { return voxelData.GetLength(0); }
    }

    public int Height {
        get { return voxelData.GetLength(1); }
    }

    public int Depth {
        get { return voxelData.GetLength(2); }
    }

    public int GetCell(int x, int y, int z) {
        return voxelData[x, y, z];
    }

    public int GetNeighbor (int x, int y, int z, Direction dir) {
        DataCoordinate offsetToCheck = offsets[(int)dir];
        DataCoordinate neighborCoord = new DataCoordinate(x + offsetToCheck.x, y + offsetToCheck.y, z + offsetToCheck.z);

        if (neighborCoord.x < 0 || neighborCoord.x >= Width || neighborCoord.y < 0 || neighborCoord.y >= Height || neighborCoord.z < 0 || neighborCoord.z >= Depth)
        {
            return 0;
        }
        
        return GetCell(neighborCoord.x, neighborCoord.y, neighborCoord.z);
    }

    struct DataCoordinate {
        public int x;
        public int y;
        public int z;
        
        public DataCoordinate(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    DataCoordinate[] offsets = {
        new DataCoordinate (0, 0, 1),
        new DataCoordinate (1, 0, 0),
        new DataCoordinate (0, 0, -1),
        new DataCoordinate (-1, 0, 0),
        new DataCoordinate (0, 1, 0),
        new DataCoordinate (0, -1, 0),
    };

    public static int[,,] temp = {
        {{1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}},
        {{1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}},
        {{1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}},
        {{1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}},
        {{1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}, {1,1,1,1,1}}
    };

}

public enum Direction {
    North,
    East,
    South,
    West,
    Up,
    Down
}
