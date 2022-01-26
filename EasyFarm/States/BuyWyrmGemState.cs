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
using EasyFarm.Classes;
using EasyFarm.Context;
using EasyFarm.Persistence;
using EasyFarm.UserSettings;
using EliteMMO.API;
using MemoryAPI;
using MemoryAPI.Navigation;
using System.IO;

namespace EasyFarm.States
{
    /// <summary>
    ///     Enters Reisenjima after we've teleported to the canyon.
    /// </summary>
    public class BuyWyrmGemState : BaseState
    {
        Position vendorPosition = new Position() { X = 198.34203f, Y = -8.340302f, Z = 179.56676f };

        public override bool Check(IGameContext context)
        {
            // If we aren't in the paradox.
            if (context.Zone != Zone.Port_Windurst)
                return false;

            if (context.API.Player.HasKeyItem(3261))
                return false;

            //if (context.API.Player.Job == ChangeJobsState.shinJob.Item1)
            //    return false;

            return context.API.Player.MeritCount() >= 30;
        }

        public override void Enter(IGameContext context)
        {
            // We reset this here so it only gets reset if we actually make it back to town.
            BattleState.KillCount = 0;

            var persister = new Persister();
            var fileName = $"town.eup";
            if (string.IsNullOrWhiteSpace(fileName)) return;
            if (!File.Exists(fileName)) return;
            var config = persister.Deserialize<Config>(fileName);
            Config.Instance = config;
            AppServices.SendConfigLoaded();

            config.Route.Waypoints.Clear();
        }

        public override void Run(IGameContext context)
        {
            // This is to ensure we don't get stuck in home point menu after warping
            // back to town.
            context.API.Windower.SendKeyPress(Keys.ESCAPE);

            // Move forward to portal.
            // X: 261.00595, Y: 35.1506, Z: 340.02786
            context.NavMesh.GoToPosition(context.API, vendorPosition);

            // Enter portal.
            // Name = Dimensional Portal
            // Options = [ 2 ]

            if (context.API.Player.Position.Distance(vendorPosition) <= 3)
            {
                //context.API.NPC.MenuSequence("Mimble-Pimble", new int[] { 2, 2, 2, -2 });
                context.API.Windower.SendString("//htmb");
            }
        }
    }
}