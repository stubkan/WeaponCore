﻿using System.Collections.Generic;
using CoreSystems.Support;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;

namespace CoreSystems.Platform
{
    public partial class Weapon
    {
        public class WeaponComponent : CoreComponent
        {
            internal readonly IMyAutomaticRifleGun Rifle;
            internal readonly IMyHandheldGunObject<MyGunBase> GunBase;
            internal readonly IMyLargeTurretBase VanillaTurretBase;
            internal Trigger DefaultTrigger;
            internal readonly ShootManager ShootManager;
            internal readonly WeaponCompData Data;
            internal readonly WeaponStructure Structure;
            internal readonly List<Weapon> Collection;
            internal readonly List<MyEntity> Friends = new List<MyEntity>();
            internal readonly List<MyEntity> Enemies = new List<MyEntity>();
            internal readonly Dictionary<string, Vector3D> Positions = new Dictionary<string, Vector3D>();

            internal readonly int TotalWeapons;
            internal Weapon TrackingWeapon;
            internal int DefaultAmmoId;
            internal int DefaultReloads;
            internal int MaxAmmoCount;
            internal uint LastOwnerRequestTick;
            internal uint LastRayCastTick;
            internal float EffectiveDps;
            internal float PerfectDps;
            internal float PeakDps;
            internal float RealShotsPerSec;
            internal float ShotsPerSec;
            internal float BaseDps;
            internal float AreaDps;
            internal float DetDps;
            internal bool HasEnergyWeapon;
            internal bool HasGuidance;
            internal bool HasDisabledBurst;
            internal bool HasRofSlider;
            internal bool ShootSubmerged;
            internal bool HasTracking;
            internal bool HasRequireTarget;
            internal bool HasDrone;


            internal WeaponComponent(Session session, MyEntity coreEntity, MyDefinitionId id)
            {
                var cube = coreEntity as MyCubeBlock;

                if (cube != null) {

                    var turret = coreEntity as IMyLargeTurretBase;
                    if (turret != null)
                    {
                        VanillaTurretBase = turret;
                        VanillaTurretBase.EnableIdleRotation = false;
                    }
                }
                else {

                    var gun = coreEntity as IMyAutomaticRifleGun;

                    if (gun != null) {
                        Rifle = gun;
                        GunBase = gun;
                        TopEntity = Rifle.Owner;
                    }
                }

                //Bellow order is important
                Data = new WeaponCompData(this);
                ShootManager = new ShootManager(this);
                Init(session, coreEntity, cube != null, Data, id);
                Structure = (WeaponStructure)Platform.Structure;
                Collection = TypeSpecific != CompTypeSpecific.Phantom ? Platform.Weapons : Platform.Phantoms;
                TotalWeapons = Structure.HashToId.Count;
            }

            internal void WeaponInit()
            {
                var wValues = Data.Repo.Values;
                var triggered = wValues.State.Trigger == Trigger.On;
                for (int i = 0; i < Collection.Count; i++)
                {
                    var w = Collection[i];
                    w.UpdatePivotPos();

                    if (Session.IsClient)
                    {
                        w.Target.ClientDirty = true;
                        if (triggered)
                            ShootManager.MakeReadyToShoot(true);
                    }

                    if (w.ProtoWeaponAmmo.CurrentAmmo == 0 && !w.Loading)
                        w.EventTriggerStateChanged(WeaponDefinition.AnimationDef.PartAnimationSetDef.EventTriggers.EmptyOnGameLoad, true);

                    if (TypeSpecific == CompTypeSpecific.Rifle)
                    {
                        Ai.AiOwner = GunBase.OwnerIdentityId;
                        Ai.SmartHandheld = w.System.HasGuidedAmmo;
                    }

                    if (w.TurretAttached) {
                        w.Azimuth = w.System.HomeAzimuth;
                        w.Elevation = w.System.HomeElevation;
                        w.AimBarrel();
                    }



                }
            }

            internal void OnAddedToSceneWeaponTasks(bool firstRun)
            {
                var maxTrajectory1 = 0f;

                var weaponStructure = (WeaponStructure)Platform.Structure;
                if (weaponStructure.MaxLockRange > Ai.Construct.MaxLockRange)
                    Ai.Construct.MaxLockRange = weaponStructure.MaxLockRange;

                if (firstRun && TypeSpecific == CompTypeSpecific.Phantom)
                    Ai.AiOwner = CustomIdentity;
                
                for (int i = 0; i < Collection.Count; i++)
                {
                    var w = Collection[i];
                    w.MasterComp = null;
                    w.RotorTurretTracking = false;

                    if (Session.IsServer)
                        w.ChangeActiveAmmoServer();
                    else
                    {
                        w.ChangeActiveAmmoClient();
                        w.AmmoName = w.ActiveAmmoDef.AmmoName;
                    }

                    if (w.ActiveAmmoDef.AmmoDef == null || !w.ActiveAmmoDef.AmmoDef.Const.IsTurretSelectable && w.System.AmmoTypes.Length > 1)
                    {
                        Platform.PlatformCrash(this, false, true, $"[{w.System.PartName}] Your first ammoType is broken (isNull:{w.ActiveAmmoDef.AmmoDef == null}), I am crashing now Dave.");
                        return;
                    }

                    w.UpdateWeaponRange();
                    if (maxTrajectory1 < w.MaxTargetDistance)
                        maxTrajectory1 = (float)w.MaxTargetDistance;

                }

                if (Data.Repo.Values.Set.Range <= 0)
                    Data.Repo.Values.Set.Range = maxTrajectory1;

                var maxTrajectory2 = 0d;

                for (int i = 0; i < Collection.Count; i++)
                {

                    var weapon = Collection[i];
                    weapon.InitTracking();

                    double weaponMaxRange;
                    DpsAndHeatInit(weapon, out weaponMaxRange);

                    if (maxTrajectory2 < weaponMaxRange)
                        maxTrajectory2 = weaponMaxRange;

                    if (weapon.ProtoWeaponAmmo.CurrentAmmo > weapon.ActiveAmmoDef.AmmoDef.Const.MaxAmmo)
                        weapon.ProtoWeaponAmmo.CurrentAmmo = weapon.ActiveAmmoDef.AmmoDef.Const.MaxAmmo;

                    if (Session.IsServer && weapon.System.HasRequiresTarget)
                        Session.AcqManager.Monitor(weapon.Acquire);
                }

                var expandedMaxTrajectory2 = maxTrajectory2 + Ai.TopEntity.PositionComp.LocalVolume.Radius;
                if (expandedMaxTrajectory2 > Ai.MaxTargetingRange)
                {

                    Ai.MaxTargetingRange = MathHelperD.Min(expandedMaxTrajectory2, Session.Settings.Enforcement.MaxHudFocusDistance);
                    Ai.MaxTargetingRangeSqr = Ai.MaxTargetingRange * Ai.MaxTargetingRange;
                }

                Ai.OptimalDps += PeakDps;
                Ai.EffectiveDps += EffectiveDps;
                Ai.PerfectDps += PerfectDps;

                VanillaTurretBase?.SetTarget(Vector3D.MaxValue);

                if (firstRun)
                    WeaponInit();
            }

            private void DpsAndHeatInit(Weapon weapon, out double maxTrajectory)
            {
                MaxHeat += weapon.System.MaxHeat;

                weapon.RateOfFire = (int)(weapon.System.WConst.RateOfFire * Data.Repo.Values.Set.RofModifier);
                weapon.BarrelSpinRate = (int)(weapon.System.BarrelSpinRate * Data.Repo.Values.Set.RofModifier);
                HeatSinkRate += weapon.HsRate * 3f;

                if (weapon.System.HasBarrelRotation) weapon.UpdateBarrelRotation();

                if (weapon.RateOfFire < 1)
                    weapon.RateOfFire = 1;

                weapon.SetWeaponDps();

                if (!weapon.System.DesignatorWeapon)
                {
                    
                    if (!weapon.System.DesignatorWeapon)
                    {
                        var ammo = weapon.ActiveAmmoDef.AmmoDef;
                        weapon.Comp.PeakDps += ammo.Const.PeakDps;
                        weapon.Comp.EffectiveDps += ammo.Const.EffectiveDps;
                        weapon.Comp.PerfectDps += ammo.Const.PerfectDps;
                        weapon.Comp.RealShotsPerSec += ammo.Const.RealShotsPerSec;
                        weapon.Comp.ShotsPerSec += ammo.Const.ShotsPerSec;
                        weapon.Comp.BaseDps += ammo.Const.BaseDps;
                        weapon.Comp.AreaDps += ammo.Const.AreaDps;
                        weapon.Comp.DetDps += ammo.Const.DetDps;
                    }
                    
                }

                maxTrajectory = 0;
                if (weapon.ActiveAmmoDef.AmmoDef.Const.MaxTrajectory > maxTrajectory)
                    maxTrajectory = weapon.ActiveAmmoDef.AmmoDef.Const.MaxTrajectory;

                if (weapon.System.TrackProjectile)
                    Ai.PointDefense = true;
            }

            internal bool SequenceReady(Ai.Constructs rootConstruct)
            {
                var wValues = Data.Repo.Values;
                var overrides = wValues.Set.Overrides;

                Ai.WeaponGroup group;
                Ai.WeaponSequence sequence;

                if (rootConstruct.WeaponGroups.TryGetValue(overrides.WeaponGroupId, out group) && group.OrderSequencesIds[group.SequenceStep] == overrides.SequenceId && group.Sequences.TryGetValue(overrides.SequenceId, out sequence))
                {
                    if (ShootManager.LastShootTick > ShootManager.PrevShootEventTick)
                    {
                        ShootManager.PrevShootEventTick = ShootManager.LastShootTick;
                        ++sequence.ShotsFired;
                    }

                    if (sequence.ShotsFired < overrides.BurstCount)
                        return true;

                    if (Session.Tick - ShootManager.PrevShootEventTick > overrides.BurstDelay && ++sequence.WeaponsFinished >= sequence.TotalWeapons)
                    {
                        if (++group.SequenceStep >= group.Sequences.Count)
                            group.SequenceStep = 0;

                        sequence.WeaponsFinished = 0;
                        sequence.ShotsFired = 0;

                        if (overrides.ShootMode == ShootManager.ShootModes.AiShoot)
                            ShootManager.ShootDelay = overrides.BurstDelay;
                    }

                    return false;
                }

                return false;
            }

            internal bool AllWeaponsOutOfAmmo()
            {
                var wCount = Collection.Count;
                var outCount = 0;

                for (int i = 0; i < wCount; i++) {
                    var w = Collection[i];
                    if (w.Reload.CurrentMags == 0 && w.ProtoWeaponAmmo.CurrentAmmo == 0)
                        ++outCount;
                }
                return outCount == wCount;
            }

            internal static void SetRange(Weapon.WeaponComponent comp)
            {
                foreach (var w in comp.Collection)
                {
                    w.UpdateWeaponRange();
                }
            }

            internal static void SetRof(WeaponComponent comp)
            {
                for (int i = 0; i < comp.Collection.Count; i++)
                {
                    var w = comp.Collection[i];

                    w.UpdateRof();
                }

                SetDps(comp);
            }

            internal static void SetDps(WeaponComponent comp, bool change = false)
            {
                if (comp == null || comp.Platform.State != CorePlatform.PlatformState.Ready) return;

                for (int i = 0; i < comp.Collection.Count; i++)
                {
                    var w = comp.Collection[i];
                    comp.Session.FutureEvents.Schedule(w.SetWeaponDps, null, 1);
                }
            }

            internal void ResetShootState(Trigger action, long playerId)
            {

                Data.Repo.Values.State.Control = ProtoWeaponState.ControlMode.Ui;

                var cycleShootOn = Data.Repo.Values.State.Trigger == Trigger.On && action == Trigger.On;
                Data.Repo.Values.State.Trigger = cycleShootOn ? Trigger.Off : action;
                Data.Repo.Values.State.PlayerId = playerId;

                if (Session.MpActive && Session.IsServer)
                    Session.SendState(this);
            }

            internal void DetectStateChanges()
            {
                if (Platform.State != CorePlatform.PlatformState.Ready)
                    return;

                if (Session.Tick - Ai.LastDetectEvent > 59)
                {
                    Ai.LastDetectEvent = Session.Tick;
                    Ai.SleepingComps = 0;
                    Ai.AwakeComps = 0;
                    Ai.DetectOtherSignals = false;
                }

                if (IsBlock && !HasAim)
                {
                    var distSqr = Vector3.DistanceSquared(Cube.PositionComp.LocalAABB.Center, Ai.TopEntity.PositionComp.LocalAABB.Center);
                    if (distSqr < Ai.ClosestFixedWeaponCompSqr)
                    {
                        Ai.ClosestFixedWeaponCompSqr = distSqr;
                        Ai.RootFixedWeaponComp = this;
                    }
                }

                ActivePlayer = Ai.Construct.RootAi.Construct.ControllingPlayers.ContainsKey(Data.Repo.Values.State.PlayerId);

                UpdatedState = true;

                var overRides = Data.Repo.Values.Set.Overrides;
                var attackNeutrals = overRides.Neutrals;
                var attackNoOwner = overRides.Unowned;
                var attackFriends = overRides.Friendly;
                var targetNonThreats = (attackNoOwner || attackNeutrals || attackFriends);

                DetectOtherSignals = targetNonThreats;
                if (DetectOtherSignals)
                    Ai.DetectOtherSignals = true;
                var wasAsleep = IsAsleep;
                IsAsleep = false;
                IsDisabled = Ai.TouchingWater && !ShootSubmerged && Ai.WaterVolume.Contains(CoreEntity.PositionComp.WorldAABB.Center) != ContainmentType.Disjoint;
                
                if (!Ai.Session.IsServer)
                    return;

                var otherRangeSqr = Ai.DetectionInfo.OtherRangeSqr;
                var threatRangeSqr = Ai.DetectionInfo.PriorityRangeSqr;
                var targetInrange = DetectOtherSignals ? otherRangeSqr <= MaxDetectDistanceSqr && otherRangeSqr >= MinDetectDistanceSqr || threatRangeSqr <= MaxDetectDistanceSqr && threatRangeSqr >= MinDetectDistanceSqr : threatRangeSqr <= MaxDetectDistanceSqr && threatRangeSqr >= MinDetectDistanceSqr;
                if (Ai.Session.Settings.Enforcement.ServerSleepSupport && !targetInrange && PartTracking == 0 && Ai.Construct.RootAi.Construct.ControllingPlayers.Count <= 0 && Session.TerminalMon.Comp != this && Data.Repo.Values.State.Trigger == Trigger.Off)
                {

                    IsAsleep = true;
                    Ai.SleepingComps++;
                }
                else if (wasAsleep)
                {

                    Ai.AwakeComps++;
                }
                else
                    Ai.AwakeComps++;
            }

            internal static void RequestCountDown(WeaponComponent comp, bool value)
            {
                if (value != comp.Data.Repo.Values.State.CountingDown)
                {
                    comp.Session.SendCountingDownUpdate(comp, value);
                }
            }

            internal static void RequestCriticalReaction(WeaponComponent comp)
            {
                if (true != comp.Data.Repo.Values.State.CriticalReaction)
                {
                    comp.Session.SendTriggerCriticalReaction(comp);
                }
            }

            internal static void RequestSetValue(WeaponComponent comp, string setting, int value, long playerId)
            {
                if (comp.Session.IsServer)
                {
                    SetValue(comp, setting, value, playerId);
                }
                else if (comp.Session.IsClient)
                {
                    comp.Session.SendOverRidesClientComp(comp, setting, value);
                }
            }

            internal static void SetValue(WeaponComponent comp, string setting, int v, long playerId)
            {
                var o = comp.Data.Repo.Values.Set.Overrides;
                var enabled = v > 0;
                var clearTargets = false;
                var resetState = false;
                switch (setting)
                {
                    case "MaxSize":
                        o.MaxSize = v;
                        clearTargets = true;
                        break;
                    case "MinSize":
                        o.MinSize = v;
                        clearTargets = true;
                        break;
                    case "SubSystems":
                        o.SubSystem = (WeaponDefinition.TargetingDef.BlockTypes)v;
                        clearTargets = true;
                        break;
                    case "MovementModes":
                        o.MoveMode = (ProtoWeaponOverrides.MoveModes)v;
                        clearTargets = true;
                        break;
                    case "ControlModes":
                        o.Control = (ProtoWeaponOverrides.ControlModes)v;
                        clearTargets = true;
                        resetState = true;
                        break;
                    case "FocusSubSystem":
                        o.FocusSubSystem = enabled;
                        break;
                    case "FocusTargets":
                        o.FocusTargets = enabled;
                        clearTargets = true;
                        break;
                    case "Unowned":
                        o.Unowned = enabled;
                        clearTargets = true;
                        break;
                    case "Friendly":
                        o.Friendly = enabled;
                        clearTargets = true;
                        break;
                    case "Meteors":
                        o.Meteors = enabled;
                        break;
                    case "Grids":
                        o.Grids = enabled;
                        clearTargets = true;
                        break;
                    case "Biologicals":
                        o.Biologicals = enabled;
                        clearTargets = true;
                        break;
                    case "Projectiles":
                        o.Projectiles = enabled;
                        clearTargets = true;
                        break;
                    case "Neutrals":
                        o.Neutrals = enabled;
                        clearTargets = true;
                        break;
                    case "Repel":
                        o.Repel = enabled;
                        clearTargets = true;
                        break;
                    case "CameraChannel":
                        o.CameraChannel = v;
                        break;
                    case "BurstCount":
                        o.BurstCount = v;
                        break;
                    case "BurstDelay":
                        o.BurstDelay = v;
                        break;
                    case "SequenceId":

                        if (o.SequenceId != v)
                            comp.ChangeSequenceId();

                        o.SequenceId = v;
                        break;
                    case "WeaponGroupId":

                        if (o.WeaponGroupId != v)
                            comp.ChangeWeaponGroup(v);

                        o.WeaponGroupId = v;
                        break;
                    case "ShootMode":
                        o.ShootMode = (ShootManager.ShootModes)v;
                        break;
                    case "LeadGroup":
                        o.LeadGroup = v;
                        break;
                    case "ArmedTimer":
                        o.ArmedTimer = v;
                        break;
                    case "Armed":
                        o.Armed = enabled;
                        break;
                    case "Debug":
                        o.Debug = enabled;
                        break;
                    case "Override":
                        o.Override = enabled;
                        break;
                }

                ResetCompState(comp, playerId, clearTargets, resetState);

                if (comp.Session.MpActive)
                {
                    comp.Data.Repo.Values.State.PlayerId = playerId;
                    comp.Session.SendComp(comp);
                }
            }

            internal static void ResetCompState(WeaponComponent comp, long playerId, bool resetTarget, bool resetState, Dictionary<string, int> settings = null)
            {
                var wValues = comp.Data.Repo.Values;

                var o = wValues.Set.Overrides;
                var userControl = o.Control != ProtoWeaponOverrides.ControlModes.Auto;

                if (userControl)
                {
                    wValues.State.Trigger = Trigger.Off;
                    if (settings != null) settings["ControlModes"] = (int)o.Control;
                }

                wValues.State.Control = ProtoWeaponState.ControlMode.Ui;

                comp.ManualMode = wValues.State.TrackingReticle && wValues.Set.Overrides.Control == ProtoWeaponOverrides.ControlModes.Manual; // needs to be set everywhere dedicated and non-tracking clients receive TrackingReticle or Control updates.

                if (resetTarget)
                    ClearTargets(comp);
            }

            private static void ClearTargets(WeaponComponent comp)
            {
                for (int i = 0; i < comp.Collection.Count; i++)
                {
                    var weapon = comp.Collection[i];
                    if (weapon.Target.HasTarget)
                        comp.Collection[i].Target.Reset(comp.Session.Tick, Target.States.ControlReset);
                }
            }

            internal void ChangeWeaponGroup(int newGroup)
            {
                var oldGroup = Data.Repo.Values.Set.Overrides.WeaponGroupId;

                if (oldGroup != newGroup) {

                    if (newGroup > 0)
                        Ai.CompWeaponGroups[this] = newGroup;
                    else 
                        Ai.CompWeaponGroups.Remove(this);
                }

                Ai.Construct.RootAi.Construct.DirtyWeaponGroups = true;
            }

            internal void ChangeSequenceId()
            {
                Ai.Construct.RootAi.Construct.DirtyWeaponGroups = true;
            }


            internal void NotFunctional()
            {
                for (int i = 0; i < Collection.Count; i++)
                {

                    var w = Collection[i];
                    PartAnimation[] partArray;
                    if (w.AnimationsSet.TryGetValue(WeaponDefinition.AnimationDef.PartAnimationSetDef.EventTriggers.TurnOff, out partArray))
                    {
                        for (int j = 0; j < partArray.Length; j++)
                            w.PlayEmissives(partArray[j]);
                    }
                    if (!Session.IsClient && !IsWorking)
                        w.Target.Reset(Session.Tick, Target.States.Offline);
                }
            }

            internal void PowerLoss()
            {
                Session.SendComp(this);
                if (IsWorking)
                {
                    foreach (var w in Collection)
                    {
                        Session.SendWeaponAmmoData(w);
                        Session.SendWeaponReload(w);
                    }
                }
            }

            internal bool TakeOwnerShip()
            {
                if (LastOwnerRequestTick > 0 && Session.Tick - LastOwnerRequestTick < 120)
                    return true;

                LastOwnerRequestTick = Session.Tick;
                if (Session.IsClient)
                {
                    Session.SendPlayerControlRequest(this, Session.PlayerId, ProtoWeaponState.ControlMode.Ui);
                    return true;
                }
                Data.Repo.Values.State.PlayerId = Session.PlayerId;
                
                if (Session.MpActive)
                    Session.SendState(this);

                return true;
            }

            internal void RequestFriend()
            {
                if (Session.TargetUi.SelectedEntity != null)
                {
                    AssignFriend(Session.TargetUi.SelectedEntity);
                }
                else 
                    ClearFriend();
            }

            internal void RequestEnemy()
            {
                var rootAi = Ai.Construct.RootAi;
                if (Session.TargetUi.SelectedEntity != null || rootAi.Construct.Focus.GetPriorityTarget(rootAi, out Session.TargetUi.SelectedEntity))
                {
                    AssignEnemy(Session.TargetUi.SelectedEntity);
                }
                else
                    ClearEnemy();
            }

            internal void RequestPoint()
            {

            }


            internal void AssignFriend(MyEntity entity)
            {
                if (Ai.Targets.ContainsKey(entity))
                    return;
                var tasks = Data.Repo.Values.State.Tasks;
                tasks.FriendId = entity.EntityId;
                tasks.Task = ProtoWeaponCompTasks.Tasks.Defend;
                tasks.Update(this);

                Friends.Clear();
                Friends.Add(entity);

                SendTargetNotice($"Friend {entity.DisplayName} assigned to {Collection[0].System.ShortName}");
            }

            internal void ClearFriend()
            {
                var tasks = Data.Repo.Values.State.Tasks;
                tasks.FriendId = 0;
                tasks.Task = ProtoWeaponCompTasks.Tasks.None;

                if (Friends.Count > 0)
                    SendTargetNotice($"Friend {Friends[0].DisplayName} unassigned from {Collection[0].System.ShortName}");

                Friends.Clear();
            }


            internal void AssignEnemy(MyEntity entity)
            {
                if (!Ai.Targets.ContainsKey(entity))
                    return;

                var tasks = Data.Repo.Values.State.Tasks;
                tasks.EnemyId = entity.EntityId;
                tasks.Task = ProtoWeaponCompTasks.Tasks.Attack;
                tasks.Update(this);

                Enemies.Clear();
                Enemies.Add(entity);

                SendTargetNotice($"Enemy {entity.DisplayName} assigned to {Collection[0].System.ShortName}");

            }

            internal void ClearEnemy()
            {
                var tasks = Data.Repo.Values.State.Tasks;
                tasks.EnemyId = 0;
                tasks.Task = ProtoWeaponCompTasks.Tasks.None;

                if (Enemies.Count > 0)
                    SendTargetNotice($"Friend {Enemies[0].DisplayName} unassigned from {Collection[0].System.ShortName}");

                Enemies.Clear();

            }

            internal void AddPoint(string name, Vector3D position)
            {
                Positions[name] = position;
            }

            internal void RemovePoint(string name)
            {
                Positions.Remove(name);
            }

            internal void RequestForceReload()
            {
                foreach (var w in Collection)
                {
                    w.ProtoWeaponAmmo.CurrentAmmo = 0;
                    w.ProtoWeaponAmmo.CurrentCharge = 0;
                    w.ClientMakeUpShots = 0;

                    if (Session.MpActive && Session.IsServer)
                        Session.SendWeaponAmmoData(w);
                }

                if (Session.IsClient)
                    Session.RequestToggle(this, PacketType.ForceReload);
            }

            private void SendTargetNotice(string message)
            {
                if (Session.TargetUi.LastTargetNoticeTick != Session.Tick && Session.Tick - Session.TargetUi.LastTargetNoticeTick > 30)
                {
                    Session.ShowLocalNotify(message, 2000, "White");
                    Session.TargetUi.LastTargetNoticeTick = Session.Tick;
                }
            }

            public enum AmmoStates
            {
                Empty,
                Makeup,
                Ammo,
                Mixed,

            }
            internal AmmoStates AmmoStatus()
            {
                var ammo = 0;
                var makeUp = 0;
                for (int i = 0; i < Collection.Count; i++)
                {
                    var w = Collection[i];
                    ammo += w.ProtoWeaponAmmo.CurrentCharge > 0 ? 1 : w.ProtoWeaponAmmo.CurrentAmmo;
                    makeUp += w.ClientMakeUpShots;
                }

                var status = ammo > 0 && makeUp > 0 ? AmmoStates.Mixed : ammo > 0 ? AmmoStates.Ammo : makeUp > 0 ? AmmoStates.Makeup : AmmoStates.Empty;
                return status;
            }


            internal void GeneralWeaponCleanUp()
            {
                if (Platform?.State == CorePlatform.PlatformState.Ready)
                {
                    foreach (var w in Collection)
                    {

                        w.RayCallBackClean();

                        w.Comp.Session.AcqManager.Asleep.Remove(w.Acquire);
                        w.Comp.Session.AcqManager.MonitorState.Remove(w.Acquire);
                        w.Acquire.Monitoring = false;
                        w.Acquire.IsSleeping = false;

                    }
                }
            }

            public void CleanCompParticles()
            {
                if (Platform?.State == CorePlatform.PlatformState.Ready)
                {
                    foreach (var w in Collection)
                    {
                        for (int i = 0; i < w.System.Values.Assignments.Muzzles.Length; i++)
                        {
                            if (w.HitEffects?[i] != null)
                            {
                                Log.Line($"[Clean CompHitPartice] Weapon:{w.System.PartName} - Particle:{w.HitEffects[i].GetName()}");
                                w.HitEffects[i].Stop();
                                w.HitEffects[i] = null;
                            }
                            if (w.Effects1?[i] != null)
                            {
                                Log.Line($"[Clean Effects1] Weapon:{w.System.PartName} - Particle:{w.Effects1[i].GetName()}");
                                w.Effects1[i].Stop();
                                w.Effects1[i] = null;
                            }
                            if (w.Effects2?[i] != null)
                            {
                                Log.Line($"[Clean Effects2] Weapon:{w.System.PartName} - Particle:{ w.Effects2[i].GetName()}");
                                w.Effects2[i].Stop();
                                w.Effects2[i] = null;
                            }
                        }
                    }
                }
            }

            public void CleanCompSounds()
            {
                if (Platform?.State == CorePlatform.PlatformState.Ready)
                {

                    foreach (var w in Collection)
                    {

                        if (w.AvCapable && w.System.FiringSound == WeaponSystem.FiringSoundState.WhenDone)
                            Session.SoundsToClean.Add(new Session.CleanSound { Force = true, Emitter = w.FiringEmitter, EmitterPool = Session.Emitters, SpawnTick = Session.Tick });

                        if (w.AvCapable && w.System.PreFireSound)
                            Session.SoundsToClean.Add(new Session.CleanSound { Force = true, Emitter = w.PreFiringEmitter, EmitterPool = Session.Emitters, SpawnTick = Session.Tick });

                        if (w.AvCapable && w.System.WeaponReloadSound)
                            Session.SoundsToClean.Add(new Session.CleanSound { Force = true, Emitter = w.ReloadEmitter, EmitterPool = Session.Emitters, SpawnTick = Session.Tick });

                        if (w.AvCapable && w.System.BarrelRotateSound)
                            Session.SoundsToClean.Add(new Session.CleanSound { Emitter = w.BarrelRotateEmitter, EmitterPool = Session.Emitters, SpawnTick = Session.Tick });
                    }

                    if (Session.PurgedAll)
                    {
                        Session.CleanSounds();
                        Log.Line("purged already called");
                    }
                }
            }

            public void StopAllSounds()
            {
                foreach (var w in Collection)
                {
                    w.StopReloadSound();
                    w.StopBarrelRotateSound();
                    w.StopShootingAv(false);
                    //w.IsShooting = false;
                }
            }

            internal void TookControl(long playerId)
            {
                if (Session.IsServer)
                {

                    if (Data.Repo != null)
                    {
                        Data.Repo.Values.State.PlayerId = playerId;
                        Data.Repo.Values.State.Control = ProtoWeaponState.ControlMode.Camera;

                        if (Session.MpActive)
                            Session.SendComp(this);
                    }
                    else
                        Log.Line($"OnPlayerController enter Repo null");

                }

                if (Session.HandlesInput)
                    Session.GunnerAcquire(Cube);
            }

            internal void ReleaseControl(long playerId)
            {
                if (Session.IsServer)
                {

                    if (Data.Repo != null)
                    {
                        Data.Repo.Values.State.Control = ProtoWeaponState.ControlMode.Ui;

                        if (Session.MpActive)
                            Session.SendComp(this);
                    }
                    else
                        Log.Line($"OnPlayerController exit Repo null");
                }

                if (Session.HandlesInput)
                {
                    Session.GunnerRelease(Cube);
                }
            }
        }
    }
}
