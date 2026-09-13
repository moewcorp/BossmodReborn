using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace BossMod.Autorotation;

public sealed class UIRotationModule
{
    public static void DescribeModule(Type type, RotationModuleDefinition definition)
    {
        ImGui.TextUnformatted(definition.DisplayName);
        ImGui.TextUnformatted(definition.Description);
        ImGui.TextUnformatted($"L{definition.MinLevel}-{definition.MaxLevel} {string.Join(" ", GetClasses(definition))}");
        ImGui.TextUnformatted($"作者/贡献者：{definition.Author}");
        ImGui.TextUnformatted($"质量：{(int)definition.Quality}/{(int)RotationModuleQuality.Count - 1} {GeneratedEnumMetadata.Attribute<PropertyDisplayAttribute>(definition.Quality)?.Label ?? ""}");
        using (ImRaii.Disabled())
        {
            ImGui.TextUnformatted($"职业：{type.FullName}");
            ImGui.TextUnformatted($"排序分组：{definition.Order}");
        }
    }

    private static Class[] GetClasses(RotationModuleDefinition definition)
    {
        var classes = definition.Classes.SetBits();
        var len = classes.Length;
        var strings = new Class[len];
        for (var i = 0; i < len; ++i)
        {
            strings[i] = (Class)classes[i];
        }
        return strings;
    }
}
