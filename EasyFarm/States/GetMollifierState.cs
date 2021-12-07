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
using System.Threading;

namespace EasyFarm.States
{
    /// <summary>
    ///     Enters Reisenjima after we've teleported to the canyon.
    /// </summary>
    public class GetMollifierState : BaseState
    {
        public override bool Check(IGameContext context)
        {
            // If we aren't in zone
            if (context.Zone != Zone.Reisenjima)
                return false;

            // If we already have the mollifier.
            if (context.API.Player.HasKeyItem(3032))
                return false;

            return true;
        }

        public override void Run(IGameContext context)
        {
            // TODO: How to account for elvorseal? Packets necessary?
            // Buy Mollifier
            // Name = Shiftrix
            // Options = [ 3, 4, 1 ]

            // TODO: This is targeting players, sometimes when finishing the menu selection.
            // And not getting mollifier even when elvorseal up.
            context.API.NPC.MenuSequence("Shiftrix", new int[] { 3, 4, 1 });

            Thread.Sleep(2000);

            context.API.NPC.EscapeMenu();

            if (context.API.Player.HasKeyItem(3261))
            {
                return;
            }

            // If we do the first sequence and don't have the KI, then elvorseal probably
            // an option.
            context.API.NPC.MenuSequence("Shiftrix", new int[] { 4, 4, 1 });

            Thread.Sleep(2000);

            context.API.NPC.EscapeMenu();
        }
    }
}