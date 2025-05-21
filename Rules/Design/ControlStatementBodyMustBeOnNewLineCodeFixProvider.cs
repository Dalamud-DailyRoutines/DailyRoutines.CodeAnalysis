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
/// 代码修复提供程序：将控制语句的语句体移到新行
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ControlStatementBodyMustBeOnNewLineCodeFixProvider)), Shared]
public class ControlStatementBodyMustBeOnNewLineCodeFixProvider : BaseCodeFixProvider
{
    protected override ImmutableArray<string> GetFixableDiagnosticIds() =>
        ImmutableArray.Create(DiagnosticRules.ControlStatementBodyMustBeOnNewLine.Id);

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // 找到需要修复的语句
        var statement = root.FindNode(diagnosticSpan) as StatementSyntax;
        if (statement == null) return;

        // 注册代码修复
        context.RegisterCodeFix(
            CodeAction.Create(
                "将语句体移到新行",
                c => ApplyFixAsync(context.Document, statement, c),
                nameof(ControlStatementBodyMustBeOnNewLineCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(Document document, StatementSyntax statement, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        // 获取父节点，即控制语句
        var parentNode = statement.Parent;
        if (parentNode == null) return document;

        // 根据不同的控制语句类型进行处理
        SyntaxNode newParentNode = null;

        switch (parentNode)
        {
            case IfStatementSyntax ifStatement:
            {
                // 为语句添加换行和缩进
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace("    "))); // 4个空格的缩进

                newParentNode = ifStatement.WithStatement(newStatement);
                break;
            }
            case ElseClauseSyntax elseClause:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace("    ")));

                newParentNode = elseClause.WithStatement(newStatement);
                break;
            }
            case ForStatementSyntax forStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace("    ")));

                newParentNode = forStatement.WithStatement(newStatement);
                break;
            }
            case ForEachStatementSyntax forEachStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace("    ")));

                newParentNode = forEachStatement.WithStatement(newStatement);
                break;
            }
            case WhileStatementSyntax whileStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace("    ")));

                newParentNode = whileStatement.WithStatement(newStatement);
                break;
            }
            case DoStatementSyntax doStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace("    ")));

                newParentNode = doStatement.WithStatement(newStatement);
                break;
            }
            case UsingStatementSyntax usingStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace("    ")));

                newParentNode = usingStatement.WithStatement(newStatement);
                break;
            }
            case LockStatementSyntax lockStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace("    ")));

                newParentNode = lockStatement.WithStatement(newStatement);
                break;
            }
        }

        if (newParentNode == null) return document;

        // 替换节点
        var newRoot = root.ReplaceNode(parentNode, newParentNode);

        // 应用格式化
        return document.WithSyntaxRoot(newRoot);
    }
}
