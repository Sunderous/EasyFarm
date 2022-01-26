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
using EasyFarm.Context;
using MemoryAPI;
using MemoryAPI.Navigation;
using System.Threading;

namespace EasyFarm.States
{
    /// <summary>
    ///     Enters Reisenjima after we've teleported to the canyon.
    /// </summary>
    public class WarpToQufimState : BaseState
    {
        Position norgHPPosition = new Position() { X = -65.16393f, Y = -5.249864f, Z = 53.82993f };

        public override bool Check(IGameContext context)
        {
            if (context.Zone != Zone.Norg)
                return false;

            if (!context.API.Player.HasKeyItem(3261))
                return false;

            if (new ChangeJobsState().Check(context))
                return false;


            return true;
        }

        public override void Run(IGameContext context)
        {

            context.NavMesh.GoToPosition(context.API, norgHPPosition);

            if (context.API.Player.Position.Distance(norgHPPosition) <= 1.0)
            {
                context.API.Windower.SendString("//hp qufim 1");
                Thread.Sleep(5000);
            }
        }
    }
}