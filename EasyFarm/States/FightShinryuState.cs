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
using MemoryAPI.Navigation;
using System.IO;
using System.Linq;
using System.Threading;

namespace EasyFarm.States
{
    /// <summary>
    ///     Enters Shinryu fight after we zone into paradox
    /// </summary>
    public class FightShinryuState : BaseState
    {
        Position entrance = new Position() { X = 539.9569f, Y = -500f, Z = -594.7143f };

        public override bool Check(IGameContext context)
        {
            // If we aren't in qufim.
            if (context.Zone != Zone.Abyssea_Empyreal_Paradox)
                return false;

            // If we don't have the KI
            if (context.API.Player.HasKeyItem(3261))
                return false;

            // If We're in the zone, standing, with the key item, we know we need to zone in.
            return context.API.Player.Status.Equals(Status.Standing) && context.API.Player.StatusEffects.Contains(StatusEffect.Level_Restriction);
        }

        // If we exit, and don't have the KI anymore, we need to load the settings for the fight.
        public override void Enter(IGameContext context)
        {
            var persister = new Persister();
            var fileName = $"shinryu_easy.eup";
            if (string.IsNullOrWhiteSpace(fileName)) return;
            if (!File.Exists(fileName)) return;
            var config = persister.Deserialize<Config>(fileName);
            Config.Instance = config;
            AppServices.SendConfigLoaded();

            Thread.Sleep(5000);

            Config.Instance.Route.Waypoints.Clear();
        }
    }
}