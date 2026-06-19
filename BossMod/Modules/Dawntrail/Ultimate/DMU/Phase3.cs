using Lumina.Extensions;

namespace BossMod.Dawntrail.Ultimate.DMU;

// TODO check knockback point is from the boss
class AeroIIIAssault(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AeroIIIAssault, 15f);

class TheDecisiveBattle(BossModule module) : BossComponent(module) {
    private Actor? chaosBoss;
    private Actor? exDeathBoss;
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (chaosBoss == null || exDeathBoss == null) {
            return;
        }

        var players = Raid.WithoutSlot().SortedByRange(chaosBoss.Position).Take(4).ToList();
        foreach (var player in players) {
            Arena.AddLine(chaosBoss.Position, player.Position, Colors.Danger);
        }

        players = Raid.WithoutSlot().SortedByRange(exDeathBoss.Position).Take(4).ToList();
        foreach (var player in players) {
            Arena.AddLine(exDeathBoss.Position, player.Position, Colors.Danger);
        }

        foreach (var (_, player) in Raid.WithSlot()) {
            Arena.Actor(player, player == pc ? Colors.PC : Colors.PlayerGeneric, true);
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }

        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (assignment == PartyRolesConfig.Assignment.MT) {
            Arena.AddCircle(new(92 - MathF.Sqrt(2), 100 - MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.H1) {
            Arena.AddCircle(new(92 + MathF.Sqrt(2), 100 - MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.M1) {
            Arena.AddCircle(new(92 - MathF.Sqrt(2), 100 + MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.M2) {
            Arena.AddCircle(new(92 + MathF.Sqrt(2), 100 + MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.OT) {
            Arena.AddCircle(new(108 - MathF.Sqrt(2), 100 - MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.H2) {
            Arena.AddCircle(new(108 + MathF.Sqrt(2), 100 - MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.R1) {
            Arena.AddCircle(new(108 - MathF.Sqrt(2), 100 + MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.R2) {
            Arena.AddCircle(new(108 + MathF.Sqrt(2), 100 + MathF.Sqrt(2)), 1, Colors.Safe);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TheDecisiveBattle) {
            chaosBoss = caster;
        }

        if (spell.Action.ID == (uint)AID.TheDecisiveBattle1) {
            exDeathBoss = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TheDecisiveBattle || spell.Action.ID == (uint)AID.TheDecisiveBattle1) {
            chaosBoss = null;
            exDeathBoss = null;
        }
    }
}

class BowelsOfAgony(BossModule module) : Components.RaidwideCast(module, (uint)AID.BowelsOfAgony);

class Crystals(BossModule module) : BossComponent(module) {
    public List<(Actor actor, uint colour)> crystals = [];
    public List<(Actor actor, uint colour)> crystalsStored = []; // Used as a reference point for where the crystals were for easier hint logic
    public List<(Actor actor, ActorStatus debuff)> debuffPlayers = [];

    public Element nextElement = Element.None;
    public enum Element { None, Water, Fire, Wind }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.DynamicFluid) {
            debuffPlayers.Add((actor, status));
        }

        if (status.ID == (uint)SID.Entropy) {
            debuffPlayers.Add((actor, status));
        }

        if (debuffPlayers.Count == 4) {
            debuffPlayers.Sort((a, b) => a.debuff.ExpireAt.CompareTo(b.debuff.ExpireAt));
            if (debuffPlayers[0].debuff.ID == (uint)SID.DynamicFluid) {
                nextElement = Element.Water;
            } else {
                nextElement = Element.Fire;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.DynamicFluid || status.ID == (uint)SID.Entropy) {
            debuffPlayers.RemoveAll(d => d.actor == actor);
        }
    }

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.FireP3) {
            crystals.Add((actor, Colors.Enemy));
        }

        if (actor.OID == (uint)OID.WaterP3) {
            crystals.Add((actor, Color.FromRGBA(0x268BD280).ABGR));
        }

        if (actor.OID == (uint)OID.WindP3) {
            crystals.Add((actor, Colors.Safe));
        }

        if (crystals.Count == 3) {
            crystalsStored = crystals.ToList();
        }
    }

    public override void OnActorDestroyed(Actor actor) {
        if (actor.OID == (uint)OID.FireP3) {
            crystals.RemoveAll(c => c.actor == actor);
            nextElement = Element.Water;
        }

        if (actor.OID == (uint)OID.WaterP3) {
            crystals.RemoveAll(c => c.actor == actor);
            nextElement = Element.Fire;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        foreach (var (actor, colour) in crystals) {
            Arena.AddCircleFilled(actor.Position, 1, colour);
        }
    }
}

class ThunderIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderIII, new AOEShapeCircle(15.0f));

// TODO if I ever need to update the crystal functions due to a timing issue it should just be redesigned into the following:
//  - Crystals elements - This is for the people baiting the crystals attack
//  - Crystals debuffs - This is for the people that have the debuff that matches the crystal colours
//  - This might be needed due to the fact the debuffs go off 0.8 seconds before the crystals actually go off
class WaterCrystal(BossModule module) : Components.GenericBaitProximity(module) {
    private Crystals? crystals = module.FindComponent<Crystals>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Tsunami) {
            if (crystals == null) {
                return;
            }

            NumCasts++;
        }
    }

    public override void Update() {
        CurrentBaits.Clear();

        if (crystals == null || crystals.crystals.Count == 0) {
            return;
        }

        if (crystals.nextElement != Crystals.Element.Water) {
            return;
        }

        var waterCrystal = crystals.crystals.FirstOrNull(c => c.actor.OID == (uint)OID.WaterP3);
        if (waterCrystal == null) {
            return;
        }

        var players = Raid.WithoutSlot().SortedByRange(waterCrystal.Value.actor.Position).ToList();
        for (int i = 0; i < 2; i++) {
            CurrentBaits.Add(new(players[i], new AOEShapeCircle(5.0f)));
        }

        var debuffPlayers = crystals.debuffPlayers.Where(d => d.debuff.ID == (uint)SID.DynamicFluid).Take(2).ToList();
        if (debuffPlayers.Count < 2) {
            return;
        }

        for (int i = 0; i < 2; i++) {
            CurrentBaits.Add(new(debuffPlayers[i].actor, new AOEShapeDonut(4.0f, 10.0f)));
        }
    }

    public override void AddGlobalHints(GlobalHints hints) {
        hints.Add($"Element: {crystals?.nextElement}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (crystals == null || crystals.crystalsStored.Count != 3) {
            return;
        }

        var waterCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.WaterP3);
        var fireCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.FireP3);
        var windCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.WindP3);

        if (assignment == PartyRolesConfig.Assignment.H1) {
            Arena.AddCircle(waterCrystal.actor.Position, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment == PartyRolesConfig.Assignment.H2) {
            Arena.AddCircle(Module.Center + (waterCrystal.actor.Position - Module.Center).Normalized() * 3.5f, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment == PartyRolesConfig.Assignment.R1 || assignment == PartyRolesConfig.Assignment.R2) {
            Arena.AddCircle(fireCrystal.actor.Position, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment == PartyRolesConfig.Assignment.M1 || assignment == PartyRolesConfig.Assignment.M2 ||
            assignment == PartyRolesConfig.Assignment.MT || assignment == PartyRolesConfig.Assignment.OT) {
            Arena.AddCircle(windCrystal.actor.Position, 1.5f, Colors.Safe, 2.0f);
        }
    }
}

class FireCrystal(BossModule module) : Components.GenericBaitProximity(module) {
    private Crystals? crystals = module.FindComponent<Crystals>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Inferno) {
            if (crystals == null) {
                return;
            }

            NumCasts++;
        }
    }

    public override void Update() {
        CurrentBaits.Clear();

        if (crystals == null || crystals.crystals.Count == 0) {
            return;
        }

        if (crystals.nextElement != Crystals.Element.Fire) {
            return;
        }

        var fireCrystal = crystals.crystals.FirstOrNull(c => c.actor.OID == (uint)OID.FireP3);
        if (fireCrystal == null) {
            return;
        }

        var players = Raid.WithoutSlot().SortedByRange(fireCrystal.Value.actor.Position).ToList();
        for (int i = 0; i < 2; i++) {
            CurrentBaits.Add(new(players[i], new AOEShapeDonut(4.0f, 10.0f)));
        }

        var debuffPlayers = crystals.debuffPlayers.Where(d => d.debuff.ID == (uint)SID.Entropy).Take(2).ToList();
        if (debuffPlayers.Count < 2) {
            return;
        }

        for (int i = 0; i < 2; i++) {
            CurrentBaits.Add(new(debuffPlayers[i].actor, new AOEShapeCircle(5.0f)));
        }
    }

    public override void AddGlobalHints(GlobalHints hints) {
        hints.Add($"Element: {crystals?.nextElement}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (crystals == null || crystals.crystalsStored.Count != 3) {
            return;
        }

        var waterCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.WaterP3);
        var fireCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.FireP3);
        var windCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.WindP3);

        if (assignment == PartyRolesConfig.Assignment.H1) {
            Arena.AddCircle(waterCrystal.actor.Position, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment == PartyRolesConfig.Assignment.H2) {
            Arena.AddCircle(Module.Center + (waterCrystal.actor.Position - Module.Center).Normalized() * 3.5f, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment == PartyRolesConfig.Assignment.R1 || assignment == PartyRolesConfig.Assignment.R2) {
            Arena.AddCircle(fireCrystal.actor.Position, 1.5f, Colors.Safe, 2.0f);
        }

        if (assignment == PartyRolesConfig.Assignment.M1 || assignment == PartyRolesConfig.Assignment.M2) {
            Arena.AddCircle(windCrystal.actor.Position + (windCrystal.actor.Position - Module.Center).Normalized().OrthoL() * 2.5f, 1.0f, Colors.Safe, 2.0f);
        }

        if (assignment == PartyRolesConfig.Assignment.MT || assignment == PartyRolesConfig.Assignment.OT) {
            Arena.AddCircle(windCrystal.actor.Position + (windCrystal.actor.Position - Module.Center).Normalized().OrthoR() * 2.5f, 1.0f, Colors.Safe, 2.0f);
        }
    }
}

class LongitudinalLatitudinalImplosion(BossModule module) : Components.GenericAOEs(module, (uint)AID.Shockwave) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.LongitudinalImplosion) {
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.LatitudinalImplosion) {
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40, 45.Degrees()), spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Shockwave) {
            NumCasts++;
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        var nextAOEs = aoes.Take(2).ToList();
        return CollectionsMarshal.AsSpan(nextAOEs);
    }
}

class ThunderIIITB(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ThunderIIITBCast, new AOEShapeCircle(5.0f), true) {
    private Actor? boss = null;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.ThunderIIITBCast) {
            boss = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.ThunderIIITB) {
            NumCasts++;
            if (NumCasts == 2) {
                boss = null;
            }
        }
    }

    public override void Update() {
        CurrentBaits.Clear();

        if (boss == null) {
            return;
        }

        var player = Raid.WithoutSlot().SortedByRange(boss.Position).FirstOrDefault();
        if (player == null) {
            return;
        }

        CurrentBaits.Add(new(boss, player, new AOEShapeCircle(5.0f)));
    }
}

// TODO bait spot
// TODO Wind crystal
// TODO boss jumping around the map
// TODO Limit cut

// TODO change crystal colours - mainly wind hard to tell you should stand on it
