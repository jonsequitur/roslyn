// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests.Parsing;

public class InterpolationTests
{
    [Fact]
    public void APIBackCompatTest1()
    {
        SyntaxFactory.Interpolation(SyntaxFactory.IdentifierName("a")).ToFullString().Should().Be("{a}");
    }

    [Fact]
    public void APIBackCompatTest2()
    {
        SyntaxFactory.Interpolation(
            SyntaxFactory.IdentifierName("a"),
            SyntaxFactory.InterpolationAlignmentClause(
                SyntaxFactory.Token(SyntaxKind.CommaToken),
                SyntaxFactory.IdentifierName("b")),
            SyntaxFactory.InterpolationFormatClause(
                SyntaxFactory.Token(SyntaxKind.ColonToken),
                SyntaxFactory.Token(default, SyntaxKind.InterpolatedStringTextToken, "c", "c", default))).ToFullString().Should().Be("{a,b:c}");
    }
}
