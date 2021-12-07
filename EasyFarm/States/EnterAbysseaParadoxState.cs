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
    ///     Enters abby paradox after we zone into qufim.
    /// </summary>
    public class EnterAbysseaParadoxState : BaseState
    {
        Position paradoxPortal = new Position() { X = -258.78757f, Y = -21.382929f, Z = 221.74797f };

        public override bool Check(IGameContext context)
        {
            // If we aren't in qufim.
            if (context.Zone != Zone.Qufim_Island)
                return false;

            return true;
        }

        public override void Run(IGameContext context)
        {
            // Move to portal.
            // X: -258.78757, Y: -21.382929, Z: 221.74797
            context.NavMesh.GoToPosition(context.API, paradoxPortal);

            // Enter portal.
            // Name = Transcendental radiance
            // Options = [ 1 ]
            if (context.API.Player.Position.Distance(paradoxPortal) <= 3)
            {
                context.API.NPC.MenuSequence("Transcendental Radiance", new int[] { 1 });
            }
        }
    }
}