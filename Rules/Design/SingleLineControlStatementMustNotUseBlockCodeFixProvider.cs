using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DailyRoutines.CodeAnalysis.Rules.Design;

/// <summary>
/// 代码修复提供程序：移除只有一行代码的控制语句的大括号
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SingleLineControlStatementMustNotUseBlockCodeFixProvider)), Shared]
public class SingleLineControlStatementMustNotUseBlockCodeFixProvider : BaseCodeFixProvider
{
    protected override ImmutableArray<string> GetFixableDiagnosticIds() =>
        ImmutableArray.Create(DiagnosticRules.SingleLineControlStatementMustNotUseBlock.Id);

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // 找到需要修复的块
        var blockNode = root.FindNode(diagnosticSpan) as BlockSyntax;
        if (blockNode == null || blockNode.Statements.Count != 1) return;

        // 注册代码修复
        context.RegisterCodeFix(
            CodeAction.Create(
                "移除大括号",
                c => ApplyFixAsync(context.Document, blockNode, c),
                nameof(SingleLineControlStatementMustNotUseBlockCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(Document document, BlockSyntax blockNode, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null || blockNode.Statements.Count != 1) return document;

        // 获取块内的唯一语句
        var statement = blockNode.Statements[0];

        // 保留语句的缩进
        var leadingTrivia = SyntaxFactory.TriviaList(
            blockNode.GetLeadingTrivia()
                .Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia) || t.IsKind(SyntaxKind.EndOfLineTrivia))
                .Concat([SyntaxFactory.Whitespace("    ")])); // 添加额外的缩进

        // 添加语句的缩进
        var newStatement = statement
            .WithLeadingTrivia(leadingTrivia)
            .WithTrailingTrivia(statement.GetTrailingTrivia());

        // 替换节点
        var newRoot = root.ReplaceNode(blockNode, newStatement);

        // 应用格式化
        return document.WithSyntaxRoot(newRoot);
    }
}
