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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EliteMMO.API;
using MemoryAPI.Chat;
using MemoryAPI.Navigation;
using MemoryAPI.Resources;
using MemoryAPI.Windower;

namespace MemoryAPI.Memory
{
    public class EliteMmoWrapper : MemoryWrapper
    {
        public enum ViewMode
        {
            ThirdPerson = 0,
            FirstPerson
        }

        public EliteMmoWrapper(int pid)
        {
            var eliteApi = new EliteAPI(pid);

            MemoryService.Initialize("pol");

            Navigator = new NavigationTools(eliteApi);
            NPC = new NpcTools(eliteApi);
            PartyMember = new Dictionary<byte, IPartyMemberTools>();
            Player = new PlayerTools(eliteApi);
            Target = new TargetTools(eliteApi);
            Timer = new TimerTools(eliteApi);
            Windower = new WindowerTools(eliteApi);
            Chat = new ChatTools(eliteApi);
            Resource = new ResourcesTools(eliteApi);

            for (byte i = 0; i < 16; i++)
            {
                PartyMember.Add(i, new PartyMemberTools(eliteApi, i));
            }
        }

        public class NavigationTools : INavigatorTools
        {
            private const double TooCloseDistance = 1.5;
            private readonly EliteAPI _api;

            public double DistanceTolerance { get; set; } = 1;
            public bool IsStuck { get; set; } = false;

            public NavigationTools(EliteAPI api)
            {
                _api = api;
            }

            private static double Bearing(Position player, Position position)
            {
                var bearing = -Math.Atan2(position.Z - player.Z, position.X - player.X);

                return bearing;
            }

            private static double Radius(Position player, Position position)
            {
                return Math.Sqrt(Math.Pow(position.Z - player.Z, 2) + Math.Pow(position.X - player.X, 2));
            }

            public void FaceTarget()
            {
                var player = _api.Entity.GetLocalPlayer();
                FaceHeading(GetEntityPosition((int)player.TargetID), false);
            }

            public bool IsFollowing()
            {
                return _api.AutoFollow.IsAutoFollowing;
            }

            public void CancelFollow()
            {
                _api.AutoFollow.IsAutoFollowing = false;
            }

            public void FaceHeading(Position position, bool isRunning)
            {
                var player = _api.Entity.GetLocalPlayer();

                var playerPosition = Helpers.ToPosition(player.X, player.Y, player.Z, player.H);

                //var radius = Radius(playerPosition, position);
                var heading = Bearing(playerPosition, position);

                //SetViewMode(ViewMode.FirstPerson);
                _api.Entity.SetEntityHPosition(_api.Entity.LocalPlayerIndex, (float)heading);
            }

            public void RotateAroundMob(float initialHeading)
            {
                var player = _api.Entity.GetLocalPlayer();
                var initialHeadingDeg = (180 / Math.PI) * initialHeading;

                // We rotated around the mob with a 10 degree fuzz factor.
                var upperHeading = initialHeadingDeg + 185;
                var lowerHeading = initialHeadingDeg + 175;

                while((180 / Math.PI) * player.H < lowerHeading || (180 / Math.PI) * player.H > upperHeading )
                {
                    _api.ThirdParty.KeyDown(Keys.NUMPAD6);
                }

                _api.ThirdParty.KeyUp(Keys.NUMPAD6);
            }

            private double DistanceTo(Position position)
            {
                var player = _api.Entity.GetLocalPlayer();

                return Math.Sqrt(
                    Math.Pow(position.X - player.X, 2) +
                    Math.Pow(position.Y - player.Y, 2) +
                    Math.Pow(position.Z - player.Z, 2));
            }

            public void GotoWaypoint(Position position, bool keepRunning)
            {
                if (DistanceTo(position) <= DistanceTolerance)
                {
                    if (!keepRunning) Reset();
                    return;
                }
                MoveForwardTowardsPosition(position, keepRunning);
            }

            public void GotoNPC(int id, Position position, bool keepRunning)
            {
                var entity = _api.GetCachedEntity(id);
                _api.AutoFollow.SetAutoFollowInfo(entity.TargetingIndex, entity.TargetID, 0, 0);
                _api.AutoFollow.IsAutoFollowing = true;

                //if (DistanceTo(position) <= DistanceTolerance)
                //{
                //    if (!keepRunning) Reset();
                //    return;
                //}
                //MoveForwardTowardsPosition(position, keepRunning, true);
            }

            private Position GetEntityPosition(int id)
            {
                var entity = _api.GetCachedEntity(id);
                var position = Helpers.ToPosition(entity.X, entity.Y, entity.Z, entity.H);
                return position;
            }

            private void MoveForwardTowardsPosition(Position targetPosition, bool keepRunning = true, bool keepOneYalmBack = false)
            {
                //FaceHeading(targetPosition, keepRunning);
                //_api.ThirdParty.KeyDown(Keys.NUMPAD8);
                _api.AutoFollow.SetAutoFollowCoords(targetPosition.X - _api.Player.X, targetPosition.Y - _api.Player.Y, targetPosition.Z - _api.Player.Z);
                _api.AutoFollow.IsAutoFollowing = true;

                //if (keepOneYalmBack)
                //{
                //    KeepOneYalmBack(targetPosition, keepRunning);
                //}
                //if(!IsEngaged())
                //{
                //    AvoidObstacles();
                //}
                
                if (!keepRunning) Reset();
            }

            private void KeepRunningWithKeyboard()
            {
                _api.ThirdParty.KeyDown(Keys.NUMPAD8);
            }

            private void KeepOneYalmBack(Position position, bool isRunning)
            {
                if (DistanceTo(position) > TooCloseDistance) return;

                DateTime duration = DateTime.Now.AddSeconds(5);
                _api.ThirdParty.KeyDown(Keys.NUMPAD2);

                while (DistanceTo(position) <= TooCloseDistance && DateTime.Now < duration)
                {
                    FaceHeading(position, isRunning);
                    Thread.Sleep(30);
                }

                _api.ThirdParty.KeyUp(Keys.NUMPAD2);
            }

            private void SetViewMode(ViewMode viewMode)
            {
                if ((ViewMode)_api.Player.ViewMode != viewMode)
                {
                    _api.Player.ViewMode = (int)viewMode;
                    Thread.Sleep(200);
                }
            }

            /// <summary>
            /// Attempts to get a stuck player moving again.
            /// </summary>
            private void AvoidObstacles()
            {
                if (IsStuck)
                {
                    //if (IsEngaged()) Disengage();
                    WiggleCharacter(attempts: 3);
                }
            }


            /// <summary>
            /// If the player is in fighting stance.
            /// </summary>
            /// <returns></returns>
            private bool IsEngaged()
            {
                return _api.Player.Status == (ulong)Status.Fighting;
            }

            /// <summary>
            /// Stop fighting the current target.
            /// </summary>
            private void Disengage()
            {
                _api.ThirdParty.SendString("/attack off");
            }

            /// <summary>
            /// Wiggles the character left and right to become unstuck when stuck on an object.
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// Author: dlsmd
            /// http://www.elitemmonetwork.com/forums/viewtopic.php?p=4627#p4627
            /// </remarks>
            private void WiggleCharacter(int attempts)
            {
                int originalAttempts = attempts;
                int count = 0;
                float dir = -45;
                var dirKey = Keys.NUMPAD4;

                while (IsStuck && attempts-- > 0)
                {
                    var rand = new Random();
                    var tryLeft = rand.NextDouble() > 0.5;

                    dirKey = tryLeft ? Keys.NUMPAD4 : Keys.NUMPAD6;

                    Console.WriteLine("Wiggle character");
                    //_api.Entity.GetLocalPlayer().H = _api.Player.H + (float)(Math.PI / 180 * dir);
                    _api.ThirdParty.KeyDown(dirKey);
                    var keydownTime = Math.Round((originalAttempts - attempts) * new Random().NextDouble() * 1.5);
                    Thread.Sleep(TimeSpan.FromSeconds(keydownTime));
                    _api.ThirdParty.KeyUp(dirKey);
                    count++;
                    if (count >= originalAttempts + 1)
                    {
                        dir = Math.Abs(dir - -45) < .001 ? 45 : -45;
                        count = 0;
                    }
                }
                _api.ThirdParty.KeyUp(dirKey);
            }

            public void ResetFacing(Keys? ignoreKey = null)
            {
                if (!ignoreKey.HasValue || ignoreKey.Value != Keys.Q)
                {
                    _api.ThirdParty.KeyUp(Keys.Q);
                }
                if (!ignoreKey.HasValue || ignoreKey.Value != Keys.E)
                {
                    _api.ThirdParty.KeyUp(Keys.E);
                }
                if (!ignoreKey.HasValue || ignoreKey.Value != Keys.A)
                {
                    _api.ThirdParty.KeyUp(Keys.A);
                }
                if (!ignoreKey.HasValue || ignoreKey.Value != Keys.D)
                {
                    _api.ThirdParty.KeyUp(Keys.D);
                }
            }

            public void Reset()
            {
                _api.AutoFollow.IsAutoFollowing = false;
                //ResetFacing();
            }
        }

        public class NpcTools : INPCTools
        {
            private readonly EliteAPI _api;

            public NpcTools(EliteAPI api)
            {
                _api = api;
            }

            public int ClaimedID(int id) { return (int)_api.GetCachedEntity(id).ClaimID; }

            public double Distance(int id) { return _api.GetCachedEntity(id).Distance; }

            public Position GetPosition(int id)
            {
                var entity = _api.GetCachedEntity(id);
                return Helpers.ToPosition(entity.X, entity.Y, entity.Z, entity.H);
            }

            public short HPPCurrent(int id) { return _api.GetCachedEntity(id).HealthPercent; }

            public bool IsActive(int id) { return true; }

            public bool IsClaimed(int id) { return _api.GetCachedEntity(id).ClaimID != 0; }

            public int PetID(int id) => _api.GetCachedEntity(id).PetIndex;

            /// <summary>
            /// Checks to see if the object is rendered.
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            /// Author: SG1234567
            /// https://github.com/SG1234567
            public bool IsRendered(int id)
            {
                return (_api.GetCachedEntity(id).Render0000 & 0x200) == 0x200;
            }

            public string Name(int id) { return _api.GetCachedEntity(id).Name; }

            public NpcType NPCType(int id)
            {
                var entity = _api.GetCachedEntity(id);
                return Helpers.GetNpcType(entity);
            }

            public float PosX(int id) { return _api.GetCachedEntity(id).X; }

            public float PosY(int id) { return _api.GetCachedEntity(id).Y; }

            public float PosZ(int id) { return _api.GetCachedEntity(id).Z; }

            public Status Status(int id)
            {
                var status = (EntityStatus)_api.GetCachedEntity(id).Status;
                return Helpers.ToStatus(status);
            }

            public void MenuSequence(string npcName, int[] optionsSequence)
            {
                // Press escape a bunch in case we accidentally opened another menu.
                for(int i = 0; i < 4; i++)
                {
                    _api.ThirdParty.KeyPress(Keys.ESCAPE);
                    Thread.Sleep(500);
                }
                _api.ThirdParty.SendString("/targetnpc");

                Thread.Sleep(2000);

                _api.ThirdParty.KeyPress(Keys.RETURN);

                // Sometimes if menus check KI, then opening the NPC/menu page takes
                // a large delay. So erring on caution and just waiting longer than necessary.
                Thread.Sleep(5000);

                if(_api.Target.GetTargetInfo().TargetName == npcName && _api.Menu.IsMenuOpen)
                {
                    for (int i = 0; i < optionsSequence.Length; i++)
                    {             
                        
                        var option = optionsSequence[i];

                        _api.Menu.MenuIndex = option;
                        Thread.Sleep(2000); 

                        // Very rarely the menu index doesn't get set properly for some reason.
                        // Maybe the game just doesn't register it if it hits a certain timing.
                        // This makes sure if we didn't actually set the index, we retry the same
                        // value again until we do.
                        if(_api.Menu.MenuIndex != option)
                        {
                            i--;
                            continue;
                        }

                        _api.ThirdParty.KeyPress(Keys.RETURN);
                        Thread.Sleep(3000);
                    }
                }
            }

            public void EscapeMenu()
            {
                _api.ThirdParty.KeyPress(Keys.ESCAPE);
            }
        }
     

        public class PartyMemberTools : IPartyMemberTools
        {
            private readonly EliteAPI _api;
            private readonly int _index;

            private EliteAPI.PartyMember Unit
            {
                get
                {
                    var member = _api.Party.GetPartyMember(_index);
                    return member;
                }
            }

            public PartyMemberTools(EliteAPI api, int index)
            {
                _api = api;
                _index = index;
            }

            public bool UnitPresent => Convert.ToBoolean(Unit.Active);

            public int ServerID => (int)Unit.ID;

            public string Name => Unit.Name;

            public int HPCurrent => (int)Unit.CurrentHP;

            public int HPPCurrent => Unit.CurrentHPP;

            public int MPCurrent => (int)Unit.CurrentMP;

            public int MPPCurrent => Unit.CurrentMPP;

            public int TPCurrent => (int)Unit.CurrentTP;

            public Job Job => (Job)Unit.MainJob;

            public Job SubJob => (Job)Unit.SubJob;

            public NpcType NpcType
            {
                get
                {
                    var key = $"PartyMember.NpcType.{_index}";
                    var result = RuntimeCache.Get<NpcType?>(key);

                    if (result == null)
                    {
                        var entity = FindEntityByServerId(ServerID);
                        var npcType = Helpers.GetNpcType(entity);
                        RuntimeCache.Set(key, npcType, DateTimeOffset.Now.AddSeconds(3));
                        return npcType;
                    }

                    return result.Value;
                }
            }

            private EliteAPI.EntityEntry FindEntityByServerId(int serverId)
            {
                return Enumerable.Range(0, 4096)
                    .Select(_api.GetCachedEntity)
                    .FirstOrDefault(x => x.ServerID == serverId);
            }
        }

        public class PlayerTools : IPlayerTools
        {
            private readonly EliteAPI _api;

            public PlayerTools(EliteAPI api)
            {
                _api = api;
            }

            public float CastPercentEx => (_api.CastBar.Percent * 100);

            public int HPPCurrent => (int)_api.Player.HPP;

            public int ID => _api.Player.ServerId;

            public int MPCurrent => (int)_api.Player.MP;

            public int MPPCurrent => (int)_api.Player.MPP;

            public string Name => _api.Player.Name;

            public float Heading => _api.Player.H;

            public Position Position
            {
                get
                {
                    var x = _api.Player.X;
                    var y = _api.Player.Y;
                    var z = _api.Player.Z;
                    var h = _api.Player.H;

                    return Helpers.ToPosition(x, y, z, h);
                }
            }

            public float PosX => Position.X;

            public float PosY => Position.Y;

            public float PosZ => Position.Z;

            public Structures.PlayerStats Stats
            {
                get
                {
                    var stats = _api.Player.Stats;

                    return new Structures.PlayerStats()
                    {
                        Agi = stats.Agility,
                        Chr = stats.Charisma,
                        Dex = stats.Dexterity,
                        Int = stats.Intelligence,
                        Mnd = stats.Mind,
                        Str = stats.Strength,
                        Vit = stats.Vitality
                    };
                }
            }

            public Status Status => Helpers.ToStatus((EntityStatus)_api.Player.Status);

            public StatusEffect[] StatusEffects
            {
                get
                {
                    return _api.Player.Buffs.Select(x => (StatusEffect)x).ToArray();
                }
            }

            public int TPCurrent => (int)_api.Player.TP;

            public Zone Zone => (Zone)_api.Player.ZoneId;

            public Job Job => (Job)_api.Player.MainJob;

            public Job SubJob => (Job)_api.Player.SubJob;

            public bool HasKeyItem(uint id) => _api.Player.HasKeyItem(id);

            public int MeritCount() => MemoryService.ReadMemory<byte>(0x4791E8);
        }

        public class TargetTools : ITargetTools
        {
            private readonly EliteAPI _api;

            public TargetTools(EliteAPI api)
            {
                _api = api;
            }

            public int ID => (int)_api.Target.GetTargetInfo().TargetIndex;

            public bool LockedOn => _api.Target.GetTargetInfo().LockedOn;

            public bool SetNPCTarget(int index)
            {
                return _api.Target.SetTarget(index);
            }
        }

        public class TimerTools : ITimerTools
        {
            private readonly EliteAPI _api;

            public TimerTools(EliteAPI api)
            {
                _api = api;
            }

            public int GetAbilityRecast(int index)
            {
                var ids = _api.Recast.GetAbilityIds();
                var idx = ids.IndexOf(index);
                var reuslt = _api.Recast.GetAbilityRecast(idx);
                return reuslt;
            }

            public int GetSpellRecast(int index)
            {
                return _api.Recast.GetSpellRecast(index);
            }
        }

        public class WindowerTools : IWindowerTools
        {
            private readonly EliteAPI _api;

            public WindowerTools(EliteAPI api)
            {
                _api = api;
            }

            public void SendString(string stringToSend)
            {
                _api.ThirdParty.SendString(stringToSend);
            }

            public void SendKeyPress(Keys key)
            {
                _api.ThirdParty.KeyPress(key);
            }

            public void SendHoldKey(Keys key, int milliseconds)
            {
                _api.ThirdParty.KeyDown(key);
                Thread.Sleep(milliseconds);
                _api.ThirdParty.KeyUp(key);
            }

            public void SendKeyDown(Keys key)
            {
                _api.ThirdParty.KeyDown(key);
            }

            public void SendKeyUp(Keys key)
            {
                _api.ThirdParty.KeyUp(key);
            }
        }

        public class ChatTools : IChatTools
        {
            private static readonly object LockObject = new object();
            private readonly EliteAPI _api;
            public FixedSizeQueue<EliteAPI.ChatEntry> ChatEntries { get; set; } = new FixedSizeQueue<EliteAPI.ChatEntry>(50);

            public ChatTools(EliteAPI api)
            {
                _api = api;
                var timer = new PollingProcessor(QueueChatEntries);
                timer.Start();
            }

            private void QueueChatEntries()
            {
                EliteAPI.ChatEntry chatEntry;
                while ((chatEntry = _api.Chat.GetNextChatLine()) != null)
                {
                    ChatEntries.Enqueue(chatEntry);
                }
            }
        }
    }
}
