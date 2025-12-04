// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using AwesomeAssertions;

//test

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    internal class NodeValidators
    {
        #region Verifiers
        internal static void PointerNameVerification(ExpressionSyntax nameTree, string name)
        {
            nameTree.Should().BeOfType<PointerTypeSyntax>();
            var pointerName = nameTree as PointerTypeSyntax;
            name.Should().Be(pointerName.ElementType.ToString());
        }

        internal static void PredefinedNameVerification(ExpressionSyntax nameTree, string typeName)
        {
            nameTree.Should().BeOfType<PredefinedTypeSyntax>();
            var predefName = nameTree as PredefinedTypeSyntax;
            typeName.Should().Be(predefName.ToString());
        }

        internal static void ArrayNameVerification(ExpressionSyntax nameTree, string arrayName, int numRanks)
        {
            nameTree.Should().BeOfType<ArrayTypeSyntax>();
            var arrayType = nameTree as ArrayTypeSyntax;
            arrayName.Should().Be(arrayType.ElementType.ToString());
            numRanks.Should().Be(arrayType.RankSpecifiers.Count());
        }

        internal static void AliasedNameVerification(ExpressionSyntax nameTree, string alias, string name)
        {
            // Verification of the change
            nameTree.Should().BeOfType<AliasQualifiedNameSyntax>();
            var aliasName = nameTree as AliasQualifiedNameSyntax;
            alias.Should().Be(aliasName.Alias.ToString());
            name.Should().Be(aliasName.Name.ToString());
        }

        internal static void DottedNameVerification(ExpressionSyntax nameTree, string left, string right)
        {
            // Verification of the change
            nameTree.Should().BeOfType<QualifiedNameSyntax>();
            var dottedName = nameTree as QualifiedNameSyntax;
            left.Should().Be(dottedName.Left.ToString());
            right.Should().Be(dottedName.Right.ToString());
        }

        internal static void GenericNameVerification(ExpressionSyntax nameTree, string name, params string[] typeNames)
        {
            // Verification of the change
            nameTree.Should().BeOfType<GenericNameSyntax>();
            var genericName = nameTree as GenericNameSyntax;
            name.Should().Be(genericName.Identifier.ToString());
            typeNames.Count().Should().Be(genericName.TypeArgumentList.Arguments.Count);
            int i = 0;
            foreach (string str in typeNames)
            {
                str.Should().Be(genericName.TypeArgumentList.Arguments[i].ToString());
                i++;
            }
        }

        internal static void BasicNameVerification(ExpressionSyntax nameTree, string name)
        {
            // Verification of the change
            nameTree.Should().BeOfType<IdentifierNameSyntax>();
            var genericName = nameTree as IdentifierNameSyntax;
            name.Should().Be(genericName.ToString());
        }
        #endregion
    }
}
