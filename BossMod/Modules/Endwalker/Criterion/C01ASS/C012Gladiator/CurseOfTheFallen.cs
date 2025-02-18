namespace BossMod.Endwalker.VariantCriterion.C01ASS.C012Gladiator;

class CurseOfTheFallen(BossModule module) : Components.UniformStackSpread(module, 5f, 6f, 3, 3, true)
{
    private readonly List<Actor> _fallen = [];
    private Actor? _thunderous;
    private BitMask _lingering;
    private DateTime _spreadResolve;
    private DateTime _stackResolve;
    private bool _dirty;

    public override void Update()
    {
        if (_dirty)
        {
            _dirty = false;

            Spreads.Clear();
            Stacks.Clear();

            if (_fallen.Count > 0 && (_thunderous == null || _spreadResolve < _stackResolve))
            {
                AddSpreads(_fallen, _spreadResolve);
            }
            else if (_thunderous != null && (_fallen.Count == 0 || _stackResolve < _spreadResolve))
            {
                AddStack(_thunderous, _stackResolve, _lingering);
            }
        }
        base.Update();
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.EchoOfTheFallen:
                _fallen.Add(actor);
                _spreadResolve = status.ExpireAt;
                _dirty = true;
                break;
            case (uint)SID.ThunderousEcho:
                _thunderous = actor;
                _stackResolve = status.ExpireAt;
                _dirty = true;
                break;
            case (uint)SID.LingeringEchoes:
                _lingering.Set(Raid.FindSlot(actor.InstanceID));
                _dirty = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NEchoOfTheFallen:
            case AID.SEchoOfTheFallen:
                _fallen.RemoveAll(a => a.InstanceID == spell.MainTargetID);
                _dirty = true;
                break;
            case AID.NThunderousEcho:
            case AID.SThunderousEcho:
                _thunderous = null;
                _dirty = true;
                break;
            case AID.NLingeringEcho:
            case AID.SLingeringEcho:
                _lingering.Reset();
                _dirty = true;
                break;
        }
    }
}

abstract class RingOfMight1Out(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 8);
class NRingOfMight1Out(BossModule module) : RingOfMight1Out(module, AID.NRingOfMight1Out);
class SRingOfMight1Out(BossModule module) : RingOfMight1Out(module, AID.SRingOfMight1Out);

abstract class RingOfMight2Out(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 13);
class NRingOfMight2Out(BossModule module) : RingOfMight2Out(module, AID.NRingOfMight2Out);
class SRingOfMight2Out(BossModule module) : RingOfMight2Out(module, AID.SRingOfMight2Out);

abstract class RingOfMight3Out(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 18);
class NRingOfMight3Out(BossModule module) : RingOfMight3Out(module, AID.NRingOfMight3Out);
class SRingOfMight3Out(BossModule module) : RingOfMight3Out(module, AID.SRingOfMight3Out);

abstract class RingOfMight1In(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(8, 30));
class NRingOfMight1In(BossModule module) : RingOfMight1In(module, AID.NRingOfMight1In);
class SRingOfMight1In(BossModule module) : RingOfMight1In(module, AID.SRingOfMight1In);

abstract class RingOfMight2In(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(13, 30));
class NRingOfMight2In(BossModule module) : RingOfMight2In(module, AID.NRingOfMight2In);
class SRingOfMight2In(BossModule module) : RingOfMight2In(module, AID.SRingOfMight2In);

abstract class RingOfMight3In(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(18, 30));
class NRingOfMight3In(BossModule module) : RingOfMight3In(module, AID.NRingOfMight3In);
class SRingOfMight3In(BossModule module) : RingOfMight3In(module, AID.SRingOfMight3In);
