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
using MemoryAPI;
using System.IO;
using System.Threading;

namespace EasyFarm.States
{
    /// <summary>
    ///     Enters Shinryu fight after we zone into paradox
    /// </summary>
    public class FarmMeritsState : BaseState
    {
        public override bool Check(IGameContext context)
        {
            // If we aren't in zone
            if (context.Zone != Zone.Reisenjima)
                return false;

            if (new GetMollifierState().Check(context))
                return false;

            if (new TakeIngressState().Check(context))
                return false;

            if (context.API.PartyMember[1] == null || context.API.PartyMember[1].Name == "Monberaux")
                return false;

            return context.Player.Status.Equals(Status.Standing);
        }

        // If we exit, and don't have the KI anymore, we need to load the settings for the fight.
        public override void Enter(IGameContext context)
        {
            var persister = new Persister();
            var fileName = $"toads.eup";
            if (string.IsNullOrWhiteSpace(fileName)) return;
            if (!File.Exists(fileName)) return;
            var config = persister.Deserialize<Config>(fileName);
            Config.Instance = config;
            AppServices.SendConfigLoaded();

            Thread.Sleep(5000);

            config.Route = persister.Deserialize<Route>("reisen_frogs_15.ewl");
        }
    }
}