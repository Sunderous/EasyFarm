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
using EasyFarm.Classes;
using EasyFarm.Context;
using EasyFarm.Persistence;
using EasyFarm.UserSettings;
using MemoryAPI;
using MemoryAPI.Navigation;
using System.IO;

namespace EasyFarm.States
{
    /// <summary>
    ///     Enters Shinryu fight after we zone into paradox
    /// </summary>
    public class FarmMeritsState : BaseState
    {
        Position firstIngressPosition = new Position() { X = -495.81787f, Y = -19.378101f, Z = -478.688f };

        public override bool Check(IGameContext context)
        {
            // If we aren't in zone
            if (context.Zone != Zone.Reisenjima)
                return false;

            if (new GetMollifierState().Check(context))
                return false;

            if (new TakeIngressState().Check(context))
                return false;

            // If we've got all our trusts out don't enter this state.
            //if(context.API.PartyMember.Count(pm => pm.Value.UnitPresent) < 6)
            //    return false;

            //return Config.Instance.Route.Waypoints.Count == 0;
            return context.API.Player.Position.Distance(firstIngressPosition) > 20;
        }

        // If we exit, and don't have the KI anymore, we need to load the settings for the fight.
        public override void Enter(IGameContext context)
        {
            var persister = new Persister();
            var fileName = $"ingress_3.eup";
            if (string.IsNullOrWhiteSpace(fileName)) return;
            if (!File.Exists(fileName)) return;
            var config = persister.Deserialize<Config>(fileName);
            Config.Instance = config;
            AppServices.SendConfigLoaded();
        }
    }
}