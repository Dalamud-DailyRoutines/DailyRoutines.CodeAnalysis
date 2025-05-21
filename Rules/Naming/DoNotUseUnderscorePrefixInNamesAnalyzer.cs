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

        if (name.StartsWith("_", StringComparison.Ordinal))
            ReportDiagnostic(context, variableDeclarator.Identifier.GetLocation());
    }

    private void AnalyzeParameterDeclaration(SyntaxNodeAnalysisContext context)
    {
        var parameter = (ParameterSyntax)context.Node;
        var name      = parameter.Identifier.Text;

        if (name.StartsWith("_", StringComparison.Ordinal))
            ReportDiagnostic(context, parameter.Identifier.GetLocation());
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var name              = methodDeclaration.Identifier.Text;

        if (name.StartsWith("_", StringComparison.Ordinal))
            ReportDiagnostic(context, methodDeclaration.Identifier.GetLocation());
    }

    private void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        var name                = propertyDeclaration.Identifier.Text;

        if (name.StartsWith("_", StringComparison.Ordinal))
            ReportDiagnostic(context, propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        foreach (var variable in from variable in fieldDeclaration.Declaration.Variables
                 let name = variable.Identifier.Text
                 where name.StartsWith("_", StringComparison.Ordinal)
                 select variable)
            ReportDiagnostic(context, variable.Identifier.GetLocation());
    }

    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var name             = classDeclaration.Identifier.Text;

        if (name.StartsWith("_", StringComparison.Ordinal))
            ReportDiagnostic(context, classDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;
        var name                 = interfaceDeclaration.Identifier.Text;

        if (name.StartsWith("_", StringComparison.Ordinal))
            ReportDiagnostic(context, interfaceDeclaration.Identifier.GetLocation());
    }
}
