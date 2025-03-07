﻿// ///////////////////////////////////////////////////////////////////
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
using System.Linq;
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
            if (context.Zone != Zone.Reisenjima && context.Zone != Zone.Abyssea_Empyreal_Paradox)
                return false;

            // If we're still fighting.
            if (context.IsFighting)
                return false;
            
            if(context.Zone == Zone.Reisenjima)
            {
                if (context.API.Player.Position.Distance(firstIngressPosition) < 20)
                    return false;

                // If We're in the zone, standing, with the key item gone and no level restriction,
                // it means the fight is over and we're good to teleport out.

                //return context.API.Player.Status.Equals(Status.Standing) && context.API.Player.MeritCount() >= 30;

                //var meritCount = context.API.Player.MeritCount();

                // TODO: This kill count is a hack, but the API is currently broken so we can't read our Merit count
                // to determine when to warp back. So we approximate it with this, will fix when API is fixed.
                return context.API.Player.Status.Equals(Status.Standing) && context.API.Player.MeritCount() >= 30;
            }
            else
            {
                // 3261 = Wyrm god phantom gem
                // If we haven't turned in the key item yet.
                if (context.API.Player.HasKeyItem(3261))
                    return false;

                // If we still have our level restriction.
                return !context.API.Player.StatusEffects.Contains(StatusEffect.Level_Restriction);
            }
        }

        public override void Run(IGameContext context)
        {
            Thread.Sleep(2000);

            context.API.Navigator.CancelFollow();       

            // Equip the teleport ring, wait 10 seconds, then use it.
            context.API.Windower.SendString("/equip ring2 \"Warp Ring\"");

            Thread.Sleep(12000);

            if(context.API.Player.HasKeyItem(3261))
            {
                return;
            }

            context.API.Windower.SendString("/item \"Warp Ring\" <me>");

            Thread.Sleep(15000);
        }
    }
}