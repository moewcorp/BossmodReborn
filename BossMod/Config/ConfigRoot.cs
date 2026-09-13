using System.IO;
using System.Text.Json;

namespace BossMod;

public sealed class ConfigRoot
{
    public Event Modified = new();
    public readonly Dictionary<Type, ConfigNode> _nodes = [];
    private readonly Dictionary<string, ConfigNode> _nodesByName = [];

    public void Initialize() => GeneratedRegistries.RegisterConfigNodes(RegisterNode);

    private void RegisterNode(Type type, ConfigNode node)
    {
        node.Modified.Subscribe(Modified.Fire);
        _nodes[type] = node;
        if (type.FullName is { } fullName)
        {
            _nodesByName[fullName] = node;
        }
    }

    public T Get<T>() where T : ConfigNode => (T)_nodes[typeof(T)];
    public T Get<T>(Type derived) where T : ConfigNode => (T)_nodes[derived];
    public ConfigListener<T> GetAndSubscribe<T>(Action<T> modified) where T : ConfigNode => new(Get<T>(), modified);

    public void LoadFromFile(FileInfo file)
    {
        try
        {
            var data = ConfigConverter.Schema.Load(file);
            using var json = data.document;
            var ser = Serialization.BuildSerializationOptions();
            foreach (var jconfig in data.payload.EnumerateObject())
            {
                var node = _nodesByName.GetValueOrDefault(jconfig.Name);
                try
                {
                    node?.Deserialize(jconfig.Value, ser);
                }
                catch (AggregateException exc)
                {
                    Service.Logger.Warning(exc, "An error occurred while deserializing the plugin config. As a result, some settings may have unexpected values.");
                }
            }
        }
        catch (Exception e)
        {
            Service.Log($"Failed to load config from {file.FullName}: {e}");
        }
    }

    public void SaveToFile(FileInfo file)
    {
        try
        {
            var ser = Serialization.BuildSerializationOptions();
            var serializedNodes = new ConcurrentDictionary<Type, string>();
            Parallel.ForEach(_nodes, entry =>
            {
                using var ms = new MemoryStream();
                using var tempWriter = new Utf8JsonWriter(ms);
                entry.Value.Serialize(tempWriter, ser);
                tempWriter.Flush();
                serializedNodes[entry.Key] = Encoding.UTF8.GetString(ms.ToArray());
            });

            using var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            writer.WriteStartObject();
            writer.WriteNumber("Version", ConfigConverter.Schema.CurrentVersion);
            writer.WritePropertyName("Payload");
            writer.WriteStartObject();
            foreach (var (type, json) in serializedNodes)
            {
                writer.WritePropertyName(type.FullName!);
                writer.WriteRawValue(json);
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        catch (Exception e)
        {
            Service.Log($"Failed to save config to {file.FullName}: {e}");
        }
    }

    public List<string> ConsoleCommand(ReadOnlySpan<string> args, bool save = true)
    {
        List<string> result = [];
        if (args.Length == 0)
        {
            result.Add("用法：/bmr cfg <配置类型> <字段> <值>");
            result.Add("配置类型与字段均支持简写。可用的配置类型：");
            foreach (var type in _nodes.Keys)
                result.Add($"- {type.Name}");
            return result;
        }

        List<ConfigNode> matchingNodes = [];
        foreach (var (type, node) in _nodes)
        {
            var arg = args[0];
            if (!type.Name.Contains(arg, StringComparison.CurrentCultureIgnoreCase))
                continue;
            if (type.Name.Length == arg.Length)
            {
                matchingNodes.Clear();
                matchingNodes.Add(node);
                break;
            }
            matchingNodes.Add(node);
        }

        if (matchingNodes.Count == 0)
        {
            result.Add("未找到该配置类型。可用类型：");
            foreach (var type in _nodes.Keys)
                result.Add($"- {type.Name}");
            return result;
        }
        if (matchingNodes.Count > 1)
        {
            result.Add("配置类型不唯一，请提供更长的匹配串。候选：");
            foreach (var node in matchingNodes)
                result.Add($"- {node.GetType().Name}");
            return result;
        }

        var selectedNode = matchingNodes[0];
        var fields = GeneratedConfigMetadata.Get(selectedNode).DisplayFields;
        if (args.Length == 1)
        {
            result.Add("用法：/bmr cfg <配置类型> <字段> <值>");
            result.Add($"{selectedNode.GetType().Name} 的可用字段：");
            foreach (var field in fields)
                result.Add($"- {field.Name}");
            return result;
        }

        List<ConfigFieldMetadata> matchingFields = [];
        foreach (var field in fields)
        {
            var arg = args[1];
            if (!field.Name.Contains(arg, StringComparison.CurrentCultureIgnoreCase))
                continue;
            if (field.Name.Length == arg.Length)
            {
                matchingFields.Clear();
                matchingFields.Add(field);
                break;
            }
            matchingFields.Add(field);
        }

        if (matchingFields.Count == 0)
        {
            result.Add($"未找到字段 {args[1]}，可用字段：");
            foreach (var field in fields)
                result.Add($"- {field.Name}");
            return result;
        }
        if (matchingFields.Count > 1)
        {
            result.Add("字段名不唯一，请提供更长的匹配串。候选：");
            foreach (var field in matchingFields)
                result.Add($"- {field.Name}");
            return result;
        }

        var selectedField = matchingFields[0];
        try
        {
            if (args.Length == 2)
            {
                result.Add(selectedField.Getter(selectedNode)?.ToString() ?? $"无法读取「{selectedField.Name}」的值");
            }
            else
            {
                var value = FromConsoleString(args[2], selectedField.FieldType);
                if (value == null)
                {
                    result.Add($"无法将「{args[2]}」转换为 {selectedField.FieldType}");
                }
                else
                {
                    selectedField.Setter(selectedNode, value);
                    if (save)
                        selectedNode.Modified.Fire();
                }
            }
        }
        catch (Exception e)
        {
            result.Add(args.Length == 2
                ? $"读取 {selectedNode.GetType().Name}.{selectedField.Name} 的值失败：{e}"
                : $"将 {selectedNode.GetType().Name}.{selectedField.Name} 设为 {args[2]} 失败：{e}");
        }
        return result;
    }

    private static object? FromConsoleString(string str, Type type)
        => type == typeof(bool) ? bool.Parse(str)
        : type == typeof(float) ? float.Parse(str)
        : type == typeof(int) ? int.Parse(str)
        : GeneratedEnumMetadata.IsRegistered(type) ? GeneratedEnumMetadata.Parse(type, str)
        : null;
}
