using System;
using Microsoft.CodeAnalysis;

namespace DailyRoutines.CodeAnalysis.Common;

/// <summary>
///     用于创建诊断描述符的工厂类，确保所有诊断描述符格式一致
/// </summary>
public static class DiagnosticDescriptorFactory
{
    /// <summary>
    ///     DR项目诊断ID前缀
    /// </summary>
    public const string DiagnosticIdPrefix = "DR";

    /// <summary>
    ///     创建标准诊断描述符
    /// </summary>
    /// <param name="id">规则ID (不含前缀)</param>
    /// <param name="title">规则标题</param>
    /// <param name="messageFormat">诊断消息格式</param>
    /// <param name="category">诊断类别</param>
    /// <param name="defaultSeverity">默认严重程度</param>
    /// <param name="description">详细描述</param>
    /// <param name="isEnabledByDefault">是否默认启用</param>
    /// <returns>标准格式的诊断描述符</returns>
    public static DiagnosticDescriptor Create
    (
        string             id,
        string             title,
        string             messageFormat,
        string             category,
        DiagnosticSeverity defaultSeverity    = DiagnosticSeverity.Warning,
        string             description        = null,
        bool               isEnabledByDefault = true
    )
    {
        // 确保ID格式标准化：DR#### 形式
        var diagnosticId = id.StartsWith(DiagnosticIdPrefix, StringComparison.Ordinal)
                               ? id
                               : $"{DiagnosticIdPrefix}{id.PadLeft(4, '0')}";

        return new DiagnosticDescriptor
        (
            diagnosticId,
            title,
            messageFormat,
            category,
            defaultSeverity,
            isEnabledByDefault,
            description ?? title
        );
    }
}
