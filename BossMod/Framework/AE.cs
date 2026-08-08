using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
namespace BossMod;

// BossMod 消费 AEAssist 的 IPC 端点，实现 AEAssist → BossMod 联动（期望身位）。
// AEAssist 侧框架层通过 MeleePositionalTable 自动推算"下一个 GCD 的期望身位"，
// 经 DesiredPositionalIpc 暴露以下端点（与 RSR 命名对齐）：
//   AEAssist.GetDesiredPositional                    () -> byte  轮询（0=None, 1=Rear, 2=Flank）
//   AEAssist.ActionUpdater.DesiredPositionalChanged  (byte) -> void  事件推送
public sealed class AEAssistModule : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ICallGateSubscriber<byte> _getDesiredPositional;
    private readonly ICallGateSubscriber<byte, object?> _desiredPositionalChanged;
    private const string aeassist = "AEAssistV3";

    // 当前 AEAssist 期望身位，通过 DesiredPositionalChanged 事件保持更新
    public Positional DesiredPositional { get; private set; } = Positional.Any;

    public event Action<Positional>? DesiredPositionalChanged;

    public AEAssistModule(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
        _getDesiredPositional = pluginInterface.GetIpcSubscriber<byte>("AEAssist.GetDesiredPositional");
        _desiredPositionalChanged = pluginInterface.GetIpcSubscriber<byte, object?>("AEAssist.ActionUpdater.DesiredPositionalChanged");
        try
        {
            _desiredPositionalChanged.Subscribe(OnDesiredPositionalChanged);
        }
        catch
        {
            // AEAssist 未安装/未加载 - 忽略，仍可轮询 GetDesiredPositional()
        }
        DesiredPositional = GetDesiredPositional();
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

    private void OnDesiredPositionalChanged(byte value)
    {
        DesiredPositional = MapPositional(value);
        DesiredPositionalChanged?.Invoke(DesiredPositional);
    }

    // 与 AEAssist MeleePositionalTable 的编码对齐：1=Rear, 2=Flank, 3=Front（AE 目前无 Front）
    private static Positional MapPositional(byte value) => value switch
    {
        1 => Positional.Rear,
        2 => Positional.Flank,
        3 => Positional.Front,
        _ => Positional.Any,
    };
}
