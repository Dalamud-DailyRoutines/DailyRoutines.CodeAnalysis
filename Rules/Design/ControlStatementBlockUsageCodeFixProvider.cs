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
/// 代码修复提供程序：控制语句的大括号使用规范
/// - 移除只有一行代码的控制语句的大括号
/// - 为多行代码的控制语句添加大括号
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ControlStatementBlockUsageCodeFixProvider)), Shared]
public class ControlStatementBlockUsageCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(
            DiagnosticRules.SingleLineControlStatementMustNotUseBlock.Id,
            DiagnosticRules.MultiLineControlStatementMustUseBlock.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // 根据诊断ID提供不同的修复选项
        if (diagnostic.Id == DiagnosticRules.SingleLineControlStatementMustNotUseBlock.Id)
        {
            // 单行代码不使用大括号的情况
            // 找到需要修复的块
            var blockNode = root.FindNode(diagnosticSpan) as BlockSyntax;
            if (blockNode == null || blockNode.Statements.Count != 1) return;

            // 注册代码修复
            context.RegisterCodeFix(
                CodeAction.Create(
                    "移除大括号",
                    c => RemoveBlockAsync(context.Document, blockNode, c),
                    nameof(ControlStatementBlockUsageCodeFixProvider)),
                diagnostic);
        }
        else if (diagnostic.Id == DiagnosticRules.MultiLineControlStatementMustUseBlock.Id)
        {
            // 多行代码使用大括号的情况
            // 找到需要修复的语句
            var statementNode = root.FindNode(diagnosticSpan) as StatementSyntax;
            if (statementNode == null) return;

            // 注册代码修复
            context.RegisterCodeFix(
                CodeAction.Create(
                    "添加大括号",
                    c => AddBlockAsync(context.Document, statementNode, c),
                    nameof(ControlStatementBlockUsageCodeFixProvider)),
                diagnostic);
        }
    }

    private static async Task<Document> RemoveBlockAsync(Document document, BlockSyntax blockNode, CancellationToken cancellationToken)
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
    
    private static async Task<Document> AddBlockAsync(Document document, StatementSyntax statementNode, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;
        
        // 创建新的块语句
        var block = SyntaxFactory.Block(statementNode)
            .WithLeadingTrivia(statementNode.GetLeadingTrivia())
            .WithTrailingTrivia(statementNode.GetTrailingTrivia());
        
        // 重新格式化语句的缩进
        // 这样语句会在大括号内有正确的缩进
        var formattedStatement = statementNode
            .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    ")))
            .WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\r\n")));
        
        // 创建新的块，包含格式化后的语句
        var formattedBlock = SyntaxFactory.Block(formattedStatement)
            .WithLeadingTrivia(statementNode.GetLeadingTrivia())
            .WithTrailingTrivia(statementNode.GetTrailingTrivia());
        
        // 替换节点
        var newRoot = root.ReplaceNode(statementNode, formattedBlock);
        
        // 应用格式化
        return document.WithSyntaxRoot(newRoot);
    }
}
