﻿namespace BossMod.Dawntrail.Hunt.RankS.CrystalIncarnation;

public enum OID : uint
{
    Boss = 0x4571 // R2.4
}

public enum AID : uint
{
    AutoAttack = 39622, // Boss->player, no cast, single-target

    FireII = 39623, // Boss->location, 5.0s cast, range 5 circle
    BlizzardII = 39624 // Boss->self, 5.0s cast, range 40 45-degree cone
}

sealed class FireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireII, 5f);
sealed class BlizzardII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlizzardII, new AOEShapeCone(40f, 22.5f.Degrees()));

sealed class CrystalIncarnationStates : StateMachineBuilder
{
    public CrystalIncarnationStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<BlizzardII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.SS, NameID = 13407)]
public sealed class CrystalIncarnation(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
