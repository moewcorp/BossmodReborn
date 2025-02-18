﻿using BossMod.Global.DeepDungeon;

namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos;

public enum AID : uint
{
    BigBurst = 32381, // 3DCA->self, 3.5s cast, range 6 circle
    TerrorTouch = 32389, // 3DD0->player, 4.0s cast, single-target
    Diamondback = 32431, // 3DEC->self, 4.0s cast, single-target, permanent defense up to self
    Tailwind = 33167, // 3DF6->enemy, 3.0s cast, single-target, damage up to ally
    SelfDetonate = 32410, // 3DE0->self, 10.0s cast, range 40 circle, enrage
    Electromagnetism = 32413, // 3DE1->self, 5.0s cast, range 15 circle
    Headspin = 32412, // 3DE1->self, 0.0s cast, range 6 circle, instant after Electromagnetism
    DoubleHexEye = 32437, // 3DF1->self, 4.0s cast, range 40 circle, instakill mechanic
    Bombination = 32424, // 3DE9->self, 2.0s cast, range 25 circle
    Bombination2 = 32461, // 3DFF->self, 2.0s cast, range 25 circle
    TheDragonsVoice = 32444, // 3DF4->self, 4.0s cast, range ?-30 donut
    Quake = 32470, // 3E02->self, 5.0s cast, range 30 circle, interruptible
    Explosion = 32452, // 3DFA->self, 7.0s cast, range 60 circle
    Infatuation = 32798, // 3E80->player, 3.0s cast, single-target
    AbyssalCry = 32467, // 3E00->self, 6.0s cast, range 30 circle, instakill mechanic
    SprigganHaste = 33175, // 3DFB->self, 1.5s cast
    GelidCharge = 33180, // 3E13->self, 2.0s cast, single-target
    SmolderingScales = 32952, // 3E3D->self, 2.5s cast, single-target
    ElectricCachexia = 32979, // 3E45->self, 4.0s cast, range ?-44 donut
    EyeOfTheFierce = 32667, // 3E53->self, 3.0s cast, range 40 circle
    DemonEye1 = 32762, // 3E5E->self, 5.0s cast, range 33 circle
    DemonEye2 = 32761, // 3E5E->self, 5.0s cast, range 33 circle
    Hypnotize = 32737, // 3E58->self, 3.0s cast, range 20 circle
    Catharsis = 32732, // 3E56->self, 10.0s cast, range 40 circle
    ElectricWhorl = 33186, // 3E09->self, 4.0s cast, range 60 circle
    HexEye = 32731, // 3E56->self, 3.0s cast, range 5 circle
    EclipticMeteor = 33043, // 3DC9->self, 12.0s cast, range 50 circle
    TheDragonsVoice2 = 32910,
    AllaganFear = 32896,
    HighVoltage = 32878, // 3E64->self, 7.5s cast, range 30 circle
    AquaSpear = 32500, // 3E12->self, 5.5s cast, width 5 charge, must be LOSed
    Sucker = 32477, // 3E08->self, 3.5s cast, range 25 circle, draw-in
    WaterIII = 32478, // 3E08->self, 2.5s cast, range 8 circle, used after sucker
    RipeBanana = 33195, // 3E52->self, 2s cast, single-target, damage buff
    ChestThump = 32680, // 3E52->self, 2s cast, range 52 circle, used after banana

    KillingPaw = 33193, // 3E50->self, 3.0s cast, range 9 120-degree cone
    SavageSwipe = 32654, // 3E50->self, instant cast, range 6 120-degree cone

    SewerWaterCastFront = 32491, // 3E0F->self, 3.0s cast, range 12 180-degree cone
    SewerWaterCastBack = 32492, // 3E0F->self, 3.0s cast, range 12 180-degree cone
    SewerWaterInstantFront = 32493, // 3E0F->self, instant cast, range 12 180-degree cone
    SewerWaterInstantBack = 32494, // 3E0F->self, instant cast, range 12 180-degree cone

    GoobInhale = 33178, // 3E04->self, instant cast, range 40 90-degree cone
    GoobSneeze = 32473, // 3E04->self, 1.0s cast, range 7 90-degree cone

    GourmInhale = 32748, // 3E5B->self, instant cast, range 40 90-degree cone
    GourmSneeze = 32749 // 3E5B->self, 1.0s cast, range 6 90-degree cone
}

public enum SID : uint
{
    BlazeSpikes = 197,
    IceSpikes = 198
}

public abstract class EOFloorModule(WorldState ws, bool autoRaiseOnEnter = false) : AutoClear(ws, 90)
{
    protected override void OnCastStarted(Actor actor)
    {
        switch (actor.CastInfo!.Action.ID)
        {
            // stunnable actions, either self buffs or large point blank AOEs that prevent going into melee range
            case (uint)AID.BigBurst:
            case (uint)AID.Tailwind:
            case (uint)AID.SprigganHaste:
                Stuns.Add(actor);
                break;

            // interruptible casts
            case (uint)AID.TerrorTouch:
            case (uint)AID.Diamondback:
                Interrupts.Add(actor);
                break;
            case (uint)AID.Infatuation:
                Interrupts.Add(actor);
                if (Palace.Floor < 60)
                    Stuns.Add(actor);
                break;

            // gazes
            case (uint)AID.DoubleHexEye:
            case (uint)AID.EyeOfTheFierce:
                AddGaze(actor, 40f);
                break;
            case (uint)AID.AllaganFear:
                AddGaze(actor, 30f);
                break;
            case (uint)AID.Hypnotize:
                AddGaze(actor, 20f);
                HintDisabled.Add(actor);
                break;
            case (uint)AID.DemonEye1:
            case (uint)AID.DemonEye2:
                AddGaze(actor, 33f);
                break;
            case (uint)AID.HexEye:
                AddGaze(actor, 5f);
                break;

            // donut AOEs
            case (uint)AID.TheDragonsVoice:
            case (uint)AID.TheDragonsVoice2:
                Donuts.Add((actor, 8f, 30f));
                HintDisabled.Add(actor);
                break;
            case (uint)AID.ElectricCachexia:
                Donuts.Add((actor, 8f, 44f));
                HintDisabled.Add(actor);
                break;
            case (uint)AID.ElectricWhorl:
                Donuts.Add((actor, 8f, 60f));
                HintDisabled.Add(actor);
                break;

            // very large circle AOEs that trigger autohints' "this is a raidwide" check but are actually avoidable
            case (uint)AID.Catharsis:
                Circles.Add((actor, 40f));
                break;

            // LOS attacks
            case (uint)AID.Quake:
            case (uint)AID.HighVoltage:
                Interrupts.Add(actor);
                AddLOS(actor, 30f);
                break;
            case (uint)AID.EclipticMeteor:
            case (uint)AID.AquaSpear:
                AddLOS(actor, 50f);
                break;
            case (uint)AID.Explosion:
                AddLOS(actor, 60f);
                break;
            case (uint)AID.AbyssalCry:
                AddLOS(actor, 30f);
                break;
            case (uint)AID.SelfDetonate:
                AddLOS(actor, 40f);
                break;

            // knockbacks (can be ignored on kb penalty floors or if arms length is up)
            case (uint)AID.Electromagnetism:
                KnockbackZones.Add((actor, 15f));
                HintDisabled.Add(actor);
                break;
            case (uint)AID.Sucker:
                KnockbackZones.Add((actor, 25f));
                HintDisabled.Add(actor);
                break;

            // large out of combat AOEs that are fast and generally nonthreatening, we want to ignore these so we don't interfere with pathfinding
            case (uint)AID.Bombination:
            case (uint)AID.Bombination2:
                HintDisabled.Add(actor);
                break;
        }
    }

    protected override void OnCastFinished(Actor actor)
    {
        switch (actor.CastInfo!.Action.ID)
        {
            // setting target to forbidden when it gains the spikes status is too late
            case (uint)AID.GelidCharge:
            case (uint)AID.SmolderingScales:
                Spikes.Add((actor, World.FutureTime(10d)));
                break;
        }
    }

    protected override void OnEventCast(Actor actor, ActorCastEvent ev)
    {
        switch (ev.Action.ID)
        {
            case (uint)AID.GoobInhale:
                Voidzones.Add((actor, new AOEShapeCone(7f, 45f.Degrees())));
                break;
            case (uint)AID.GourmInhale:
                Voidzones.Add((actor, new AOEShapeCone(6f, 45f.Degrees())));
                break;
            case (uint)AID.KillingPaw:
                Voidzones.Add((actor, new AOEShapeCone(6f, 60f.Degrees())));
                break;
            case (uint)AID.SewerWaterCastFront:
                Voidzones.Add((actor, new AOEShapeCone(12f, 90f.Degrees(), 180f.Degrees())));
                break;
            case (uint)AID.SewerWaterCastBack:
                Voidzones.Add((actor, new AOEShapeCone(12f, 90f.Degrees())));
                break;
            case (uint)AID.Electromagnetism:
                Voidzones.Add((actor, new AOEShapeCircle(6)));
                break;
            case (uint)AID.RipeBanana:
                Voidzones.Add((actor, new AOEShapeCircle(52)));
                break;

            case (uint)AID.GoobSneeze:
            case (uint)AID.GourmSneeze:
            case (uint)AID.Headspin:
            case (uint)AID.SavageSwipe:
            case (uint)AID.SewerWaterInstantFront:
            case (uint)AID.SewerWaterInstantBack:
            case (uint)AID.ChestThump:
                var count = Voidzones.Count;
                for (var i = 0; i < count; ++i)
                {
                    var vz = Voidzones[i];
                    if (vz.Source == actor)
                    {
                        Voidzones.Remove(vz);
                        break;
                    }
                }
                break;
        }
    }

    protected override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.IceSpikes:
            case (uint)SID.BlazeSpikes:
                var count = Spikes.Count;
                for (var i = 0; i < count; ++i)
                {
                    var spike = Spikes[i];
                    if (spike.Actor == actor)
                    {
                        Spikes.Remove(spike);
                        break;
                    }
                }
                break;
        }
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        base.CalculateAIHints(playerSlot, player, hints);

        if (autoRaiseOnEnter && Palace.Floor % 10 == 1)
        {
            var raising = Palace.GetPomanderState(PomanderID.ProtoRaising);
            if (!raising.Active && raising.Count > 0)
                hints.ActionsToExecute.Push(new ActionID(ActionType.Pomander, (uint)PomanderID.ProtoRaising), player, ActionQueue.Priority.VeryHigh);
        }
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 897)]
public class EO10(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 898)]
public class EO20(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 899)]
public class EO30(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 900)]
public class EO40(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 901)]
public class EO50(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 902)]
public class EO60(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 903)]
public class EO70(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 904)]
public class EO80(WorldState ws) : EOFloorModule(ws, true);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 905)]
public class EO90(WorldState ws) : EOFloorModule(ws, true);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 906)]
public class EO100(WorldState ws) : EOFloorModule(ws, true);
