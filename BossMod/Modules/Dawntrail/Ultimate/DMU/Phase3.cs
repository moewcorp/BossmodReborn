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

        if (actor.OID == (uint)OID.WindP3) {
            crystals.RemoveAll(c => c.actor == actor);
            nextElement = Element.None;
        }

        if (crystals.Count == 1) {
            nextElement = Element.Wind;
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

class UmbraSmash(BossModule module) : Components.GenericBaitProximity(module) {
    private Crystals? crystals = module.FindComponent<Crystals>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private bool castStarted = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.UmbraSmash) {
            castStarted = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.UmbraSmash) {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }

    public override void Update() {
        if (crystals == null || crystals.crystals.Count != 1) {
            return;
        }

        if (castStarted == true) {
            return;
        }

        CurrentBaits.Clear();

        var chaosBoss = WorldState.Actors.FirstOrDefault(a => a.OID == (uint)OID.Chaos);
        if (chaosBoss == null) {
            return;
        }

        var player = Raid.WithoutSlot().SortedByRange(chaosBoss.Position).LastOrDefault();
        if (player == null) {
            return;
        }

        CurrentBaits.Add(new(player, new AOEShapeCircle(15.0f)));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (crystals == null || crystals.crystals.Count != 1) {
            return;
        }

        if (castStarted == true) {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        var windCrystal = crystals.crystalsStored.First(c => c.actor.OID == (uint)OID.WindP3);

        if (assignment == PartyRolesConfig.Assignment.R1) {
            Arena.AddCircle((Module.Center - (windCrystal.actor.Position - Module.Center).Normalized() * Module.Bounds.Radius) + new WDir(0, 1.0f), 1.0f, Colors.Safe, 2.0f);
        }
    }
}

class UltimaBlaster(BossModule module) : Components.RaidwideInstant(module, (uint)AID.UltimaBlaster);

class UltimaBlasterLimitCut(BossModule module) : Components.GenericBaitAway(module) {
    private Actor? startClone = null;
    private Angle? angleRotation = null;
    private int[] orbNumbers = Utils.MakeArray(8, -1);

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.UltimaBlaster) {
            if (startClone == null) {
                startClone = caster;
            }

            if (angleRotation == null) {
                angleRotation = (startClone.Position - Arena.Center).OrthoL().Dot(caster.Position - Arena.Center) > 0 ? -45.Degrees() : 45.Degrees();
            }
        }

        if (spell.Action.ID == (uint)AID.UltimaBlasterBait) {
            NumCasts++;
            if (CurrentBaits.Count > 0) {
                CurrentBaits.RemoveAt(0);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        var orbNumber = (IconID)iconID switch {
            IconID.OrbNumber1 => 1,
            IconID.OrbNumber2 => 2,
            IconID.OrbNumber3 => 3,
            IconID.OrbNumber4 => 4,
            IconID.OrbNumber5 => 5,
            IconID.OrbNumber6 => 6,
            IconID.OrbNumber7 => 7,
            IconID.OrbNumber8 => 8,
            _ => -1
        };

        if (orbNumber > 0) {
            var slot = Raid.FindSlot(targetID);
            if (slot >= 0) {
                orbNumbers[slot] = orbNumber;

                if (startClone == null || angleRotation == null) {
                    return;
                }

                var player = WorldState.Actors.Find(targetID);
                if (player == null) {
                    return;
                }

                CurrentBaits.Add(new(Arena.Center + (startClone.Position - Arena.Center).Rotate(angleRotation.Value * (orbNumber - 1)), player, new AOEShapeRect(100.0f, 3.0f)));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (angleRotation == null) {
            return;
        }

        foreach (var bait in ActiveBaitsOn(pc)) {
            Arena.AddCircle(bait.Source.Position, 30.0f, Colors.Object);
            Arena.AddCircle(Arena.Center + (Arena.Center - bait.Source.Position).Normalized().Rotate(angleRotation.Value * 0.5f) * 19.0f, 0.75f, Colors.Safe);
        }
    }
}

// TODO Functions (HeadTailWind & Cyclone) below work fine for the mechanics, but should be revisited and improved to solve how the mechanic works normally
//  Most groups are currently doing LB strats which makes the mechanic resolve in a certain way / where stuff doesn't matter
class HeadTailWind(BossModule module) : Components.GenericKnockback(module) {
    public SID[] Direction = new SID[8];
    private (WPos Origin, DateTime Activation, bool EventHappened) wave;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.VacuumWave) {
            wave = new(caster.Position, DateTime.Now, false);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.VacuumWave) {
            NumCasts++;
            wave.EventHappened = true;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        SID? direction = (SID)status.ID switch {
            SID.Headwind => SID.Headwind,
            SID.Tailwind => SID.Tailwind,
            _ => null
        };

        if (direction != null) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                Direction[slot] = direction.Value;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.Headwind || status.ID == (uint)SID.Tailwind) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                Direction[slot] = 0;
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        if (wave.Origin != default) {
            return new[] { new Knockback(wave.Origin, KnockDistance(slot, actor, wave.Origin), wave.Activation) };
        }

        return [];
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (Direction[pcSlot] != SID.Headwind || Direction[pcSlot] != SID.Tailwind) {
            return;
        }

        foreach (var knockback in ActiveKnockbacks(pcSlot, pc)) {
            var toSource = (knockback.Origin - pc.Position).Normalized();
            var safeFacing = (Direction[pcSlot] == SID.Headwind ? -toSource : toSource).ToAngle();
            Arena.PathArcTo(pc.Position, 1, (safeFacing + 45.Degrees()).Rad, (safeFacing - 45.Degrees()).Rad);
            MiniArena.PathStroke(false, Colors.Safe);
            Arena.PathArcTo(pc.Position, 1, (safeFacing + 225.Degrees()).Rad, (safeFacing + 135.Degrees()).Rad);
            MiniArena.PathStroke(false, Colors.Danger);
        }
    }

    public override void Update() {
        if (wave.EventHappened) {
            foreach (var player in Raid.WithoutSlot()) {
                if (player.LastFrameMovement.Length() / WorldState.Frame.Duration > 15) {
                    wave = default;
                    break;
                }
            }
        }
    }

    float KnockDistance(int pcSlot, Actor pc, WPos source) {
        var direction = Direction[pcSlot];
        if (direction != SID.Headwind && direction != SID.Tailwind) {
            return 20;
        }

        var toSource = (source - pc.Position).Normalized();
        var safeFacing = direction == SID.Headwind ? -toSource : toSource;
        var rel = safeFacing.Normalized().Dot(pc.Rotation.ToDirection());

        if (rel > 0.7071067f) {
            return 10;
        }

        if (rel < -0.7071068f) {
            return 40;
        }

        return 20;
    }
}

class Cyclone(BossModule module) : Components.GenericStackSpread(module) {
    private HeadTailWind? windDebuffs = module.FindComponent<HeadTailWind>();
    public int NumCasts = 0;

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Cyclone) {
            NumCasts++;
        }
    }

    public override void Update() {
        Stacks.Clear();

        if (windDebuffs == null) {
            return;
        }

        foreach (var (slot, player) in Raid.WithSlot().WhereSlot(p => windDebuffs.Direction[p] is SID.Headwind or SID.Tailwind)) {
            Stacks.Add(new(player, 6.0f));
        }
    }
}
