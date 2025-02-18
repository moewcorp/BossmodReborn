﻿namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

abstract class Unenlightenment(BossModule module, AID aid) : Components.CastCounter(module, ActionID.MakeSpell(aid));
class NUnenlightenment(BossModule module) : Unenlightenment(module, AID.NUnenlightenmentAOE);
class SUnenlightenment(BossModule module) : Unenlightenment(module, AID.SUnenlightenmentAOE);

public abstract class C022Gorai(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(300f, -120f);
    public static readonly ArenaBoundsSquare StartingBounds = new(22.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12373, SortOrder = 4, PlanLevel = 90)]
public class C022NGorai(WorldState ws, Actor primary) : C022Gorai(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12373, SortOrder = 4, PlanLevel = 90)]
public class C022SGorai(WorldState ws, Actor primary) : C022Gorai(ws, primary);
