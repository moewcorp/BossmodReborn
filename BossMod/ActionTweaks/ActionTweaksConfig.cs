namespace BossMod;

[ConfigDisplay(Name = "Action tweaks", Order = 4)]
public sealed class ActionTweaksConfig : ConfigNode
{
    // TODO: consider exposing max-delay to config; 0 would mean 'remove all delay', max-value would mean 'disable'
    [PropertyDisplay("移除瞬发技能因延迟引起的额外动画锁定延迟（请阅读提示！）", tooltip: "请不要与XivAlexander或NoClippy一起使用——如果检测到这些工具，它应会自动禁用，但请务必先检查！")]
    public bool RemoveAnimationLockDelay = false;

    [PropertyDisplay("最大动画锁。模拟延迟（请阅读工具提示！）", tooltip: "配置使用动画锁定移除时的最大模拟延迟（以毫秒为单位）——这是必需的，且不能减少到零。将此设置为20毫秒时，在使用自动循环时将启用三插。移除三插的最小设置为26毫秒。20毫秒的最小值已被FFLogs接受，不应对你的日志造成问题。")]
    [PropertySlider(20, 50, Speed = 0.1f)]
    public int AnimationLockDelayMax = 20;

    [PropertyDisplay("移除因帧率引起的额外冷却延迟", tooltip: "动态调整冷却和动画锁，以确保无论帧率限制如何，队列中的技能都能立即执行")]
    public bool RemoveCooldownDelay = false;

    [PropertyDisplay("读条时锁定移动")]
    public bool PreventMovingWhileCasting = false;

    public enum ModifierKey
    {
        [PropertyDisplay("None")]
        None,
        [PropertyDisplay("Control")]
        Ctrl,
        [PropertyDisplay("Alt")]
        Alt,
        [PropertyDisplay("Shift")]
        Shift,
        [PropertyDisplay("LMB + RMB")]
        M12
    }

    [PropertyDisplay("按住此键可在读条时允许移动", tooltip: "需要同时启用上面的设置")]
    public ModifierKey MoveEscapeHatch = ModifierKey.None;

    [PropertyDisplay("当目标死亡时自动打断读条")]
    public bool CancelCastOnDeadTarget = false;

    [PropertyDisplay("在即将出现类似热病的机制时，防止移动和使用技能（设置为0以禁用，否则根据你的延迟增加阈值）。")]
    [PropertySlider(0, 10, Speed = 0.01f)]
    public float PyreticThreshold = 1.0f;

    [PropertyDisplay("自动精神失常：如果正常移动和精神失常之间的角度大于此阈值（设置为 180 度以禁用），则防止精神失常下的移动。")]
    [PropertySlider(0, 180)]
    public float MisdirectionThreshold = 180;

    [PropertyDisplay("使用技能后恢复角色面向")]
    public bool RestoreRotation = false;

    [PropertyDisplay("对鼠标悬停的目标使用技能")]
    public bool PreferMouseover = false;

    [PropertyDisplay("智能技能目标选择", tooltip: "如果通常的目标（鼠标悬停/主要目标）不适合使用某个技能，则自动选择下一个最佳目标（例如为副坦使用Shirk）")]
    public bool SmartTargets = true;

    [PropertyDisplay("为手动按下的技能使用自定义队列", tooltip: "此设置可以更好地与自动循环结合，并防止在自动循环过程中按下治疗技能时出现三插或卡GCD的情况")]
    public bool UseManualQueue = false;

    [PropertyDisplay("自动管理自动循环", tooltip: "此设置可防止在倒计时期间过早启动自动循环，在切换目标时以及使用任何未明确取消自动循环的操作时自动启动自动循环。")]
    public bool AutoAutos = false;

    [PropertyDisplay("使用技能时自动下坐骑")]
    public bool AutoDismount = true;

    public enum GroundTargetingMode
    {
        [PropertyDisplay("通过额外点击手动选择位置（正常游戏行为）")]
        Manual,

        [PropertyDisplay("在当前鼠标位置施放")]
        AtCursor,

        [PropertyDisplay("在选定目标的位置施放")]
        AtTarget
    }
    [PropertyDisplay("地面目标技能的自动目标选择")]
    public GroundTargetingMode GTMode = GroundTargetingMode.Manual;

    [PropertyDisplay("尽量避免冲入 AOE", tooltip: "防止自动使用带位移技能（如战士猛攻），如果它们会将您带入危险区域。在没有模块的情况下可能无法按预期工作。")]
    public bool PreventDangerousDash = false;

    public bool ActivateAnticheat = true;
}
