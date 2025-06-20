using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QnAutoSend.InjectQn;

public class IniParser
{
    private readonly string _filePath;
    private readonly Dictionary<string, Dictionary<string, string>> _sections;
    private readonly object _lock = new object();
    
    /// <summary>
    /// INI文件编码格式 (默认为UTF-8)
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// 是否保留原始格式和注释 (默认为false)
    /// </summary>
    public bool PreserveFormatting { get; set; } = false;

    /// <summary>
    /// 初始化INI解析器
    /// </summary>
    /// <param name="filePath">INI文件路径</param>
    public IniParser(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        _filePath = Path.GetFullPath(filePath);
        _sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        
        if (File.Exists(_filePath))
        {
            Load();
        }
        else
        {
            // 如果文件不存在，创建空文件
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            File.Create(_filePath).Close();
        }
    }

    /// <summary>
    /// 加载并解析INI文件
    /// </summary>
    public void Load()
    {
        lock (_lock)
        {
            _sections.Clear();
            
            var currentSection = string.Empty;
            _sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadLines(_filePath, Encoding))
            {
                var trimmedLine = line.Trim();
                
                // 跳过空行
                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                // 处理注释
                if (trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                {
                    if (PreserveFormatting)
                    {
                        if (!_sections[currentSection].ContainsKey("__comments__"))
                            _sections[currentSection]["__comments__"] = string.Empty;
                        
                        _sections[currentSection]["__comments__"] += trimmedLine + Environment.NewLine;
                    }
                    continue;
                }

                // 处理节(section)
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    if (!_sections.ContainsKey(currentSection))
                    {
                        _sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    continue;
                }

                // 处理键值对
                var separatorIndex = trimmedLine.IndexOf('=');
                if (separatorIndex > 0)
                {
                    var key = trimmedLine.Substring(0, separatorIndex).Trim();
                    var value = trimmedLine.Substring(separatorIndex + 1).Trim();
                    
                    // 处理引号包裹的值
                    if (value.Length > 1 && value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    
                    _sections[currentSection][key] = value;
                }
                else if (PreserveFormatting)
                {
                    // 保留格式错误的行
                    if (!_sections[currentSection].ContainsKey("__format_errors__"))
                        _sections[currentSection]["__format_errors__"] = string.Empty;
                    
                    _sections[currentSection]["__format_errors__"] += line + Environment.NewLine;
                }
            }
        }
    }

    /// <summary>
    /// 读取指定键的值
    /// </summary>
    /// <param name="section">节名称</param>
    /// <param name="key">键名称</param>
    /// <param name="defaultValue">默认值(当键不存在时返回)</param>
    /// <returns>键对应的值</returns>
    public string GetValue(string section, string key, string defaultValue = null)
    {
        lock (_lock)
        {
            if (_sections.TryGetValue(section ?? string.Empty, out var sectionData) &&
                sectionData.TryGetValue(key, out var value))
            {
                return value;
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// 读取指定键的值并尝试转换为指定类型
    /// </summary>
    public T GetValue<T>(string section, string key, T defaultValue = default)
    {
        var value = GetValue(section, key);
        if (value == null)
            return defaultValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 设置键值对
    /// </summary>
    public void SetValue(string section, string key, string value)
    {
        lock (_lock)
        {
            if (!_sections.ContainsKey(section ?? string.Empty))
            {
                _sections[section ?? string.Empty] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            
            _sections[section ?? string.Empty][key] = value ?? string.Empty;
        }
    }

    /// <summary>
    /// 删除指定键
    /// </summary>
    public void DeleteKey(string section, string key)
    {
        lock (_lock)
        {
            if (_sections.TryGetValue(section ?? string.Empty, out var sectionData))
            {
                sectionData.Remove(key);
            }
        }
    }

    /// <summary>
    /// 删除整个节
    /// </summary>
    public void DeleteSection(string section)
    {
        lock (_lock)
        {
            _sections.Remove(section ?? string.Empty);
        }
    }

    /// <summary>
    /// 获取所有节名称
    /// </summary>
    public IEnumerable<string> GetSections()
    {
        lock (_lock)
        {
            return _sections.Keys.Where(k => !string.IsNullOrEmpty(k)).ToList();
        }
    }

    /// <summary>
    /// 获取指定节下的所有键
    /// </summary>
    public IEnumerable<string> GetKeys(string section)
    {
        lock (_lock)
        {
            if (_sections.TryGetValue(section ?? string.Empty, out var sectionData))
            {
                return sectionData.Keys
                    .Where(k => k != "__comments__" && k != "__format_errors__")
                    .ToList();
            }
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 检查节是否存在
    /// </summary>
    public bool SectionExists(string section)
    {
        lock (_lock)
        {
            return _sections.ContainsKey(section ?? string.Empty);
        }
    }

    /// <summary>
    /// 检查键是否存在
    /// </summary>
    public bool KeyExists(string section, string key)
    {
        lock (_lock)
        {
            return _sections.TryGetValue(section ?? string.Empty, out var sectionData) && 
                   sectionData.ContainsKey(key);
        }
    }

    /// <summary>
    /// 将修改后的内容保存回文件
    /// </summary>
    public void Save()
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            
            // 写入无节部分的内容
            if (_sections.TryGetValue(string.Empty, out var defaultSection))
            {
                WriteSectionContent(sb, defaultSection);
                sb.AppendLine();
            }

            // 写入各节内容
            foreach (var section in _sections.Keys.Where(k => !string.IsNullOrEmpty(k)))
            {
                sb.AppendLine($"[{section}]");
                
                if (_sections.TryGetValue(section, out var sectionData))
                {
                    WriteSectionContent(sb, sectionData);
                }
                
                sb.AppendLine();
            }

            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            
            // 写入文件
            File.WriteAllText(_filePath, sb.ToString(), Encoding);
        }
    }

    private void WriteSectionContent(StringBuilder sb, Dictionary<string, string> sectionData)
    {
        // 写入注释
        if (PreserveFormatting && sectionData.TryGetValue("__comments__", out var comments))
        {
            sb.Append(comments);
        }

        // 写入键值对
        foreach (var kvp in sectionData.Where(kvp => 
            kvp.Key != "__comments__" && kvp.Key != "__format_errors__"))
        {
            sb.AppendLine($"{kvp.Key}={kvp.Value}");
        }

        // 写入格式错误的内容
        if (PreserveFormatting && sectionData.TryGetValue("__format_errors__", out var formatErrors))
        {
            sb.Append(formatErrors);
        }
    }

    /// <summary>
    /// 获取指定节的所有键值对
    /// </summary>
    public IDictionary<string, string> GetSection(string section)
    {
        lock (_lock)
        {
            if (_sections.TryGetValue(section ?? string.Empty, out var sectionData))
            {
                return new Dictionary<string, string>(sectionData
                    .Where(kvp => kvp.Key != "__comments__" && kvp.Key != "__format_errors__")
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }
            return new Dictionary<string, string>();
        }
    }
}