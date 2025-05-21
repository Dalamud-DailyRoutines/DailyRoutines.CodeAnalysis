using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace DailyRoutines.CodeAnalysis.Rules.Naming;

/// <summary>
/// 代码修复提供程序：将标识符转换为驼峰命名法
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NameMustFollowCamelCaseConventionCodeFixProvider)), Shared]
public class NameMustFollowCamelCaseConventionCodeFixProvider : BaseCodeFixProvider
{
    protected override ImmutableArray<string> GetFixableDiagnosticIds() =>
        ImmutableArray.Create(DiagnosticRules.NameMustFollowCamelCaseConvention.Id);

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // 获取需要修复的标识符
        var token = root.FindToken(diagnosticSpan.Start);
        var identifierName = token.ValueText;

        if (string.IsNullOrEmpty(identifierName)) return;

        // 生成大驼峰命名的修复选项
        var pascalCaseName = ToPascalCase(identifierName);
        if (pascalCaseName != identifierName)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    $"使用大驼峰命名法: '{pascalCaseName}'",
                    c => RenameSymbolAsync(context.Document, token, pascalCaseName, c),
                    equivalenceKey: $"{nameof(NameMustFollowCamelCaseConventionCodeFixProvider)}_Pascal"),
                diagnostic);
        }

        // 生成小驼峰命名的修复选项
        var camelCaseName = ToCamelCase(identifierName);
        if (camelCaseName != identifierName && camelCaseName != pascalCaseName)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    $"使用小驼峰命名法: '{camelCaseName}'",
                    c => RenameSymbolAsync(context.Document, token, camelCaseName, c),
                    equivalenceKey: $"{nameof(NameMustFollowCamelCaseConventionCodeFixProvider)}_Camel"),
                diagnostic);
        }
    }

    /// <summary>
    /// 将标识符转换为大驼峰命名法(PascalCase)
    /// </summary>
    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // 处理以下划线开头的情况
        var startIndex = name.StartsWith("_") ? 1 : 0;
        if (startIndex >= name.Length)
            return name;

        // 处理全大写的情况
        if (name.All(c => !char.IsLetter(c) || char.IsUpper(c)))
        {
            name = name.ToLower();
        }

        // 分割词组 (下划线、连字符或大小写转换点)
        var parts = Regex.Split(name.Substring(startIndex), @"[_\-]|(?<=[a-z])(?=[A-Z])|(?<=.)(?=[0-9])|(?<=[0-9])(?=.)");

        // 转换每个部分为首字母大写
        var pascalCase = string.Join("", parts
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => char.ToUpper(p[0], CultureInfo.InvariantCulture) + p.Substring(1).ToLower()));

        return pascalCase;
    }

    /// <summary>
    /// 将标识符转换为小驼峰命名法(camelCase)
    /// </summary>
    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var pascalCase = ToPascalCase(name);
        
        if (string.IsNullOrEmpty(pascalCase) || pascalCase.Length < 2)
            return pascalCase;

        // 将第一个字母转换为小写
        return char.ToLower(pascalCase[0], CultureInfo.InvariantCulture) + pascalCase.Substring(1);
    }

    /// <summary>
    /// 执行符号重命名操作
    /// </summary>
    private static async Task<Solution> RenameSymbolAsync(Document document, SyntaxToken token, string newName, CancellationToken cancellationToken)
    {
        // 获取语义模型
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null) return document.Project.Solution;

        // 获取要重命名的符号
        var symbol = semanticModel.GetDeclaredSymbol(token.Parent, cancellationToken);
        if (symbol == null) return document.Project.Solution;

        // 执行重命名操作
        var solution = document.Project.Solution;
        var options = solution.Workspace.Options;
        
        // 使用工作区选项作为OptionSet参数
        return await Renamer.RenameSymbolAsync(
            solution,
            symbol,
            newName,
            options,
            cancellationToken).ConfigureAwait(false);
    }
} 