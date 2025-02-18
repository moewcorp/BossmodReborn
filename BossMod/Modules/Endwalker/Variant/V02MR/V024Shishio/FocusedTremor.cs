namespace BossMod.Endwalker.VariantCriterion.V02MR.V024Shishio;

class YokiUzu(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.YokiUzu), 23);

class FocusedTremor(BossModule module) : Components.GenericAOEs(module)
{
    private readonly YokiUzu _yokiUzu = module.FindComponent<YokiUzu>()!;
    private static readonly AOEShapeRect rect = new(30, 20);
    private AOEInstance _aoe;
    private Circle? circle;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe == default)
            yield break;

        var isCasterActive = _yokiUzu.ActiveCasters.Count != 0;
        var firstAOEActivation = _yokiUzu.ActiveAOEs(slot, actor).FirstOrDefault().Activation;
        var sixFulmsUnderStatus = actor.FindStatus(SID.SixFulmsUnder);
        var expireAt = sixFulmsUnderStatus?.ExpireAt ?? DateTime.MaxValue;
        var extra = circle != null ? 10 : 0;
        RectangleSE[] rectShape = [new(_aoe.Origin, _aoe.Origin + (rect.LengthFront + extra) * _aoe.Rotation.ToDirection(), rect.HalfWidth)];
        Circle[] circleShape = circle != null ? [circle] : [];
        var aoeInstance = _aoe with
        {
            Origin = Arena.Center,
            Shape = new AOEShapeCustom(rectShape, null, circleShape, false, circle != null ? OperandType.Xor : OperandType.Union) with { InvertForbiddenZone = isCasterActive },
            Color = isCasterActive ? Colors.SafeFromAOE : Colors.AOE,
            Activation = !isCasterActive ? expireAt : firstAOEActivation
        };

        yield return aoeInstance;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.YokiUzu)
            circle = new(caster.Position, 23);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.YokiUzu)
            circle = null;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
            _aoe = index switch
            {
                0x67 => new(rect, Arena.Center - new WDir(20, 0), 90.Degrees()),
                0x65 => new(rect, Arena.Center - new WDir(-20, 0), -90.Degrees()),
                0x66 => new(rect, Arena.Center - new WDir(0, 20)),
                0x68 => new(rect, Arena.Center - new WDir(0, -20), 180.Degrees()),
                _ => default
            };
        if (state == 0x00080004 && index is 0x65 or 0x66 or 0x67 or 0x68)
            _aoe = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var sixFulmsUnderStatus = actor.FindStatus(SID.SixFulmsUnder);
        var expireAt = sixFulmsUnderStatus?.ExpireAt ?? DateTime.MaxValue;
        if (circle != null && (expireAt - WorldState.CurrentTime).TotalSeconds <= 8)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveAOEs(slot, actor).Any(c => c.Color == Colors.AOE))
            base.AddHints(slot, actor, hints);
        else if (ActiveAOEs(slot, actor).Any(c => !c.Check(actor.Position)))
            hints.Add("Go into quicksand to avoid AOE!");
    }
}
