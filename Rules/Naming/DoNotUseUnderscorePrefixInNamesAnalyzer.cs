using System;
using System.Linq;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DailyRoutines.CodeAnalysis.Rules.Naming;

/// <summary>
///     分析器：不允许命名以下划线开头
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotUseUnderscorePrefixInNamesAnalyzer() : BaseAnalyzer(DiagnosticRules.DoNotUseUnderscorePrefixInNames)
{
    protected override void RegisterAnalyzers(AnalysisContext context)
    {
        // 检查各种声明
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration,  SyntaxKind.VariableDeclarator);
        context.RegisterSyntaxNodeAction(AnalyzeParameterDeclaration, SyntaxKind.Parameter);
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration,    SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration,  SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration,     SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration,     SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
    }

    private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var variableDeclarator = (VariableDeclaratorSyntax)context.Node;
        var name               = variableDeclarator.Identifier.Text;

        if (ShouldReportUnderscorePrefix(name))
            ReportDiagnostic(context, variableDeclarator.Identifier.GetLocation());
    }

    private void AnalyzeParameterDeclaration(SyntaxNodeAnalysisContext context)
    {
        var parameter = (ParameterSyntax)context.Node;
        var name      = parameter.Identifier.Text;

        if (ShouldReportUnderscorePrefix(name))
            ReportDiagnostic(context, parameter.Identifier.GetLocation());
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var name              = methodDeclaration.Identifier.Text;

        if (ShouldReportUnderscorePrefix(name))
            ReportDiagnostic(context, methodDeclaration.Identifier.GetLocation());
    }

    private void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        var name                = propertyDeclaration.Identifier.Text;

        if (ShouldReportUnderscorePrefix(name))
            ReportDiagnostic(context, propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        foreach (var variable in from variable in fieldDeclaration.Declaration.Variables
                 let name = variable.Identifier.Text
                 where ShouldReportUnderscorePrefix(name)
                 select variable)
            ReportDiagnostic(context, variable.Identifier.GetLocation());
    }

    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var name             = classDeclaration.Identifier.Text;

        if (ShouldReportUnderscorePrefix(name))
            ReportDiagnostic(context, classDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;
        var name                 = interfaceDeclaration.Identifier.Text;

        if (ShouldReportUnderscorePrefix(name))
            ReportDiagnostic(context, interfaceDeclaration.Identifier.GetLocation());
    }

    /// <summary>
    /// 判断是否应该报告下划线前缀的问题
    /// </summary>
    /// <param name="name">标识符名称</param>
    /// <returns>如果应该报告则返回true，否则返回false</returns>
    private static bool ShouldReportUnderscorePrefix(string name)
    {
        // 排除单独一个下划线的情况（C#中通常用作弃元标识符）
        if (name == "_")
            return false;
            
        return name.StartsWith("_", StringComparison.Ordinal);
    }
}
