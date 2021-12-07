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
using System.Threading;
using EasyFarm.Context;
using MemoryAPI;
using MemoryAPI.Navigation;

namespace EasyFarm.States
{
    /// <summary>
    ///     Teleports us out of the Shinryu fight if we're finished.
    /// </summary>
    public class WarpHomeState : BaseState
    {
        Position firstIngressPosition = new Position() { X = -495.81787f, Y = -19.378101f, Z = -478.688f };

        public override bool Check(IGameContext context)
        {
            // If we aren't in the paradox.
            if (context.Zone != Zone.Reisenjima)
                return false;

            // If we're still fighting.
            if (context.IsFighting)
                return false;

            if (context.API.Player.Position.Distance(firstIngressPosition) < 20)
                return false;

            // If We're in the zone, standing, with the key item gone and no level restriction,
            // it means the fight is over and we're good to teleport out.

            //return context.API.Player.Status.Equals(Status.Standing) && context.API.Player.MeritCount() >= 30;

            // TODO: This kill count is a hack, but the API is currently broken so we can't read our Merit count
            // to determine when to warp back. So we approximate it with this, will fix when API is fixed.
            return context.API.Player.Status.Equals(Status.Standing) && BattleState.KillCount >= 55;
        }

        public override void Run(IGameContext context)
        {
            context.API.Navigator.CancelFollow();

            // Equip the teleport ring, wait 10 seconds, then use it.
            context.API.Windower.SendString("/equip ring2 \"Warp Ring\"");
            
            Thread.Sleep(10000);

            context.API.Windower.SendString("/item \"Warp Ring\" <me>");
        }
    }
}