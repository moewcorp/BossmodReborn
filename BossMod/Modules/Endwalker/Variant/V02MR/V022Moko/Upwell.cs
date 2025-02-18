namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

// note: each initial line sends out two 'exaflares' to the left & right
// each subsequent exaflare moves by distance 5, and happen approximately 2s apart
// each wave is 5 subsequent lines, except for two horizontal ones that go towards edges - they only have 1 line - meaning there's a total 22 'rest' casts
class UpwellFirst : Components.SimpleAOEs
{
    public UpwellFirst(BossModule module) : base(module, ActionID.MakeSpell(AID.UpwellFirst), new AOEShapeRect(60, 5)) { Color = Colors.Danger; }
}
class UpwellRest(BossModule module) : Components.Exaflare(module, new AOEShapeRect(30, 2.5f, 30))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var linesCount = Lines.Count;
        if (linesCount == 0)
            return [];
        var futureAOEs = FutureAOEs(linesCount);
        var imminentAOEs = ImminentAOEs(linesCount);
        var futureCount = futureAOEs.Count;
        var imminentCount = imminentAOEs.Length;
        var imminentDeadline = WorldState.FutureTime(5);
        var index = 0;
        var aoes = new AOEInstance[futureCount + imminentCount];
        for (var i = 0; i < futureCount; ++i)
        {
            var aoe = futureAOEs[i];
            if (aoe.Item2 <= imminentDeadline)
                aoes[index++] = new(Shape, aoe.Item1, aoe.Item3, aoe.Item2, FutureColor);
        }

        for (var i = 0; i < imminentCount; ++i)
        {
            var aoe = imminentAOEs[i];
            if (aoe.Item2 <= imminentDeadline)
                aoes[index++] = new(Shape, aoe.Item1, aoe.Item3, aoe.Item2, ImminentColor);
        }
        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UpwellFirst)
        {
            var pos = caster.Position;
            var check = pos.AlmostEqual(Arena.Center, 1);
            var isNorth = pos.Z == 530;
            var isSouth = pos.Z == 550;
            var numExplosions1 = check || isSouth ? 5 : 1;
            var numExplosions2 = check || isNorth ? 5 : 1;
            var dir = spell.Rotation.ToDirection().OrthoR();
            var advance1 = dir * 7.5f;
            var advance2 = dir * 5;
            Lines.Add(new() { Next = pos + advance1, Advance = advance2, Rotation = spell.Rotation, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2, ExplosionsLeft = numExplosions2, MaxShownExplosions = 2 });
            Lines.Add(new() { Next = pos - advance1, Advance = -advance2, Rotation = (spell.Rotation + 180.Degrees()).Normalized(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2, ExplosionsLeft = numExplosions1, MaxShownExplosions = 2 });
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UpwellRest)
        {
            ++NumCasts;
            var index = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 3) && l.Rotation.AlmostEqual(caster.Rotation, Angle.DegToRad));
            if (index >= 0)
            {
                AdvanceLine(Lines[index], caster.Position + 2.5f * Lines[index].Rotation.ToDirection().OrthoR());
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
}
