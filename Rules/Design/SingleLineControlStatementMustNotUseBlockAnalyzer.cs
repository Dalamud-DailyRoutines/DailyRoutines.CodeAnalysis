using System.Collections.Immutable;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DailyRoutines.CodeAnalysis.Rules.Design;

/// <summary>
///     分析器：只有一行代码的控制语句不应使用大括号
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SingleLineControlStatementMustNotUseBlockAnalyzer() : BaseAnalyzer(DiagnosticRules.SingleLineControlStatementMustNotUseBlock)
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
        SyntaxKind.LockStatement);

    protected override void RegisterAnalyzers(AnalysisContext context) =>
        context.RegisterSyntaxNodeAction(AnalyzeNode, SupportedStatementKinds);

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var             node          = context.Node;
        StatementSyntax body          = null;
        var             statementType = GetStatementTypeName(node.Kind());

        body = node switch
        {
            // 获取控制语句的语句体
            IfStatementSyntax ifStatement           => ifStatement.Statement,
            ElseClauseSyntax elseClause             => elseClause.Statement,
            ForStatementSyntax forStatement         => forStatement.Statement,
            ForEachStatementSyntax forEachStatement => forEachStatement.Statement,
            WhileStatementSyntax whileStatement     => whileStatement.Statement,
            DoStatementSyntax doStatement           => doStatement.Statement,
            UsingStatementSyntax usingStatement     => usingStatement.Statement,
            LockStatementSyntax lockStatement       => lockStatement.Statement,
            _                                       => body
        };

        // 检查语句体是否是一个块，且块内只有一个语句
        if (body is BlockSyntax { Statements.Count: 1 } blockSyntax) 
            ReportDiagnostic(context, blockSyntax.GetLocation(), statementType);
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
            _                           => "控制"
        };
    }
}
