﻿namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

class Roar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Roar));

abstract class ColossalImpact(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(61, 10));
class ColossalImpact1(BossModule module) : ColossalImpact(module, AID.ColossalImpact1);
class ColossalImpact2(BossModule module) : ColossalImpact(module, AID.ColossalImpact2);
class ColossalImpact3(BossModule module) : ColossalImpact(module, AID.ColossalImpact3);
class ColossalImpactLeft(BossModule module) : ColossalImpact(module, AID.ColossalImpactLeft);
class ColossalImpactRight(BossModule module) : ColossalImpact(module, AID.ColossalImpactRight);
class ColossalImpactCenter(BossModule module) : ColossalImpact(module, AID.ColossalImpactCenter);

class MagicArtilleryBeta(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.MagicArtilleryBeta), 3);
class MagicArtilleryAlpha(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.MagicArtilleryAlpha), 5);
class LightLeap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LightLeap), 25);
class BoxSpawn(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BoxSpawn), new AOEShapeRect(8, 4));
class MagicBarrage(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MagicBarrage), new AOEShapeRect(61, 2.5f));
class Lunge(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Lunge), 60, stopAtWall: true, kind: Kind.DirForward);
class Energy(BossModule module) : Components.PersistentVoidzone(module, 1, m => m.Enemies(OID.Energy).Where(z => z.EventState != 7));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9955)]
public class A31KnaveofHearts(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, -724.4f), new ArenaBoundsSquare(29.5f));
