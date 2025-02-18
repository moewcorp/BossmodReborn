namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.DD40Bhima;

public enum OID : uint
{
    Boss = 0x23E2, // R2.400, x1
    Whirlwind = 0x23E3, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    AncientAero = 11905, // Boss->self, 3.0s cast, range 50+R width 8 rect // casted after the KB
    AncientAeroII = 11903, // Boss->location, 3.0s cast, range 6 circle 
    AncientAeroIII = 11904, // Boss->self, 5.0s cast, range 50+R circle // KB, not immunable (though shield on hp does block the kb)
    Tornado = 11902, // Boss->player, 3.0s cast, range 6 circle // untelegraph'd circle aoe, mimimum damage
    Windage = 11906, // 23E3->self, 1.0s cast, range 6 circle // need to make this show up as a void zone while it's still up, just because they cast so quickly
}

class AncientAero(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AncientAero), new AOEShapeRect(52.4f, 4f));
class AncientAeroII(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AncientAeroII), 6f);
class AncientAeroIII(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.AncientAeroIII), 23.5f, true, stopAtWall: true);
class Tornado(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Tornado), 6f);
class Windage(BossModule module) : Components.PersistentVoidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Whirlwind);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class DD40BhimaStates : StateMachineBuilder
{
    public DD40BhimaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<AncientAeroII>()
            .ActivateOnEnter<AncientAeroIII>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<Windage>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 543, NameID = 7483)]
public class DD40Bhima(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsCircle(25f));
