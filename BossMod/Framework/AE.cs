using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
namespace BossMod;

// BossMod 消费 AEAssist 的 IPC 端点，实现 AEAssist → BossMod 联动（期望身位、停手控制）。
// AEAssist 侧框架层通过 MeleePositionalTable 自动推算"下一个 GCD 的期望身位"，
// 经 DesiredPositionalIpc 暴露以下端点（与 RSR 命名对齐）：
//   AEAssist.GetDesiredPositional                    () -> byte  轮询（0=None, 1=Rear, 2=Flank）
//   AEAssist.ActionUpdater.DesiredPositionalChanged  (byte) -> void  事件推送
// AEAssist 侧 BMR_IPCProvider 暴露停手控制端点（与 RSR 命名对齐）：
//   AEAssist.TriggerSpecialStateWithDuration        (byte command, float duration) -> void
public sealed class AEAssistModule : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ICallGateSubscriber<byte> _getDesiredPositional;
    private readonly ICallGateSubscriber<byte, object?> _desiredPositionalChanged;
    private readonly ICallGateSubscriber<byte, float, object> _triggerSpecialStateWithDuration;
    private const string aeassist = "AEAssistV3";
    private Positional _desiredPositional = Positional.Any;

    // 当前 AEAssist 期望身位：getter 每次实时轮询 IPC。
    // AEAssist 侧 ACR 每帧在 OnBattleUpdate 重绘身位（提前一个 GCD），事件只覆盖框架自动推算的变化，
    // 因此必须每次访问都轮询，才能拿到提前量充足的最新值。
    public Positional DesiredPositional
    {
        get
        {
            var pos = GetDesiredPositional();
            if (pos != _desiredPositional)
            {
                _desiredPositional = pos;
                DesiredPositionalChanged?.Invoke(pos);
            }
            return _desiredPositional;
        }
    }

    public event Action<Positional>? DesiredPositionalChanged;

    public AEAssistModule(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
        _getDesiredPositional = pluginInterface.GetIpcSubscriber<byte>("AEAssist.GetDesiredPositional");
        _desiredPositionalChanged = pluginInterface.GetIpcSubscriber<byte, object?>("AEAssist.ActionUpdater.DesiredPositionalChanged");
        _triggerSpecialStateWithDuration = pluginInterface.GetIpcSubscriber<byte, float, object>("AEAssist.TriggerSpecialStateWithDuration");
        try
        {
            _desiredPositionalChanged.Subscribe(OnDesiredPositionalChanged);
        }
        catch
        {
            // AEAssist 未安装/未加载 - 忽略，仍可轮询 GetDesiredPositional()
        }
        _desiredPositional = GetDesiredPositional();
    }

    public void Dispose()
    {
        try
        {
            _desiredPositionalChanged.Unsubscribe(OnDesiredPositionalChanged);
        }
        catch
        {
            // ignore
        }
    }

    public bool IsInstalled
    {
        get
        {
            var installedPlugins = _pluginInterface.InstalledPlugins;
            foreach (var x in installedPlugins)
            {
                if (x.IsLoaded && x.InternalName.Equals(aeassist, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }

    // 轮询 AEAssist 当前期望身位（0=None, 1=Rear, 2=Flank, 3=Front）；未安装/未加载时返回 Any
    public Positional GetDesiredPositional()
    {
        try
        {
            return MapPositional(_getDesiredPositional.InvokeFunc());
        }
        catch
        {
            return Positional.Any;
        }
    }

    // 让 AEAssist 进入指定特殊状态并持续指定时长（如 NoCasting 停手）；AEAssist 未加载时静默失败
    public void TriggerSpecialStateWithDuration(SpecialCommandType specialCommand, float duration) => _triggerSpecialStateWithDuration.InvokeAction((byte)specialCommand, duration);

    private void OnDesiredPositionalChanged(byte value)
    {
        var pos = MapPositional(value);
        if (pos != _desiredPositional)
        {
            _desiredPositional = pos;
            DesiredPositionalChanged?.Invoke(pos);
        }
    }

    // 与 AEAssist MeleePositionalTable 的编码对齐：1=Rear, 2=Flank, 3=Front（AE 目前无 Front）
    private static Positional MapPositional(byte value) => value switch
    {
        1 => Positional.Rear,
        2 => Positional.Flank,
        3 => Positional.Front,
        _ => Positional.Any,
    };

    // 与 AEAssist BMR_IPCProvider.SpecialCommandType 对齐（NoCasting = 13）
    public enum SpecialCommandType : byte
    {
        EndSpecial,
        HealArea,
        HealSingle,
        DefenseArea,
        DefenseSingle,
        DispelStancePositional,
        RaiseShirk,
        MoveForward,
        MoveBack,
        AntiKnockback,
        Burst,
        Speed,
        LimitBreak,
        NoCasting,
    }
}
