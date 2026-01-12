using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DailyRoutines.CodeAnalysis.Rules.Design;

/// <summary>
///     分析器：配置类中不允许使用readonly字段
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConfigurationClassesMustNotUseReadonlyFieldsAnalyzer : BaseAnalyzer
{
    // 需要检查的基类名称
    private const string ManagerConfigurationClassName = "ManagerConfiguration";
    private const string ModuleConfigurationClassName  = "ModuleConfiguration";

    public ConfigurationClassesMustNotUseReadonlyFieldsAnalyzer()
        : base(DiagnosticRules.ConfigurationClassesMustNotUseReadonlyFields)
    {
    }

    protected override void RegisterAnalyzers(AnalysisContext context)
    {
        // 注册分析器处理的语法节点类型
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        // 获取字段声明节点
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

        // 检查字段是否有readonly修饰符
        if (!fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
            return;

        // 获取包含此字段的类声明
        var classDeclaration = fieldDeclaration.Parent as ClassDeclarationSyntax;
        if (classDeclaration == null)
            return;

        // 获取类的语义模型
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null)
            return;

        // 检查类是否继承自ManagerConfiguration或ModuleConfiguration
        var baseType = classSymbol.BaseType;
        if (baseType == null)
            return;

        string configurationBaseClassName = null;

        if (baseType.Name == ManagerConfigurationClassName)
            configurationBaseClassName = ManagerConfigurationClassName;
        else if (baseType.Name == ModuleConfigurationClassName)
            configurationBaseClassName = ModuleConfigurationClassName;
        else
        {
            // 递归检查更高层次的基类
            var currentBaseType = baseType.BaseType;

            while (currentBaseType != null)
            {
                if (currentBaseType.Name == ManagerConfigurationClassName)
                {
                    configurationBaseClassName = ManagerConfigurationClassName;
                    break;
                }

                if (currentBaseType.Name == ModuleConfigurationClassName)
                {
                    configurationBaseClassName = ModuleConfigurationClassName;
                    break;
                }

                currentBaseType = currentBaseType.BaseType;
            }
        }

        // 如果类继承自目标基类，则报告诊断
        if (configurationBaseClassName != null)
        {
            foreach (var variable in fieldDeclaration.Declaration.Variables)
                ReportDiagnostic(context, variable.GetLocation(), configurationBaseClassName);
        }
    }
}
