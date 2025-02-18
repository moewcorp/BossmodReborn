namespace BossMod.Shadowbringers.Quest.MSQ.FadedMemories;

class Overcome(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Overcome), new AOEShapeCone(8, 60.Degrees()), 2);
class Skydrive(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Skydrive), 5);

class SkyHighDrive(BossModule module) : Components.GenericRotatingAOE(module)
{
    Angle angle;
    private static readonly AOEShapeRect rect = new(40, 4);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SkyHighDriveCCW:
                angle = -20.Degrees();
                return;
            case AID.SkyHighDriveCW:
                angle = 20.Degrees();
                return;
            case AID.SkyHighDriveFirst:
                if (angle != default)
                {
                    Sequences.Add(new(rect, spell.LocXZ, spell.Rotation, angle, Module.CastFinishAt(spell, 0.5f), 0.6f, 10, 4));
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SkyHighDriveFirst or AID.SkyHighDriveRest)
        {
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
            if (Sequences.Count == 0)
                angle = default;
        }
    }
}

class AvalancheAxe1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AvalanceAxe1), 10);
class AvalancheAxe2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AvalanceAxe2), 10);
class AvalancheAxe3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AvalanceAxe3), 10);
class OvercomeAllOdds(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OvercomeAllOdds), new AOEShapeCone(60, 15.Degrees()), 1)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (NumCasts > 0)
            MaxCasts = 2;
    }
}
class Soulflash(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Soulflash1), 4);
class EtesianAxe(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.EtesianAxe), 15, kind: Kind.DirForward);
class Soulflash2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Soulflash2), 8);

class GroundbreakerExaflares(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GroundbreakerExaFirst)
        {
            Lines.Add(new() { Next = spell.LocXZ, Advance = spell.Rotation.ToDirection() * 6, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1, ExplosionsLeft = 8, MaxShownExplosions = 3 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GroundbreakerExaFirst or AID.GroundbreakerExaRest)
        {
            var index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index < 0)
                return;
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class GroundbreakerCone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GroundbreakerCone), new AOEShapeCone(40, 45.Degrees()));
class GroundbreakerDonut(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GroundbreakerDonut), new AOEShapeDonut(5, 20));
class GroundbreakerCircle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GroundbreakerCircle), new AOEShapeCircle(15));

class ArdbertStates : StateMachineBuilder
{
    public ArdbertStates(BossModule module) : base(module)
    {
        TrivialPhase(0)
            .ActivateOnEnter<SkyHighDrive>()
            .ActivateOnEnter<Skydrive>()
            .ActivateOnEnter<Overcome>()
            .ActivateOnEnter<AvalancheAxe1>()
            .ActivateOnEnter<AvalancheAxe2>()
            .ActivateOnEnter<AvalancheAxe3>()
            .ActivateOnEnter<OvercomeAllOdds>()
            .ActivateOnEnter<Soulflash>()
            .ActivateOnEnter<EtesianAxe>()
            .ActivateOnEnter<Soulflash2>()
            .ActivateOnEnter<GroundbreakerExaflares>()
            .ActivateOnEnter<GroundbreakerCone>()
            .ActivateOnEnter<GroundbreakerDonut>()
            .ActivateOnEnter<GroundbreakerCircle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69311, NameID = 8258, PrimaryActorOID = (uint)OID.Ardbert)]
public class Ardbert(WorldState ws, Actor primary) : BossModule(ws, primary, new(-392, 780), new ArenaBoundsCircle(20));
