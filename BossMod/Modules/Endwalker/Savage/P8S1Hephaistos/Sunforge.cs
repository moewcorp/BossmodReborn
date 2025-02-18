namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class SunforgeCenterHint(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SunforgeCenter), "Avoid center")
{
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
        {
            Arena.ZoneRect(Module.Center, new WDir(1, 0), 21, -7, 21, Colors.SafeFromAOE);
            Arena.ZoneRect(Module.Center, new WDir(-1, 0), 21, -7, 21, Colors.SafeFromAOE);
        }
    }
}

class SunforgeSidesHint(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SunforgeSides), "Avoid sides")
{
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
        {
            Arena.ZoneRect(Module.Center, new WDir(0, 1), 21, 21, 7, Colors.SafeFromAOE);
        }
    }
}

class SunforgeCenter(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ScorchingFang), new AOEShapeRect(21, 7, 21));
class SunforgeSides(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SunsPinion), new AOEShapeRect(21, 21, -7));
class SunforgeCenterIntermission(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ScorchingFangIntermission), new AOEShapeRect(42, 7));
class SunforgeSidesIntermission(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ScorchedPinion), new AOEShapeRect(21, 42, -7));
