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
using EasyFarm.Context;
using MemoryAPI;
using MemoryAPI.Navigation;

namespace EasyFarm.States
{
    /// <summary>
    ///     Enters Reisenjima after we've teleported to the canyon.
    /// </summary>
    public class WarpToQufimState : BaseState
    {
        Position homePointPosition = new Position() { X = 181.21f, Y = -12f, Z = 224.802f };

        public override bool Check(IGameContext context)
        {
            if (context.Zone != Zone.Port_Windurst)
                return false;

            if (!context.API.Player.HasKeyItem(3261))
                return false;


            return true;
        }

        public override void Run(IGameContext context)
        {

            context.NavMesh.GoToPosition(context.API, homePointPosition);

            if (context.API.Player.Position.Distance(homePointPosition) <= 3)
            {
                context.API.NPC.MenuSequence("Home Point #3", new int[] { 1, 2, 2, 2 });
            }
        }
    }
}