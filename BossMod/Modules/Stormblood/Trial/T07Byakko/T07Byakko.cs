namespace BossMod.Stormblood.Trial.T07Byakko;

class StormPulse(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.StormPulse));
class HeavenlyStrike(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HeavenlyStrike));
class HeavenlyStrikeSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HeavenlyStrike), 3f);
class SweepTheLeg1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SweepTheLeg1), new AOEShapeCone(28.5f, 135f.Degrees()));
class SweepTheLeg3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SweepTheLeg3), new AOEShapeDonut(5f, 30f));
class TheRoarOfThunder(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TheRoarOfThunder));
class ImperialGuard(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ImperialGuard), new AOEShapeRect(44.75f, 2.5f));

abstract class FireAndLightning(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(50f, 10f));
class FireAndLightning1(BossModule module) : FireAndLightning(module, AID.FireAndLightning1);
class FireAndLightning2(BossModule module) : FireAndLightning(module, AID.FireAndLightning2);

class DistantClap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DistantClap), new AOEShapeDonut(5f, 3f));

class HighestStakes(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, ActionID.MakeSpell(AID.HighestStakes2), 6f, 5f, 7, 7);

class AratamaForce(BossModule module) : Components.PersistentVoidzone(module, 2f, GetVoidzones, 2)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.AratamaForce);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
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
public class T07Byakko(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20f));
