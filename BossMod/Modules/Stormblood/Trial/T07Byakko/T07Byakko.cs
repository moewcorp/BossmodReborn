namespace BossMod.Stormblood.Trial.T07Byakko;

class StormPulse(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.StormPulse));
class HeavenlyStrike(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HeavenlyStrike));
class HeavenlyStrikeSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HeavenlyStrike), 3);
class SweepTheLeg1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SweepTheLeg1), new AOEShapeCone(28.5f, 135.Degrees()));
class SweepTheLeg3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SweepTheLeg3), new AOEShapeDonut(5, 30));
class TheRoarOfThunder(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TheRoarOfThunder));
class ImperialGuard(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ImperialGuard), new AOEShapeRect(44.75f, 2.5f));
class FireAndLightning1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FireAndLightning1), new AOEShapeRect(50, 10));
class FireAndLightning2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FireAndLightning2), new AOEShapeRect(50, 10));
//class Aratama1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Aratama1), 4);
class DistantClap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DistantClap), new AOEShapeDonut(5, 30));

class HighestStakes(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, ActionID.MakeSpell(AID.HighestStakes2), 6, 5, 7, 7);

class AratamaForce(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _bubbles = module.Enemies((uint)OID.AratamaForce);

    private static readonly AOEShapeCircle _shape = new(2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _bubbles.Where(actor => !actor.IsDead).Select(b => new AOEInstance(_shape, b.Position));
}

class HundredfoldHavoc(BossModule module) : Components.Exaflare(module, 5f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HundredfoldHavocFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 5f * caster.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1f, ExplosionsLeft = 10, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HundredfoldHavocFirst or (uint)AID.HundredfoldHavocRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 290, NameID = 6221)]
public class T07Byakko(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20));
