using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using DailyRoutines.CodeAnalysis.Common;

namespace DailyRoutines.CodeAnalysis.Rules.Naming;

/// <summary>
/// 分析器：英文缩写大小写必须保持一致
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AcronymCasingConsistencyAnalyzer() : BaseAnalyzer(DiagnosticRules.AcronymCasingConsistency)
{
    protected override void RegisterAnalyzers(AnalysisContext context)
    {
        // 检查各种声明
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclarator);
        context.RegisterSyntaxNodeAction(AnalyzeParameterDeclaration, SyntaxKind.Parameter);
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeStructDeclaration, SyntaxKind.StructDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeEnumDeclaration, SyntaxKind.EnumDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeEnumMemberDeclaration, SyntaxKind.EnumMemberDeclaration);
    }

    private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var variableDeclarator = (VariableDeclaratorSyntax)context.Node;
        var name = variableDeclarator.Identifier.Text;
        AnalyzeName(context, name, variableDeclarator.Identifier.GetLocation());
    }

    private void AnalyzeParameterDeclaration(SyntaxNodeAnalysisContext context)
    {
        var parameter = (ParameterSyntax)context.Node;
        var name = parameter.Identifier.Text;
        AnalyzeName(context, name, parameter.Identifier.GetLocation());
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var name = methodDeclaration.Identifier.Text;
        AnalyzeName(context, name, methodDeclaration.Identifier.GetLocation());
    }

    private void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        var name = propertyDeclaration.Identifier.Text;
        AnalyzeName(context, name, propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            var name = variable.Identifier.Text;
            AnalyzeName(context, name, variable.Identifier.GetLocation());
        }
    }

    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var name = classDeclaration.Identifier.Text;
        AnalyzeName(context, name, classDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;
        var name = interfaceDeclaration.Identifier.Text;
        AnalyzeName(context, name, interfaceDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeStructDeclaration(SyntaxNodeAnalysisContext context)
    {
        var structDeclaration = (StructDeclarationSyntax)context.Node;
        var name = structDeclaration.Identifier.Text;
        AnalyzeName(context, name, structDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
    {
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;
        var name = enumDeclaration.Identifier.Text;
        AnalyzeName(context, name, enumDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeEnumMemberDeclaration(SyntaxNodeAnalysisContext context)
    {
        var enumMemberDeclaration = (EnumMemberDeclarationSyntax)context.Node;
        var name = enumMemberDeclaration.Identifier.Text;
        AnalyzeName(context, name, enumMemberDeclaration.Identifier.GetLocation());
    }

    /// <summary>
    /// 分析名称中的缩写大小写一致性
    /// </summary>
    /// <param name="context">分析上下文</param>
    /// <param name="name">标识符名称</param>
    /// <param name="location">位置信息</param>
    private void AnalyzeName(SyntaxNodeAnalysisContext context, string name, Location location)
    {
        if (string.IsNullOrEmpty(name))
            return;

        var inconsistentAcronyms = FindInconsistentAcronyms(name);
        foreach (var (acronym, upperCase, lowerCase) in inconsistentAcronyms)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticRules.AcronymCasingConsistency,
                location,
                acronym,
                upperCase,
                lowerCase);
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// 查找名称中大小写不一致的缩写
    /// </summary>
    /// <param name="name">标识符名称</param>
    /// <returns>不一致的缩写列表，包含原始缩写、全大写形式和全小写形式</returns>
    private static List<(string acronym, string upperCase, string lowerCase)> FindInconsistentAcronyms(string name)
    {
        var result = new List<(string, string, string)>();

        foreach (var acronym in AcronymConstants.CommonAcronyms)
        {
            // 使用正则表达式查找缩写，确保它是完整的单词边界
            var pattern = $@"\b{Regex.Escape(acronym)}\b";
            var matches = Regex.Matches(name, pattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var foundAcronym = match.Value;
                var upperCase = acronym.ToUpperInvariant();
                var lowerCase = acronym.ToLowerInvariant();

                // 检查是否为混合大小写（既不是全大写也不是全小写）
                if (foundAcronym != upperCase && foundAcronym != lowerCase)
                {
                    result.Add((foundAcronym, upperCase, lowerCase));
                }
            }
        }

        return result;
    }
}
