namespace BossMod.Shadowbringers.Quest.MSQ.FadedMemories;

class DragonsGaze(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.TheDragonsGaze));

class KingThordanStates : StateMachineBuilder
{
    public KingThordanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DragonsGaze>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69311, NameID = 3632, PrimaryActorOID = (uint)OID.KingThordan)]
public class KingThordan(WorldState ws, Actor primary) : BossModule(ws, primary, new(-247, 321), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var h = hints.PotentialTargets[i];
            h.Priority = h.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;
        }
    }
}
