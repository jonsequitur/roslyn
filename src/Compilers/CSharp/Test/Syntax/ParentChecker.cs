// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public static class ParentChecker
    {
        public static void CheckParents(SyntaxNodeOrToken nodeOrToken, SyntaxTree expectedSyntaxTree)
        {
            nodeOrToken.SyntaxTree.Should().Be(expectedSyntaxTree);

            var span = nodeOrToken.Span;

            if (nodeOrToken.IsToken)
            {
                var token = nodeOrToken.AsToken();
                foreach (var trivia in token.LeadingTrivia)
                {
                    var tspan = trivia.Span;
                    var parentToken = trivia.Token;
                    token.Should().Be(parentToken);
                    if (trivia.HasStructure)
                    {
                        var parentTrivia = trivia.GetStructure().Parent;
                        parentTrivia.Should().BeNull();
                        CheckParents((CSharpSyntaxNode)trivia.GetStructure(), expectedSyntaxTree);
                    }
                }

                foreach (var trivia in token.TrailingTrivia)
                {
                    var tspan = trivia.Span;
                    var parentToken = trivia.Token;
                    token.Should().Be(parentToken);
                    if (trivia.HasStructure)
                    {
                        var parentTrivia = trivia.GetStructure().Parent;
                        parentTrivia.Should().BeNull();
                        CheckParents(trivia.GetStructure(), expectedSyntaxTree);
                    }
                }
            }
            else
            {
                var node = nodeOrToken.AsNode();
                foreach (var child in node.ChildNodesAndTokens())
                {
                    var parent = child.Parent;
                    parent.Should().Be(node);
                    CheckParents(child, expectedSyntaxTree);
                }
            }
        }
    }
}

