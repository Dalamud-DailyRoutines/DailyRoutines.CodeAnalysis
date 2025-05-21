using System.Collections.Immutable;
using System.Linq;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DailyRoutines.CodeAnalysis.Rules.Design;

/// <summary>
///     分析器：检查控制语句大括号的使用规范
///     1. 只有一行代码的控制语句不应使用大括号
///     2. 多行代码的控制语句必须使用大括号
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ControlStatementBlockUsageAnalyzer : DiagnosticAnalyzer
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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(
            DiagnosticRules.SingleLineControlStatementMustNotUseBlock,
            DiagnosticRules.MultiLineControlStatementMustUseBlock);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeNode, SupportedStatementKinds);
    }

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

        // 特殊处理else if结构
        if (node is ElseClauseSyntax elseClauseNode && elseClauseNode.Statement is IfStatementSyntax)
        {
            // else if结构不需要单独报告诊断，交由if语句本身进行检查
            return;
        }

        // 特殊处理链式using语句
        if (node is UsingStatementSyntax && body is not BlockSyntax)
        {
            // 如果当前using语句的语句体也是using语句（链式using）
            if (body is UsingStatementSyntax)
            {
                var finalStatement = GetFinalStatementInUsingChain(body);
                
                // 如果最终语句是单行语句，则整个链式using都不需要大括号
                if (finalStatement != null && IsSimpleStatement(finalStatement) && IsSingleLineLogic(finalStatement) && !ContainsLambdaBlock(finalStatement))
                    return;
                
                // 否则，只有最后一个using语句需要添加大括号，而当前using是链中的非最后一个，不需要报告诊断
                return;
            }
        }

        // 情况1：检查语句体是否是一个块，且块内只有一个语句 - 单行不应使用大括号
        if (body is BlockSyntax { Statements.Count: 1 } blockSyntax) 
        {
            // 获取块内唯一的语句
            var statement = blockSyntax.Statements[0];
            
            // 如果是嵌套控制语句，不建议移除大括号
            if (!IsSimpleStatement(statement))
                return;
            
            // 如果语句包含lambda表达式的块，不建议移除大括号
            if (ContainsLambdaBlock(statement))
                return;
            
            // 如果是简单语句，检查是否为单行逻辑（实际占一行或特殊多行情况）
            if (IsSingleLineLogic(statement))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticRules.SingleLineControlStatementMustNotUseBlock, 
                        blockSyntax.GetLocation(), 
                        statementType)
                );
            }
        }
        // 情况2：检查语句体不是块但包含多行逻辑 - 多行必须使用大括号
        else if (body != null && !(body is BlockSyntax))
        {
            // 检查是否为复杂语句（控制语句）或包含多行逻辑
            if (!IsSimpleStatement(body) || !IsSingleLineLogic(body) || ContainsLambdaBlock(body))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticRules.MultiLineControlStatementMustUseBlock,
                        body.GetLocation(),
                        statementType)
                );
            }
        }
    }
    
    /// <summary>
    /// 判断语句是否在逻辑上是单行（即使可能跨多行展示）
    /// </summary>
    /// <param name="statement">待判断的语句</param>
    /// <returns>如果语句只占一行或被视为单行逻辑，返回true</returns>
    private static bool IsSingleLineLogic(StatementSyntax statement)
    {
        // 如果是参数多行的方法调用，视为单行逻辑
        if (IsInvocationWithMultiLineArguments(statement))
            return true;
        
        // 获取语句的行范围
        var lineSpan = statement.GetLocation().GetLineSpan();
        var startLine = lineSpan.StartLinePosition.Line;
        var endLine = lineSpan.EndLinePosition.Line;
        
        // 如果语句实际上只占一行，也视为单行逻辑
        return startLine == endLine;
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
    
    /// <summary>
    /// 判断语句是否包含带块的lambda表达式
    /// </summary>
    /// <param name="statement">待判断的语句</param>
    /// <returns>如果包含带块的lambda表达式返回true，否则返回false</returns>
    private static bool ContainsLambdaBlock(StatementSyntax statement)
    {
        // 查找所有lambda表达式
        var lambdas = statement.DescendantNodes().OfType<LambdaExpressionSyntax>();
        
        // 检查任意lambda是否包含块体
        return lambdas.Any(lambda => lambda.Body is BlockSyntax);
    }
    
    /// <summary>
    /// 判断语句是否为简单语句（不是控制流语句）
    /// </summary>
    /// <param name="statement">待判断的语句</param>
    /// <returns>如果是简单语句返回true，否则返回false</returns>
    private static bool IsSimpleStatement(StatementSyntax statement) =>
        statement.Kind() switch
        {
            // 控制流语句 - 这些语句通常需要保留大括号以保持可读性
            SyntaxKind.IfStatement        => false,
            SyntaxKind.ElseClause         => false,
            SyntaxKind.ForStatement       => false,
            SyntaxKind.ForEachStatement   => false,
            SyntaxKind.WhileStatement     => false,
            SyntaxKind.DoStatement        => false,
            SyntaxKind.UsingStatement     => false,
            SyntaxKind.LockStatement      => false,
            SyntaxKind.SwitchStatement    => false,
            SyntaxKind.TryStatement       => false,
            SyntaxKind.CatchClause        => false,
            SyntaxKind.FinallyClause      => false,
            SyntaxKind.CheckedStatement   => false,
            SyntaxKind.UncheckedStatement => false,
            SyntaxKind.UnsafeStatement    => false,
            SyntaxKind.FixedStatement     => false,
            
            // 其他非控制流语句 - 这些语句通常可以不使用大括号
            _ => true
        };
    
    /// <summary>
    /// 判断语句是否为参数跨多行的方法调用
    /// 这种情况下虽然语句跨多行，但仍被视为"单行逻辑"
    /// </summary>
    /// <param name="statement">待判断的语句</param>
    /// <returns>如果是参数多行的方法调用返回true，否则返回false</returns>
    private static bool IsInvocationWithMultiLineArguments(StatementSyntax statement)
    {
        // 处理表达式语句（如方法调用）
        // 确认是否为方法调用
        if (statement is ExpressionStatementSyntax { Expression: InvocationExpressionSyntax invocation })
        {
            // 获取方法调用的位置信息
            var spanStart = invocation.GetLocation().GetLineSpan().StartLinePosition.Line;
            var spanEnd   = invocation.GetLocation().GetLineSpan().EndLinePosition.Line;
            
            // 如果方法调用跨多行（通常是因为参数列表较长而换行）
            return spanStart != spanEnd;
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取链式using语句中的最终非using语句
    /// </summary>
    /// <param name="statement">链式using语句的起始语句</param>
    /// <returns>链式using语句中的最终非using语句，如果全部是using则返回最后一个using的语句体</returns>
    private static StatementSyntax GetFinalStatementInUsingChain(StatementSyntax statement)
    {
        // 递归查找链中的最后一个语句
        while (statement is UsingStatementSyntax usingStatement)
        {
            // 如果语句体是块，找到块中的最后一个语句
            if (usingStatement.Statement is BlockSyntax blockSyntax && blockSyntax.Statements.Count > 0)
                statement = blockSyntax.Statements.Last();
            else
                statement = usingStatement.Statement;
            
            // 如果语句为null，中断循环
            if (statement == null)
                break;
        }
        
        return statement;
    }
}
