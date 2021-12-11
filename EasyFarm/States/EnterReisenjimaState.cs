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
using System.Threading;

namespace EasyFarm.States
{
    /// <summary>
    ///     Enters Reisenjima after we've teleported to the canyon.
    /// </summary>
    public class EnterReisenjimaState : BaseState
    {
        Position portalPosition = new Position() { X = 261.00595f, Y = 35.1506f, Z = 340.02786f };

        public override bool Check(IGameContext context)
        {
            // If we aren't in the paradox.
            if (context.Zone != Zone.Tahrongi_Canyon)
                return false;

            // If We're in the zone, standing, with the key item gone and no level restriction,
            // it means the fight is over and we're good to teleport out.
            //return context.API.Player.Status.Equals(Status.Standing);
            return true;
        }

        public override void Run(IGameContext context)
        {
            // Move forward to portal.
            // X: 261.00595, Y: 35.1506, Z: 340.02786
            context.NavMesh.GoToPosition(context.API, portalPosition);

            // Enter portal.
            // Name = Dimensional Portal
            // Options = [ 2 ]
            if (context.API.Player.Position.Distance(portalPosition) <= 3)
            {
                // context.API.NPC.MenuSequence("Dimensional Portal", new int[] { -2 });
                context.API.Windower.SendString("//ew enter");
                Thread.Sleep(5000);
            }
        }
    }
}