// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using Roslyn.Test.Utilities;
using Xunit;
using InternalSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public partial class GreenNodeTests
    {
        private static void AttachAndCheckDiagnostics(InternalSyntax.CSharpSyntaxNode node)
        {
            var nodeWithDiags = node.SetDiagnostics(new DiagnosticInfo[] { new CSDiagnosticInfo(ErrorCode.ERR_NoBaseClass) });
            var diags = nodeWithDiags.GetDiagnostics();

            nodeWithDiags.Should().NotBe(node);
            diags.Length.Should().Be(1);
            (ErrorCode)diags[0].Code.Should().Be(ErrorCode.ERR_NoBaseClass);
        }

        private class TokenDeleteRewriter : InternalSyntax.CSharpSyntaxRewriter
        {
            public override InternalSyntax.CSharpSyntaxNode VisitToken(InternalSyntax.SyntaxToken token)
            {
                return InternalSyntax.SyntaxFactory.MissingToken(token.Kind);
            }
        }

        private class IdentityRewriter : InternalSyntax.CSharpSyntaxRewriter
        {
            protected override InternalSyntax.CSharpSyntaxNode DefaultVisit(InternalSyntax.CSharpSyntaxNode node)
            {
                return node;
            }
        }

        [Fact, WorkItem(33685, "https://github.com/dotnet/roslyn/issues/33685")]
        public void ConvenienceSwitchStatementFactoriesAddParensWhenNeeded_01()
        {
            var expression = SyntaxFactory.ParseExpression("x");
            var sw1 = SyntaxFactory.SwitchStatement(expression);
            sw1.OpenParenToken.Kind().Should().Be(SyntaxKind.OpenParenToken);
            sw1.CloseParenToken.Kind().Should().Be(SyntaxKind.CloseParenToken);
            var sw2 = SyntaxFactory.SwitchStatement(expression, default);
            sw2.OpenParenToken.Kind().Should().Be(SyntaxKind.OpenParenToken);
            sw2.CloseParenToken.Kind().Should().Be(SyntaxKind.CloseParenToken);
        }

        [Fact, WorkItem(33685, "https://github.com/dotnet/roslyn/issues/33685")]
        public void ConvenienceSwitchStatementFactoriesAddParensWhenNeeded_02()
        {
            var expression = SyntaxFactory.ParseExpression("(x)");
            var sw1 = SyntaxFactory.SwitchStatement(expression);
            sw1.OpenParenToken.Kind().Should().Be(SyntaxKind.OpenParenToken);
            sw1.CloseParenToken.Kind().Should().Be(SyntaxKind.CloseParenToken);
            var sw2 = SyntaxFactory.SwitchStatement(expression, default);
            sw2.OpenParenToken.Kind().Should().Be(SyntaxKind.OpenParenToken);
            sw2.CloseParenToken.Kind().Should().Be(SyntaxKind.CloseParenToken);
        }

        [Fact, WorkItem(33685, "https://github.com/dotnet/roslyn/issues/33685")]
        public void ConvenienceSwitchStatementFactoriesOmitParensWhenPossible()
        {
            var expression = SyntaxFactory.ParseExpression("(1, 2)");
            var sw1 = SyntaxFactory.SwitchStatement(expression);
            sw1.OpenParenToken == default.Should().BeTrue();
            sw1.CloseParenToken == default.Should().BeTrue();
            var sw2 = SyntaxFactory.SwitchStatement(expression, default);
            sw2.OpenParenToken == default.Should().BeTrue();
            sw2.CloseParenToken == default.Should().BeTrue();
        }
    }
}
