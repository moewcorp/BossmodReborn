namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

class SpurningFlames(BossModule module) : Components.RaidwideCast(module, (uint)AID.SpurningFlames);

class ScouringScorn(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScouringScorn);

class ImpassionedSparks(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImpassionedSparks3, new AOEShapeCircle(8f), 8);

class BurningPillar(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.BurningPillar) {
            aoes.Add(new AOEInstance(new AOEShapeCircle(10f), caster.Position, default, default, Colors.Danger));
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

class FireChains(BossModule module) : Components.Chains(module, (uint)TetherID._Gen_Tether_chn_hfchain1f, default, 15F);

/*
Chains are applied to everyone - upon raidwide SpurningFlames finishes

class AquaBall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AquaBall1, new AOEShapeCircle(5f));

    _Ability_ImpassionedSparks = 45483, // PariOfPlenty->self, 5.0s cast, single-target
    _Ability_ImpassionedSparks2 = 45485, // Helper->self, 2.0s cast, single-target
    _Ability_ImpassionedSparks3 = 45488, // Helper->self, 6.0s cast, range 8 circle
    
    _Ability_ImpassionedSparks1 = 45484, // PariOfPlenty->self, no cast, single-target

Impassioned Sparks - 4.7 second cast - 45483 - after this has finished into ->
4x helper casts - Impassioned sparks 45485
4x helper casts - Impassioned Sparks 45488 - as these start -> 4x helper casts - Impassioned sparks 45485
4x helper casts - Impassioned Sparks 45488 - as these start -> 4x helper casts - Impassioned sparks 45485

Baits will happen as well which are casted by Burning Pillar 45527 - Leave behind puddles - Until finished

Chains will now go off in 3 seconds - get mid -> go to corner

some random baits on everyone? - Unsure where from - but resolves itself anyway

Raidwide to finish everything - removes puddles
*/