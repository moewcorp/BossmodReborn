namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.VaultYan;

public enum OID : uint
{
    VaultYan = 0x4902,
    SilverSack = 0x1EBE48, // R0.5
    GoldSack = 0x1EBE47 // R0.5
}

public enum AID : uint
{
    Rush = 44269 // VaultYan->location, 4.0s cast, width 8 rect charge
}

sealed class Rush(BossModule module) : Components.ChargeAOEs(module, (uint)AID.Rush, 4f);

sealed class CollectSacks(BossModule module) : Components.GenericTowers(module)
{
    private readonly List<Actor> goldsacks = module.Enemies((uint)OID.GoldSack);
    private readonly List<Actor> silversacks = module.Enemies((uint)OID.SilverSack);
    private readonly Rush _aoe = module.FindComponent<Rush>()!;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.SilverSack or (uint)OID.GoldSack)
        {
            Towers.Add(new(actor.Position, 1f, 1, 1, default, WorldState.FutureTime(10d)));
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.SilverSack or (uint)OID.GoldSack)
        {
            var towers = CollectionsMarshal.AsSpan(Towers);
            var len = towers.Length;
            var pos = actor.Position;
            for (var i = 0; i < len; ++i)
            {
                ref var t = ref towers[i];
                if (t.Position == pos)
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        Actor? closest = null;
        var minDistSq = float.MaxValue;

        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);

        bool InAOE(WPos pos, ReadOnlySpan<Components.GenericAOEs.AOEInstance> aoes)
        {
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoes[i].Check(pos))
                {
                    return true;
                }
            }
            return false;
        }

        void TryUpdateClosest(Actor sack, float weight, ReadOnlySpan<Components.GenericAOEs.AOEInstance> aoes)
        {
            var pos = sack.Position;
            var distSq = (actor.Position - pos).LengthSq() * weight;
            if (distSq < minDistSq && !InAOE(pos, aoes))
            {
                minDistSq = distSq;
                closest = sack;
            }
        }

        var countG = goldsacks.Count;
        for (var i = 0; i < countG; ++i)
        {
            TryUpdateClosest(goldsacks[i], 1f, aoes);
        }

        var countS = silversacks.Count;
        for (var i = 0; i < countS; ++i)
        {
            TryUpdateClosest(silversacks[i], 3f, aoes); // gold worth 3x silver
        }

        if (closest != null)
        {
            hints.GoalZones.Add(hints.GoalSingleTarget(closest, 0.5f, 10f));
        }
    }
}

sealed class CollectionEnd(BossModule module) : BossComponent(module)
{
    public bool End;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x6u && param1 == 0xFFFFFFFF)
        {
            End = true;
        }
    }
}

sealed class VaultYanStates : StateMachineBuilder
{
    private CollectionEnd? collect;

    public VaultYanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CollectionEnd>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<CollectSacks>()
            .Raw.Update = () =>
            {
                collect ??= module.FindComponent<CollectionEnd>();
                return collect!.End || module.WorldState.CurrentCFCID != 1060u;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.VaultYan, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 14046u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 8)]
public sealed class VaultYan(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    protected override bool CheckPull() => PrimaryActor.Renderflags == 0;
}
