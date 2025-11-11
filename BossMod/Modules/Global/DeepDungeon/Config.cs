using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;

namespace BossMod.Global.DeepDungeon;

[ConfigDisplay(Name = "自动深层迷宫（实验性）", Parent = typeof(ModuleConfig))]
public sealed class AutoDDConfig : ConfigNode
{
    public enum ClearBehavior
    {
        [PropertyDisplay("不自动选中")]
        None,
        [PropertyDisplay("传送装置打开时停止")]
        Passage,
        [PropertyDisplay("如果等级未达到上限，则选中所有目标；否则，传送装置开启后停止选中。")]
        Leveling,
        [PropertyDisplay("选中所有目标")]
        All,
    }

    [PropertyDisplay("启用模块", tooltip: "警告：此功能尚处于实验阶段，很可能存在错误或意外行为。\n要启用此功能，您必须在“完整任务自动化”选项卡中激活“开发中”成熟度模块。")]
    public bool Enable = true;
    [PropertyDisplay("启用小地图")]
    public bool EnableMinimap = true;
    [PropertyDisplay("自动躲避陷阱", tooltip: "避开PalacePal数据中已知的陷阱位置。已与在线数据打通，请安装同仓库下PalacePal版本一同使用。（无论此设置如何，使用“全景”发现的陷阱都会被避开。）")]
    public bool TrapHints = true;
    [PropertyDisplay("自动导航至传送装置")]
    public bool AutoPassage = true;

    [PropertyDisplay("自动选中怪物行为逻辑")]
    public ClearBehavior AutoClear = ClearBehavior.Leveling;

    [PropertyDisplay("禁止在非BOSS楼层的使用DoT技能（仅适用于BMR自动循环）")]
    public bool ForbidDOTs = false;

    [PropertyDisplay("暂停导航前可拉的最大怪物数量（0 = 战斗中不进行导航）")]
    [PropertySlider(0, 15)]
    public int MaxPull = 0;
    [PropertyDisplay("尝试利用地形进行视线攻击")]
    public bool AutoLOS = false;

    [PropertyDisplay("自动导航至宝箱")]
    public bool AutoMoveTreasure = true;
    [PropertyDisplay("优先开启宝箱，而非传送装置")]
    public bool OpenChestsFirst = false;
    [PropertyDisplay("打开金宝箱")]
    public bool GoldCoffer = true;
    [PropertyDisplay("打开银宝箱")]
    public bool SilverCoffer = true;
    [PropertyDisplay("打开铜宝箱")]
    public bool BronzeCoffer = true;

    [PropertyDisplay("在前往下一层之前，先探索所有房间")]
    public bool FullClear = false;

    public bool AutomaticPomanderInParty = false;

    public BitMask AutoPoms;
    public BitMask AutoMagicite;
    public BitMask AutoDemiclone;

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        foreach (var _ in tree.Node("自动使用魔陶器"))
        {
            ImGui.TextWrapped("当金宝箱中包含你无法携带的魔陶器时，将会使用高亮显示的魔陶器。");
            if (ImGui.Checkbox("在组队中启用（确保小队中没有其他人同时启用该功能，否则可能重复使用）", ref Service.Config.Get<AutoDDConfig>().AutomaticPomanderInParty))
            {
                Modified.Fire();
            }

            for (var i = 1; i < (int)PomanderID.Count; i++)
                using (ImRaii.PushId($"pom{i}"))
                {
                    var row = Service.LuminaRow<DeepDungeonItem>((uint)i)!.Value;
                    var wrap = Service.Texture.GetFromGameIcon(row.Icon).GetWrapOrEmpty();
                    var scale = new Vector2(32, 32) * ImGuiHelpers.GlobalScale;
                    ImGui.Image(wrap.Handle, scale, new Vector2(0, 0), tintCol: AutoPoms[i] ? new(1, 1, 1, 1) : new(1, 1, 1, 0.4f));
                    if (ImGui.IsItemClicked())
                    {
                        AutoPoms.Toggle(i);
                        Modified.Fire();
                    }
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip(row.Name.ToString());
                    if (i % 8 > 0)
                        ImGui.SameLine();
                }

            ImGui.Text(""); // undo last sameline
        }
    }
}
