using RebornMaterial;

namespace BossMod.RebornUI;

[ConfigDisplay(Name = "界面", Order = -2, Tags = ["theme", "accent", "scale", "size", "material", "animation", "motion"])]
public sealed class InterfaceConfig : ConfigNode, IM3Settings
{
    [SectionStart("主题")]
    [PropertyDisplay("强调色", tooltip: "界面配色由此颜色生成。可选择预设颜色，也可点击最后一个色块自定义颜色。")]
    public Color AccentColor = Color.FromRGBA(0x5BA7B7FF);

    [PropertyDisplay("文字大小", tooltip: "在 Dalamud 字号基础上缩放文字。")]
    [PropertySlider(0.75f, 2f)]
    public float TextScale = 1f;

    [PropertyDisplay("控件大小", tooltip: "缩放按钮、开关、输入框等控件。")]
    [PropertySlider(0.75f, 2f)]
    public float ElementScale = 1f;

    [PropertyDisplay("间距", tooltip: "缩放控件周围及控件之间的间距，控件大小保持原样。")]
    [PropertySlider(0.5f, 2f)]
    public float PaddingScale = 1f;

    [PropertyDisplay("动画时长", tooltip: "调整折叠窗口、展开卡片等动画的时长。设为 0% 可关闭动画。")]
    [PropertySlider(0f, 2f)]
    public float MotionScale = 1f;

    [PropertyDisplay("紧凑布局", tooltip: "缩小内边距和行高，在窗口中显示更多内容。")]
    public bool CompactSettings = false;

    [PropertyDisplay("显示行内说明", tooltip: "在设置项名称下方显示说明。关闭后，将鼠标悬停在设置项上可查看说明。")]
    public bool InlineDescriptions = true;

    [PropertyDisplay("侧边栏仅显示图标", tooltip: "缩窄侧边栏，仅显示图标。将鼠标悬停在图标上可查看名称。")]
    public bool IconSidebar = false;

    Vector4 IM3Settings.UiAccentColor
    {
        get => AccentColor.ToFloat4();
        set => AccentColor = Color.FromFloat4(value);
    }

    float IM3Settings.UiTextScale
    {
        get => TextScale;
        set => TextScale = value;
    }

    float IM3Settings.UiElementScale
    {
        get => ElementScale;
        set => ElementScale = value;
    }

    float IM3Settings.UiPaddingScale
    {
        get => PaddingScale;
        set => PaddingScale = value;
    }

    float IM3Settings.UiMotionScale
    {
        get => MotionScale;
        set => MotionScale = value;
    }

    bool IM3Settings.UiIconSidebar
    {
        get => IconSidebar;
        set => IconSidebar = value;
    }

    bool IM3Settings.UiDescriptionTooltips => !InlineDescriptions;
}
