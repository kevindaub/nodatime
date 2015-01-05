﻿// Copyright 2015 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace NodaTime.CodeDiagnostics
{
    /// <summary>
    /// Diagnostic analyzer to ensure that ReadWriteForEfficiency is being used correctly
    /// - in other words, that assignments only occur within constructors.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class ReadWriteForEfficiencyDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        internal const string Category = "Style";

        internal static DiagnosticDescriptor ReadWriteForEfficiencyAssignmentOutsideConstructorRule = new DiagnosticDescriptor(
            "ReadWriteForEfficiencyAssignmentOutsideConstructor",
            "[ReadWriteForEfficiency] fields should only be assigned in constructors",
            "[ReadWriteForEfficiency] field {0} should only be assigned in constructors",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        internal static DiagnosticDescriptor ReadWriteForEfficiencyNotPrivateRule = new DiagnosticDescriptor(
            "ReadWriteForEfficiencyNotPrivate",
            "[ReadWriteForEfficiency] fields should be private",
            "[ReadWriteForEfficiency] field {0} should be private",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReadWriteForEfficiencyAssignmentOutsideConstructorRule,
                ReadWriteForEfficiencyNotPrivateRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterNodaTimeSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.Attribute);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            AttributeSyntax attribute = (AttributeSyntax)context.Node;
            var model = context.SemanticModel;
            var attributeSymbol = model.GetSymbolInfo(attribute).Symbol?.ContainingSymbol;
            // TODO: Write a convenience method to do this properly, with namespace as well...
            if (attributeSymbol?.Name != "ReadWriteForEfficiencyAttribute")
            {
                return;
            }
            // ReadWriteForEfficiency can only be applied to fields, so the grandparent node must
            // be a field declaration syntax.
            var fieldDeclaration = (FieldDeclarationSyntax)attribute.Parent.Parent;
            foreach (var individualField in fieldDeclaration.Declaration.Variables)
            {
                var fieldSymbol = model.GetDeclaredSymbol(individualField);
                if (fieldSymbol.DeclaredAccessibility != Accessibility.Private)
                {
                    context.ReportDiagnostic(ReadWriteForEfficiencyNotPrivateRule, individualField, fieldSymbol.Name);
                }

                var invalidAssignments = fieldSymbol.ContainingType.DeclaringSyntaxReferences
                    .SelectMany(reference => reference.GetSyntax().DescendantNodes())
                    .OfType<AssignmentExpressionSyntax>()
                    .Where(n => IsAssignmentOutsideConstructor(n, fieldSymbol, model));
                foreach (var invalidAssignment in invalidAssignments)
                {
                    context.ReportDiagnostic(ReadWriteForEfficiencyAssignmentOutsideConstructorRule,
                        invalidAssignment, fieldSymbol.Name);
                }
            }
        }

        private static bool IsAssignmentOutsideConstructor(AssignmentExpressionSyntax assignment, ISymbol fieldSymbol, SemanticModel model)
        {
            var assignedSymbol = model.GetSymbolInfo(assignment.Left);
            if (assignedSymbol.Symbol != fieldSymbol)
            {
                return false;
            }
            // Method (or whatever) enclosing the assignment
            var enclosingSymbol = model.GetEnclosingSymbol(assignment.SpanStart) as IMethodSymbol;
            var isCtor = enclosingSymbol?.MethodKind == MethodKind.Constructor;
            return !isCtor;
        }
    }
}
