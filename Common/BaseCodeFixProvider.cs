using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;

namespace DailyRoutines.CodeAnalysis.Common;

/// <summary>
///     代码修复提供者的基类，提供常用功能
/// </summary>
public abstract class BaseCodeFixProvider : CodeFixProvider
{
    /// <summary>
    ///     获取此提供程序可以修复的诊断ID
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => GetFixableDiagnosticIds();

    /// <summary>
    ///     获取可以使用批量修复的提供程序
    /// </summary>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>
    ///     子类必须实现此方法以提供它们可以修复的诊断ID列表
    /// </summary>
    protected abstract ImmutableArray<string> GetFixableDiagnosticIds();
}
