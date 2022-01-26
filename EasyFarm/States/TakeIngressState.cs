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
using System.Threading;

namespace EasyFarm.States
{
    /// <summary>
    ///     Enters Reisenjima after we've teleported to the canyon.
    /// </summary>
    public class TakeIngressState : BaseState
    {
        Position firstIngressPosition = new Position() { X = -495.81787f, Y = -19.378101f, Z = -478.688f };

        public override bool Check(IGameContext context)
        {
            // If we aren't in zone
            if (context.Zone != Zone.Reisenjima)
                return false;

            // If we already don't have the mollifier.
            if (!context.API.Player.HasKeyItem(3032))
                return false;

            // If we're standing in the zone, with the mollifier, and are still within 20 distance of the
            // first ingress, we need to take it to camp still.
            return context.API.Player.Position.Distance(firstIngressPosition) < 20;
        }

        public override void Run(IGameContext context)
        {
            // Move to ingress:
            context.NavMesh.GoToPosition(context.API, firstIngressPosition);

            // Take ingress
            // Name = Ethereal Ingress #1
            // Options = [ 2, 5, 1 ]
            if (context.API.Player.Position.Distance(firstIngressPosition) <= 3)
            {
                // context.API.NPC.MenuSequence("Ethereal Ingress #1", new int[] { 2, 5, -2 });
                context.API.Windower.SendString("//ew 3");
                Thread.Sleep(2000);
            }
        }

        //public override void Exit(IGameContext context)
        //{
        //    var persister = new Persister();
        //    var fileName = $"toads.eup";
        //    if (string.IsNullOrWhiteSpace(fileName)) return;
        //    if (!File.Exists(fileName)) return;
        //    var config = persister.Deserialize<Config>(fileName);
        //    Config.Instance = config;
        //    AppServices.SendConfigLoaded();

        //    Thread.Sleep(5000);

        //    config.Route = persister.Deserialize<Route>("reisen_frogs.ewl");
        //}
    }
}