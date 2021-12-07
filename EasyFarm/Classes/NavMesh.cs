using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MemoryAPI;
using MemoryAPI.Navigation;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.IO.Json;
using SharpNav.Pathfinding;
using SPath = SharpNav.Pathfinding.Path;

public class NavMesh
{

    //static int MAX_PATH = 256;
    static int MAX_PATH = 512;
    private NavMeshQuery snNavMeshQuery;
    private Zone _zone;

    public NavMesh()
    {
    }

    public struct NavMeshTileHeader
    {
        public uint tileRef;
        public int dataSize;

        public static int ByteSize()
        {
            var start = 0;

            start += sizeof(uint);
            start += sizeof(int);

            return start;
        }

        public int FromBytes(byte[] array, int start)
        {
            tileRef = BitConverter.ToUInt32(array, start); start += sizeof(uint);
            dataSize = BitConverter.ToInt32(array, start); start += sizeof(int);

            return start;
        }
    }

    public bool LoadZone(Zone zone)

    {
        if (zone == Zone.Unknown)
        {
            Unload();
            return false;
        }

        string path = "navmeshes\\" + zone.ToString() + ".snb";

        if (_zone == zone)
        {
            return true;
        }
        else
        {
            Unload();
        }

        _zone = zone;

        if (!File.Exists(path))
        {
            return false;
        }

        var tiledNavMesh = new NavMeshJsonSerializer().Deserialize(path);
        snNavMeshQuery = new NavMeshQuery(tiledNavMesh, 2048);

        return true;
    }

    public void Unload()
    {
        snNavMeshQuery = null;
    }

    private static Position ToFFXIPosition(float[] detourPosition)
    {
        var ffxiPosition = new Position();

        ffxiPosition.X = detourPosition[0];
        ffxiPosition.Y = -detourPosition[1];
        ffxiPosition.Z = -detourPosition[2];

        return ffxiPosition;

    }

    public Queue<Position> FindPathBetween(Position start, Position end, bool useStraightPath = false)
    {
        var path = new Queue<Position>();

        if (snNavMeshQuery == null)
        {
            return path;
        }

        var startDetour = start.ToDetourPosition();
        Vector3 startSN = new Vector3(startDetour[0], startDetour[1], startDetour[2]);

        var endDetour = end.ToDetourPosition();
        Vector3 endSN = new Vector3(endDetour[0], endDetour[1], endDetour[2]);

        var extents = new Vector3(5.0f, 5.0f, 5.0f);

        var startPoint = snNavMeshQuery.FindNearestPoly(startSN, extents);
        var endPoint = snNavMeshQuery.FindNearestPoly(endSN, extents);

        SPath navPath = new SPath();
        snNavMeshQuery.FindPath(ref startPoint, ref endPoint, new NavQueryFilter(), navPath);

        StraightPath straightPath = new StraightPath();
        snNavMeshQuery.FindStraightPath(startSN, endSN, navPath, straightPath, PathBuildFlags.None);

        if (straightPath.Count == 0)
        {
            path.Enqueue(start);
            path.Enqueue(end);
            return path;
        }

        if (straightPath.Count > 0)
        {
            for (int i = 0; i < straightPath.Count; i++)
            {
                var pos = straightPath[i].Point.Position;

                var posArr = new float[] { pos.X, pos.Y, pos.Z };
                var position = ToFFXIPosition(posArr);

                path.Enqueue(position);
            }
        }

        if (path.Count < 1)
        {
            path.Enqueue(end);
        }

        return path;

    }

    public void GoToPosition(IMemoryAPI api, Position position, bool keepRunning = false)
    {
        var travelFps = (int)Math.Floor(1000.0 / 50.0);

        api.Navigator.DistanceTolerance = 1.0;

        var route = FindPathBetween(api.Player.Position, position);

        while (route.Count > 0)
        {
            while (route.Count > 0 && route.Peek().Distance(api.Player.Position) <= api.Navigator.DistanceTolerance)
            {
                route.Dequeue();
            }

            if (route.Count > 0)
            {
                api.Navigator.GotoWaypoint(route.Peek(), true);
            }

            Thread.Sleep(travelFps);
        }

        if(!keepRunning)
        {
            api.Navigator.CancelFollow();
        }
    }

    public Position NextRandomPosition(Position start)
    {
        return new Position();
    }
}
