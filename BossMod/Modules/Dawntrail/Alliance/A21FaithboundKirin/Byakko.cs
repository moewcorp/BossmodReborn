namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class WallArenaChange(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WallArenaChange, new AOEShapeRect(5f, 8f));
sealed class GloamingGleam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GloamingGleam, new AOEShapeRect(50f, 6f));
sealed class RazorFang(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RazorFang, 20f);

sealed class ByakkoArenaChange(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x3E)
        {
            switch (state)
            {
                case 0x00020001u:
                    Arena.Bounds = A21FaithboundKirin.ByakkoArena;
                    Arena.Center = A21FaithboundKirin.ByakkoArena.Center;
                    break;
                case 0x00080004u:
                    Arena.Bounds = A21FaithboundKirin.DefaultArena;
                    Arena.Center = A21FaithboundKirin.DefaultCenter;
                    break;
            }
        }
    }
}
