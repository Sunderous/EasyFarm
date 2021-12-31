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
using System.IO;
using System.Linq;
using System.Threading;
using EasyFarm.Classes;
using EasyFarm.Context;
using EasyFarm.Persistence;
using EasyFarm.UserSettings;
using MemoryAPI;

namespace EasyFarm.States
{
    /// <summary>
    ///     Teleports us out of the Shinryu fight if we're finished.
    /// </summary>
    public class TeleportMeaState : BaseState
    {
        public override bool Check(IGameContext context)
        {
            // If we aren't in the paradox.
            if (context.Zone != Zone.Norg)
                return false;

            // 3261 = Wyrm god phantom gem
            // If we haven't turned in the key item yet.
            if (context.API.Player.HasKeyItem(3261))
                return false;

            // If We're in the zone, standing, with the key item gone and no level restriction,
            // it means the fight is over and we're good to teleport out.
            return context.API.Player.Job == ChangeJobsState.meritJob.Item1 && context.API.Player.SubJob == ChangeJobsState.meritJob.Item2;
        }

        public override void Enter(IGameContext context)
        {
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
            // Equip the teleport ring, wait 10 seconds, then use it.
            context.API.Windower.SendString("/equip ring2 \"Dim. Ring (Mea)\"");
            
            Thread.Sleep(10000);

            context.API.Windower.SendString("/item \"Dim. Ring (Mea)\" <me>");
        }
    }
}