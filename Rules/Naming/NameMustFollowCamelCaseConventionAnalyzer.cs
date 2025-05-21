using System;
using System.Linq;
using System.Text.RegularExpressions;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DailyRoutines.CodeAnalysis.Rules.Naming;

/// <summary>
///     分析器：命名必须遵循驼峰命名法
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NameMustFollowCamelCaseConventionAnalyzer() : BaseAnalyzer(DiagnosticRules.NameMustFollowCamelCaseConvention)
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
        context.RegisterSyntaxNodeAction(AnalyzeEnumDeclaration,      SyntaxKind.EnumDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeEnumMemberDeclaration, SyntaxKind.EnumMemberDeclaration);
    }

    private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var variableDeclarator = (VariableDeclaratorSyntax)context.Node;
        var name               = variableDeclarator.Identifier.Text;

        if (!IsValidIdentifier(name) || !IsCamelCase(name))
            ReportDiagnostic(context, variableDeclarator.Identifier.GetLocation(), name);
    }

    private void AnalyzeParameterDeclaration(SyntaxNodeAnalysisContext context)
    {
        var parameter = (ParameterSyntax)context.Node;
        var name      = parameter.Identifier.Text;

        if (!IsValidIdentifier(name) || !IsCamelCase(name))
            ReportDiagnostic(context, parameter.Identifier.GetLocation(), name);
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var name              = methodDeclaration.Identifier.Text;

        if (!IsValidIdentifier(name) || !IsCamelCase(name))
            ReportDiagnostic(context, methodDeclaration.Identifier.GetLocation(), name);
    }

    private void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        var name                = propertyDeclaration.Identifier.Text;

        if (!IsValidIdentifier(name) || !IsCamelCase(name))
            ReportDiagnostic(context, propertyDeclaration.Identifier.GetLocation(), name);
    }

    private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        
        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            var name = variable.Identifier.Text;
            if (!IsValidIdentifier(name) || !IsCamelCase(name))
                ReportDiagnostic(context, variable.Identifier.GetLocation(), name);
        }
    }

    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var name             = classDeclaration.Identifier.Text;

        if (!IsValidIdentifier(name) || !IsCamelCase(name))
            ReportDiagnostic(context, classDeclaration.Identifier.GetLocation(), name);
    }

    private void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;
        var name                 = interfaceDeclaration.Identifier.Text;

        // 接口通常以I开头，但仍然需要检查后面的部分是否符合驼峰命名法
        if (!IsValidIdentifier(name) || !IsCamelCase(name))
            ReportDiagnostic(context, interfaceDeclaration.Identifier.GetLocation(), name);
    }

    private void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
    {
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;
        var name            = enumDeclaration.Identifier.Text;

        if (!IsValidIdentifier(name) || !IsCamelCase(name))
            ReportDiagnostic(context, enumDeclaration.Identifier.GetLocation(), name);
    }

    private void AnalyzeEnumMemberDeclaration(SyntaxNodeAnalysisContext context)
    {
        var enumMemberDeclaration = (EnumMemberDeclarationSyntax)context.Node;
        var name                  = enumMemberDeclaration.Identifier.Text;

        if (!IsValidIdentifier(name) || !IsCamelCase(name))
            ReportDiagnostic(context, enumMemberDeclaration.Identifier.GetLocation(), name);
    }

    /// <summary>
    /// 检查标识符是否符合驼峰命名法（大驼峰或小驼峰）
    /// </summary>
    /// <param name="name">需要检查的标识符</param>
    /// <returns>如果符合驼峰命名法则返回true，否则返回false</returns>
    private static bool IsCamelCase(string name)
    {
        // 跳过特殊情况
        if (ShouldSkipCamelCaseCheck(name))
            return true;

        // 将标识符分割成多个词（按照下划线或数字）
        var words = Regex.Split(name, @"(?<=[a-z])(?=[A-Z])|(?<=.)(?=[0-9])|(?<=[0-9])(?=.)");
        
        // 大驼峰：每个词的首字母都必须大写
        bool isPascalCase = words.Length > 0 && words.All(word => word.Length > 0 && char.IsUpper(word[0]));
        
        // 小驼峰：第一个词的首字母小写，其他词的首字母大写
        bool isCamelCase = words.Length > 0 && 
                          !char.IsUpper(words[0][0]) && 
                          words.Skip(1).All(word => word.Length > 0 && char.IsUpper(word[0]));
        
        return isPascalCase || isCamelCase;
    }

    /// <summary>
    /// 判断是否应该跳过驼峰命名检查
    /// </summary>
    private static bool ShouldSkipCamelCaseCheck(string name)
    {
        // 检查是否是单个字母或数字
        if (name.Length <= 1)
            return true;
            
        // 跳过以"_"开头且后面没有其他字符的情况（单个下划线用作丢弃变量）
        if (name == "_")
            return true;
            
        // 跳过常量命名（全大写+下划线）
        if (name.ToUpperInvariant() == name && name.Contains('_'))
            return true;
            
        // 检查是否是有效的C#标识符
        return !IsValidIdentifier(name);
    }
    
    /// <summary>
    /// 检查是否是有效的C#标识符
    /// </summary>
    private static bool IsValidIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
            
        // 标识符的第一个字符必须是字母或下划线
        if (!char.IsLetter(name[0]) && name[0] != '_')
            return false;
            
        // 其余字符必须是字母、数字或下划线
        return name.Skip(1).All(c => char.IsLetterOrDigit(c) || c == '_');
    }
} 