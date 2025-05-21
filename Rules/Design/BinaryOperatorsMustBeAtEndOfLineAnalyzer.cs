using System.Collections.Immutable;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace DailyRoutines.CodeAnalysis.Rules.Design;

/// <summary>
///     分析器：检查二元运算符是否在行尾而不是行首
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BinaryOperatorsMustBeAtEndOfLineAnalyzer : BaseAnalyzer
{
    // 支持检查的二元表达式类型
    private static readonly ImmutableArray<SyntaxKind> BinaryExpressionKinds = ImmutableArray.Create(
        SyntaxKind.AddExpression,
        SyntaxKind.SubtractExpression,
        SyntaxKind.MultiplyExpression,
        SyntaxKind.DivideExpression,
        SyntaxKind.ModuloExpression,
        SyntaxKind.LeftShiftExpression,
        SyntaxKind.RightShiftExpression,
        SyntaxKind.LogicalOrExpression,
        SyntaxKind.LogicalAndExpression,
        SyntaxKind.BitwiseOrExpression,
        SyntaxKind.BitwiseAndExpression,
        SyntaxKind.ExclusiveOrExpression,
        SyntaxKind.CoalesceExpression);
        
    public BinaryOperatorsMustBeAtEndOfLineAnalyzer()
        : base(DiagnosticRules.BinaryOperatorsMustBeAtEndOfLine)
    {
    }

    protected override void RegisterAnalyzers(AnalysisContext context)
    {
        // 为所有支持的二元表达式类型注册分析器
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, BinaryExpressionKinds);
    }

    private void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        if (!(context.Node is BinaryExpressionSyntax binaryExpression))
            return;

        // 获取运算符的token
        var operatorToken = binaryExpression.OperatorToken;
        
        // 获取运算符的位置
        var operatorSpan = operatorToken.GetLocation().GetLineSpan();
        var operatorStartLine = operatorSpan.StartLinePosition.Line;
        var operatorStartChar = operatorSpan.StartLinePosition.Character;
        
        // 获取运算符前面的表达式的位置
        var leftSpan = binaryExpression.Left.GetLocation().GetLineSpan();
        var leftEndLine = leftSpan.EndLinePosition.Line;
        
        // 如果运算符在新行上（即运算符所在行与左侧表达式结束行不同）
        // 并且运算符所在行只有空白字符和运算符，则认为它在行首
        if (operatorStartLine > leftEndLine)
        {
            // 获取运算符所在行的起始位置
            var lineStart = operatorToken.GetLocation().SourceTree.GetText().Lines[operatorStartLine].Start;
            
            // 获取行开始到运算符前的文本，检查是否只有空白
            var lineText = operatorToken.GetLocation().SourceTree.GetText().ToString(
                new TextSpan(lineStart, operatorToken.SpanStart - lineStart));
            
            // 如果行前缀只包含空白字符，则运算符在行首
            if (string.IsNullOrWhiteSpace(lineText))
            {
                // 报告诊断
                ReportDiagnostic(
                    context,
                    operatorToken.GetLocation(),
                    operatorToken.Text);
            }
        }
    }
} 