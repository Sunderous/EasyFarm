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
    ///     Enters Shinryu fight after we zone into paradox
    /// </summary>
    public class EnterShinryuState : BaseState
    {
        Position entrance = new Position() { X = 539.9569f, Y = -500f, Z = -594.7143f };

        public override bool Check(IGameContext context)
        {
            // If we aren't in qufim.
            if (context.Zone != Zone.Abyssea_Empyreal_Paradox)
                return false;

            // If we don't have the KI
            if (!context.API.Player.HasKeyItem(3261))
                return false;

            // If We're in the zone, standing, with the key item, we know we need to zone in.
            //return context.API.Player.Status.Equals(Status.Standing);
            return true;
        }

        public override void Run(IGameContext context)
        {
            // Move to portal.
            // X: 539.9569, Y: -500, Z: -594.7143
            context.NavMesh.GoToPosition(context.API, entrance);

            // Enter portal.
            // Name = Transcendental radiance
            // Options = [ 3, 3 ]
            if(context.API.Player.Position.Distance(entrance) <= 3)
            {
                context.API.NPC.MenuSequence("Transcendental Radiance", new int[] { 3, 1 });
                //context.API.Windower.SendString("//shin enter VE");
                Thread.Sleep(5000);
            }

        }

        // If we exit, and don't have the KI anymore, we need to load the settings for the fight.
        //public override void Exit(IGameContext context)
        //{
        //    if (!context.API.Player.HasKeyItem(3261))
        //    {
        //        var persister = new Persister();
        //        var fileName = $"shinryu_easy.eup";
        //        if (string.IsNullOrWhiteSpace(fileName)) return;
        //        if (!File.Exists(fileName)) return;
        //        var config = persister.Deserialize<Config>(fileName);
        //        Config.Instance = config;
        //        AppServices.SendConfigLoaded();

        //        Config.Instance.Route.Waypoints.Clear();
        //    }
        //}
    }
}