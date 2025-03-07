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
using System;
using System.Linq;
using EasyFarm.Classes;
using EasyFarm.Context;
using EasyFarm.UserSettings;
using EasyFarm.ViewModels;
using Player = EasyFarm.Classes.Player;

namespace EasyFarm.States
{
    public class SetTargetState : BaseState
    {
        private DateTime? _lastTargetCheck;

        public override bool Check(IGameContext context)
        {
            var shouldCheck = ShouldCheckTarget();

            // Update last time target was updated. 
            _lastTargetCheck = DateTime.Now;

            if (new SummonTrustsState().Check(context))
                return false;

            // By not setting target when approaching/fighting
            // hopefully will cut down on having the wrong
            // target selected sometimes.
            if (new ApproachState().Check(context))
                return false;

            if (new BattleState().Check(context))
                return false;

            return shouldCheck;
        }

        public override void Run(IGameContext context)
        {
            // First get the first mob by distance.
            var mobs = context.Units.Where(x => x.IsValid).ToList();
            mobs = TargetPriority.Prioritize(mobs).ToList();

            var lastTarget = context.Target;

            // Set our new target at the end so that we don't accidentally cast on a new target.
            var target = mobs.FirstOrDefault(mob => {
                if((mob.HasAggroed && !context.Config.AggroFilter) || mob.PartyClaim && !context.Config.PartyFilter)
                {
                    return false;
                }
                if(mob.IsClaimed && !mob.MyClaim && !context.Config.ClaimedFilter)
                {
                    return false;
                }
                if(!mob.IsClaimed && !context.Config.UnclaimedFilter)
                {
                    return false;
                }

                return true;
            }) ?? new NullUnit();

            if (target.IsValid)
            {
                context.Target = target;

                // FIXME: if random path is set, do not reset? make this configurable?
                //context.Config.Route.ResetCurrentWaypoint();

                //LogViewModel.Write("Now targeting " + context.Target.Name + " : " + context.Target.Id);
            }

            Player.SetTarget(context.API, target);
        }

        private bool ShouldCheckTarget()
        {
            if (_lastTargetCheck == null) return true;
            return DateTime.Now >= _lastTargetCheck.Value.AddSeconds(Constants.UnitArrayCheckRate);
        }
    }
}