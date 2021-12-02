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
using System.Linq;
using EasyFarm.Classes;
using MemoryAPI;
using System;
using EasyFarm.ViewModels;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using EasyFarm.Context;
using EasyFarm.UserSettings;
using Player = EasyFarm.Classes.Player;
using System.Timers;

namespace EasyFarm.States
{
    /// <summary>
    ///     A class for defeating monsters.
    /// </summary>
    public class BattleState : BaseState
    {
        private float initialHeading = 0.0f;

        public override bool Check(IGameContext context)
        {
            if (new RestState().Check(context)) 
                return false;

            // Make sure we don't need trusts
            if (new SummonTrustsState().Check(context)) 
                return false;

            // Mobs has not been pulled if pulling moves are available. 
            if (!context.IsFighting) 
                return false;

            // target null or dead. 
            if (!context.Target.IsValid) 
                return false;

            // Engage is enabled and we are not engaged. We cannot proceed. 
            if (context.Config.IsEngageEnabled) 
                return context.Player.Status.Equals(Status.Fighting);

            // Engage is not checked, so just proceed to battle. 
            return true;
        }

        public override void Enter(IGameContext context)
        {
            Player.Stand(context.API);
            initialHeading = context.API.Player.Heading;
        }

        // Need to make sure if claim gets snaked after we /follow the target,
        // that we cancel follow when we un-engage or we won't move.
        public override void Exit(IGameContext context)
        {
            context.API.Navigator.CancelFollow();
        }

        public override void Run(IGameContext context)
        {
            // By spamming this, we ensure that we'll always be facing the mob,
            // and if target/engaged gets confused we'll always switch to attacking
            // whatever we actually have targetted. Also keeps us near the mob if we
            // get knocked back.
            if (context.Player.Status == Status.Fighting)
            {
                context.API.Windower.SendString("/attack <t>");
            }

            if(!context.API.Navigator.IsFollowing())
            {
                context.API.Windower.SendString("/follow <t>");
            }


            if (!context.Memory.EliteApi.Target.LockedOn)
            {
                context.API.Windower.SendString(Constants.ToggleLockOn);
            }

            //Console.WriteLine("ROTATING AROUND MOB");
            context.API.Navigator.RotateAroundMob(initialHeading);


            // Cast only one action to prevent blocking curing. 
            var action = context.Config.BattleLists["Battle"].Actions
                .FirstOrDefault(x => ActionFilters.TargetedFilter(context.API, x, context.Target));
            if (action == null) return;
            context.Memory.Executor.UseTargetedActions(context, new[] {action}, context.Target);
        }    
    }
}