namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class SurgingWaveCorridor(BossModule module) : BossComponent(module)
{
    public WDir CorridorDir;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x49 && state is 0x02000001 or 0x00200001 or 0x00800040 or 0x08000400)
        {
            CorridorDir = state switch
            {
                0x00800040 => new(-1f, default),
                0x08000400 => new(1f, default),
                _ => default
            };
        }
    }
}

class SurgingWaveAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SurgingWaveAOE), 6f);
class SurgingWaveShockwave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.SurgingWaveShockwave), 68f, true);
class SurgingWaveSeaFoam(BossModule module) : Components.PersistentVoidzone(module, 1.5f, m => m.Enemies(OID.SeaFoam).Where(x => !x.IsDead));

public class SurgingWaveFrothingSea : Components.Exaflare
{
    public SurgingWaveFrothingSea(BossModule module) : base(module, new AOEShapeRect(6f, 20f, 80f))
    {
        ImminentColor = Colors.AOE;
        FutureColor = Colors.Danger;
    }

    private static readonly Angle _rot1 = 90.Degrees();
    private static readonly Angle _rot2 = -90.Degrees();

    public override void OnEventEnvControl(byte index, uint state)
    {
        var _activation = WorldState.FutureTime(30d);
        if (index == 0x49)
        {
            if (state == 0x00800040)
                Lines.Add(new() { Next = new(-80, -900), Advance = 2.3f * _rot1.ToDirection(), NextExplosion = _activation, TimeToMove = 0.9f, ExplosionsLeft = 13, MaxShownExplosions = 2, Rotation = _rot1 });
            if (state == 0x08000400)
                Lines.Add(new() { Next = new(80, -900), Advance = 2.3f * _rot2.ToDirection(), NextExplosion = _activation, TimeToMove = 0.9f, ExplosionsLeft = 13, MaxShownExplosions = 2, Rotation = _rot2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SurgingWaveFrothingSea)
        {
            ++NumCasts;
            if (Lines.Count != 0)
            {
                AdvanceLine(Lines[0], Lines[0].Next + 2.3f * Lines[0].Rotation.ToDirection());
                if (Lines[0].ExplosionsLeft == 0)
                    Lines.RemoveAt(0);
            }
        }
    }
}

abstract class Strait(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(100f, 90f.Degrees()));
class LeftStrait(BossModule module) : Strait(module, AID.LeftStrait);
class RightStrait(BossModule module) : Strait(module, AID.RightStrait);
