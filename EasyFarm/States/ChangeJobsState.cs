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
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace EasyFarm.States
{
    /// <summary>
    ///     Enters Reisenjima after we've teleported to the canyon.
    /// </summary>
    public class ChangeJobsState : BaseState
    {
        Position mooglePosition = new Position() { X = -58.862057f, Y = -5.035444f, Z = 52.59803f };
        Position portHPPosition = new Position() { X = 181.21f, Y = -12f, Z = 224.802f };

        public static Tuple<Job, Job> meritJob = Tuple.Create(Job.Bard, Job.Ninja);
        public static Tuple<Job, Job> shinJob = Tuple.Create(Job.Thief, Job.Dancer);

        Dictionary<Job, string> jobStrings = new Dictionary<Job, string>() {
            { Job.Bard, "brd" },
            { Job.Ninja, "nin" },
            { Job.Thief, "thf" },
            { Job.Dragoon, "drg" },
            { Job.Dancer, "dnc" }
        };

        public override bool Check(IGameContext context)
        {
            // If we aren't in either zone
            if (context.Zone != Zone.Port_Windurst && context.Zone != Zone.Norg)
                return false;

            // If we bought the KI, but are still not on the right job for the fight.
            if (context.API.Player.HasKeyItem(3261))
                return context.API.Player.Job != shinJob.Item1 || context.API.Player.SubJob != shinJob.Item2;

            // If we warped back to windy (no KI yet), and are still in our shin jobs then
            // need to switch to merits.
            if (!(context.API.Player.Job == meritJob.Item1 && context.API.Player.SubJob == meritJob.Item2))
                return true;

            return false;
        }

        public override void Enter(IGameContext context)
        {
            // We reset this here so it only gets reset if we actually make it back to town.
            //BattleState.KillCount = 0;

            //var persister = new Persister();
            //var fileName = $"town.eup";
            //if (string.IsNullOrWhiteSpace(fileName)) return;
            //if (!File.Exists(fileName)) return;
            //var config = persister.Deserialize<Config>(fileName);
            //Config.Instance = config;
            //AppServices.SendConfigLoaded();

            //config.Route.Waypoints.Clear();
        }

        public override void Run(IGameContext context)
        {
            if(context.Zone == Zone.Port_Windurst)
            {
                context.NavMesh.GoToPosition(context.API, portHPPosition);

                if (context.API.Player.Position.Distance(portHPPosition) <= 3)
                {
                    context.API.Windower.SendString("//sw cancel");
                    Thread.Sleep(2000);
                    context.API.Windower.SendString("//hp norg 2");
                    Thread.Sleep(5000);
                }
            }
            else
            {
                // If we don't have orb need to change to merit jobs, and use dim. ring.
                if(!context.API.Player.HasKeyItem(3261))
                {
                    if(context.API.Player.Job != meritJob.Item1 || context.API.Player.SubJob != meritJob.Item2)
                    {
                        context.NavMesh.GoToPosition(context.API, mooglePosition);

                        if (context.API.Player.Position.Distance(mooglePosition) <= 1.5)
                        {
                            context.API.Windower.SendString($"//jc main {jobStrings[meritJob.Item1]}");
                            Thread.Sleep(3000);
                            context.API.Windower.SendString($"//jc sub {jobStrings[meritJob.Item2]}");
                        }
                    }
                }
                else
                {
                    if (context.API.Player.Job != shinJob.Item1 || context.API.Player.SubJob != shinJob.Item2)
                    {
                        context.NavMesh.GoToPosition(context.API, mooglePosition);

                        if (context.API.Player.Position.Distance(mooglePosition) <= 1.5)
                        {
                            context.API.Windower.SendString($"//jc main {jobStrings[shinJob.Item1]}");
                            Thread.Sleep(3000);
                            context.API.Windower.SendString($"//jc sub {jobStrings[shinJob.Item2]}");
                        }
                    }
                }
            }
        }
    }
}