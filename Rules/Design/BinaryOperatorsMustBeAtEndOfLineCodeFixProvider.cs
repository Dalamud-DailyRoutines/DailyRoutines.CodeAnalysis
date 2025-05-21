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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace DailyRoutines.CodeAnalysis.Rules.Design;

/// <summary>
///     代码修复提供程序：修复二元运算符在行首的问题
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BinaryOperatorsMustBeAtEndOfLineCodeFixProvider)), Shared]
public class BinaryOperatorsMustBeAtEndOfLineCodeFixProvider : BaseCodeFixProvider
{
    protected override ImmutableArray<string> GetFixableDiagnosticIds() =>
        ImmutableArray.Create(DiagnosticRules.BinaryOperatorsMustBeAtEndOfLine.Id);

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // 找到运算符token
        var operatorToken = root.FindToken(diagnosticSpan.Start);
        if (operatorToken == default) return;

        // 找到包含此运算符的二元表达式
        var binaryExpression = operatorToken.Parent as BinaryExpressionSyntax;
        if (binaryExpression == null) return;

        // 注册代码修复
        context.RegisterCodeFix(
            CodeAction.Create(
                "将运算符移至行尾",
                c => MoveOperatorToEndOfLineAsync(context.Document, binaryExpression, operatorToken, c),
                nameof(BinaryOperatorsMustBeAtEndOfLineCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> MoveOperatorToEndOfLineAsync(
        Document document,
        BinaryExpressionSyntax binaryExpression,
        SyntaxToken operatorToken,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        // 获取源文本
        var sourceText = await document.GetTextAsync(cancellationToken);
        
        // 获取运算符所在行和左表达式结束行
        var operatorSpan = operatorToken.GetLocation().GetLineSpan();
        var operatorLine = operatorSpan.StartLinePosition.Line;
        
        var leftSpan = binaryExpression.Left.GetLocation().GetLineSpan();
        var leftLine = leftSpan.EndLinePosition.Line;

        // 已确认运算符在新行的开头，需要移至上一行末尾
        if (operatorLine > leftLine)
        {
            // 获取左表达式行结束位置
            var leftLineEndPos = sourceText.Lines[leftLine].End;
            
            // 获取运算符和周围空白
            var operatorWithTrivia = operatorToken.ToString();
            
            // 创建文本编辑: 在左表达式行末添加运算符
            var textChanges = new[]
            {
                // 在左表达式行末添加运算符
                new TextChange(new TextSpan(leftLineEndPos, 0), " " + operatorToken.Text),
                
                // 移除原运算符及其前导空白
                new TextChange(
                    new TextSpan(
                        sourceText.Lines[operatorLine].Start,
                        operatorToken.Span.End - sourceText.Lines[operatorLine].Start
                    ),
                    "")
            };

            // 应用文本更改
            var newText = sourceText.WithChanges(textChanges);
            
            // 创建新的文档
            return document.WithText(newText);
        }

        return document;
    }
} 