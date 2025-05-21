using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DailyRoutines.CodeAnalysis.Rules.Design;

/// <summary>
/// 分析器：控制语句的语句体必须另起一行
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ControlStatementBodyMustBeOnNewLineAnalyzer() : BaseAnalyzer(DiagnosticRules.ControlStatementBodyMustBeOnNewLine)
{
    // 支持的控制语句类型
    private static readonly ImmutableArray<SyntaxKind> SupportedStatementKinds = ImmutableArray.Create(
        SyntaxKind.IfStatement,
        SyntaxKind.ElseClause,
        SyntaxKind.ForStatement,
        SyntaxKind.ForEachStatement,
        SyntaxKind.WhileStatement,
        SyntaxKind.DoStatement,
        SyntaxKind.UsingStatement,
        SyntaxKind.LockStatement,
        SyntaxKind.SwitchStatement
    );

    protected override void RegisterAnalyzers(AnalysisContext context) => 
        context.RegisterSyntaxNodeAction(AnalyzeNode, SupportedStatementKinds);

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;
        var statementType = GetStatementTypeName(node.Kind());
        
        switch (node)
        {
            case IfStatementSyntax ifStatement:
                CheckStatement(context, ifStatement.Statement, ifStatement.CloseParenToken, statementType);
                break;
            case ElseClauseSyntax elseClause:
                // 特殊处理 else if 结构，这种情况下不要求将if移到新行
                if (elseClause.Statement is IfStatementSyntax)
                {
                    // else if 结构是允许的，不检查
                }
                else
                    CheckStatement(context, elseClause.Statement, elseClause.ElseKeyword, statementType);
                break;
            case ForStatementSyntax forStatement:
                CheckStatement(context, forStatement.Statement, forStatement.CloseParenToken, statementType);
                break;
            case ForEachStatementSyntax forEachStatement:
                CheckStatement(context, forEachStatement.Statement, forEachStatement.CloseParenToken, statementType);
                break;
            case WhileStatementSyntax whileStatement:
                CheckStatement(context, whileStatement.Statement, whileStatement.CloseParenToken, statementType);
                break;
            case DoStatementSyntax doStatement:
                CheckStatement(context, doStatement.Statement, doStatement.DoKeyword, statementType);
                break;
            case UsingStatementSyntax usingStatement:
                CheckStatement(context, usingStatement.Statement, usingStatement.CloseParenToken, statementType);
                break;
            case LockStatementSyntax lockStatement:
                CheckStatement(context, lockStatement.Statement, lockStatement.CloseParenToken, statementType);
                break;
            case SwitchStatementSyntax:
                // Switch语句的大括号应该在新行，但我们这里不做分析
                // 因为Switch语句总是使用大括号块
                break;
        }
    }

    private void CheckStatement(SyntaxNodeAnalysisContext context, StatementSyntax statement, SyntaxToken precedingToken, string statementType)
    {
        if (statement == null) return;

        // 对于一些特定的简单跳转语句，允许它们出现在控制语句的同一行
        if (IsExemptedStatementType(statement))
        {
            // 这些语句类型可以和控制语句在同一行，不报告诊断问题
            return;
        }

        // 获取前置标记和语句体的位置信息
        var precedingTokenLine = precedingToken.GetLocation().GetLineSpan().EndLinePosition.Line;
        var statementStartLine = statement.GetLocation().GetLineSpan().StartLinePosition.Line;

        // 检查语句体是否与控制语句在同一行
        if (precedingTokenLine == statementStartLine)
        {
            ReportDiagnostic(context, statement.GetLocation(), statementType);
        }
    }

    // 检查是否是豁免的语句类型（允许与控制语句在同一行）
    private bool IsExemptedStatementType(StatementSyntax statement)
    {
        return statement switch
        {
            // 允许 return 语句
            ReturnStatementSyntax => true,
            // 允许 continue 语句
            ContinueStatementSyntax => true,
            // 允许 break 语句
            BreakStatementSyntax => true,
            // 允许 goto 语句
            GotoStatementSyntax => true,
            // 其他所有语句类型都需要遵循规则
            _ => false
        };
    }

    private string GetStatementTypeName(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.IfStatement      => "if",
            SyntaxKind.ElseClause       => "else",
            SyntaxKind.ForStatement     => "for",
            SyntaxKind.ForEachStatement => "foreach",
            SyntaxKind.WhileStatement   => "while",
            SyntaxKind.DoStatement      => "do",
            SyntaxKind.UsingStatement   => "using",
            SyntaxKind.LockStatement    => "lock",
            SyntaxKind.SwitchStatement  => "switch",
            _                           => "控制"
        };
    }
} 
