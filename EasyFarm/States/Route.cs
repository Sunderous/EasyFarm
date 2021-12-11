// ///////////////////////////////////////////////////////////////////
// This file is a part of EasyFarm for Final Fantasy XI
// Copyright (C) 2013 Mykezero
//  
// EasyFarm is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// EasyFarm is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
// ///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasyFarm.Context;
using MemoryAPI;
using MemoryAPI.Navigation;

namespace EasyFarm.States
{
    public class Route
    {
        private int _position = 0;

        public bool StraightRoute = true;
        public ObservableCollection<Position> Waypoints = new ObservableCollection<Position>();
        public Zone Zone { get; set; }
        private Dictionary<int, DateTime> visitTimes = new Dictionary<int, DateTime>();

        public bool IsPathSet => Waypoints.Any();

        public void ResetCurrentWaypoint()
        {
            _position = -1;
        }

        public Position GetCurrentPosition(Position playerPosition)
        {
            if (_position == -1)
            {
                return GetNextPosition(playerPosition);
            }

            if (_position >= Waypoints.Count)
            {
                return null;
            }

            return Waypoints[_position];
        }

        public Position GetNextPosition(Position playerPosition)
        {
            var positionVisited = true;

            if (Waypoints.Count <= 0)
            {
                return null;
            }

            if (Waypoints.Count < 2 && _position > -1)
            {
                return Waypoints[_position];
            }

            var byDistance = Waypoints.OrderBy(x => Distance(playerPosition, x)).ToArray();

            // Tracking when we visit waypoints is the non-geometry way I could come up with
            // to avoid backtracking after every fight a lot of the time.
            var closest = byDistance[0];
            var nextClosest = byDistance[1];

            var closestIndex = Waypoints.IndexOf(closest);
            var nextClosestIndex = Waypoints.IndexOf(nextClosest);

            var closestVisit = visitTimes.Keys.Contains(closestIndex) ? visitTimes[closestIndex] : DateTime.MinValue;
            var nextClosestVisit = visitTimes.Keys.Contains(nextClosestIndex) ? visitTimes[nextClosestIndex] : DateTime.MinValue;

            if (closestVisit > nextClosestVisit)
            {
                closest = nextClosest;
            }

            if (_position == -1)
            {
                _position = Waypoints.IndexOf(closest);
                positionVisited = false;
            }
            else if (_position == Waypoints.Count)
            {
                if (StraightRoute)
                {
                    Waypoints = new ObservableCollection<Position>(Waypoints.Reverse());
                    EasyFarm.ViewModels.LogViewModel.Write("Reached the end of waypoints; reversing.");
                }
                else
                {
                    EasyFarm.ViewModels.LogViewModel.Write("Reached the end of waypoints; circling.");
                }

                _position = 0;

            }

            var newPosition = Waypoints[_position];

            if (positionVisited) { 
                visitTimes[_position] = DateTime.Now;
            }

            EasyFarm.ViewModels.LogViewModel.Write("Navigating to waypoint ("+_position+") " + newPosition.ToString());

            _position++;

            return newPosition;
        }

        private double Distance(Position one, Position other)
        {
            return Math.Sqrt(Math.Pow(one.X - other.X, 2) + Math.Pow(one.Z - other.Z, 2));
        }

        public void Reset()
        {
            Waypoints.Clear();
            visitTimes.Clear();
            Zone = Zone.Unknown;
            _position = 0;
        }

        public bool IsWithinDistance(Position position, double distance)
        {
            return Waypoints.Any(x => Distance(position, x) <= distance);
        }

        public bool IsPathUnreachable(IGameContext context)
        {
            return Zone == context.Player.Zone &&
                context.NavMesh.FindPathBetween(context.API.Player.Position, GetCurrentPosition(context.API.Player.Position)).Count > 0;
        }
    }
}