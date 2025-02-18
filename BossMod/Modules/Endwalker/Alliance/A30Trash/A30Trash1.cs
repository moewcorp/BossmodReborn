namespace BossMod.Endwalker.Alliance.A30Trash1;

public enum OID : uint
{
    Serpent = 0x4010, // R3.45
    Triton = 0x4011, // R1.95
    DivineSprite = 0x4012, // R1.6
    WaterSprite = 0x4085 // R0.8
}

public enum AID : uint
{
    AutoAttack = 870, // Serpent/Triton->player, no cast, single-target

    WaterIII = 35438, // Serpent->location, 4.0s cast, range 8 circle
    PelagicCleaver1 = 35439, // Triton->self, 5.0s cast, range 40 60-degree cone
    PelagicCleaver2 = 35852, // Triton->self, 5.0s cast, range 40 60-degree cone
    WaterAutoAttack = 35469, // WaterSprite/DivineSprite->player, no cast, single-target, auto attack
    WaterFlood = 35442, // WaterSprite->self, 3.0s cast, range 6 circle
    WaterBurst = 35443, // WaterSprite->self, no cast, range 40 circle, raidwide when Water Sprite dies
    DivineFlood = 35440, // DivineSprite->self, 3.0s cast, range 6 circle
    DivineBurst = 35441 // DivineSprite->self, no cast, range 40 circle, raidwide when Divine Sprite dies
}

class WaterIII(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WaterIII), 8f);

abstract class PelagicCleaver(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40f, 30f.Degrees()));
class PelagicCleaver1(BossModule module) : PelagicCleaver(module, AID.PelagicCleaver1);
class PelagicCleaver2(BossModule module) : PelagicCleaver(module, AID.PelagicCleaver2);

class PelagicCleaver1Hint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.PelagicCleaver1));
class PelagicCleaver2Hint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.PelagicCleaver2));

class Flood(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6f);
class WaterFlood(BossModule module) : Flood(module, AID.WaterFlood);
class DivineFlood(BossModule module) : Flood(module, AID.DivineFlood);

public class A30Trash1States : StateMachineBuilder
{
    public A30Trash1States(A30Trash1 module) : base(module)
    {
        // as soon as the last serpent dies, other adds are spawned; serpents are destroyed a bit later
        TrivialPhase()
            .ActivateOnEnter<WaterIII>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.Serpent);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
        TrivialPhase(1)
            .ActivateOnEnter<PelagicCleaver1>()
            .ActivateOnEnter<PelagicCleaver2>()
            .ActivateOnEnter<PelagicCleaver1Hint>()
            .ActivateOnEnter<PelagicCleaver2Hint>()
            .ActivateOnEnter<WaterFlood>()
            .ActivateOnEnter<DivineFlood>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(A30Trash1.Trash);
                for (var i = 0; i < enemies.Count; ++i)
                {
                    var e = enemies[i];
                    if (!e.IsDeadOrDestroyed)
                        return false;
                }
                return module.Enemies((uint)OID.Serpent).Count == 0;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", PrimaryActorOID = (uint)OID.Serpent, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12478, SortOrder = 1)]
public class A30Trash1(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800f, -800f), new ArenaBoundsCircle(20f))
{
    public static readonly uint[] Trash = [(uint)OID.Triton, (uint)OID.DivineSprite, (uint)OID.WaterSprite];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Serpent));
        Arena.Actors(Enemies(Trash));
    }
}
