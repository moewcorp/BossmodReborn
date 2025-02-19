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
class SurgingWaveSeaFoam(BossModule module) : Components.PersistentVoidzone(module, 1.5f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SeaFoam);
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

public class SurgingWaveFrothingSea : Components.Exaflare
{
    public SurgingWaveFrothingSea(BossModule module) : base(module, new AOEShapeRect(6f, 20f, 80f))
    {
        ImminentColor = Colors.AOE;
        FutureColor = Colors.Danger;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        void AddLine(WPos first, Angle rot)
        => Lines.Add(new() { Next = first, Advance = 2.3f * rot.ToDirection(), NextExplosion = WorldState.FutureTime(30d), TimeToMove = 0.9f, ExplosionsLeft = 13, MaxShownExplosions = 2, Rotation = rot });
        if (index == 0x49)
        {
            if (state == 0x00800040)
                AddLine(new(-80f, -900f), 90f.Degrees());
            else if (state == 0x08000400)
                AddLine(new(80f, -900f), -90f.Degrees());
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SurgingWaveFrothingSea)
        {
            ++NumCasts;
            if (Lines.Count != 0)
            {
                var line = Lines[0];
                AdvanceLine(line, line.Next + 2.3f * line.Rotation.ToDirection());
                if (line.ExplosionsLeft == 0)
                    Lines.RemoveAt(0);
            }
        }
    }
}

abstract class Strait(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(100f, 90f.Degrees()));
class LeftStrait(BossModule module) : Strait(module, AID.LeftStrait);
class RightStrait(BossModule module) : Strait(module, AID.RightStrait);
