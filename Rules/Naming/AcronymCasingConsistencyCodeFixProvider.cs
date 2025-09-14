using System;
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
        // 将驼峰命名分割成单词
        var words = SplitCamelCase(name);
        var fixedWords = new List<string>();

        foreach (var word in words)
        {
            var fixedWord = word;
            
            // 检查每个单词是否完全匹配缩写列表（忽略大小写）
            foreach (var acronym in AcronymConstants.CommonAcronyms)
            {
                if (string.Equals(word, acronym, StringComparison.OrdinalIgnoreCase))
                {
                    fixedWord = useUpperCase ? acronym.ToUpperInvariant() : acronym.ToLowerInvariant();
                    break;
                }
            }
            
            fixedWords.Add(fixedWord);
        }

        // 重新组合单词
        return string.Join("", fixedWords);
    }

    /// <summary>
    /// 将驼峰命名分割成单词
    /// </summary>
    /// <param name="name">标识符名称</param>
    /// <returns>分割后的单词列表</returns>
    private static List<string> SplitCamelCase(string name)
    {
        var words = new List<string>();
        if (string.IsNullOrEmpty(name))
            return words;

        // 使用正则表达式分割驼峰命名
        // 匹配模式：
        // 1. 连续的大写字母后跟小写字母：XMLHttp -> XML, Http
        // 2. 小写字母后跟大写字母：iconId -> icon, Id
        // 3. 数字和字母的分界
        // 4. 下划线分隔
        var pattern = @"(?<!^)(?=[A-Z][a-z])|(?<=[a-z])(?=[A-Z])|(?<=[0-9])(?=[A-Za-z])|(?<=[A-Za-z])(?=[0-9])|_";
        var parts = Regex.Split(name, pattern);

        foreach (var part in parts)
        {
            if (!string.IsNullOrEmpty(part) && part != "_")
            {
                words.Add(part);
            }
        }

        return words;
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
