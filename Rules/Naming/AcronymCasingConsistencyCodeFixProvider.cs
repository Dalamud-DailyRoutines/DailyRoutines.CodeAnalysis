using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis.Rename;

namespace DailyRoutines.CodeAnalysis.Rules.Naming;

/// <summary>
/// 代码修复提供程序：修复英文缩写大小写不一致问题
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AcronymCasingConsistencyCodeFixProvider)), Shared]
public class AcronymCasingConsistencyCodeFixProvider : BaseCodeFixProvider
{
    protected override ImmutableArray<string> GetFixableDiagnosticIds() =>
        ImmutableArray.Create(DiagnosticRules.AcronymCasingConsistency.Id);

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (model == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // 查找包含缩写的标识符
        var node = root.FindNode(diagnosticSpan);
        if (node == null) return;

        // 获取符号信息
        var symbol = model.GetDeclaredSymbol(node.Parent, context.CancellationToken);
        if (symbol == null) return;

        var oldName = symbol.Name;
        if (string.IsNullOrEmpty(oldName)) return;

        // 生成修复选项
        var fixOptions = GenerateFixOptions(oldName);
        foreach (var (newName, description) in fixOptions)
        {
            if (newName != oldName)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: description,
                        createChangedSolution: c => RenameSymbolAsync(context.Document, symbol, newName, c),
                        equivalenceKey: $"{nameof(AcronymCasingConsistencyCodeFixProvider)}_{newName}"),
                    diagnostic);
            }
        }
    }

    /// <summary>
    /// 生成修复选项
    /// </summary>
    /// <param name="originalName">原始名称</param>
    /// <returns>修复选项列表，包含新名称和描述</returns>
    private static List<(string newName, string description)> GenerateFixOptions(string originalName)
    {
        var options = new List<(string, string)>();
        var upperCaseName = FixAcronymCasing(originalName, true);
        var lowerCaseName = FixAcronymCasing(originalName, false);

        if (upperCaseName != originalName)
        {
            options.Add((upperCaseName, $"将缩写转换为大写: '{originalName}' -> '{upperCaseName}'"));
        }

        if (lowerCaseName != originalName)
        {
            options.Add((lowerCaseName, $"将缩写转换为小写: '{originalName}' -> '{lowerCaseName}'"));
        }

        return options;
    }

    /// <summary>
    /// 修复名称中的缩写大小写
    /// </summary>
    /// <param name="name">原始名称</param>
    /// <param name="useUpperCase">是否使用大写</param>
    /// <returns>修复后的名称</returns>
    private static string FixAcronymCasing(string name, bool useUpperCase)
    {
        var result = name;

        foreach (var acronym in AcronymConstants.CommonAcronyms)
        {
            // 使用正则表达式查找并替换缩写
            var pattern = $@"\b{Regex.Escape(acronym)}\b";
            var replacement = useUpperCase ? acronym.ToUpperInvariant() : acronym.ToLowerInvariant();
            
            result = Regex.Replace(result, pattern, replacement, RegexOptions.IgnoreCase);
        }

        return result;
    }

    /// <summary>
    /// 重命名符号
    /// </summary>
    /// <param name="document">文档</param>
    /// <param name="symbol">符号</param>
    /// <param name="newName">新名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>修改后的解决方案</returns>
    private static async Task<Solution> RenameSymbolAsync(Document document, ISymbol symbol, string newName, CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        var options = solution.Workspace.Options;

        // 使用工作区选项进行重命名
        return await Renamer.RenameSymbolAsync(solution, symbol, newName, options, cancellationToken).ConfigureAwait(false);
    }
}
