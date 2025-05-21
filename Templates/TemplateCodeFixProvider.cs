// 注意：此文件为模板，用于创建新规则时参考
// 使用方法：复制此模板并重命名，替换对应的占位符
// 规则类别请放在对应的文件夹下（Naming、Usage、Performance等）

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace DailyRoutines.CodeAnalysis.Templates;

/// <summary>
///     代码修复提供程序：[修复描述]
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExampleRuleCodeFixProvider)), Shared]
public class ExampleRuleCodeFixProvider : BaseCodeFixProvider
{
    protected override ImmutableArray<string> GetFixableDiagnosticIds() =>
        ImmutableArray.Create(DiagnosticRules.ExampleRule.Id); // 替换为对应的规则ID

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic     = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // 找到需要修复的节点
        var nodeToFix = root.FindNode(diagnosticSpan);
        if (nodeToFix == null) return;

        // 注册代码修复
        context.RegisterCodeFix(
            CodeAction.Create(
                "应用修复", // 修复的描述
                c => ApplyFixAsync(context.Document, nodeToFix, c),
                nameof(ExampleRuleCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        // 这里实现具体的修复逻辑，比如：
        // 1. 创建新的修复后的节点
        // var newNode = ...

        // 2. 替换节点
        // var newRoot = root.ReplaceNode(nodeToFix, newNode);

        // 3. 返回更新后的文档
        // return document.WithSyntaxRoot(newRoot);

        return document; // 替换为实际的修复逻辑
    }
}
