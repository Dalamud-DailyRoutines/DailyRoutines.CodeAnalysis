using System.Collections.Generic;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace DailyRoutines.CodeAnalysis.Rules.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UsePrimaryConstructorAnalyzer() : BaseAnalyzer(DiagnosticRules.UsePrimaryConstructor)
{
    protected override void RegisterAnalyzers
    (
        AnalysisContext context
    ) =>
        context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);

    private void AnalyzeConstructor
    (
        SyntaxNodeAnalysisContext context
    )
    {
        var constructor = (ConstructorDeclarationSyntax)context.Node;
        if (context.Node.SyntaxTree.Options is not CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp12 } ||
            constructor.Parent is not TypeDeclarationSyntax type                                                       ||
            type is not (ClassDeclarationSyntax or StructDeclarationSyntax)                                            ||
            type.ParameterList != null                                                                                 ||
            constructor.Modifiers.Any(SyntaxKind.StaticKeyword)                                                        ||
            constructor.Modifiers.Any(SyntaxKind.ExternKeyword)                                                        ||
            constructor.Modifiers.Any(SyntaxKind.UnsafeKeyword)                                                        ||
            constructor.AttributeLists.Count                                          != 0                             ||
            constructor.Initializer?.ThisOrBaseKeyword.IsKind(SyntaxKind.ThisKeyword) == true                          ||
            constructor.ContainsDiagnostics)
            return;

        var constructorSymbol = context.SemanticModel.GetDeclaredSymbol(constructor, context.CancellationToken);
        if (constructorSymbol == null)
            return;

        var typeSymbol    = constructorSymbol.ContainingType;
        var accessibility = typeSymbol.IsAbstract ? Accessibility.Protected : Accessibility.Public;
        if (constructorSymbol.DeclaredAccessibility != accessibility)
            return;

        foreach (var parameter in constructorSymbol.Parameters)
        {
            if (parameter.RefKind != RefKind.None)
                return;
        }

        foreach (var otherConstructor in typeSymbol.InstanceConstructors)
        {
            if (otherConstructor.IsImplicitlyDeclared ||
                SymbolEqualityComparer.Default.Equals(otherConstructor, constructorSymbol))
                continue;

            foreach (var reference in otherConstructor.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax(context.CancellationToken) is not ConstructorDeclarationSyntax otherDeclaration ||
                    otherDeclaration.Initializer?.ThisOrBaseKeyword.IsKind(SyntaxKind.ThisKeyword) != true)
                    return;
            }
        }

        var assignedMembers = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

        if (constructor.ExpressionBody != null)
        {
            if (!CanMoveAssignment(constructor.ExpressionBody.Expression, constructorSymbol, assignedMembers, context))
                return;
        }
        else if (constructor.Body != null)
        {
            foreach (var statement in constructor.Body.Statements)
            {
                if (statement is not ExpressionStatementSyntax expressionStatement ||
                    !CanMoveAssignment(expressionStatement.Expression, constructorSymbol, assignedMembers, context))
                    return;
            }
        }
        else
            return;

        ReportDiagnostic(context, constructor.Identifier.GetLocation(), constructor.Identifier.ValueText);
    }

    private static bool CanMoveAssignment
    (
        ExpressionSyntax          expression,
        IMethodSymbol             constructor,
        HashSet<ISymbol>          assignedMembers,
        SyntaxNodeAnalysisContext context
    )
    {
        if (context.SemanticModel.GetOperation(expression, context.CancellationToken) is not ISimpleAssignmentOperation assignment ||
            !CanMoveValue(assignment.Value, constructor))
            return false;

        ISymbol     member;
        IOperation? instance;

        switch (assignment.Target)
        {
            case IFieldReferenceOperation fieldReference:
                member   = fieldReference.Field;
                instance = fieldReference.Instance;
                break;
            case IPropertyReferenceOperation propertyReference:
                member   = propertyReference.Property;
                instance = propertyReference.Instance;
                break;
            default:
                return false;
        }

        if (member.IsStatic                                                                                             ||
            !SymbolEqualityComparer.Default.Equals(member.ContainingType, constructor.ContainingType)                   ||
            instance is not IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance } ||
            !assignedMembers.Add(member))
            return false;

        foreach (var reference in member.DeclaringSyntaxReferences)
        {
            switch (reference.GetSyntax(context.CancellationToken))
            {
                case VariableDeclaratorSyntax { Initializer: null }:
                    return true;
                case PropertyDeclarationSyntax { Initializer: null, ExpressionBody: null, AccessorList: not null } property:
                    if (member.IsAbstract ||
                        member.IsVirtual  ||
                        member.IsOverride)
                        return false;

                    foreach (var accessor in property.AccessorList.Accessors)
                    {
                        if (accessor.Body           != null ||
                            accessor.ExpressionBody != null)
                            return false;
                    }

                    return true;
            }
        }

        return false;
    }

    private static bool CanMoveValue
    (
        IOperation    operation,
        IMethodSymbol constructor
    )
    {
        if (operation is IInstanceReferenceOperation or ILocalReferenceOperation or IAnonymousFunctionOperation or IInvalidOperation)
            return false;

        if (operation is IParameterReferenceOperation parameterReference &&
            !SymbolEqualityComparer.Default.Equals(parameterReference.Parameter.ContainingSymbol, constructor))
            return false;

        foreach (var child in operation.ChildOperations)
        {
            if (!CanMoveValue(child, constructor))
                return false;
        }

        return true;
    }
}
