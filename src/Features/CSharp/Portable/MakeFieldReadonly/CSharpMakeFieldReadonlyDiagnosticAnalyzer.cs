﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MakeFieldReadonly;

namespace Microsoft.CodeAnalysis.CSharp.MakeFieldReadonly
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class CSharpMakeFieldReadonlyDiagnosticAnalyzer :
        AbstractMakeFieldReadonlyDiagnosticAnalyzer<IdentifierNameSyntax, ConstructorDeclarationSyntax, LambdaExpressionSyntax>
    {
        protected override void InitializeWorker(AnalysisContext context)
            => context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);

        internal override bool CanBeReadonly(IdentifierNameSyntax name, SemanticModel model, CancellationToken cancellationToken)
        {
            return !name.IsWrittenTo();
        }

        internal override bool IsMemberOfThisInstance(SyntaxNode node)
        {
            // if it is a qualified name, make sure it is `this.name`
            if (node.Parent is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Expression is ThisExpressionSyntax;
            }

            // make sure it isn't in an object initializer
            if (node.Parent.Parent is InitializerExpressionSyntax)
            {
                return false;
            }

            return true;
        }
    }
}
