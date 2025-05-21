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

        // 获取父节点的缩进
        string parentIndentation = GetIndentationWhitespace(parentNode);
        // 标准缩进是4个空格
        string additionalIndentation = "    ";

        // 根据不同的控制语句类型进行处理
        SyntaxNode newParentNode = null;

        switch (parentNode)
        {
            case IfStatementSyntax ifStatement:
            {
                // 为语句添加换行和缩进，保留原有的缩进并添加4个空格作为额外缩进
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace(parentIndentation + additionalIndentation))); 

                newParentNode = ifStatement.WithStatement(newStatement);
                break;
            }
            case ElseClauseSyntax elseClause:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace(parentIndentation + additionalIndentation)));

                newParentNode = elseClause.WithStatement(newStatement);
                break;
            }
            case ForStatementSyntax forStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace(parentIndentation + additionalIndentation)));

                newParentNode = forStatement.WithStatement(newStatement);
                break;
            }
            
            case ForEachStatementSyntax forEachStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace(parentIndentation + additionalIndentation)));

                newParentNode = forEachStatement.WithStatement(newStatement);
                break;
            }
            case WhileStatementSyntax whileStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace(parentIndentation + additionalIndentation)));

                newParentNode = whileStatement.WithStatement(newStatement);
                break;
            }
            case DoStatementSyntax doStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace(parentIndentation + additionalIndentation)));

                newParentNode = doStatement.WithStatement(newStatement);
                break;
            }
            case UsingStatementSyntax usingStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace(parentIndentation + additionalIndentation)));

                newParentNode = usingStatement.WithStatement(newStatement);
                break;
            }
            case LockStatementSyntax lockStatement:
            {
                var newStatement = statement.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\r\n"),
                        SyntaxFactory.Whitespace(parentIndentation + additionalIndentation)));

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

    /// <summary>
    /// 从节点的前导三项符中提取缩进空白
    /// </summary>
    /// <param name="node">需要提取缩进的节点</param>
    /// <returns>该节点的缩进空白字符串</returns>
    private static string GetIndentationWhitespace(SyntaxNode node)
    {
        // 获取节点的所有前导trivia
        var leadingTrivia = node.GetLeadingTrivia();
        
        // 查找最后一个换行符后面的空白
        for (int i = leadingTrivia.Count - 1; i >= 0; i--)
        {
            if (leadingTrivia[i].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                // 找到换行符，看它后面是否有空白
                if (i + 1 < leadingTrivia.Count && leadingTrivia[i + 1].IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    return leadingTrivia[i + 1].ToString();
                }
                // 找到换行符但后面没有空白，返回空字符串
                return string.Empty;
            }
        }
        
        // 没有找到换行符，尝试直接获取开头的空白
        if (leadingTrivia.Count > 0 && leadingTrivia[0].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            return leadingTrivia[0].ToString();
        }
        
        // 没有找到任何缩进
        return string.Empty;
    }
}
