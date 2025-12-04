// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class ExpressionParsingTests : ParsingTests
    {
        public ExpressionParsingTests(ITestOutputHelper output) : base(output) { }

        protected override SyntaxTree ParseTree(string text, CSharpParseOptions options)
        {
            return SyntaxFactory.ParseSyntaxTree(text, options: options);
        }

        private ExpressionSyntax ParseExpression(string text, ParseOptions options = null)
        {
            return SyntaxFactory.ParseExpression(text, options: options);
        }

        [Fact]
        public void TestEmptyString()
        {
            var text = string.Empty;
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)expr).Identifier.IsMissing.Should().BeTrue();
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(1);
            expr.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_ExpressionExpected);
        }

        [Fact]
        public void TestInterpolatedVerbatimString()
        {
            UsingExpression(@"$@""hello""");
            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedVerbatimStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.InterpolatedStringEndToken);
            }
            EOF();
        }

        [Fact]
        public void TestInterpolatedSingleLineRawString1()
        {
            UsingExpression(@"$""""""{1 + 1}""""""");
            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedSingleLineRawStringStartToken);
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.AddExpression);
                    {
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "1");
                        }
                        N(SyntaxKind.PlusToken);
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "1");
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedRawStringEndToken);
            }
            EOF();
        }

        [Fact]
        public void TestInterpolatedSingleLineRawString2()
        {
            UsingExpression(@"$$""""""{{{1 + 1}}}""""""");
            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedSingleLineRawStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.AddExpression);
                    {
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "1");
                        }
                        N(SyntaxKind.PlusToken);
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "1");
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.InterpolatedRawStringEndToken);
            }
            EOF();
        }

        [Fact]
        public void TestInterpolatedMultiLineRawString1()
        {
            UsingExpression(@"$""""""
    {1 + 1}
    """"""");
            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedMultiLineRawStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.AddExpression);
                    {
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "1");
                        }
                        N(SyntaxKind.PlusToken);
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "1");
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedRawStringEndToken);
            }
            EOF();
        }

        [Fact]
        public void TestInterpolatedMultiLineRawString2()
        {
            UsingExpression(@"$$""""""
    {{{1 + 1}}}
    """"""");
            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedMultiLineRawStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.AddExpression);
                    {
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "1");
                        }
                        N(SyntaxKind.PlusToken);
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "1");
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.InterpolatedRawStringEndToken);
            }
            EOF();
        }

        [Fact]
        public void TestAltInterpolatedVerbatimString_CSharp73()
        {
            var text = @"@$""hello""";
            CreateCompilation($@"
class C
{{
    void M()
    {{
        var v = {text};
    }}
}}", parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3)).VerifyDiagnostics(
                // (6,17): error CS8370: Feature 'alternative interpolated verbatim strings' is not available in C# 7.3. Please use language version 8.0 or greater.
                //         var v = @$"hello";
                Diagnostic(ErrorCode.ERR_AltInterpolatedVerbatimStringsNotAvailable, @"@$""").WithArguments("8.0").WithLocation(6, 17));

            UsingExpression(text, TestOptions.Regular7_3);

            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedVerbatimStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.InterpolatedStringEndToken);
            }
            EOF();
        }

        [Fact]
        public void TestAltInterpolatedVerbatimString_CSharp8()
        {
            var text = @"@$""hello""";
            CreateCompilation($@"
class C
{{
    void M()
    {{
        var v = {text};
    }}
}}", parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8)).VerifyDiagnostics();

            UsingExpression(text, TestOptions.Regular8);
            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedVerbatimStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.InterpolatedStringEndToken);
            }
            EOF();
        }

        [Fact]
        public void TestNestedAltInterpolatedVerbatimString_CSharp73()
        {
            var text = "$@\"aaa{@$\"bbb\nccc\"}ddd\"";
            CreateCompilation($@"
class C
{{
    void M()
    {{
        var v = {text};
    }}
}}", parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3)).VerifyDiagnostics(
                // (6, 24): error CS8401: To use '@$' instead of '$@' for an interpolated verbatim string, please use language version '8.0' or greater.
                // $@"aaa{@$"bbb
                Diagnostic(ErrorCode.ERR_AltInterpolatedVerbatimStringsNotAvailable, @"@$""").WithArguments("8.0").WithLocation(6, 24));

            UsingExpression(text, TestOptions.Regular7_3);

            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedVerbatimStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken, "aaa");
                }
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.InterpolatedStringExpression);
                    {
                        N(SyntaxKind.InterpolatedVerbatimStringStartToken);
                        N(SyntaxKind.InterpolatedStringText);
                        {
                            N(SyntaxKind.InterpolatedStringTextToken, "bbb\nccc");
                        }
                        N(SyntaxKind.InterpolatedStringEndToken);
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken, "ddd");
                }
                N(SyntaxKind.InterpolatedStringEndToken);
            }
            EOF();
        }

        [Fact]
        public void TestNestedAltInterpolatedVerbatimString_CSharp8()
        {
            var text = "$@\"aaa{@$\"bbb\nccc\"}ddd\"";

            CreateCompilation($@"
class C
{{
    void M()
    {{
        var v = {text};
    }}
}}", parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8)).VerifyDiagnostics();

            UsingExpression(text, TestOptions.Regular8);

            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedVerbatimStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken, "aaa");
                }
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.InterpolatedStringExpression);
                    {
                        N(SyntaxKind.InterpolatedVerbatimStringStartToken);
                        N(SyntaxKind.InterpolatedStringText);
                        {
                            N(SyntaxKind.InterpolatedStringTextToken, "bbb\nccc");
                        }
                        N(SyntaxKind.InterpolatedStringEndToken);
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken, "ddd");
                }
                N(SyntaxKind.InterpolatedStringEndToken);
            }
            EOF();
        }

        [Fact]
        public void TestInterpolatedStringWithNewLinesInExpression()
        {
            var text = @"$""Text with {
    new[] {
        1, 2, 3
    }[2]
} parts and new line expressions!""";

            UsingExpression(text, TestOptions.RegularPreview);

            var expr = (InterpolatedStringExpressionSyntax)N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.ElementAccessExpression);
                    {
                        N(SyntaxKind.ImplicitArrayCreationExpression);
                        {
                            N(SyntaxKind.NewKeyword);
                            N(SyntaxKind.OpenBracketToken);
                            N(SyntaxKind.CloseBracketToken);
                            N(SyntaxKind.ArrayInitializerExpression);
                            {
                                N(SyntaxKind.OpenBraceToken);
                                N(SyntaxKind.NumericLiteralExpression);
                                {
                                    N(SyntaxKind.NumericLiteralToken, "1");
                                }
                                N(SyntaxKind.CommaToken);
                                N(SyntaxKind.NumericLiteralExpression);
                                {
                                    N(SyntaxKind.NumericLiteralToken, "2");
                                }
                                N(SyntaxKind.CommaToken);
                                N(SyntaxKind.NumericLiteralExpression);
                                {
                                    N(SyntaxKind.NumericLiteralToken, "3");
                                }
                                N(SyntaxKind.CloseBraceToken);
                            }
                        }
                        N(SyntaxKind.BracketedArgumentList);
                        {
                            N(SyntaxKind.OpenBracketToken);
                            N(SyntaxKind.Argument);
                            {
                                N(SyntaxKind.NumericLiteralExpression);
                                {
                                    N(SyntaxKind.NumericLiteralToken, "2");
                                }
                            }
                            N(SyntaxKind.CloseBracketToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.InterpolatedStringEndToken);
            }
            EOF();

            expr.Contents[0].ToString().Should().Be("Text with ");
            expr.Contents[2].ToString().Should().Be(" parts and new line expressions!");
        }

        [Fact]
        public void TestName()
        {
            var text = "goo";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)expr).Identifier.IsMissing.Should().BeFalse();
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestParenthesizedExpression()
        {
            var text = "(goo)";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ParenthesizedExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
        }

        private void TestLiteralExpression(SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            var opKind = SyntaxFacts.GetLiteralExpression(kind);
            expr.Kind().Should().Be(opKind);
            expr.Errors().Length.Should().Be(0);
            var us = (LiteralExpressionSyntax)expr;
            us.Token.Should().NotBe(default);
            us.Token.Kind().Should().Be(kind);
        }

        [Fact]
        public void TestPrimaryExpressions()
        {
            TestLiteralExpression(SyntaxKind.NullKeyword);
            TestLiteralExpression(SyntaxKind.TrueKeyword);
            TestLiteralExpression(SyntaxKind.FalseKeyword);
            TestLiteralExpression(SyntaxKind.ArgListKeyword);
        }

        private void TestInstanceExpression(SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            var opKind = SyntaxFacts.GetInstanceExpression(kind);
            expr.Kind().Should().Be(opKind);
            expr.Errors().Length.Should().Be(0);
            SyntaxToken token;
            switch (expr.Kind())
            {
                case SyntaxKind.ThisExpression:
                    token = ((ThisExpressionSyntax)expr).Token;
                    token.Should().NotBe(default);
                    token.Kind().Should().Be(kind);
                    break;
                case SyntaxKind.BaseExpression:
                    token = ((BaseExpressionSyntax)expr).Token;
                    token.Should().NotBe(default);
                    token.Kind().Should().Be(kind);
                    break;
            }
        }

        [Fact]
        public void TestInstanceExpressions()
        {
            TestInstanceExpression(SyntaxKind.ThisKeyword);
            TestInstanceExpression(SyntaxKind.BaseKeyword);
        }

        [Fact]
        public void TestStringLiteralExpression()
        {
            var text = "\"stuff\"";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.StringLiteralExpression);
            expr.Errors().Length.Should().Be(0);
            var us = (LiteralExpressionSyntax)expr;
            us.Token.Should().NotBe(default);
            us.Token.Kind().Should().Be(SyntaxKind.StringLiteralToken);
        }

        [WorkItem(540379, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540379")]
        [Fact]
        public void TestVerbatimLiteralExpression()
        {
            var text = "@\"\"\"stuff\"\"\"";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.StringLiteralExpression);
            expr.Errors().Length.Should().Be(0);
            var us = (LiteralExpressionSyntax)expr;
            us.Token.Should().NotBe(default);
            us.Token.Kind().Should().Be(SyntaxKind.StringLiteralToken);
            us.Token.ValueText.Should().Be("\"stuff\"");
        }

        [Fact]
        public void TestCharacterLiteralExpression()
        {
            var text = "'c'";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.CharacterLiteralExpression);
            expr.Errors().Length.Should().Be(0);
            var us = (LiteralExpressionSyntax)expr;
            us.Token.Should().NotBe(default);
            us.Token.Kind().Should().Be(SyntaxKind.CharacterLiteralToken);
        }

        [Fact]
        public void TestNumericLiteralExpression()
        {
            var text = "0";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.NumericLiteralExpression);
            expr.Errors().Length.Should().Be(0);
            var us = (LiteralExpressionSyntax)expr;
            us.Token.Should().NotBe(default);
            us.Token.Kind().Should().Be(SyntaxKind.NumericLiteralToken);
        }

        private void TestPrefixUnary(SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind) + "a";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            var opKind = SyntaxFacts.GetPrefixUnaryExpression(kind);
            expr.Kind().Should().Be(opKind);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var us = (PrefixUnaryExpressionSyntax)expr;
            us.OperatorToken.Should().NotBe(default);
            us.OperatorToken.Kind().Should().Be(kind);
            us.Operand.Should().NotBeNull();
            us.Operand.Kind().Should().Be(SyntaxKind.IdentifierName);
            us.Operand.ToString().Should().Be("a");
        }

        [Fact]
        public void TestPrefixUnaryOperators()
        {
            TestPrefixUnary(SyntaxKind.PlusToken);
            TestPrefixUnary(SyntaxKind.MinusToken);
            TestPrefixUnary(SyntaxKind.TildeToken);
            TestPrefixUnary(SyntaxKind.ExclamationToken);
            TestPrefixUnary(SyntaxKind.PlusPlusToken);
            TestPrefixUnary(SyntaxKind.MinusMinusToken);
            TestPrefixUnary(SyntaxKind.AmpersandToken);
            TestPrefixUnary(SyntaxKind.AsteriskToken);
        }

        private void TestPostfixUnary(SyntaxKind kind, ParseOptions options = null)
        {
            var text = "a" + SyntaxFacts.GetText(kind);
            var expr = this.ParseExpression(text, options: options);

            expr.Should().NotBeNull();
            var opKind = SyntaxFacts.GetPostfixUnaryExpression(kind);
            expr.Kind().Should().Be(opKind);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var us = (PostfixUnaryExpressionSyntax)expr;
            us.OperatorToken.Should().NotBe(default);
            us.OperatorToken.Kind().Should().Be(kind);
            us.Operand.Should().NotBeNull();
            us.Operand.Kind().Should().Be(SyntaxKind.IdentifierName);
            us.Operand.ToString().Should().Be("a");
        }

        [Fact]
        public void TestPostfixUnaryOperators()
        {
            TestPostfixUnary(SyntaxKind.PlusPlusToken);
            TestPostfixUnary(SyntaxKind.MinusMinusToken);
            TestPostfixUnary(SyntaxKind.ExclamationToken, TestOptions.Regular8);
        }

        private void TestBinary(SyntaxKind kind)
        {
            var text = "(a) " + SyntaxFacts.GetText(kind) + " b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            var opKind = SyntaxFacts.GetBinaryExpression(kind);
            expr.Kind().Should().Be(opKind);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var b = (BinaryExpressionSyntax)expr;
            b.OperatorToken.Should().NotBe(default);
            b.OperatorToken.Kind().Should().Be(kind);
            b.Left.Should().NotBeNull();
            b.Right.Should().NotBeNull();
            b.Left.ToString().Should().Be("(a)");
            b.Right.ToString().Should().Be("b");
        }

        [Fact]
        public void TestBinaryOperators()
        {
            TestBinary(SyntaxKind.PlusToken);
            TestBinary(SyntaxKind.MinusToken);
            TestBinary(SyntaxKind.AsteriskToken);
            TestBinary(SyntaxKind.SlashToken);
            TestBinary(SyntaxKind.PercentToken);
            TestBinary(SyntaxKind.EqualsEqualsToken);
            TestBinary(SyntaxKind.ExclamationEqualsToken);
            TestBinary(SyntaxKind.LessThanToken);
            TestBinary(SyntaxKind.LessThanEqualsToken);
            TestBinary(SyntaxKind.LessThanLessThanToken);
            TestBinary(SyntaxKind.GreaterThanToken);
            TestBinary(SyntaxKind.GreaterThanEqualsToken);
            TestBinary(SyntaxKind.GreaterThanGreaterThanToken);
            TestBinary(SyntaxKind.GreaterThanGreaterThanGreaterThanToken);
            TestBinary(SyntaxKind.AmpersandToken);
            TestBinary(SyntaxKind.AmpersandAmpersandToken);
            TestBinary(SyntaxKind.BarToken);
            TestBinary(SyntaxKind.BarBarToken);
            TestBinary(SyntaxKind.CaretToken);
            TestBinary(SyntaxKind.IsKeyword);
            TestBinary(SyntaxKind.AsKeyword);
            TestBinary(SyntaxKind.QuestionQuestionToken);
        }

        private void TestAssignment(SyntaxKind kind, ParseOptions options = null)
        {
            var text = "(a) " + SyntaxFacts.GetText(kind) + " b";
            var expr = this.ParseExpression(text, options);

            expr.Should().NotBeNull();
            var opKind = SyntaxFacts.GetAssignmentExpression(kind);
            expr.Kind().Should().Be(opKind);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var a = (AssignmentExpressionSyntax)expr;
            a.OperatorToken.Should().NotBe(default);
            a.OperatorToken.Kind().Should().Be(kind);
            a.Left.Should().NotBeNull();
            a.Right.Should().NotBeNull();
            a.Left.ToString().Should().Be("(a)");
            a.Right.ToString().Should().Be("b");
        }

        [Fact]
        public void TestAssignmentOperators()
        {
            TestAssignment(SyntaxKind.PlusEqualsToken);
            TestAssignment(SyntaxKind.MinusEqualsToken);
            TestAssignment(SyntaxKind.AsteriskEqualsToken);
            TestAssignment(SyntaxKind.SlashEqualsToken);
            TestAssignment(SyntaxKind.PercentEqualsToken);
            TestAssignment(SyntaxKind.EqualsToken);
            TestAssignment(SyntaxKind.LessThanLessThanEqualsToken);
            TestAssignment(SyntaxKind.GreaterThanGreaterThanEqualsToken);
            TestAssignment(SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken);
            TestAssignment(SyntaxKind.AmpersandEqualsToken);
            TestAssignment(SyntaxKind.BarEqualsToken);
            TestAssignment(SyntaxKind.CaretEqualsToken);
            TestAssignment(SyntaxKind.QuestionQuestionEqualsToken, options: TestOptions.Regular8);
        }

        private void TestMemberAccess(SyntaxKind kind)
        {
            var text = "(a)" + SyntaxFacts.GetText(kind) + " b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var e = (MemberAccessExpressionSyntax)expr;
            e.OperatorToken.Should().NotBe(default);
            e.OperatorToken.Kind().Should().Be(kind);
            e.Expression.Should().NotBeNull();
            e.Name.Should().NotBeNull();
            e.Expression.ToString().Should().Be("(a)");
            e.Name.ToString().Should().Be("b");
        }

        [Fact]
        public void TestMemberAccessTokens()
        {
            TestMemberAccess(SyntaxKind.DotToken);
            TestMemberAccess(SyntaxKind.MinusGreaterThanToken);
        }

        [Fact]
        public void TestConditionalAccessNotVersion5()
        {
            var text = "a.b?.c.d?[1]?.e()?.f";
            var expr = this.ParseExpression(text, options: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp5));

            expr.Should().NotBeNull();
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var e = (ConditionalAccessExpressionSyntax)expr;
            e.Expression.ToString().Should().Be("a.b");
            e.WhenNotNull.ToString().Should().Be(".c.d?[1]?.e()?.f");

            var testWithStatement = @$"class C {{ void M() {{ var v = {text}; }} }}";
            CreateCompilation(testWithStatement, parseOptions: TestOptions.Regular5).VerifyDiagnostics(
                // (1,30): error CS0103: The name 'a' does not exist in the current context
                // class C { void M() { var v = a.b?.c.d?[1]?.e()?.f; } }
                Diagnostic(ErrorCode.ERR_NameNotInContext, "a").WithArguments("a").WithLocation(1, 30),
                // (1,33): error CS8026: Feature 'null propagating operator' is not available in C# 5. Please use language version 6 or greater.
                // class C { void M() { var v = a.b?.c.d?[1]?.e()?.f; } }
                Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion5, "?").WithArguments("null propagating operator", "6").WithLocation(1, 33),
                // (1,38): error CS8026: Feature 'null propagating operator' is not available in C# 5. Please use language version 6 or greater.
                // class C { void M() { var v = a.b?.c.d?[1]?.e()?.f; } }
                Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion5, "?").WithArguments("null propagating operator", "6").WithLocation(1, 38),
                // (1,42): error CS8026: Feature 'null propagating operator' is not available in C# 5. Please use language version 6 or greater.
                // class C { void M() { var v = a.b?.c.d?[1]?.e()?.f; } }
                Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion5, "?").WithArguments("null propagating operator", "6").WithLocation(1, 42),
                // (1,47): error CS8026: Feature 'null propagating operator' is not available in C# 5. Please use language version 6 or greater.
                // class C { void M() { var v = a.b?.c.d?[1]?.e()?.f; } }
                Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion5, "?").WithArguments("null propagating operator", "6").WithLocation(1, 47));
        }

        [Fact]
        public void TestConditionalAccess()
        {
            var text = "a.b?.c.d?[1]?.e()?.f";
            var expr = this.ParseExpression(text, options: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp6));

            expr.Should().NotBeNull();
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var e = (ConditionalAccessExpressionSyntax)expr;
            e.Expression.ToString().Should().Be("a.b");
            var cons = e.WhenNotNull;
            cons.ToString().Should().Be(".c.d?[1]?.e()?.f");
            cons.Kind().Should().Be(SyntaxKind.ConditionalAccessExpression);

            e = e.WhenNotNull as ConditionalAccessExpressionSyntax;
            e.Expression.ToString().Should().Be(".c.d");
            cons = e.WhenNotNull;
            cons.ToString().Should().Be("[1]?.e()?.f");
            cons.Kind().Should().Be(SyntaxKind.ConditionalAccessExpression);

            e = e.WhenNotNull as ConditionalAccessExpressionSyntax;
            e.Expression.ToString().Should().Be("[1]");
            cons = e.WhenNotNull;
            cons.ToString().Should().Be(".e()?.f");
            cons.Kind().Should().Be(SyntaxKind.ConditionalAccessExpression);

            e = e.WhenNotNull as ConditionalAccessExpressionSyntax;
            e.Expression.ToString().Should().Be(".e()");
            cons = e.WhenNotNull;
            cons.ToString().Should().Be(".f");
            cons.Kind().Should().Be(SyntaxKind.MemberBindingExpression);
        }

        private void TestFunctionKeyword(SyntaxKind kind, SyntaxToken keyword)
        {
            keyword.Should().NotBe(default);
            keyword.Kind().Should().Be(kind);
        }

        private void TestParenthesizedArgument(SyntaxToken openParen, CSharpSyntaxNode arg, SyntaxToken closeParen)
        {
            openParen.Should().NotBe(default);
            openParen.IsMissing.Should().BeFalse();
            closeParen.Should().NotBe(default);
            closeParen.IsMissing.Should().BeFalse();
            arg.ToString().Should().Be("a");
        }

        private void TestSingleParamFunctionalOperator(SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind) + "(a)";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            var opKind = SyntaxFacts.GetPrimaryFunction(kind);
            expr.Kind().Should().Be(opKind);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            switch (opKind)
            {
                case SyntaxKind.MakeRefExpression:
                    var makeRefSyntax = (MakeRefExpressionSyntax)expr;
                    TestFunctionKeyword(kind, makeRefSyntax.Keyword);
                    TestParenthesizedArgument(makeRefSyntax.OpenParenToken, makeRefSyntax.Expression, makeRefSyntax.CloseParenToken);
                    break;

                case SyntaxKind.RefTypeExpression:
                    var refTypeSyntax = (RefTypeExpressionSyntax)expr;
                    TestFunctionKeyword(kind, refTypeSyntax.Keyword);
                    TestParenthesizedArgument(refTypeSyntax.OpenParenToken, refTypeSyntax.Expression, refTypeSyntax.CloseParenToken);
                    break;

                case SyntaxKind.CheckedExpression:
                case SyntaxKind.UncheckedExpression:
                    var checkedSyntax = (CheckedExpressionSyntax)expr;
                    TestFunctionKeyword(kind, checkedSyntax.Keyword);
                    TestParenthesizedArgument(checkedSyntax.OpenParenToken, checkedSyntax.Expression, checkedSyntax.CloseParenToken);
                    break;

                case SyntaxKind.TypeOfExpression:
                    var typeOfSyntax = (TypeOfExpressionSyntax)expr;
                    TestFunctionKeyword(kind, typeOfSyntax.Keyword);
                    TestParenthesizedArgument(typeOfSyntax.OpenParenToken, typeOfSyntax.Type, typeOfSyntax.CloseParenToken);
                    break;

                case SyntaxKind.SizeOfExpression:
                    var sizeOfSyntax = (SizeOfExpressionSyntax)expr;
                    TestFunctionKeyword(kind, sizeOfSyntax.Keyword);
                    TestParenthesizedArgument(sizeOfSyntax.OpenParenToken, sizeOfSyntax.Type, sizeOfSyntax.CloseParenToken);
                    break;

                case SyntaxKind.DefaultExpression:
                    var defaultSyntax = (DefaultExpressionSyntax)expr;
                    TestFunctionKeyword(kind, defaultSyntax.Keyword);
                    TestParenthesizedArgument(defaultSyntax.OpenParenToken, defaultSyntax.Type, defaultSyntax.CloseParenToken);
                    break;
            }
        }

        [Fact]
        public void TestFunctionOperators()
        {
            TestSingleParamFunctionalOperator(SyntaxKind.MakeRefKeyword);
            TestSingleParamFunctionalOperator(SyntaxKind.RefTypeKeyword);
            TestSingleParamFunctionalOperator(SyntaxKind.CheckedKeyword);
            TestSingleParamFunctionalOperator(SyntaxKind.UncheckedKeyword);
            TestSingleParamFunctionalOperator(SyntaxKind.SizeOfKeyword);
            TestSingleParamFunctionalOperator(SyntaxKind.TypeOfKeyword);
            TestSingleParamFunctionalOperator(SyntaxKind.DefaultKeyword);
        }

        [Fact]
        public void TestRefValue()
        {
            var text = SyntaxFacts.GetText(SyntaxKind.RefValueKeyword) + "(a, b)";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.RefValueExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var fs = (RefValueExpressionSyntax)expr;
            fs.Keyword.Should().NotBe(default);
            fs.Keyword.Kind().Should().Be(SyntaxKind.RefValueKeyword);
            fs.OpenParenToken.Should().NotBe(default);
            fs.OpenParenToken.IsMissing.Should().BeFalse();
            fs.CloseParenToken.Should().NotBe(default);
            fs.CloseParenToken.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("a");
            fs.Type.ToString().Should().Be("b");
        }

        [Fact]
        public void TestConditional()
        {
            var text = "a ? b : c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ConditionalExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ts = (ConditionalExpressionSyntax)expr;
            ts.QuestionToken.Should().NotBe(default);
            ts.ColonToken.Should().NotBe(default);
            ts.QuestionToken.Kind().Should().Be(SyntaxKind.QuestionToken);
            ts.ColonToken.Kind().Should().Be(SyntaxKind.ColonToken);
            ts.Condition.ToString().Should().Be("a");
            ts.WhenTrue.ToString().Should().Be("b");
            ts.WhenFalse.ToString().Should().Be("c");
        }

        [Fact]
        public void TestConditional02()
        {
            // ensure that ?: has lower precedence than assignment.
            var text = "a ? b=c : d=e";
            var expr = this.ParseExpression(text);
            expr.Kind().Should().Be(SyntaxKind.ConditionalExpression);
            expr.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void TestCast()
        {
            var text = "(a) b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.CastExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var cs = (CastExpressionSyntax)expr;
            cs.OpenParenToken.Should().NotBe(default);
            cs.CloseParenToken.Should().NotBe(default);
            cs.OpenParenToken.IsMissing.Should().BeFalse();
            cs.CloseParenToken.IsMissing.Should().BeFalse();
            cs.Type.Should().NotBeNull();
            cs.Expression.Should().NotBeNull();
            cs.Type.ToString().Should().Be("a");
            cs.Expression.ToString().Should().Be("b");
        }

        [Fact]
        public void TestCall()
        {
            var text = "a(b)";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.InvocationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var cs = (InvocationExpressionSyntax)expr;
            cs.ArgumentList.OpenParenToken.Should().NotBe(default);
            cs.ArgumentList.CloseParenToken.Should().NotBe(default);
            cs.ArgumentList.OpenParenToken.IsMissing.Should().BeFalse();
            cs.ArgumentList.CloseParenToken.IsMissing.Should().BeFalse();
            cs.Expression.Should().NotBeNull();
            cs.ArgumentList.Arguments.Count.Should().Be(1);
            cs.Expression.ToString().Should().Be("a");
            cs.ArgumentList.Arguments[0].ToString().Should().Be("b");
        }

        [Fact]
        public void TestCallWithRef()
        {
            var text = "a(ref b)";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.InvocationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var cs = (InvocationExpressionSyntax)expr;
            cs.ArgumentList.OpenParenToken.Should().NotBe(default);
            cs.ArgumentList.CloseParenToken.Should().NotBe(default);
            cs.ArgumentList.OpenParenToken.IsMissing.Should().BeFalse();
            cs.ArgumentList.CloseParenToken.IsMissing.Should().BeFalse();
            cs.Expression.Should().NotBeNull();
            cs.ArgumentList.Arguments.Count.Should().Be(1);
            cs.Expression.ToString().Should().Be("a");
            cs.ArgumentList.Arguments[0].ToString().Should().Be("ref b");
            cs.ArgumentList.Arguments[0].RefOrOutKeyword.Should().NotBe(default);
            cs.ArgumentList.Arguments[0].RefOrOutKeyword.Kind().Should().Be(SyntaxKind.RefKeyword);
            cs.ArgumentList.Arguments[0].Expression.Should().NotBeNull();
            cs.ArgumentList.Arguments[0].Expression.ToString().Should().Be("b");
        }

        [Fact]
        public void TestCallWithOut()
        {
            var text = "a(out b)";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.InvocationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var cs = (InvocationExpressionSyntax)expr;
            cs.ArgumentList.OpenParenToken.Should().NotBe(default);
            cs.ArgumentList.CloseParenToken.Should().NotBe(default);
            cs.ArgumentList.OpenParenToken.IsMissing.Should().BeFalse();
            cs.ArgumentList.CloseParenToken.IsMissing.Should().BeFalse();
            cs.Expression.Should().NotBeNull();
            cs.ArgumentList.Arguments.Count.Should().Be(1);
            cs.Expression.ToString().Should().Be("a");
            cs.ArgumentList.Arguments[0].ToString().Should().Be("out b");
            cs.ArgumentList.Arguments[0].RefOrOutKeyword.Should().NotBe(default);
            cs.ArgumentList.Arguments[0].RefOrOutKeyword.Kind().Should().Be(SyntaxKind.OutKeyword);
            cs.ArgumentList.Arguments[0].Expression.Should().NotBeNull();
            cs.ArgumentList.Arguments[0].Expression.ToString().Should().Be("b");
        }

        [Fact]
        public void TestCallWithNamedArgument()
        {
            var text = "a(B: b)";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.InvocationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var cs = (InvocationExpressionSyntax)expr;
            cs.ArgumentList.OpenParenToken.Should().NotBe(default);
            cs.ArgumentList.CloseParenToken.Should().NotBe(default);
            cs.ArgumentList.OpenParenToken.IsMissing.Should().BeFalse();
            cs.ArgumentList.CloseParenToken.IsMissing.Should().BeFalse();
            cs.Expression.Should().NotBeNull();
            cs.ArgumentList.Arguments.Count.Should().Be(1);
            cs.Expression.ToString().Should().Be("a");
            cs.ArgumentList.Arguments[0].ToString().Should().Be("B: b");
            cs.ArgumentList.Arguments[0].NameColon.Should().NotBeNull();
            cs.ArgumentList.Arguments[0].NameColon.Name.ToString().Should().Be("B");
            cs.ArgumentList.Arguments[0].NameColon.ColonToken.Should().NotBe(default);
            cs.ArgumentList.Arguments[0].Expression.ToString().Should().Be("b");
        }

        [Fact]
        public void TestIndex()
        {
            var text = "a[b]";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ea = (ElementAccessExpressionSyntax)expr;
            ea.ArgumentList.OpenBracketToken.Should().NotBe(default);
            ea.ArgumentList.CloseBracketToken.Should().NotBe(default);
            ea.ArgumentList.OpenBracketToken.IsMissing.Should().BeFalse();
            ea.ArgumentList.CloseBracketToken.IsMissing.Should().BeFalse();
            ea.Expression.Should().NotBeNull();
            ea.ArgumentList.Arguments.Count.Should().Be(1);
            ea.Expression.ToString().Should().Be("a");
            ea.ArgumentList.Arguments[0].ToString().Should().Be("b");
        }

        [Fact]
        public void TestIndexWithRef()
        {
            var text = "a[ref b]";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ea = (ElementAccessExpressionSyntax)expr;
            ea.ArgumentList.OpenBracketToken.Should().NotBe(default);
            ea.ArgumentList.CloseBracketToken.Should().NotBe(default);
            ea.ArgumentList.OpenBracketToken.IsMissing.Should().BeFalse();
            ea.ArgumentList.CloseBracketToken.IsMissing.Should().BeFalse();
            ea.Expression.Should().NotBeNull();
            ea.ArgumentList.Arguments.Count.Should().Be(1);
            ea.Expression.ToString().Should().Be("a");
            ea.ArgumentList.Arguments[0].ToString().Should().Be("ref b");
            ea.ArgumentList.Arguments[0].RefOrOutKeyword.Should().NotBe(default);
            ea.ArgumentList.Arguments[0].RefOrOutKeyword.Kind().Should().Be(SyntaxKind.RefKeyword);
            ea.ArgumentList.Arguments[0].Expression.Should().NotBeNull();
            ea.ArgumentList.Arguments[0].Expression.ToString().Should().Be("b");
        }

        [Fact]
        public void TestIndexWithOut()
        {
            var text = "a[out b]";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ea = (ElementAccessExpressionSyntax)expr;
            ea.ArgumentList.OpenBracketToken.Should().NotBe(default);
            ea.ArgumentList.CloseBracketToken.Should().NotBe(default);
            ea.ArgumentList.OpenBracketToken.IsMissing.Should().BeFalse();
            ea.ArgumentList.CloseBracketToken.IsMissing.Should().BeFalse();
            ea.Expression.Should().NotBeNull();
            ea.ArgumentList.Arguments.Count.Should().Be(1);
            ea.Expression.ToString().Should().Be("a");
            ea.ArgumentList.Arguments[0].ToString().Should().Be("out b");
            ea.ArgumentList.Arguments[0].RefOrOutKeyword.Should().NotBe(default);
            ea.ArgumentList.Arguments[0].RefOrOutKeyword.Kind().Should().Be(SyntaxKind.OutKeyword);
            ea.ArgumentList.Arguments[0].Expression.Should().NotBeNull();
            ea.ArgumentList.Arguments[0].Expression.ToString().Should().Be("b");
        }

        [Fact]
        public void TestIndexWithNamedArgument()
        {
            var text = "a[B: b]";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ea = (ElementAccessExpressionSyntax)expr;
            ea.ArgumentList.OpenBracketToken.Should().NotBe(default);
            ea.ArgumentList.CloseBracketToken.Should().NotBe(default);
            ea.ArgumentList.OpenBracketToken.IsMissing.Should().BeFalse();
            ea.ArgumentList.CloseBracketToken.IsMissing.Should().BeFalse();
            ea.Expression.Should().NotBeNull();
            ea.ArgumentList.Arguments.Count.Should().Be(1);
            ea.Expression.ToString().Should().Be("a");
            ea.ArgumentList.Arguments[0].ToString().Should().Be("B: b");
        }

        [Fact]
        public void TestNew()
        {
            var text = "new a()";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var oc = (ObjectCreationExpressionSyntax)expr;
            oc.ArgumentList.Should().NotBeNull();
            oc.ArgumentList.OpenParenToken.Should().NotBe(default);
            oc.ArgumentList.CloseParenToken.Should().NotBe(default);
            oc.ArgumentList.OpenParenToken.IsMissing.Should().BeFalse();
            oc.ArgumentList.CloseParenToken.IsMissing.Should().BeFalse();
            oc.ArgumentList.Arguments.Count.Should().Be(0);
            oc.Type.Should().NotBeNull();
            oc.Type.ToString().Should().Be("a");
            oc.Initializer.Should().BeNull();
        }

        [Fact]
        public void TestNewWithArgument()
        {
            var text = "new a(b)";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var oc = (ObjectCreationExpressionSyntax)expr;
            oc.ArgumentList.Should().NotBeNull();
            oc.ArgumentList.OpenParenToken.Should().NotBe(default);
            oc.ArgumentList.CloseParenToken.Should().NotBe(default);
            oc.ArgumentList.OpenParenToken.IsMissing.Should().BeFalse();
            oc.ArgumentList.CloseParenToken.IsMissing.Should().BeFalse();
            oc.ArgumentList.Arguments.Count.Should().Be(1);
            oc.ArgumentList.Arguments[0].ToString().Should().Be("b");
            oc.Type.Should().NotBeNull();
            oc.Type.ToString().Should().Be("a");
            oc.Initializer.Should().BeNull();
        }

        [Fact]
        public void TestNewWithNamedArgument()
        {
            var text = "new a(B: b)";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var oc = (ObjectCreationExpressionSyntax)expr;
            oc.ArgumentList.Should().NotBeNull();
            oc.ArgumentList.OpenParenToken.Should().NotBe(default);
            oc.ArgumentList.CloseParenToken.Should().NotBe(default);
            oc.ArgumentList.OpenParenToken.IsMissing.Should().BeFalse();
            oc.ArgumentList.CloseParenToken.IsMissing.Should().BeFalse();
            oc.ArgumentList.Arguments.Count.Should().Be(1);
            oc.ArgumentList.Arguments[0].ToString().Should().Be("B: b");
            oc.Type.Should().NotBeNull();
            oc.Type.ToString().Should().Be("a");
            oc.Initializer.Should().BeNull();
        }

        [Fact]
        public void TestNewWithEmptyInitializer()
        {
            var text = "new a() { }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var oc = (ObjectCreationExpressionSyntax)expr;
            oc.ArgumentList.Should().NotBeNull();
            oc.ArgumentList.OpenParenToken.Should().NotBe(default);
            oc.ArgumentList.CloseParenToken.Should().NotBe(default);
            oc.ArgumentList.OpenParenToken.IsMissing.Should().BeFalse();
            oc.ArgumentList.CloseParenToken.IsMissing.Should().BeFalse();
            oc.ArgumentList.Arguments.Count.Should().Be(0);
            oc.Type.Should().NotBeNull();
            oc.Type.ToString().Should().Be("a");

            oc.Initializer.Should().NotBeNull();
            oc.Initializer.OpenBraceToken.Should().NotBe(default);
            oc.Initializer.CloseBraceToken.Should().NotBe(default);
            oc.Initializer.OpenBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.CloseBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.Expressions.Count.Should().Be(0);
        }

        [Fact]
        public void TestNewWithNoArgumentsAndEmptyInitializer()
        {
            var text = "new a { }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var oc = (ObjectCreationExpressionSyntax)expr;
            oc.ArgumentList.Should().BeNull();
            oc.Type.Should().NotBeNull();
            oc.Type.ToString().Should().Be("a");

            oc.Initializer.Should().NotBeNull();
            oc.Initializer.OpenBraceToken.Should().NotBe(default);
            oc.Initializer.CloseBraceToken.Should().NotBe(default);
            oc.Initializer.OpenBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.CloseBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.Expressions.Count.Should().Be(0);
        }

        [Fact]
        public void TestNewWithNoArgumentsAndInitializer()
        {
            var text = "new a { b }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var oc = (ObjectCreationExpressionSyntax)expr;
            oc.ArgumentList.Should().BeNull();
            oc.Type.Should().NotBeNull();
            oc.Type.ToString().Should().Be("a");

            oc.Initializer.Should().NotBeNull();
            oc.Initializer.OpenBraceToken.Should().NotBe(default);
            oc.Initializer.CloseBraceToken.Should().NotBe(default);
            oc.Initializer.OpenBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.CloseBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.Expressions.Count.Should().Be(1);
            oc.Initializer.Expressions[0].ToString().Should().Be("b");
        }

        [Fact]
        public void TestNewWithNoArgumentsAndInitializers()
        {
            var text = "new a { b, c, d }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var oc = (ObjectCreationExpressionSyntax)expr;
            oc.ArgumentList.Should().BeNull();
            oc.Type.Should().NotBeNull();
            oc.Type.ToString().Should().Be("a");

            oc.Initializer.Should().NotBeNull();
            oc.Initializer.OpenBraceToken.Should().NotBe(default);
            oc.Initializer.CloseBraceToken.Should().NotBe(default);
            oc.Initializer.OpenBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.CloseBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.Expressions.Count.Should().Be(3);
            oc.Initializer.Expressions[0].ToString().Should().Be("b");
            oc.Initializer.Expressions[1].ToString().Should().Be("c");
            oc.Initializer.Expressions[2].ToString().Should().Be("d");
        }

        [Fact]
        public void TestNewWithNoArgumentsAndAssignmentInitializer()
        {
            var text = "new a { B = b }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var oc = (ObjectCreationExpressionSyntax)expr;
            oc.ArgumentList.Should().BeNull();
            oc.Type.Should().NotBeNull();
            oc.Type.ToString().Should().Be("a");

            oc.Initializer.Should().NotBeNull();
            oc.Initializer.OpenBraceToken.Should().NotBe(default);
            oc.Initializer.CloseBraceToken.Should().NotBe(default);
            oc.Initializer.OpenBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.CloseBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.Expressions.Count.Should().Be(1);
            oc.Initializer.Expressions[0].ToString().Should().Be("B = b");
        }

        [Fact]
        public void TestNewWithNoArgumentsAndNestedAssignmentInitializer()
        {
            var text = "new a { B = { X = x } }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var oc = (ObjectCreationExpressionSyntax)expr;
            oc.ArgumentList.Should().BeNull();
            oc.Type.Should().NotBeNull();
            oc.Type.ToString().Should().Be("a");

            oc.Initializer.Should().NotBeNull();
            oc.Initializer.OpenBraceToken.Should().NotBe(default);
            oc.Initializer.CloseBraceToken.Should().NotBe(default);
            oc.Initializer.OpenBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.CloseBraceToken.IsMissing.Should().BeFalse();
            oc.Initializer.Expressions.Count.Should().Be(1);
            oc.Initializer.Expressions[0].ToString().Should().Be("B = { X = x }");
            oc.Initializer.Expressions[0].Kind().Should().Be(SyntaxKind.SimpleAssignmentExpression);
            var b = (AssignmentExpressionSyntax)oc.Initializer.Expressions[0];
            b.Left.ToString().Should().Be("B");
            b.Right.Kind().Should().Be(SyntaxKind.ObjectInitializerExpression);
        }

        [Fact]
        public void TestNewArray()
        {
            var text = "new a[1]";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ArrayCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ac = (ArrayCreationExpressionSyntax)expr;
            ac.Type.Should().NotBeNull();
            ac.Type.ToString().Should().Be("a[1]");
            ac.Initializer.Should().BeNull();
        }

        [Fact]
        public void TestNewArrayWithInitializer()
        {
            var text = "new a[] {b}";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ArrayCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ac = (ArrayCreationExpressionSyntax)expr;
            ac.Type.Should().NotBeNull();
            ac.Type.ToString().Should().Be("a[]");
            ac.Initializer.Should().NotBeNull();
            ac.Initializer.OpenBraceToken.Should().NotBe(default);
            ac.Initializer.CloseBraceToken.Should().NotBe(default);
            ac.Initializer.OpenBraceToken.IsMissing.Should().BeFalse();
            ac.Initializer.CloseBraceToken.IsMissing.Should().BeFalse();
            ac.Initializer.Expressions.Count.Should().Be(1);
            ac.Initializer.Expressions[0].ToString().Should().Be("b");
        }

        [Fact]
        public void TestNewArrayWithInitializers()
        {
            var text = "new a[] {b, c, d}";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ArrayCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ac = (ArrayCreationExpressionSyntax)expr;
            ac.Type.Should().NotBeNull();
            ac.Type.ToString().Should().Be("a[]");
            ac.Initializer.Should().NotBeNull();
            ac.Initializer.OpenBraceToken.Should().NotBe(default);
            ac.Initializer.CloseBraceToken.Should().NotBe(default);
            ac.Initializer.OpenBraceToken.IsMissing.Should().BeFalse();
            ac.Initializer.CloseBraceToken.IsMissing.Should().BeFalse();
            ac.Initializer.Expressions.Count.Should().Be(3);
            ac.Initializer.Expressions[0].ToString().Should().Be("b");
            ac.Initializer.Expressions[1].ToString().Should().Be("c");
            ac.Initializer.Expressions[2].ToString().Should().Be("d");
        }

        [Fact]
        public void TestNewMultiDimensionalArrayWithInitializer()
        {
            var text = "new a[][,][,,] {b}";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ArrayCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ac = (ArrayCreationExpressionSyntax)expr;
            ac.Type.Should().NotBeNull();
            ac.Type.ToString().Should().Be("a[][,][,,]");
            ac.Initializer.Should().NotBeNull();
            ac.Initializer.OpenBraceToken.Should().NotBe(default);
            ac.Initializer.CloseBraceToken.Should().NotBe(default);
            ac.Initializer.OpenBraceToken.IsMissing.Should().BeFalse();
            ac.Initializer.CloseBraceToken.IsMissing.Should().BeFalse();
            ac.Initializer.Expressions.Count.Should().Be(1);
            ac.Initializer.Expressions[0].ToString().Should().Be("b");
        }

        [Fact]
        public void TestImplicitArrayCreation()
        {
            var text = "new [] {b}";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ImplicitArrayCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ac = (ImplicitArrayCreationExpressionSyntax)expr;
            ac.Initializer.Should().NotBeNull();
            ac.Initializer.OpenBraceToken.Should().NotBe(default);
            ac.Initializer.CloseBraceToken.Should().NotBe(default);
            ac.Initializer.OpenBraceToken.IsMissing.Should().BeFalse();
            ac.Initializer.CloseBraceToken.IsMissing.Should().BeFalse();
            ac.Initializer.Expressions.Count.Should().Be(1);
            ac.Initializer.Expressions[0].ToString().Should().Be("b");
        }

        [Fact]
        public void TestAnonymousObjectCreation()
        {
            var text = "new {a, b}";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var ac = (AnonymousObjectCreationExpressionSyntax)expr;
            ac.NewKeyword.Should().NotBe(default);
            ac.OpenBraceToken.Should().NotBe(default);
            ac.CloseBraceToken.Should().NotBe(default);
            ac.OpenBraceToken.IsMissing.Should().BeFalse();
            ac.CloseBraceToken.IsMissing.Should().BeFalse();
            ac.Initializers.Count.Should().Be(2);
            ac.Initializers[0].ToString().Should().Be("a");
            ac.Initializers[1].ToString().Should().Be("b");
        }

        [Fact]
        public void TestAnonymousMethod()
        {
            var text = "delegate (int a) { }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.AnonymousMethodExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var am = (AnonymousMethodExpressionSyntax)expr;

            am.DelegateKeyword.Should().NotBe(default);
            am.DelegateKeyword.IsMissing.Should().BeFalse();

            am.ParameterList.Should().NotBeNull();
            am.ParameterList.OpenParenToken.Should().NotBe(default);
            am.ParameterList.CloseParenToken.Should().NotBe(default);
            am.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            am.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            am.ParameterList.Parameters.Count.Should().Be(1);
            am.ParameterList.Parameters[0].ToString().Should().Be("int a");

            am.Block.Should().NotBeNull();
            am.Block.OpenBraceToken.Should().NotBe(default);
            am.Block.CloseBraceToken.Should().NotBe(default);
            am.Block.OpenBraceToken.IsMissing.Should().BeFalse();
            am.Block.CloseBraceToken.IsMissing.Should().BeFalse();
            am.Block.Statements.Count.Should().Be(0);
        }

        [Fact]
        public void TestAnonymousMethodWithNoArguments()
        {
            var text = "delegate () { }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.AnonymousMethodExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var am = (AnonymousMethodExpressionSyntax)expr;

            am.DelegateKeyword.Should().NotBe(default);
            am.DelegateKeyword.IsMissing.Should().BeFalse();

            am.ParameterList.Should().NotBeNull();
            am.ParameterList.OpenParenToken.Should().NotBe(default);
            am.ParameterList.CloseParenToken.Should().NotBe(default);
            am.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            am.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            am.ParameterList.Parameters.Count.Should().Be(0);

            am.Block.Should().NotBeNull();
            am.Block.OpenBraceToken.Should().NotBe(default);
            am.Block.CloseBraceToken.Should().NotBe(default);
            am.Block.OpenBraceToken.IsMissing.Should().BeFalse();
            am.Block.CloseBraceToken.IsMissing.Should().BeFalse();
            am.Block.Statements.Count.Should().Be(0);
        }

        [Fact]
        public void TestAnonymousMethodWithNoArgumentList()
        {
            var text = "delegate { }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.AnonymousMethodExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var am = (AnonymousMethodExpressionSyntax)expr;

            am.DelegateKeyword.Should().NotBe(default);
            am.DelegateKeyword.IsMissing.Should().BeFalse();

            am.ParameterList.Should().BeNull();

            am.Block.Should().NotBeNull();
            am.Block.OpenBraceToken.Should().NotBe(default);
            am.Block.CloseBraceToken.Should().NotBe(default);
            am.Block.OpenBraceToken.IsMissing.Should().BeFalse();
            am.Block.CloseBraceToken.IsMissing.Should().BeFalse();
            am.Block.Statements.Count.Should().Be(0);
        }

        [Fact]
        public void TestSimpleLambda()
        {
            var text = "a => b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.SimpleLambdaExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var lambda = (SimpleLambdaExpressionSyntax)expr;
            lambda.Parameter.Identifier.Should().NotBe(default);
            lambda.Parameter.Identifier.IsMissing.Should().BeFalse();
            lambda.Parameter.Identifier.ToString().Should().Be("a");
            lambda.Body.Should().NotBeNull();
            lambda.Body.ToString().Should().Be("b");
        }

        [Fact]
        public void TestSimpleLambdaWithRefReturn()
        {
            var text = "a => ref b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.SimpleLambdaExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var lambda = (SimpleLambdaExpressionSyntax)expr;
            lambda.Parameter.Identifier.Should().NotBe(default);
            lambda.Parameter.Identifier.IsMissing.Should().BeFalse();
            lambda.Parameter.Identifier.ToString().Should().Be("a");
            lambda.Body.Kind().Should().Be(SyntaxKind.RefExpression);
            lambda.Body.ToString().Should().Be("ref b");
        }

        [Fact]
        public void TestSimpleLambdaWithBlock()
        {
            var text = "a => { }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.SimpleLambdaExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var lambda = (SimpleLambdaExpressionSyntax)expr;
            lambda.Parameter.Identifier.Should().NotBe(default);
            lambda.Parameter.Identifier.IsMissing.Should().BeFalse();
            lambda.Parameter.Identifier.ToString().Should().Be("a");
            lambda.Body.Should().NotBeNull();
            lambda.Body.Kind().Should().Be(SyntaxKind.Block);
            var b = (BlockSyntax)lambda.Body;
            lambda.Body.ToString().Should().Be("{ }");
        }

        [Fact]
        public void TestLambdaWithNoParameters()
        {
            var text = "() => b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ParenthesizedLambdaExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var lambda = (ParenthesizedLambdaExpressionSyntax)expr;
            lambda.ParameterList.OpenParenToken.Should().NotBe(default);
            lambda.ParameterList.CloseParenToken.Should().NotBe(default);
            lambda.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.Parameters.Count.Should().Be(0);
            lambda.Body.Should().NotBeNull();
            lambda.Body.ToString().Should().Be("b");
        }

        [Fact]
        public void TestLambdaWithNoParametersAndRefReturn()
        {
            var text = "() => ref b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ParenthesizedLambdaExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var lambda = (ParenthesizedLambdaExpressionSyntax)expr;
            lambda.ParameterList.OpenParenToken.Should().NotBe(default);
            lambda.ParameterList.CloseParenToken.Should().NotBe(default);
            lambda.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.Parameters.Count.Should().Be(0);
            lambda.Body.Kind().Should().Be(SyntaxKind.RefExpression);
            lambda.Body.ToString().Should().Be("ref b");
        }

        [Fact]
        public void TestLambdaWithNoParametersAndBlock()
        {
            var text = "() => { }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ParenthesizedLambdaExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var lambda = (ParenthesizedLambdaExpressionSyntax)expr;
            lambda.ParameterList.OpenParenToken.Should().NotBe(default);
            lambda.ParameterList.CloseParenToken.Should().NotBe(default);
            lambda.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.Parameters.Count.Should().Be(0);
            lambda.Body.Should().NotBeNull();
            lambda.Body.Kind().Should().Be(SyntaxKind.Block);
            var b = (BlockSyntax)lambda.Body;
            lambda.Body.ToString().Should().Be("{ }");
        }

        [Fact]
        public void TestLambdaWithOneParameter()
        {
            var text = "(a) => b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ParenthesizedLambdaExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var lambda = (ParenthesizedLambdaExpressionSyntax)expr;
            lambda.ParameterList.OpenParenToken.Should().NotBe(default);
            lambda.ParameterList.CloseParenToken.Should().NotBe(default);
            lambda.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.Parameters.Count.Should().Be(1);
            lambda.ParameterList.Parameters[0].Kind().Should().Be(SyntaxKind.Parameter);
            var ps = (ParameterSyntax)lambda.ParameterList.Parameters[0];
            ps.Type.Should().BeNull();
            ps.Identifier.ToString().Should().Be("a");
            lambda.Body.Should().NotBeNull();
            lambda.Body.ToString().Should().Be("b");
        }

        [Fact]
        public void TestLambdaWithTwoParameters()
        {
            var text = "(a, a2) => b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ParenthesizedLambdaExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var lambda = (ParenthesizedLambdaExpressionSyntax)expr;
            lambda.ParameterList.OpenParenToken.Should().NotBe(default);
            lambda.ParameterList.CloseParenToken.Should().NotBe(default);
            lambda.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.Parameters.Count.Should().Be(2);
            lambda.ParameterList.Parameters[0].Kind().Should().Be(SyntaxKind.Parameter);
            var ps = (ParameterSyntax)lambda.ParameterList.Parameters[0];
            ps.Type.Should().BeNull();
            ps.Identifier.ToString().Should().Be("a");
            var ps2 = (ParameterSyntax)lambda.ParameterList.Parameters[1];
            ps2.Type.Should().BeNull();
            ps2.Identifier.ToString().Should().Be("a2");
            lambda.Body.Should().NotBeNull();
            lambda.Body.ToString().Should().Be("b");
        }

        [Fact]
        public void TestLambdaWithOneTypedParameter()
        {
            var text = "(T a) => b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ParenthesizedLambdaExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var lambda = (ParenthesizedLambdaExpressionSyntax)expr;
            lambda.ParameterList.OpenParenToken.Should().NotBe(default);
            lambda.ParameterList.CloseParenToken.Should().NotBe(default);
            lambda.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.Parameters.Count.Should().Be(1);
            lambda.ParameterList.Parameters[0].Kind().Should().Be(SyntaxKind.Parameter);
            var ps = (ParameterSyntax)lambda.ParameterList.Parameters[0];
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("T");
            ps.Identifier.ToString().Should().Be("a");
            lambda.Body.Should().NotBeNull();
            lambda.Body.ToString().Should().Be("b");
        }

        [Fact]
        public void TestLambdaWithOneRefParameter()
        {
            var text = "(ref T a) => b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ParenthesizedLambdaExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var lambda = (ParenthesizedLambdaExpressionSyntax)expr;
            lambda.ParameterList.OpenParenToken.Should().NotBe(default);
            lambda.ParameterList.CloseParenToken.Should().NotBe(default);
            lambda.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            lambda.ParameterList.Parameters.Count.Should().Be(1);
            lambda.ParameterList.Parameters[0].Kind().Should().Be(SyntaxKind.Parameter);
            var ps = (ParameterSyntax)lambda.ParameterList.Parameters[0];
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("T");
            ps.Identifier.ToString().Should().Be("a");
            ps.Modifiers.Count.Should().Be(1);
            ps.Modifiers[0].Kind().Should().Be(SyntaxKind.RefKeyword);
            lambda.Body.Should().NotBeNull();
            lambda.Body.ToString().Should().Be("b");
        }

        [Fact]
        public void TestTupleWithTwoArguments()
        {
            var text = "(a, a2)";
            var expr = this.ParseExpression(text, options: TestOptions.Regular);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.TupleExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var tuple = (TupleExpressionSyntax)expr;
            tuple.OpenParenToken.Should().NotBe(default);
            tuple.CloseParenToken.Should().NotBe(default);
            tuple.OpenParenToken.IsMissing.Should().BeFalse();
            tuple.CloseParenToken.IsMissing.Should().BeFalse();
            tuple.Arguments.Count.Should().Be(2);
            tuple.Arguments[0].Expression.Kind().Should().Be(SyntaxKind.IdentifierName);
            tuple.Arguments[1].NameColon.Should().BeNull();
        }

        [Fact]
        public void TestTupleWithTwoNamedArguments()
        {
            var text = "(arg1: (a, a2), arg2: a2)";
            var expr = this.ParseExpression(text, options: TestOptions.Regular);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.TupleExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
            var tuple = (TupleExpressionSyntax)expr;
            tuple.OpenParenToken.Should().NotBe(default);
            tuple.CloseParenToken.Should().NotBe(default);
            tuple.OpenParenToken.IsMissing.Should().BeFalse();
            tuple.CloseParenToken.IsMissing.Should().BeFalse();
            tuple.Arguments.Count.Should().Be(2);
            tuple.Arguments[0].Expression.Kind().Should().Be(SyntaxKind.TupleExpression);
            tuple.Arguments[0].NameColon.Name.Should().NotBeNull();
            tuple.Arguments[1].NameColon.Name.ToString().Should().Be("arg2");
        }

        [Fact]
        public void TestFromSelect()
        {
            var text = "from a in A select b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(0);

            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.Kind().Should().Be(SyntaxKind.FromKeyword);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.Kind().Should().Be(SyntaxKind.SelectKeyword);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("b");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromWithType()
        {
            var text = "from T a in A select b";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(0);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().NotBeNull();
            fs.Type.ToString().Should().Be("T");
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("b");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromSelectIntoSelect()
        {
            var text = "from a in A select b into c select d";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(0);
            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);

            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("b");

            qs.Body.Continuation.Should().NotBeNull();
            qs.Body.Continuation.Kind().Should().Be(SyntaxKind.QueryContinuation);
            qs.Body.Continuation.IntoKeyword.Should().NotBe(default);
            qs.Body.Continuation.IntoKeyword.Kind().Should().Be(SyntaxKind.IntoKeyword);
            qs.Body.Continuation.IntoKeyword.IsMissing.Should().BeFalse();
            qs.Body.Continuation.Identifier.ToString().Should().Be("c");

            qs.Body.Continuation.Body.Should().NotBeNull();
            qs.Body.Continuation.Body.Clauses.Count.Should().Be(0);
            qs.Body.Continuation.Body.SelectOrGroup.Should().NotBeNull();

            qs.Body.Continuation.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            ss = (SelectClauseSyntax)qs.Body.Continuation.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("d");

            qs.Body.Continuation.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromWhereSelect()
        {
            var text = "from a in A where b select c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(1);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");

            qs.Body.Clauses[0].Kind().Should().Be(SyntaxKind.WhereClause);
            var ws = (WhereClauseSyntax)qs.Body.Clauses[0];
            ws.WhereKeyword.Should().NotBe(default);
            ws.WhereKeyword.Kind().Should().Be(SyntaxKind.WhereKeyword);
            ws.WhereKeyword.IsMissing.Should().BeFalse();
            ws.Condition.Should().NotBeNull();
            ws.Condition.ToString().Should().Be("b");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("c");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromFromSelect()
        {
            var text = "from a in A from b in B select c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(1);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");
            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);

            qs.Body.Clauses[0].Kind().Should().Be(SyntaxKind.FromClause);
            fs = (FromClauseSyntax)qs.Body.Clauses[0];
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("b");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("B");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("c");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromLetSelect()
        {
            var text = "from a in A let b = B select c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(1);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");
            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);

            qs.Body.Clauses[0].Kind().Should().Be(SyntaxKind.LetClause);
            var ls = (LetClauseSyntax)qs.Body.Clauses[0];
            ls.LetKeyword.Should().NotBe(default);
            ls.LetKeyword.Kind().Should().Be(SyntaxKind.LetKeyword);
            ls.LetKeyword.IsMissing.Should().BeFalse();
            ls.Identifier.Should().NotBe(default);
            ls.Identifier.ToString().Should().Be("b");
            ls.EqualsToken.Should().NotBe(default);
            ls.EqualsToken.IsMissing.Should().BeFalse();
            ls.Expression.Should().NotBeNull();
            ls.Expression.ToString().Should().Be("B");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("c");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromOrderBySelect()
        {
            var text = "from a in A orderby b select c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(1);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");
            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);

            qs.Body.Clauses[0].Kind().Should().Be(SyntaxKind.OrderByClause);
            var obs = (OrderByClauseSyntax)qs.Body.Clauses[0];
            obs.OrderByKeyword.Should().NotBe(default);
            obs.OrderByKeyword.Kind().Should().Be(SyntaxKind.OrderByKeyword);
            obs.OrderByKeyword.IsMissing.Should().BeFalse();
            obs.Orderings.Count.Should().Be(1);

            var os = (OrderingSyntax)obs.Orderings[0];
            os.AscendingOrDescendingKeyword.Kind().Should().Be(SyntaxKind.None);
            os.Expression.Should().NotBeNull();
            os.Expression.ToString().Should().Be("b");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("c");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromOrderBy2Select()
        {
            var text = "from a in A orderby b, b2 select c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(1);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");
            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);

            qs.Body.Clauses[0].Kind().Should().Be(SyntaxKind.OrderByClause);
            var obs = (OrderByClauseSyntax)qs.Body.Clauses[0];
            obs.OrderByKeyword.Should().NotBe(default);
            obs.OrderByKeyword.IsMissing.Should().BeFalse();
            obs.Orderings.Count.Should().Be(2);

            var os = (OrderingSyntax)obs.Orderings[0];
            os.AscendingOrDescendingKeyword.Kind().Should().Be(SyntaxKind.None);
            os.Expression.Should().NotBeNull();
            os.Expression.ToString().Should().Be("b");

            os = (OrderingSyntax)obs.Orderings[1];
            os.AscendingOrDescendingKeyword.Kind().Should().Be(SyntaxKind.None);
            os.Expression.Should().NotBeNull();
            os.Expression.ToString().Should().Be("b2");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("c");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromOrderByAscendingSelect()
        {
            var text = "from a in A orderby b ascending select c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(1);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");
            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);

            qs.Body.Clauses[0].Kind().Should().Be(SyntaxKind.OrderByClause);
            var obs = (OrderByClauseSyntax)qs.Body.Clauses[0];
            obs.OrderByKeyword.Should().NotBe(default);
            obs.OrderByKeyword.IsMissing.Should().BeFalse();
            obs.Orderings.Count.Should().Be(1);

            var os = (OrderingSyntax)obs.Orderings[0];
            os.AscendingOrDescendingKeyword.Should().NotBe(default);
            os.AscendingOrDescendingKeyword.Kind().Should().Be(SyntaxKind.AscendingKeyword);
            os.AscendingOrDescendingKeyword.IsMissing.Should().BeFalse();
            os.AscendingOrDescendingKeyword.ContextualKind().Should().Be(SyntaxKind.AscendingKeyword);

            os.Expression.Should().NotBeNull();
            os.Expression.ToString().Should().Be("b");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("c");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromOrderByDescendingSelect()
        {
            var text = "from a in A orderby b descending select c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(1);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");
            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);

            qs.Body.Clauses[0].Kind().Should().Be(SyntaxKind.OrderByClause);
            var obs = (OrderByClauseSyntax)qs.Body.Clauses[0];
            obs.OrderByKeyword.Should().NotBe(default);
            obs.OrderByKeyword.IsMissing.Should().BeFalse();
            obs.Orderings.Count.Should().Be(1);

            var os = (OrderingSyntax)obs.Orderings[0];
            os.AscendingOrDescendingKeyword.Should().NotBe(default);
            os.AscendingOrDescendingKeyword.Kind().Should().Be(SyntaxKind.DescendingKeyword);
            os.AscendingOrDescendingKeyword.IsMissing.Should().BeFalse();
            os.AscendingOrDescendingKeyword.ContextualKind().Should().Be(SyntaxKind.DescendingKeyword);

            os.Expression.Should().NotBeNull();
            os.Expression.ToString().Should().Be("b");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("c");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromGroupBy()
        {
            var text = "from a in A group b by c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(0);

            var fs = qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.GroupClause);
            var gbs = (GroupClauseSyntax)qs.Body.SelectOrGroup;
            gbs.GroupKeyword.Should().NotBe(default);
            gbs.GroupKeyword.Kind().Should().Be(SyntaxKind.GroupKeyword);
            gbs.GroupKeyword.IsMissing.Should().BeFalse();
            gbs.GroupExpression.Should().NotBeNull();
            gbs.GroupExpression.ToString().Should().Be("b");
            gbs.ByKeyword.Should().NotBe(default);
            gbs.ByKeyword.Kind().Should().Be(SyntaxKind.ByKeyword);
            gbs.ByKeyword.IsMissing.Should().BeFalse();
            gbs.ByExpression.Should().NotBeNull();
            gbs.ByExpression.ToString().Should().Be("c");

            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromGroupByIntoSelect()
        {
            var text = "from a in A group b by c into d select e";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(0);

            var fs = qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.GroupClause);
            var gbs = (GroupClauseSyntax)qs.Body.SelectOrGroup;
            gbs.GroupKeyword.Should().NotBe(default);
            gbs.GroupKeyword.IsMissing.Should().BeFalse();
            gbs.GroupExpression.Should().NotBeNull();
            gbs.GroupExpression.ToString().Should().Be("b");
            gbs.ByKeyword.Should().NotBe(default);
            gbs.ByKeyword.IsMissing.Should().BeFalse();
            gbs.ByExpression.Should().NotBeNull();
            gbs.ByExpression.ToString().Should().Be("c");

            qs.Body.Continuation.Should().NotBeNull();
            qs.Body.Continuation.Kind().Should().Be(SyntaxKind.QueryContinuation);
            qs.Body.Continuation.IntoKeyword.Should().NotBe(default);
            qs.Body.Continuation.IntoKeyword.IsMissing.Should().BeFalse();
            qs.Body.Continuation.Identifier.ToString().Should().Be("d");

            qs.Body.Continuation.Should().NotBeNull();
            qs.Body.Continuation.Body.Clauses.Count.Should().Be(0);
            qs.Body.Continuation.Body.SelectOrGroup.Should().NotBeNull();

            qs.Body.Continuation.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.Continuation.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("e");

            qs.Body.Continuation.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromJoinSelect()
        {
            var text = "from a in A join b in B on a equals b select c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(1);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");

            qs.Body.Clauses[0].Kind().Should().Be(SyntaxKind.JoinClause);
            var js = (JoinClauseSyntax)qs.Body.Clauses[0];
            js.JoinKeyword.Should().NotBe(default);
            js.JoinKeyword.Kind().Should().Be(SyntaxKind.JoinKeyword);
            js.JoinKeyword.IsMissing.Should().BeFalse();
            js.Type.Should().BeNull();
            js.Identifier.Should().NotBe(default);
            js.Identifier.ToString().Should().Be("b");
            js.InKeyword.Should().NotBe(default);
            js.InKeyword.IsMissing.Should().BeFalse();
            js.InExpression.Should().NotBeNull();
            js.InExpression.ToString().Should().Be("B");
            js.OnKeyword.Should().NotBe(default);
            js.OnKeyword.Kind().Should().Be(SyntaxKind.OnKeyword);
            js.OnKeyword.IsMissing.Should().BeFalse();
            js.LeftExpression.Should().NotBeNull();
            js.LeftExpression.ToString().Should().Be("a");
            js.EqualsKeyword.Should().NotBe(default);
            js.EqualsKeyword.Kind().Should().Be(SyntaxKind.EqualsKeyword);
            js.EqualsKeyword.IsMissing.Should().BeFalse();
            js.RightExpression.Should().NotBeNull();
            js.RightExpression.ToString().Should().Be("b");
            js.Into.Should().BeNull();

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("c");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromJoinWithTypesSelect()
        {
            var text = "from Ta a in A join Tb b in B on a equals b select c";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(1);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().NotBeNull();
            fs.Type.ToString().Should().Be("Ta");
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");

            qs.Body.Clauses[0].Kind().Should().Be(SyntaxKind.JoinClause);
            var js = (JoinClauseSyntax)qs.Body.Clauses[0];
            js.JoinKeyword.Should().NotBe(default);
            js.JoinKeyword.IsMissing.Should().BeFalse();
            js.Type.Should().NotBeNull();
            js.Type.ToString().Should().Be("Tb");
            js.Identifier.Should().NotBe(default);
            js.Identifier.ToString().Should().Be("b");
            js.InKeyword.Should().NotBe(default);
            js.InKeyword.IsMissing.Should().BeFalse();
            js.InExpression.Should().NotBeNull();
            js.InExpression.ToString().Should().Be("B");
            js.OnKeyword.Should().NotBe(default);
            js.OnKeyword.IsMissing.Should().BeFalse();
            js.LeftExpression.Should().NotBeNull();
            js.LeftExpression.ToString().Should().Be("a");
            js.EqualsKeyword.Should().NotBe(default);
            js.EqualsKeyword.IsMissing.Should().BeFalse();
            js.RightExpression.Should().NotBeNull();
            js.RightExpression.ToString().Should().Be("b");
            js.Into.Should().BeNull();

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("c");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromJoinIntoSelect()
        {
            var text = "from a in A join b in B on a equals b into c select d";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.Clauses.Count.Should().Be(1);

            qs.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            var fs = (FromClauseSyntax)qs.FromClause;
            fs.FromKeyword.Should().NotBe(default);
            fs.FromKeyword.IsMissing.Should().BeFalse();
            fs.Type.Should().BeNull();
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.Expression.ToString().Should().Be("A");

            qs.Body.Clauses[0].Kind().Should().Be(SyntaxKind.JoinClause);
            var js = (JoinClauseSyntax)qs.Body.Clauses[0];
            js.JoinKeyword.Should().NotBe(default);
            js.JoinKeyword.IsMissing.Should().BeFalse();
            js.Type.Should().BeNull();
            js.Identifier.Should().NotBe(default);
            js.Identifier.ToString().Should().Be("b");
            js.InKeyword.Should().NotBe(default);
            js.InKeyword.IsMissing.Should().BeFalse();
            js.InExpression.Should().NotBeNull();
            js.InExpression.ToString().Should().Be("B");
            js.OnKeyword.Should().NotBe(default);
            js.OnKeyword.IsMissing.Should().BeFalse();
            js.LeftExpression.Should().NotBeNull();
            js.LeftExpression.ToString().Should().Be("a");
            js.EqualsKeyword.Should().NotBe(default);
            js.EqualsKeyword.IsMissing.Should().BeFalse();
            js.RightExpression.Should().NotBeNull();
            js.RightExpression.ToString().Should().Be("b");
            js.Into.Should().NotBeNull();
            js.Into.IntoKeyword.Should().NotBe(default);
            js.Into.IntoKeyword.IsMissing.Should().BeFalse();
            js.Into.Identifier.Should().NotBe(default);
            js.Into.Identifier.ToString().Should().Be("c");

            qs.Body.SelectOrGroup.Kind().Should().Be(SyntaxKind.SelectClause);
            var ss = (SelectClauseSyntax)qs.Body.SelectOrGroup;
            ss.SelectKeyword.Should().NotBe(default);
            ss.SelectKeyword.IsMissing.Should().BeFalse();
            ss.Expression.ToString().Should().Be("d");
            qs.Body.Continuation.Should().BeNull();
        }

        [Fact]
        public void TestFromGroupBy1()
        {
            var text = "from it in goo group x by y";
            var expr = SyntaxFactory.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);

            var qs = (QueryExpressionSyntax)expr;
            qs.Body.SelectOrGroup.Should().NotBeNull();
            qs.Body.SelectOrGroup.Should().BeOfType<GroupClauseSyntax>();

            var gs = (GroupClauseSyntax)qs.Body.SelectOrGroup;
            gs.GroupExpression.Should().NotBeNull();
            gs.GroupExpression.ToString().Should().Be("x");
            gs.ByExpression.ToString().Should().Be("y");
        }

        [WorkItem(543075, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/543075")]
        [Fact]
        public void UnterminatedRankSpecifier()
        {
            var text = "new int[";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ArrayCreationExpression);

            var arrayCreation = (ArrayCreationExpressionSyntax)expr;
            arrayCreation.Type.RankSpecifiers.Single().Rank.Should().Be(1);
        }

        [WorkItem(543075, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/543075")]
        [Fact]
        public void UnterminatedTypeArgumentList()
        {
            var text = "new C<";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);

            var objectCreation = (ObjectCreationExpressionSyntax)expr;
            ((NameSyntax)objectCreation.Type).Arity.Should().Be(1);
        }

        [WorkItem(675602, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/675602")]
        [Fact]
        public void QueryKeywordInObjectInitializer()
        {
            //'on' is a keyword here
            var text = "from elem in aRay select new Result { A = on = true }";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.QueryExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().NotBe(0);
        }

        [Fact]
        public void IndexingExpressionInParens()
        {
            var text = "(aRay[i,j])";
            var expr = this.ParseExpression(text);

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.ParenthesizedExpression);

            var parenExp = (ParenthesizedExpressionSyntax)expr;
            parenExp.Expression.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
        }

        [WorkItem(543993, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/543993")]
        [Fact]
        public void ShiftOperator()
        {
            UsingTree(@"
class C
{
    int x = 1 << 2 << 3;
}
");
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken);
                    N(SyntaxKind.OpenBraceToken);

                    N(SyntaxKind.FieldDeclaration);
                    {
                        N(SyntaxKind.VariableDeclaration);
                        {
                            N(SyntaxKind.PredefinedType); N(SyntaxKind.IntKeyword);
                            N(SyntaxKind.VariableDeclarator);
                            {
                                N(SyntaxKind.IdentifierToken);
                                N(SyntaxKind.EqualsValueClause);
                                {
                                    N(SyntaxKind.EqualsToken);
                                    // NB: left associative
                                    N(SyntaxKind.LeftShiftExpression);
                                    {
                                        N(SyntaxKind.LeftShiftExpression);
                                        {
                                            N(SyntaxKind.NumericLiteralExpression); N(SyntaxKind.NumericLiteralToken);
                                            N(SyntaxKind.LessThanLessThanToken);
                                            N(SyntaxKind.NumericLiteralExpression); N(SyntaxKind.NumericLiteralToken);
                                        }
                                        N(SyntaxKind.LessThanLessThanToken);
                                        N(SyntaxKind.NumericLiteralExpression); N(SyntaxKind.NumericLiteralToken);
                                    }
                                }
                            }
                            N(SyntaxKind.SemicolonToken);
                        }
                    }

                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
        }

        [WorkItem(1091974, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1091974")]
        [Fact]
        public void ParseBigExpression()
        {
            var text = @"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AwesomeAssertions;

namespace WB.Core.SharedKernels.DataCollection.Generated
{
   internal partial class QuestionnaireTopLevel
   {   
      private bool IsValid_a()
      {
            return (stackDepth == 100) || ((stackDepth == 200) || ((stackDepth == 300) || ((stackDepth == 400) || ((stackDepth == 501) || ((stackDepth == 502) || ((stackDepth == 600) || ((stackDepth == 701) || ((stackDepth == 702) || ((stackDepth == 801) || ((stackDepth == 802) || ((stackDepth == 901) || ((stackDepth == 902) || ((stackDepth == 903) || ((stackDepth == 1001) || ((stackDepth == 1002) || ((stackDepth == 1101) || ((stackDepth == 1102) || ((stackDepth == 1201) || ((stackDepth == 1202) || ((stackDepth == 1301) || ((stackDepth == 1302) || ((stackDepth == 1401) || ((stackDepth == 1402) || ((stackDepth == 1403) || ((stackDepth == 1404) || ((stackDepth == 1405) || ((stackDepth == 1406) || ((stackDepth == 1407) || ((stackDepth == 1408) || ((stackDepth == 1409) || ((stackDepth == 1410) || ((stackDepth == 1411) || ((stackDepth == 1412) || ((stackDepth == 1413) || ((stackDepth == 1500) || ((stackDepth == 1601) || ((stackDepth == 1602) || ((stackDepth == 1701) || ((stackDepth == 1702) || ((stackDepth == 1703) || ((stackDepth == 1800) || ((stackDepth == 1901) || ((stackDepth == 1902) || ((stackDepth == 1903) || ((stackDepth == 1904) || ((stackDepth == 2000) || ((stackDepth == 2101) || ((stackDepth == 2102) || ((stackDepth == 2103) || ((stackDepth == 2104) || ((stackDepth == 2105) || ((stackDepth == 2106) || ((stackDepth == 2107) || ((stackDepth == 2201) || ((stackDepth == 2202) || ((stackDepth == 2203) || ((stackDepth == 2301) || ((stackDepth == 2302) || ((stackDepth == 2303) || ((stackDepth == 2304) || ((stackDepth == 2305) || ((stackDepth == 2401) || ((stackDepth == 2402) || ((stackDepth == 2403) || ((stackDepth == 2404) || ((stackDepth == 2501) || ((stackDepth == 2502) || ((stackDepth == 2503) || ((stackDepth == 2504) || ((stackDepth == 2505) || ((stackDepth == 2601) || ((stackDepth == 2602) || ((stackDepth == 2603) || ((stackDepth == 2604) || ((stackDepth == 2605) || ((stackDepth == 2606) || ((stackDepth == 2607) || ((stackDepth == 2608) || ((stackDepth == 2701) || ((stackDepth == 2702) || ((stackDepth == 2703) || ((stackDepth == 2704) || ((stackDepth == 2705) || ((stackDepth == 2706) || ((stackDepth == 2801) || ((stackDepth == 2802) || ((stackDepth == 2803) || ((stackDepth == 2804) || ((stackDepth == 2805) || ((stackDepth == 2806) || ((stackDepth == 2807) || ((stackDepth == 2808) || ((stackDepth == 2809) || ((stackDepth == 2810) || ((stackDepth == 2901) || ((stackDepth == 2902) || ((stackDepth == 3001) || ((stackDepth == 3002) || ((stackDepth == 3101) || ((stackDepth == 3102) || ((stackDepth == 3103) || ((stackDepth == 3104) || ((stackDepth == 3105) || ((stackDepth == 3201) || ((stackDepth == 3202) || ((stackDepth == 3203) || ((stackDepth == 3301) || ((stackDepth == 3302) || ((stackDepth == 3401) || ((stackDepth == 3402) || ((stackDepth == 3403) || ((stackDepth == 3404) || ((stackDepth == 3405) || ((stackDepth == 3406) || ((stackDepth == 3407) || ((stackDepth == 3408) || ((stackDepth == 3409) || ((stackDepth == 3410) || ((stackDepth == 3501) || ((stackDepth == 3502) || ((stackDepth == 3503) || ((stackDepth == 3504) || ((stackDepth == 3505) || ((stackDepth == 3506) || ((stackDepth == 3507) || ((stackDepth == 3508) || ((stackDepth == 3509) || ((stackDepth == 3601) || ((stackDepth == 3602) || ((stackDepth == 3701) || ((stackDepth == 3702) || ((stackDepth == 3703) || ((stackDepth == 3704) || ((stackDepth == 3705) || ((stackDepth == 3706) || ((stackDepth == 3801) || ((stackDepth == 3802) || ((stackDepth == 3803) || ((stackDepth == 3804) || ((stackDepth == 3805) || ((stackDepth == 3901) || ((stackDepth == 3902) || ((stackDepth == 3903) || ((stackDepth == 3904) || ((stackDepth == 3905) || ((stackDepth == 4001) || ((stackDepth == 4002) || ((stackDepth == 4003) || ((stackDepth == 4004) || ((stackDepth == 4005) || ((stackDepth == 4006) || ((stackDepth == 4007) || ((stackDepth == 4100) || ((stackDepth == 4201) || ((stackDepth == 4202) || ((stackDepth == 4203) || ((stackDepth == 4204) || ((stackDepth == 4301) || ((stackDepth == 4302) || ((stackDepth == 4304) || ((stackDepth == 4401) || ((stackDepth == 4402) || ((stackDepth == 4403) || ((stackDepth == 4404) || ((stackDepth == 4501) || ((stackDepth == 4502) || ((stackDepth == 4503) || ((stackDepth == 4504) || ((stackDepth == 4600) || ((stackDepth == 4701) || ((stackDepth == 4702) || ((stackDepth == 4801) || ((stackDepth == 4802) || ((stackDepth == 4803) || ((stackDepth == 4804) || ((stackDepth == 4805) || ((stackDepth == 4806) || ((stackDepth == 4807) || ((stackDepth == 4808) || ((stackDepth == 4809) || ((stackDepth == 4811) || ((stackDepth == 4901) || ((stackDepth == 4902) || ((stackDepth == 4903) || ((stackDepth == 4904) || ((stackDepth == 4905) || ((stackDepth == 4906) || ((stackDepth == 4907) || ((stackDepth == 4908) || ((stackDepth == 4909) || ((stackDepth == 4910) || ((stackDepth == 4911) || ((stackDepth == 4912) || ((stackDepth == 4913) || ((stackDepth == 4914) || ((stackDepth == 4915) || ((stackDepth == 4916) || ((stackDepth == 4917) || ((stackDepth == 4918) || ((stackDepth == 4919) || ((stackDepth == 4920) || ((stackDepth == 4921) || ((stackDepth == 4922) || ((stackDepth == 4923) || ((stackDepth == 5001) || ((stackDepth == 5002) || ((stackDepth == 5003) || ((stackDepth == 5004) || ((stackDepth == 5005) || ((stackDepth == 5006) || ((stackDepth == 5100) || ((stackDepth == 5200) || ((stackDepth == 5301) || ((stackDepth == 5302) || ((stackDepth == 5400) || ((stackDepth == 5500) || ((stackDepth == 5600) || ((stackDepth == 5700) || ((stackDepth == 5801) || ((stackDepth == 5802) || ((stackDepth == 5901) || ((stackDepth == 5902) || ((stackDepth == 6001) || ((stackDepth == 6002) || ((stackDepth == 6101) || ((stackDepth == 6102) || ((stackDepth == 6201) || ((stackDepth == 6202) || ((stackDepth == 6203) || ((stackDepth == 6204) || ((stackDepth == 6205) || ((stackDepth == 6301) || ((stackDepth == 6302) || ((stackDepth == 6401) || ((stackDepth == 6402) || ((stackDepth == 6501) || ((stackDepth == 6502) || ((stackDepth == 6503) || ((stackDepth == 6504) || ((stackDepth == 6601) || ((stackDepth == 6602) || ((stackDepth == 6701) || ((stackDepth == 6702) || ((stackDepth == 6703) || ((stackDepth == 6704) || ((stackDepth == 6801) || ((stackDepth == 6802) || ((stackDepth == 6901) || ((stackDepth == 6902) || ((stackDepth == 6903) || ((stackDepth == 6904) || ((stackDepth == 7001) || ((stackDepth == 7002) || ((stackDepth == 7101) || ((stackDepth == 7102) || ((stackDepth == 7103) || ((stackDepth == 7200) || ((stackDepth == 7301) || ((stackDepth == 7302) || ((stackDepth == 7400) || ((stackDepth == 7501) || ((stackDepth == 7502) || ((stackDepth == 7503) || ((stackDepth == 7600) || ((stackDepth == 7700) || ((stackDepth == 7800) || ((stackDepth == 7900) || ((stackDepth == 8001) || ((stackDepth == 8002) || ((stackDepth == 8101) || ((stackDepth == 8102) || ((stackDepth == 8103) || ((stackDepth == 8200) || ((stackDepth == 8300) || ((stackDepth == 8400) || ((stackDepth == 8501) || ((stackDepth == 8502) || ((stackDepth == 8601) || ((stackDepth == 8602) || ((stackDepth == 8700) || ((stackDepth == 8801) || ((stackDepth == 8802) || ((stackDepth == 8901) || ((stackDepth == 8902) || ((stackDepth == 8903) || ((stackDepth == 9001) || ((stackDepth == 9002) || ((stackDepth == 9003) || ((stackDepth == 9004) || ((stackDepth == 9005) || ((stackDepth == 9101) || ((stackDepth == 9102) || ((stackDepth == 9200) || ((stackDepth == 9300) || ((stackDepth == 9401) || ((stackDepth == 9402) || ((stackDepth == 9403) || ((stackDepth == 9500) || ((stackDepth == 9601) || ((stackDepth == 9602) || ((stackDepth == 9701) || ((stackDepth == 9702) || ((stackDepth == 9801) || ((stackDepth == 9802) || ((stackDepth == 9900) || ((stackDepth == 10000) || ((stackDepth == 10100) || ((stackDepth == 10201) || ((stackDepth == 10202) || ((stackDepth == 10301) || ((stackDepth == 10302) || ((stackDepth == 10401) || ((stackDepth == 10402) || ((stackDepth == 10403) || ((stackDepth == 10501) || ((stackDepth == 10502) || ((stackDepth == 10601) || ((stackDepth == 10602) || ((stackDepth == 10701) || ((stackDepth == 10702) || ((stackDepth == 10703) || ((stackDepth == 10704) || ((stackDepth == 10705) || ((stackDepth == 10706) || ((stackDepth == 10801) || ((stackDepth == 10802) || ((stackDepth == 10803) || ((stackDepth == 10804) || ((stackDepth == 10805) || ((stackDepth == 10806) || ((stackDepth == 10807) || ((stackDepth == 10808) || ((stackDepth == 10809) || ((stackDepth == 10900) || ((stackDepth == 11000) || ((stackDepth == 11100) || ((stackDepth == 11201) || ((stackDepth == 11202) || ((stackDepth == 11203) || ((stackDepth == 11204) || ((stackDepth == 11205) || ((stackDepth == 11206) || ((stackDepth == 11207) || ((stackDepth == 11208) || ((stackDepth == 11209) || ((stackDepth == 11210) || ((stackDepth == 11211) || ((stackDepth == 11212) || ((stackDepth == 11213) || ((stackDepth == 11214) || ((stackDepth == 11301) || ((stackDepth == 11302) || ((stackDepth == 11303) || ((stackDepth == 11304) || ((stackDepth == 11305) || ((stackDepth == 11306) || ((stackDepth == 11307) || ((stackDepth == 11308) || ((stackDepth == 11309) || ((stackDepth == 11401) || ((stackDepth == 11402) || ((stackDepth == 11403) || ((stackDepth == 11404) || ((stackDepth == 11501) || ((stackDepth == 11502) || ((stackDepth == 11503) || ((stackDepth == 11504) || ((stackDepth == 11505) || ((stackDepth == 11601) || ((stackDepth == 11602) || ((stackDepth == 11603) || ((stackDepth == 11604) || ((stackDepth == 11605) || ((stackDepth == 11606) || ((stackDepth == 11701) || ((stackDepth == 11702) || ((stackDepth == 11800) || ((stackDepth == 11901) || ((stackDepth == 11902) || ((stackDepth == 11903) || ((stackDepth == 11904) || ((stackDepth == 11905) || ((stackDepth == 12001) || ((stackDepth == 12002) || ((stackDepth == 12003) || ((stackDepth == 12004) || ((stackDepth == 12101) || ((stackDepth == 12102) || ((stackDepth == 12103) || ((stackDepth == 12104) || ((stackDepth == 12105) || ((stackDepth == 12106) || ((stackDepth == 12107) || ((stackDepth == 12108) || ((stackDepth == 12109) || ((stackDepth == 12110) || ((stackDepth == 12111) || ((stackDepth == 12112) || ((stackDepth == 12113) || ((stackDepth == 12114) || ((stackDepth == 12115) || ((stackDepth == 12116) || ((stackDepth == 12201) || ((stackDepth == 12202) || ((stackDepth == 12203) || ((stackDepth == 12204) || ((stackDepth == 12205) || ((stackDepth == 12301) || ((stackDepth == 12302) || ((stackDepth == 12401) || ((stackDepth == 12402) || ((stackDepth == 12403) || ((stackDepth == 12404) || ((stackDepth == 12405) || ((stackDepth == 12406) || ((stackDepth == 12501) || ((stackDepth == 12502) || ((stackDepth == 12601) || ((stackDepth == 12602) || ((stackDepth == 12603) || ((stackDepth == 12700) || ((stackDepth == 12800) || ((stackDepth == 12900) || ((stackDepth == 13001) || ((stackDepth == 13002) || ((stackDepth == 13003) || ((stackDepth == 13004) || ((stackDepth == 13005) || ((stackDepth == 13101) || ((stackDepth == 13102) || ((stackDepth == 13103) || ((stackDepth == 13201) || ((stackDepth == 13202) || ((stackDepth == 13203) || ((stackDepth == 13301) || ((stackDepth == 13302) || ((stackDepth == 13303) || ((stackDepth == 13304) || ((stackDepth == 13401) || ((stackDepth == 13402) || ((stackDepth == 13403) || ((stackDepth == 13404) || ((stackDepth == 13405) || ((stackDepth == 13501) || ((stackDepth == 13502) || ((stackDepth == 13600) || ((stackDepth == 13701) || ((stackDepth == 13702) || ((stackDepth == 13703) || ((stackDepth == 13800) || ((stackDepth == 13901) || ((stackDepth == 13902) || ((stackDepth == 13903) || ((stackDepth == 14001) || ((stackDepth == 14002) || ((stackDepth == 14100) || ((stackDepth == 14200) || ((stackDepth == 14301) || ((stackDepth == 14302) || ((stackDepth == 14400) || ((stackDepth == 14501) || ((stackDepth == 14502) || ((stackDepth == 14601) || ((stackDepth == 14602) || ((stackDepth == 14603) || ((stackDepth == 14604) || ((stackDepth == 14605) || ((stackDepth == 14606) || ((stackDepth == 14607) || ((stackDepth == 14701) || ((stackDepth == 14702) || ((stackDepth == 14703) || ((stackDepth == 14704) || ((stackDepth == 14705) || ((stackDepth == 14706) || ((stackDepth == 14707) || ((stackDepth == 14708) || ((stackDepth == 14709) || ((stackDepth == 14710) || ((stackDepth == 14711) || ((stackDepth == 14712) || ((stackDepth == 14713) || ((stackDepth == 14714) || ((stackDepth == 14715) || ((stackDepth == 14716) || ((stackDepth == 14717) || ((stackDepth == 14718) || ((stackDepth == 14719) || ((stackDepth == 14720 || ((stackDepth == 14717 || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717 || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717 || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717) || ((stackDepth == 14717))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
      }      
   }
}
";
            var root = SyntaxFactory.ParseSyntaxTree(text).GetRoot();

            root.Should().NotBeNull();
            root.Kind().Should().Be(SyntaxKind.CompilationUnit);
        }

        [Fact, WorkItem(15885, "https://github.com/dotnet/roslyn/pull/15885")]
        public void InProgressLocalDeclaration1()
        {
            const string text = @"
class C
{
    async void M()
    {
        Task.
        await Task.Delay();
    }
}
";
            UsingTree(text,
                // (6,14): error CS1001: Identifier expected
                //         Task.
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "").WithLocation(6, 14),
                // (6,14): error CS1002: ; expected
                //         Task.
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "").WithLocation(6, 14));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.AsyncKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.ExpressionStatement);
                            {
                                N(SyntaxKind.SimpleMemberAccessExpression);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "Task");
                                    }
                                    N(SyntaxKind.DotToken);
                                    M(SyntaxKind.IdentifierName);
                                    {
                                        M(SyntaxKind.IdentifierToken);
                                    }
                                }
                                M(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.ExpressionStatement);
                            {
                                N(SyntaxKind.AwaitExpression);
                                {
                                    N(SyntaxKind.AwaitKeyword);
                                    N(SyntaxKind.InvocationExpression);
                                    {
                                        N(SyntaxKind.SimpleMemberAccessExpression);
                                        {
                                            N(SyntaxKind.IdentifierName);
                                            {
                                                N(SyntaxKind.IdentifierToken, "Task");
                                            }
                                            N(SyntaxKind.DotToken);
                                            N(SyntaxKind.IdentifierName);
                                            {
                                                N(SyntaxKind.IdentifierToken, "Delay");
                                            }
                                        }
                                        N(SyntaxKind.ArgumentList);
                                        {
                                            N(SyntaxKind.OpenParenToken);
                                            N(SyntaxKind.CloseParenToken);
                                        }
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(15885, "https://github.com/dotnet/roslyn/pull/15885")]
        public void InProgressLocalDeclaration2()
        {
            const string text = @"
class C
{
    async void M()
    {
        Task.await Task.Delay();
    }
}
";
            UsingTree(text,
                // (6,14): error CS4003: 'await' cannot be used as an identifier within an async method or lambda expression
                //         Task.await Task.Delay();
                Diagnostic(ErrorCode.ERR_BadAwaitAsIdentifier, "await").WithLocation(6, 14),
                // (6,24): error CS1003: Syntax error, ',' expected
                //         Task.await Task.Delay();
                Diagnostic(ErrorCode.ERR_SyntaxError, ".").WithArguments(",").WithLocation(6, 24),
                // (6,25): error CS1002: ; expected
                //         Task.await Task.Delay();
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "Delay").WithLocation(6, 25));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.AsyncKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.QualifiedName);
                                    {
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken, "Task");
                                        }
                                        N(SyntaxKind.DotToken);
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken, "await");
                                        }
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "Task");
                                    }
                                }
                                M(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.ExpressionStatement);
                            {
                                N(SyntaxKind.InvocationExpression);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "Delay");
                                    }
                                    N(SyntaxKind.ArgumentList);
                                    {
                                        N(SyntaxKind.OpenParenToken);
                                        N(SyntaxKind.CloseParenToken);
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(15885, "https://github.com/dotnet/roslyn/pull/15885")]
        public void InProgressLocalDeclaration3()
        {
            const string text = @"
class C
{
    async void M()
    {
        Task.
        await Task;
    }
}
";
            UsingTree(text,
                // (7,9): error CS4003: 'await' cannot be used as an identifier within an async method or lambda expression
                //         await Task;
                Diagnostic(ErrorCode.ERR_BadAwaitAsIdentifier, "await").WithLocation(7, 9));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.AsyncKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.QualifiedName);
                                    {
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken, "Task");
                                        }
                                        N(SyntaxKind.DotToken);
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken, "await");
                                        }
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "Task");
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(15885, "https://github.com/dotnet/roslyn/pull/15885")]
        public void InProgressLocalDeclaration4()
        {
            const string text = @"
class C
{
    async void M()
    {
        Task.
        await Task = 1;
    }
}
";
            UsingTree(text,
                // (7,9): error CS4003: 'await' cannot be used as an identifier within an async method or lambda expression
                //         await Task = 1;
                Diagnostic(ErrorCode.ERR_BadAwaitAsIdentifier, "await").WithLocation(7, 9));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.AsyncKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.QualifiedName);
                                    {
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken, "Task");
                                        }
                                        N(SyntaxKind.DotToken);
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken, "await");
                                        }
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "Task");
                                        N(SyntaxKind.EqualsValueClause);
                                        {
                                            N(SyntaxKind.EqualsToken);
                                            N(SyntaxKind.NumericLiteralExpression);
                                            {
                                                N(SyntaxKind.NumericLiteralToken, "1");
                                            }
                                        }
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(15885, "https://github.com/dotnet/roslyn/pull/15885")]
        public void InProgressLocalDeclaration5()
        {
            const string text = @"
class C
{
    async void M()
    {
        Task.
        await Task, Task2;
    }
}
";
            UsingTree(text,
                // (7,9): error CS4003: 'await' cannot be used as an identifier within an async method or lambda expression
                //         await Task, Task2;
                Diagnostic(ErrorCode.ERR_BadAwaitAsIdentifier, "await").WithLocation(7, 9));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.AsyncKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.QualifiedName);
                                    {
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken, "Task");
                                        }
                                        N(SyntaxKind.DotToken);
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken, "await");
                                        }
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "Task");
                                    }
                                    N(SyntaxKind.CommaToken);
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "Task2");
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(15885, "https://github.com/dotnet/roslyn/pull/15885")]
        public void InProgressLocalDeclaration6()
        {
            const string text = @"
class C
{
    async void M()
    {
        Task.
        await Task();
    }
}
";
            UsingTree(text,
                // (7,9): error CS4003: 'await' cannot be used as an identifier within an async method or lambda expression
                //         await Task();
                Diagnostic(ErrorCode.ERR_BadAwaitAsIdentifier, "await").WithLocation(7, 9));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.AsyncKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalFunctionStatement);
                            {
                                N(SyntaxKind.QualifiedName);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "Task");
                                    }
                                    N(SyntaxKind.DotToken);
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "await");
                                    }
                                }
                                N(SyntaxKind.IdentifierToken, "Task");
                                N(SyntaxKind.ParameterList);
                                {
                                    N(SyntaxKind.OpenParenToken);
                                    N(SyntaxKind.CloseParenToken);
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(15885, "https://github.com/dotnet/roslyn/pull/15885")]
        public void InProgressLocalDeclaration7()
        {
            const string text = @"
class C
{
    async void M()
    {
        Task.
        await Task<T>();
    }
}
";
            UsingTree(text,
                // (7,9): error CS4003: 'await' cannot be used as an identifier within an async method or lambda expression
                //         await Task<T>();
                Diagnostic(ErrorCode.ERR_BadAwaitAsIdentifier, "await").WithLocation(7, 9));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.AsyncKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalFunctionStatement);
                            {
                                N(SyntaxKind.QualifiedName);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "Task");
                                    }
                                    N(SyntaxKind.DotToken);
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "await");
                                    }
                                }
                                N(SyntaxKind.IdentifierToken, "Task");
                                N(SyntaxKind.TypeParameterList);
                                {
                                    N(SyntaxKind.LessThanToken);
                                    N(SyntaxKind.TypeParameter);
                                    {
                                        N(SyntaxKind.IdentifierToken, "T");
                                    }
                                    N(SyntaxKind.GreaterThanToken);
                                }
                                N(SyntaxKind.ParameterList);
                                {
                                    N(SyntaxKind.OpenParenToken);
                                    N(SyntaxKind.CloseParenToken);
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(15885, "https://github.com/dotnet/roslyn/pull/15885")]
        public void InProgressLocalDeclaration8()
        {
            const string text = @"
class C
{
    async void M()
    {
        Task.
        await Task[1];
    }
}
";
            UsingTree(text,
                // (6,14): error CS1001: Identifier expected
                //         Task.
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "").WithLocation(6, 14),
                // (6,14): error CS1002: ; expected
                //         Task.
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "").WithLocation(6, 14));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.AsyncKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.ExpressionStatement);
                            {
                                N(SyntaxKind.SimpleMemberAccessExpression);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "Task");
                                    }
                                    N(SyntaxKind.DotToken);
                                    M(SyntaxKind.IdentifierName);
                                    {
                                        M(SyntaxKind.IdentifierToken);
                                    }
                                }
                                M(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.ExpressionStatement);
                            {
                                N(SyntaxKind.AwaitExpression);
                                {
                                    N(SyntaxKind.AwaitKeyword);
                                    N(SyntaxKind.ElementAccessExpression);
                                    {
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken, "Task");
                                        }
                                        N(SyntaxKind.BracketedArgumentList);
                                        {
                                            N(SyntaxKind.OpenBracketToken);
                                            N(SyntaxKind.Argument);
                                            {
                                                N(SyntaxKind.NumericLiteralExpression);
                                                {
                                                    N(SyntaxKind.NumericLiteralToken, "1");
                                                }
                                            }
                                            N(SyntaxKind.CloseBracketToken);
                                        }
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(377556, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?id=377556")]
        public void TypeArgumentShiftAmbiguity_01()
        {
            const string text = @"
class C
{
    void M()
    {
        //int a = 1;
        //int i = 1;
        var j = a < i >> 2;
    }
}
";
            var tree = UsingTree(text);
            tree.GetDiagnostics().Verify();
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "var");
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "j");
                                        N(SyntaxKind.EqualsValueClause);
                                        {
                                            N(SyntaxKind.EqualsToken);
                                            N(SyntaxKind.LessThanExpression);
                                            {
                                                N(SyntaxKind.IdentifierName);
                                                {
                                                    N(SyntaxKind.IdentifierToken, "a");
                                                }
                                                N(SyntaxKind.LessThanToken);
                                                N(SyntaxKind.RightShiftExpression);
                                                {
                                                    N(SyntaxKind.IdentifierName);
                                                    {
                                                        N(SyntaxKind.IdentifierToken, "i");
                                                    }
                                                    N(SyntaxKind.GreaterThanGreaterThanToken);
                                                    N(SyntaxKind.NumericLiteralExpression);
                                                    {
                                                        N(SyntaxKind.NumericLiteralToken, "2");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(377556, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?id=377556")]
        public void TypeArgumentShiftAmbiguity_02()
        {
            const string text = @"
class C
{
    void M()
    {
        //const int a = 1;
        //const int i = 2;
        switch (false)
        {
            case a < i >> 2: break;
        }
    }
}
";
            var tree = UsingTree(text);
            tree.GetDiagnostics().Verify();
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.SwitchStatement);
                            {
                                N(SyntaxKind.SwitchKeyword);
                                N(SyntaxKind.OpenParenToken);
                                N(SyntaxKind.FalseLiteralExpression);
                                {
                                    N(SyntaxKind.FalseKeyword);
                                }
                                N(SyntaxKind.CloseParenToken);
                                N(SyntaxKind.OpenBraceToken);
                                N(SyntaxKind.SwitchSection);
                                {
                                    N(SyntaxKind.CaseSwitchLabel);
                                    {
                                        N(SyntaxKind.CaseKeyword);
                                        N(SyntaxKind.LessThanExpression);
                                        {
                                            N(SyntaxKind.IdentifierName);
                                            {
                                                N(SyntaxKind.IdentifierToken, "a");
                                            }
                                            N(SyntaxKind.LessThanToken);
                                            N(SyntaxKind.RightShiftExpression);
                                            {
                                                N(SyntaxKind.IdentifierName);
                                                {
                                                    N(SyntaxKind.IdentifierToken, "i");
                                                }
                                                N(SyntaxKind.GreaterThanGreaterThanToken);
                                                N(SyntaxKind.NumericLiteralExpression);
                                                {
                                                    N(SyntaxKind.NumericLiteralToken, "2");
                                                }
                                            }
                                        }
                                        N(SyntaxKind.ColonToken);
                                    }
                                    N(SyntaxKind.BreakStatement);
                                    {
                                        N(SyntaxKind.BreakKeyword);
                                        N(SyntaxKind.SemicolonToken);
                                    }
                                }
                                N(SyntaxKind.CloseBraceToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(377556, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?id=377556")]
        public void TypeArgumentShiftAmbiguity_03()
        {
            const string text = @"
class C
{
    void M()
    {
        M(out a < i >> 2);
    }
}
";
            var tree = UsingTree(text);
            tree.GetDiagnostics().Verify();
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.ExpressionStatement);
                            {
                                N(SyntaxKind.InvocationExpression);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "M");
                                    }
                                    N(SyntaxKind.ArgumentList);
                                    {
                                        N(SyntaxKind.OpenParenToken);
                                        N(SyntaxKind.Argument);
                                        {
                                            N(SyntaxKind.OutKeyword);
                                            N(SyntaxKind.LessThanExpression);
                                            {
                                                N(SyntaxKind.IdentifierName);
                                                {
                                                    N(SyntaxKind.IdentifierToken, "a");
                                                }
                                                N(SyntaxKind.LessThanToken);
                                                N(SyntaxKind.RightShiftExpression);
                                                {
                                                    N(SyntaxKind.IdentifierName);
                                                    {
                                                        N(SyntaxKind.IdentifierToken, "i");
                                                    }
                                                    N(SyntaxKind.GreaterThanGreaterThanToken);
                                                    N(SyntaxKind.NumericLiteralExpression);
                                                    {
                                                        N(SyntaxKind.NumericLiteralToken, "2");
                                                    }
                                                }
                                            }
                                        }
                                        N(SyntaxKind.CloseParenToken);
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(377556, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?id=377556")]
        public void TypeArgumentShiftAmbiguity_04()
        {
            const string text = @"
class C
{
    void M()
    {
        // (e is a<i>) > 2
        var j = e is a < i >> 2;
    }
}
";
            var tree = UsingTree(text);
            tree.GetDiagnostics().Verify();
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "var");
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "j");
                                        N(SyntaxKind.EqualsValueClause);
                                        {
                                            N(SyntaxKind.EqualsToken);
                                            N(SyntaxKind.GreaterThanExpression);
                                            {
                                                N(SyntaxKind.IsExpression);
                                                {
                                                    N(SyntaxKind.IdentifierName);
                                                    {
                                                        N(SyntaxKind.IdentifierToken, "e");
                                                    }
                                                    N(SyntaxKind.IsKeyword);
                                                    N(SyntaxKind.GenericName);
                                                    {
                                                        N(SyntaxKind.IdentifierToken, "a");
                                                        N(SyntaxKind.TypeArgumentList);
                                                        {
                                                            N(SyntaxKind.LessThanToken);
                                                            N(SyntaxKind.IdentifierName);
                                                            {
                                                                N(SyntaxKind.IdentifierToken, "i");
                                                            }
                                                            N(SyntaxKind.GreaterThanToken);
                                                        }
                                                    }
                                                }
                                                N(SyntaxKind.GreaterThanToken);
                                                N(SyntaxKind.NumericLiteralExpression);
                                                {
                                                    N(SyntaxKind.NumericLiteralToken, "2");
                                                }
                                            }
                                        }
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(377556, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?id=377556")]
        public void TypeArgumentShiftAmbiguity_05()
        {
            const string text = @"
class C
{
    void M()
    {
        var j = e is a < i >>> 2;
    }
}
";
            var tree = UsingTree(text);
            tree.GetDiagnostics().Verify();
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "var");
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "j");
                                        N(SyntaxKind.EqualsValueClause);
                                        {
                                            N(SyntaxKind.EqualsToken);
                                            N(SyntaxKind.LessThanExpression);
                                            {
                                                N(SyntaxKind.IsExpression);
                                                {
                                                    N(SyntaxKind.IdentifierName);
                                                    {
                                                        N(SyntaxKind.IdentifierToken, "e");
                                                    }
                                                    N(SyntaxKind.IsKeyword);
                                                    N(SyntaxKind.IdentifierName);
                                                    {
                                                        N(SyntaxKind.IdentifierToken, "a");
                                                    }
                                                }
                                                N(SyntaxKind.LessThanToken);
                                                N(SyntaxKind.UnsignedRightShiftExpression);
                                                {
                                                    N(SyntaxKind.IdentifierName);
                                                    {
                                                        N(SyntaxKind.IdentifierToken, "i");
                                                    }
                                                    N(SyntaxKind.GreaterThanGreaterThanGreaterThanToken);
                                                    N(SyntaxKind.NumericLiteralExpression);
                                                    {
                                                        N(SyntaxKind.NumericLiteralToken, "2");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(377556, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?id=377556")]
        public void TypeArgumentShiftAmbiguity_06()
        {
            const string text = @"
class C
{
    void M()
    {
        // syntax error
        var j = e is a < i > << 2;
    }
}
";
            var tree = UsingTree(text,
                // (7,30): error CS1525: Invalid expression term '<<'
                //         var j = e is a < i > << 2;
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "<<").WithArguments("<<").WithLocation(7, 30)
                );
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "var");
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "j");
                                        N(SyntaxKind.EqualsValueClause);
                                        {
                                            N(SyntaxKind.EqualsToken);
                                            N(SyntaxKind.GreaterThanExpression);
                                            {
                                                N(SyntaxKind.LessThanExpression);
                                                {
                                                    N(SyntaxKind.IsExpression);
                                                    {
                                                        N(SyntaxKind.IdentifierName);
                                                        {
                                                            N(SyntaxKind.IdentifierToken, "e");
                                                        }
                                                        N(SyntaxKind.IsKeyword);
                                                        N(SyntaxKind.IdentifierName);
                                                        {
                                                            N(SyntaxKind.IdentifierToken, "a");
                                                        }
                                                    }
                                                    N(SyntaxKind.LessThanToken);
                                                    N(SyntaxKind.IdentifierName);
                                                    {
                                                        N(SyntaxKind.IdentifierToken, "i");
                                                    }
                                                }
                                                N(SyntaxKind.GreaterThanToken);
                                                N(SyntaxKind.LeftShiftExpression);
                                                {
                                                    M(SyntaxKind.IdentifierName);
                                                    {
                                                        M(SyntaxKind.IdentifierToken);
                                                    }
                                                    N(SyntaxKind.LessThanLessThanToken);
                                                    N(SyntaxKind.NumericLiteralExpression);
                                                    {
                                                        N(SyntaxKind.NumericLiteralToken, "2");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(377556, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?id=377556")]
        public void TypeArgumentShiftAmbiguity_07()
        {
            const string text = @"
class C
{
    void M()
    {
        // syntax error
        var j = e is a < i >>>> 2;
    }
}
";
            var tree = UsingTree(text,
                // (7,31): error CS1525: Invalid expression term '>'
                //         var j = e is a < i >>>> 2;
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, ">").WithArguments(">").WithLocation(7, 31)
                );
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "var");
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "j");
                                        N(SyntaxKind.EqualsValueClause);
                                        {
                                            N(SyntaxKind.EqualsToken);
                                            N(SyntaxKind.GreaterThanExpression);
                                            {
                                                N(SyntaxKind.LessThanExpression);
                                                {
                                                    N(SyntaxKind.IsExpression);
                                                    {
                                                        N(SyntaxKind.IdentifierName);
                                                        {
                                                            N(SyntaxKind.IdentifierToken, "e");
                                                        }
                                                        N(SyntaxKind.IsKeyword);
                                                        N(SyntaxKind.IdentifierName);
                                                        {
                                                            N(SyntaxKind.IdentifierToken, "a");
                                                        }
                                                    }
                                                    N(SyntaxKind.LessThanToken);
                                                    N(SyntaxKind.UnsignedRightShiftExpression);
                                                    {
                                                        N(SyntaxKind.IdentifierName);
                                                        {
                                                            N(SyntaxKind.IdentifierToken, "i");
                                                        }
                                                        N(SyntaxKind.GreaterThanGreaterThanGreaterThanToken);
                                                        M(SyntaxKind.IdentifierName);
                                                        {
                                                            M(SyntaxKind.IdentifierToken);
                                                        }
                                                    }
                                                }
                                                N(SyntaxKind.GreaterThanToken);
                                                N(SyntaxKind.NumericLiteralExpression);
                                                {
                                                    N(SyntaxKind.NumericLiteralToken, "2");
                                                }
                                            }
                                        }
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(377556, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?id=377556")]
        public void TypeArgumentShiftAmbiguity_08()
        {
            const string text = @"
class C
{
    void M()
    {
        M(out a < i >>> 2);
    }
}
";
            var tree = UsingTree(text);
            tree.GetDiagnostics().Verify();
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.ExpressionStatement);
                            {
                                N(SyntaxKind.InvocationExpression);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "M");
                                    }
                                    N(SyntaxKind.ArgumentList);
                                    {
                                        N(SyntaxKind.OpenParenToken);
                                        N(SyntaxKind.Argument);
                                        {
                                            N(SyntaxKind.OutKeyword);
                                            N(SyntaxKind.LessThanExpression);
                                            {
                                                N(SyntaxKind.IdentifierName);
                                                {
                                                    N(SyntaxKind.IdentifierToken, "a");
                                                }
                                                N(SyntaxKind.LessThanToken);
                                                N(SyntaxKind.UnsignedRightShiftExpression);
                                                {
                                                    N(SyntaxKind.IdentifierName);
                                                    {
                                                        N(SyntaxKind.IdentifierToken, "i");
                                                    }
                                                    N(SyntaxKind.GreaterThanGreaterThanGreaterThanToken);
                                                    N(SyntaxKind.NumericLiteralExpression);
                                                    {
                                                        N(SyntaxKind.NumericLiteralToken, "2");
                                                    }
                                                }
                                            }
                                        }
                                        N(SyntaxKind.CloseParenToken);
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(377556, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?id=377556")]
        public void TypeArgumentShiftAmbiguity_09()
        {
            const string text = @"
class C
{
    void M()
    {
        //const int a = 1;
        //const int i = 2;
        switch (false)
        {
            case a < i >>> 2: break;
        }
    }
}
";
            var tree = UsingTree(text);
            tree.GetDiagnostics().Verify();
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.SwitchStatement);
                            {
                                N(SyntaxKind.SwitchKeyword);
                                N(SyntaxKind.OpenParenToken);
                                N(SyntaxKind.FalseLiteralExpression);
                                {
                                    N(SyntaxKind.FalseKeyword);
                                }
                                N(SyntaxKind.CloseParenToken);
                                N(SyntaxKind.OpenBraceToken);
                                N(SyntaxKind.SwitchSection);
                                {
                                    N(SyntaxKind.CaseSwitchLabel);
                                    {
                                        N(SyntaxKind.CaseKeyword);
                                        N(SyntaxKind.LessThanExpression);
                                        {
                                            N(SyntaxKind.IdentifierName);
                                            {
                                                N(SyntaxKind.IdentifierToken, "a");
                                            }
                                            N(SyntaxKind.LessThanToken);
                                            N(SyntaxKind.UnsignedRightShiftExpression);
                                            {
                                                N(SyntaxKind.IdentifierName);
                                                {
                                                    N(SyntaxKind.IdentifierToken, "i");
                                                }
                                                N(SyntaxKind.GreaterThanGreaterThanGreaterThanToken);
                                                N(SyntaxKind.NumericLiteralExpression);
                                                {
                                                    N(SyntaxKind.NumericLiteralToken, "2");
                                                }
                                            }
                                        }
                                        N(SyntaxKind.ColonToken);
                                    }
                                    N(SyntaxKind.BreakStatement);
                                    {
                                        N(SyntaxKind.BreakKeyword);
                                        N(SyntaxKind.SemicolonToken);
                                    }
                                }
                                N(SyntaxKind.CloseBraceToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(377556, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?id=377556")]
        public void TypeArgumentShiftAmbiguity_10()
        {
            const string text = @"
class C
{
    void M()
    {
        //int a = 1;
        //int i = 1;
        var j = a < i >>> 2;
    }
}
";
            var tree = UsingTree(text);
            tree.GetDiagnostics().Verify();
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "var");
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "j");
                                        N(SyntaxKind.EqualsValueClause);
                                        {
                                            N(SyntaxKind.EqualsToken);
                                            N(SyntaxKind.LessThanExpression);
                                            {
                                                N(SyntaxKind.IdentifierName);
                                                {
                                                    N(SyntaxKind.IdentifierToken, "a");
                                                }
                                                N(SyntaxKind.LessThanToken);
                                                N(SyntaxKind.UnsignedRightShiftExpression);
                                                {
                                                    N(SyntaxKind.IdentifierName);
                                                    {
                                                        N(SyntaxKind.IdentifierToken, "i");
                                                    }
                                                    N(SyntaxKind.GreaterThanGreaterThanGreaterThanToken);
                                                    N(SyntaxKind.NumericLiteralExpression);
                                                    {
                                                        N(SyntaxKind.NumericLiteralToken, "2");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void TestTargetTypedDefaultWithCSharp7_1()
        {
            var text = "default";
            var expr = this.ParseExpression(text, TestOptions.Regular.WithLanguageVersion(LanguageVersion.CSharp7_1));

            expr.Should().NotBeNull();
            expr.Kind().Should().Be(SyntaxKind.DefaultLiteralExpression);
            expr.ToString().Should().Be(text);
            expr.Errors().Length.Should().Be(0);
        }

        [Fact, WorkItem(17683, "https://github.com/dotnet/roslyn/issues/17683")]
        public void Bug17683a()
        {
            var source =
@"from t in e
where
t == Int32.
MinValue
select t";
            UsingExpression(source);
            N(SyntaxKind.QueryExpression);
            {
                N(SyntaxKind.FromClause);
                {
                    N(SyntaxKind.FromKeyword);
                    N(SyntaxKind.IdentifierToken, "t");
                    N(SyntaxKind.InKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "e");
                    }
                }
                N(SyntaxKind.QueryBody);
                {
                    N(SyntaxKind.WhereClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.EqualsExpression);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "t");
                            }
                            N(SyntaxKind.EqualsEqualsToken);
                            N(SyntaxKind.SimpleMemberAccessExpression);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "Int32");
                                }
                                N(SyntaxKind.DotToken);
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "MinValue");
                                }
                            }
                        }
                    }
                    N(SyntaxKind.SelectClause);
                    {
                        N(SyntaxKind.SelectKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "t");
                        }
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Bug17683b()
        {
            var source =
@"switch (e)
{
    case Int32.
               MaxValue when true:
            break;
}";
            UsingStatement(source);
            N(SyntaxKind.SwitchStatement);
            {
                N(SyntaxKind.SwitchKeyword);
                N(SyntaxKind.OpenParenToken);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "e");
                }
                N(SyntaxKind.CloseParenToken);
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.SwitchSection);
                {
                    N(SyntaxKind.CasePatternSwitchLabel);
                    {
                        N(SyntaxKind.CaseKeyword);
                        N(SyntaxKind.ConstantPattern);
                        {
                            N(SyntaxKind.SimpleMemberAccessExpression);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "Int32");
                                }
                                N(SyntaxKind.DotToken);
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "MaxValue");
                                }
                            }
                        }
                        N(SyntaxKind.WhenClause);
                        {
                            N(SyntaxKind.WhenKeyword);
                            N(SyntaxKind.TrueLiteralExpression);
                            {
                                N(SyntaxKind.TrueKeyword);
                            }
                        }
                        N(SyntaxKind.ColonToken);
                    }
                    N(SyntaxKind.BreakStatement);
                    {
                        N(SyntaxKind.BreakKeyword);
                        N(SyntaxKind.SemicolonToken);
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact, WorkItem(22830, "https://github.com/dotnet/roslyn/issues/22830")]
        public void TypeArgumentIndexerInitializer()
        {
            UsingExpression("new C { [0] = op1 < op2, [1] = true }");
            N(SyntaxKind.ObjectCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "C");
                }
                N(SyntaxKind.ObjectInitializerExpression);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.SimpleAssignmentExpression);
                    {
                        N(SyntaxKind.ImplicitElementAccess);
                        {
                            N(SyntaxKind.BracketedArgumentList);
                            {
                                N(SyntaxKind.OpenBracketToken);
                                N(SyntaxKind.Argument);
                                {
                                    N(SyntaxKind.NumericLiteralExpression);
                                    {
                                        N(SyntaxKind.NumericLiteralToken, "0");
                                    }
                                }
                                N(SyntaxKind.CloseBracketToken);
                            }
                        }
                        N(SyntaxKind.EqualsToken);
                        N(SyntaxKind.LessThanExpression);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "op1");
                            }
                            N(SyntaxKind.LessThanToken);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "op2");
                            }
                        }
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.SimpleAssignmentExpression);
                    {
                        N(SyntaxKind.ImplicitElementAccess);
                        {
                            N(SyntaxKind.BracketedArgumentList);
                            {
                                N(SyntaxKind.OpenBracketToken);
                                N(SyntaxKind.Argument);
                                {
                                    N(SyntaxKind.NumericLiteralExpression);
                                    {
                                        N(SyntaxKind.NumericLiteralToken, "1");
                                    }
                                }
                                N(SyntaxKind.CloseBracketToken);
                            }
                        }
                        N(SyntaxKind.EqualsToken);
                        N(SyntaxKind.TrueLiteralExpression);
                        {
                            N(SyntaxKind.TrueKeyword);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void InterpolatedStringExpressionSurroundedByCurlyBraces()
        {
            UsingExpression("$\"{{{12}}}\"");
            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "12");
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.InterpolatedStringEndToken);
            }
        }

        [Fact]
        public void InterpolatedStringExpressionWithFormatClauseSurroundedByCurlyBraces()
        {
            UsingExpression("$\"{{{12:X}}}\"");
            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedStringStartToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "12");
                    }
                    N(SyntaxKind.InterpolationFormatClause);
                    {
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.InterpolatedStringTextToken, "X");
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken);
                }
                N(SyntaxKind.InterpolatedStringEndToken);
            }
        }

        [Fact, WorkItem(12214, "https://github.com/dotnet/roslyn/issues/12214")]
        public void ConditionalExpressionInInterpolation()
        {
            UsingExpression("$\"{a ? b : d}\"",
                // (1,4): error CS8361: A conditional expression cannot be used directly in a string interpolation because the ':' ends the interpolation. Parenthesize the conditional expression.
                // $"{a ? b : d}"
                Diagnostic(ErrorCode.ERR_ConditionalInInterpolation, "a ? b ").WithLocation(1, 4)
                );
            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.InterpolatedStringStartToken);
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.ConditionalExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "a");
                        }
                        N(SyntaxKind.QuestionToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                        M(SyntaxKind.ColonToken);
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                    }
                    N(SyntaxKind.InterpolationFormatClause);
                    {
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.InterpolatedStringTextToken);
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedStringEndToken);
            }
            EOF();
        }

        [Fact]
        public void NullCoalescingAssignmentExpression()
        {
            UsingExpression("a ??= b");
            N(SyntaxKind.CoalesceAssignmentExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.QuestionQuestionEqualsToken);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "b");
                }
            }
            EOF();
        }

        [Fact]
        public void NullCoalescingAssignmentExpressionParenthesized()
        {
            UsingExpression("(a) ??= b");
            N(SyntaxKind.CoalesceAssignmentExpression);
            {
                N(SyntaxKind.ParenthesizedExpression);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.QuestionQuestionEqualsToken);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "b");
                }
            }
            EOF();
        }

        [Fact]
        public void NullCoalescingAssignmentExpressionInvocation()
        {
            UsingExpression("M(a) ??= b");
            N(SyntaxKind.CoalesceAssignmentExpression);
            {
                N(SyntaxKind.InvocationExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "M");
                    }
                    N(SyntaxKind.ArgumentList);
                    {
                        N(SyntaxKind.OpenParenToken);
                        N(SyntaxKind.Argument);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "a");
                            }
                        }
                        N(SyntaxKind.CloseParenToken);
                    }
                }
                N(SyntaxKind.QuestionQuestionEqualsToken);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "b");
                }
            }
            EOF();
        }

        [Fact]
        public void NullCoalescingAssignmentExpressionAndCoalescingOperator()
        {
            UsingExpression("a ?? b ??= c");
            N(SyntaxKind.CoalesceAssignmentExpression);
            {
                N(SyntaxKind.CoalesceExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                    N(SyntaxKind.QuestionQuestionToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                }
                N(SyntaxKind.QuestionQuestionEqualsToken);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "c");
                }
            }
            EOF();
        }

        [Fact]
        public void NullCoalescingAssignmentExpressionNested()
        {
            UsingExpression("a ??= b ??= c");
            N(SyntaxKind.CoalesceAssignmentExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.QuestionQuestionEqualsToken);
                N(SyntaxKind.CoalesceAssignmentExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                    N(SyntaxKind.QuestionQuestionEqualsToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "c");
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void NullCoalescingAssignmentParenthesizedNested()
        {
            UsingExpression("(a ??= b) ??= c");
            N(SyntaxKind.CoalesceAssignmentExpression);
            {
                N(SyntaxKind.ParenthesizedExpression);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.CoalesceAssignmentExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "a");
                        }
                        N(SyntaxKind.QuestionQuestionEqualsToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.QuestionQuestionEqualsToken);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "c");
                }
            }
            EOF();
        }

        [Fact]
        public void NullCoalescingAssignmentCSharp7_3()
        {
            var test = "a ??= b";
            var testWithStatement = @$"class C {{ void M() {{ var v = {test}; }} }}";

            CreateCompilation(testWithStatement, parseOptions: TestOptions.Regular7_3).VerifyDiagnostics(
                // (1,30): error CS0103: The name 'a' does not exist in the current context
                // class C { void M() { var v = a ??= b; } }
                Diagnostic(ErrorCode.ERR_NameNotInContext, "a").WithArguments("a").WithLocation(1, 30),
                // (1,32): error CS8370: Feature 'coalescing assignment' is not available in C# 7.3. Please use language version 8.0 or greater.
                // class C { void M() { var v = a ??= b; } }
                Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion7_3, "??=").WithArguments("coalescing assignment", "8.0").WithLocation(1, 32),
                // (1,36): error CS0103: The name 'b' does not exist in the current context
                // class C { void M() { var v = a ??= b; } }
                Diagnostic(ErrorCode.ERR_NameNotInContext, "b").WithArguments("b").WithLocation(1, 36));

            UsingExpression(test, TestOptions.Regular7_3);

            N(SyntaxKind.CoalesceAssignmentExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.QuestionQuestionEqualsToken);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "b");
                }
            }
            EOF();
        }

        [Fact]
        public void IndexExpression()
        {
            UsingExpression("^1");
            N(SyntaxKind.IndexExpression);
            {
                N(SyntaxKind.CaretToken);
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "1");
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_ThreeDots()
        {
            UsingExpression("1...2",
                // (1,2): error CS8401: Unexpected character sequence '...'
                // 1...2
                Diagnostic(ErrorCode.ERR_TripleDotNotAllowed, "").WithLocation(1, 2));

            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "1");
                }
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, ".2");
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Binary()
        {
            UsingExpression("1..1");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "1");
                }
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "1");
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Binary_WithIndexes()
        {
            UsingExpression("^5..^3");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.IndexExpression);
                {
                    N(SyntaxKind.CaretToken);
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "5");
                    }
                }
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.IndexExpression);
                {
                    N(SyntaxKind.CaretToken);
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "3");
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Binary_WithALowerPrecedenceOperator_01()
        {
            UsingExpression("1<<2..3>>4");
            N(SyntaxKind.RightShiftExpression);
            {
                N(SyntaxKind.LeftShiftExpression);
                {
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "1");
                    }
                    N(SyntaxKind.LessThanLessThanToken);
                    N(SyntaxKind.RangeExpression);
                    {
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "2");
                        }
                        N(SyntaxKind.DotDotToken);
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "3");
                        }
                    }
                }
                N(SyntaxKind.GreaterThanGreaterThanToken);
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "4");
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Binary_WithALowerPrecedenceOperator_02()
        {
            UsingExpression("1<<2..3>>>4");
            N(SyntaxKind.UnsignedRightShiftExpression);
            {
                N(SyntaxKind.LeftShiftExpression);
                {
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "1");
                    }
                    N(SyntaxKind.LessThanLessThanToken);
                    N(SyntaxKind.RangeExpression);
                    {
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "2");
                        }
                        N(SyntaxKind.DotDotToken);
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "3");
                        }
                    }
                }
                N(SyntaxKind.GreaterThanGreaterThanGreaterThanToken);
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "4");
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Binary_WithAHigherPrecedenceOperator()
        {
            UsingExpression("1+2..3-4");
            N(SyntaxKind.SubtractExpression);
            {
                N(SyntaxKind.AddExpression);
                {
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "1");
                    }
                    N(SyntaxKind.PlusToken);
                    N(SyntaxKind.RangeExpression);
                    {
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "2");
                        }
                        N(SyntaxKind.DotDotToken);
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "3");
                        }
                    }
                }
                N(SyntaxKind.MinusToken);
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "4");
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_UnaryBadLeft()
        {
            UsingExpression("a*..b");
            N(SyntaxKind.MultiplyExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.AsteriskToken);
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.DotDotToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_BinaryLeftPlus()
        {
            UsingExpression("a + b..c");
            N(SyntaxKind.AddExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.PlusToken);
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                    N(SyntaxKind.DotDotToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "c");
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_UnaryLeftPlus()
        {
            UsingExpression("a + b..");
            N(SyntaxKind.AddExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.PlusToken);
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                    N(SyntaxKind.DotDotToken);
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_UnaryRightMult()
        {
            UsingExpression("a.. && b");
            N(SyntaxKind.LogicalAndExpression);
            {
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                    N(SyntaxKind.DotDotToken);
                }
                N(SyntaxKind.AmpersandAmpersandToken);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "b");
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_UnaryRightMult2()
        {
            UsingExpression("..a && b");
            N(SyntaxKind.LogicalAndExpression);
            {
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.DotDotToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                }
                N(SyntaxKind.AmpersandAmpersandToken);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "b");
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Ambiguity1()
        {
            UsingExpression(".. ..");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.DotDotToken);
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Ambiguity2()
        {
            UsingExpression(".. .. e");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.DotDotToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "e");
                    }
                }
            }
            EOF();
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/36514")]
        public void RangeExpression_Ambiguity3()
        {
            UsingExpression(".. e ..");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "e");
                    }
                    N(SyntaxKind.DotDotToken);
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Ambiguity4()
        {
            UsingExpression("a .. .. b");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.DotDotToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                }
            }
            EOF();
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/36514")]
        public void RangeExpression_Ambiguity5()
        {
            UsingExpression("a .. b ..");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                    N(SyntaxKind.DotDotToken);
                }
            }
            EOF();
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/36514")]
        public void RangeExpression_Ambiguity6()
        {
            UsingExpression("a .. b .. c");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                    N(SyntaxKind.DotDotToken);
                }
            }
            EOF();
        }

        [Fact, WorkItem(36122, "https://github.com/dotnet/roslyn/issues/36122")]
        public void RangeExpression_NotCast()
        {
            UsingExpression("(Offset)..(Offset + Count)");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.ParenthesizedExpression);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "Offset");
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.ParenthesizedExpression);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.AddExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "Offset");
                        }
                        N(SyntaxKind.PlusToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "Count");
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Right()
        {
            UsingExpression("..1");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "1");
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Right_WithIndexes()
        {
            UsingExpression("..^3");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.IndexExpression);
                {
                    N(SyntaxKind.CaretToken);
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "3");
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Left()
        {
            UsingExpression("1..");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "1");
                }
                N(SyntaxKind.DotDotToken);
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_Left_WithIndexes()
        {
            UsingExpression("^5..");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.IndexExpression);
                {
                    N(SyntaxKind.CaretToken);
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "5");
                    }
                }
                N(SyntaxKind.DotDotToken);
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_NoOperands()
        {
            UsingExpression("..");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.DotDotToken);
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_NoOperands_WithOtherOperators()
        {
            UsingExpression("1+..<<2");
            N(SyntaxKind.LeftShiftExpression);
            {
                N(SyntaxKind.AddExpression);
                {
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "1");
                    }
                    N(SyntaxKind.PlusToken);
                    N(SyntaxKind.RangeExpression);
                    {
                        N(SyntaxKind.DotDotToken);
                    }
                }
                N(SyntaxKind.LessThanLessThanToken);
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "2");
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_DotSpaceDot()
        {
            UsingExpression("1. .2",
                // (1,1): error CS1073: Unexpected token '.2'
                // 1. .2
                Diagnostic(ErrorCode.ERR_UnexpectedToken, "1. ").WithArguments(".2").WithLocation(1, 1),
                // (1,4): error CS1001: Identifier expected
                // 1. .2
                Diagnostic(ErrorCode.ERR_IdentifierExpected, ".2").WithLocation(1, 4));

            N(SyntaxKind.SimpleMemberAccessExpression);
            {
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "1");
                }
                N(SyntaxKind.DotToken);
                M(SyntaxKind.IdentifierName);
                {
                    M(SyntaxKind.IdentifierToken);
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_MethodInvocation_NoOperands()
        {
            UsingExpression(".. .ToString()",
                // (1,1): error CS1073: Unexpected token '.'
                // .. .ToString()
                Diagnostic(ErrorCode.ERR_UnexpectedToken, "..").WithArguments(".").WithLocation(1, 1));

            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.DotDotToken);
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_MethodInvocation_LeftOperand()
        {
            UsingExpression("1.. .ToString()",
                // (1,1): error CS1073: Unexpected token '.'
                // 1.. .ToString()
                Diagnostic(ErrorCode.ERR_UnexpectedToken, "1..").WithArguments(".").WithLocation(1, 1));

            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "1");
                }
                N(SyntaxKind.DotDotToken);
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_MethodInvocation_RightOperand()
        {
            UsingExpression("..2 .ToString()");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.InvocationExpression);
                {
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "2");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "ToString");
                        }
                        N(SyntaxKind.ArgumentList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_MethodInvocation_TwoOperands()
        {
            UsingExpression("1..2 .ToString()");
            N(SyntaxKind.RangeExpression);
            {
                N(SyntaxKind.NumericLiteralExpression);
                {
                    N(SyntaxKind.NumericLiteralToken, "1");
                }
                N(SyntaxKind.DotDotToken);
                N(SyntaxKind.InvocationExpression);
                {
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "2");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "ToString");
                        }
                        N(SyntaxKind.ArgumentList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void RangeExpression_ConditionalAccessExpression()
        {
            UsingExpression("c?..b",
                // (1,6): error CS1003: Syntax error, ':' expected
                // c?..b
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments(":").WithLocation(1, 6),
                // (1,6): error CS1733: Expected expression
                // c?..b
                Diagnostic(ErrorCode.ERR_ExpressionExpected, "").WithLocation(1, 6));

            N(SyntaxKind.ConditionalExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "c");
                }
                N(SyntaxKind.QuestionToken);
                N(SyntaxKind.RangeExpression);
                {
                    N(SyntaxKind.DotDotToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                }
                M(SyntaxKind.ColonToken);
                M(SyntaxKind.IdentifierName);
                {
                    M(SyntaxKind.IdentifierToken);
                }
            }
            EOF();
        }

        [Fact]
        public void BaseExpression_01()
        {
            UsingExpression("base");
            N(SyntaxKind.BaseExpression);
            {
                N(SyntaxKind.BaseKeyword);
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void ArrayCreation_BadRef()
        {
            UsingExpression("new[] { ref }",
                // (1,9): error CS1525: Invalid expression term 'ref'
                // new[] { ref }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "ref ").WithArguments("ref").WithLocation(1, 9),
                // (1,13): error CS1525: Invalid expression term '}'
                // new[] { ref }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "}").WithArguments("}").WithLocation(1, 13));

            N(SyntaxKind.ImplicitArrayCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.OpenBracketToken);
                N(SyntaxKind.CloseBracketToken);
                N(SyntaxKind.ArrayInitializerExpression);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.RefExpression);
                    {
                        N(SyntaxKind.RefKeyword);
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void ArrayCreation_BadRefExpression()
        {
            UsingExpression("new[] { ref obj }",
                // (1,9): error CS1525: Invalid expression term 'ref'
                // new[] { ref obj }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "ref obj").WithArguments("ref").WithLocation(1, 9));

            N(SyntaxKind.ImplicitArrayCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.OpenBracketToken);
                N(SyntaxKind.CloseBracketToken);
                N(SyntaxKind.ArrayInitializerExpression);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.RefExpression);
                    {
                        N(SyntaxKind.RefKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "obj");
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void ArrayCreation_BadRefElementAccess()
        {
            UsingExpression("new[] { ref[] }",
                // (1,9): error CS1525: Invalid expression term 'ref'
                // new[] { ref[] }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "ref[]").WithArguments("ref").WithLocation(1, 9),
                // (1,12): error CS1525: Invalid expression term '['
                // new[] { ref[] }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "[").WithArguments("[").WithLocation(1, 12),
                // (1,13): error CS0443: Syntax error; value expected
                // new[] { ref[] }
                Diagnostic(ErrorCode.ERR_ValueExpected, "]").WithLocation(1, 13));

            N(SyntaxKind.ImplicitArrayCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.OpenBracketToken);
                N(SyntaxKind.CloseBracketToken);
                N(SyntaxKind.ArrayInitializerExpression);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.RefExpression);
                    {
                        N(SyntaxKind.RefKeyword);
                        N(SyntaxKind.ElementAccessExpression);
                        {
                            M(SyntaxKind.IdentifierName);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                            N(SyntaxKind.BracketedArgumentList);
                            {
                                N(SyntaxKind.OpenBracketToken);
                                M(SyntaxKind.Argument);
                                {
                                    M(SyntaxKind.IdentifierName);
                                    {
                                        M(SyntaxKind.IdentifierToken);
                                    }
                                }
                                N(SyntaxKind.CloseBracketToken);
                            }
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void AnonymousObjectCreation_BadRef()
        {
            UsingExpression("new { ref }",
                // (1,7): error CS1525: Invalid expression term 'ref'
                // new { ref }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "ref ").WithArguments("ref").WithLocation(1, 7),
                // (1,11): error CS1525: Invalid expression term '}'
                // new { ref }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "}").WithArguments("}").WithLocation(1, 11));

            N(SyntaxKind.AnonymousObjectCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.AnonymousObjectMemberDeclarator);
                {
                    N(SyntaxKind.RefExpression);
                    {
                        N(SyntaxKind.RefKeyword);
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void ObjectInitializer_BadRef()
        {
            UsingExpression("new C { P = ref }",
                // (1,17): error CS1525: Invalid expression term '}'
                // new C { P = ref }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "}").WithArguments("}").WithLocation(1, 17));

            N(SyntaxKind.ObjectCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "C");
                }
                N(SyntaxKind.ObjectInitializerExpression);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.SimpleAssignmentExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "P");
                        }
                        N(SyntaxKind.EqualsToken);
                        N(SyntaxKind.RefExpression);
                        {
                            N(SyntaxKind.RefKeyword);
                            M(SyntaxKind.IdentifierName);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void CollectionInitializer_BadRef_01()
        {
            UsingExpression("new C { ref }",
                // (1,13): error CS1525: Invalid expression term '}'
                // new C { ref }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "}").WithArguments("}").WithLocation(1, 13));

            N(SyntaxKind.ObjectCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "C");
                }
                N(SyntaxKind.CollectionInitializerExpression);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.RefExpression);
                    {
                        N(SyntaxKind.RefKeyword);
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void CollectionInitializer_BadRef_02()
        {
            UsingExpression("new C { { 0, ref } }",
                // (1,14): error CS1525: Invalid expression term 'ref'
                // new C { { 0, ref } }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "ref ").WithArguments("ref").WithLocation(1, 14),
                // (1,18): error CS1525: Invalid expression term '}'
                // new C { { 0, ref } }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "}").WithArguments("}").WithLocation(1, 18));

            N(SyntaxKind.ObjectCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "C");
                }
                N(SyntaxKind.CollectionInitializerExpression);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.ComplexElementInitializerExpression);
                    {
                        N(SyntaxKind.OpenBraceToken);
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "0");
                        }
                        N(SyntaxKind.CommaToken);
                        N(SyntaxKind.RefExpression);
                        {
                            N(SyntaxKind.RefKeyword);
                            M(SyntaxKind.IdentifierName);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                        }
                        N(SyntaxKind.CloseBraceToken);
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void AttributeArgument_BadRef()
        {
            UsingTree("class C { [Attr(ref)] void M() { } }",
                // (1,17): error CS1525: Invalid expression term 'ref'
                // class C { [Attr(ref)] void M() { } }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "ref").WithArguments("ref").WithLocation(1, 17),
                // (1,20): error CS1525: Invalid expression term ')'
                // class C { [Attr(ref)] void M() { } }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, ")").WithArguments(")").WithLocation(1, 20));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.AttributeList);
                        {
                            N(SyntaxKind.OpenBracketToken);
                            N(SyntaxKind.Attribute);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "Attr");
                                }
                                N(SyntaxKind.AttributeArgumentList);
                                {
                                    N(SyntaxKind.OpenParenToken);
                                    N(SyntaxKind.AttributeArgument);
                                    {
                                        N(SyntaxKind.RefExpression);
                                        {
                                            N(SyntaxKind.RefKeyword);
                                            M(SyntaxKind.IdentifierName);
                                            {
                                                M(SyntaxKind.IdentifierToken);
                                            }
                                        }
                                    }
                                    N(SyntaxKind.CloseParenToken);
                                }
                            }
                            N(SyntaxKind.CloseBracketToken);
                        }
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void ForLoop_BadRefCondition()
        {
            UsingStatement("for (int i = 0; ref; i++) { }",
                // (1,17): error CS1525: Invalid expression term 'ref'
                // for (int i = 0; ref; i++) { }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "ref").WithArguments("ref").WithLocation(1, 17),
                // (1,20): error CS1525: Invalid expression term ';'
                // for (int i = 0; ref; i++) { }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, ";").WithArguments(";").WithLocation(1, 20));

            N(SyntaxKind.ForStatement);
            {
                N(SyntaxKind.ForKeyword);
                N(SyntaxKind.OpenParenToken);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.PredefinedType);
                    {
                        N(SyntaxKind.IntKeyword);
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "i");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NumericLiteralExpression);
                            {
                                N(SyntaxKind.NumericLiteralToken, "0");
                            }
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
                N(SyntaxKind.RefExpression);
                {
                    N(SyntaxKind.RefKeyword);
                    M(SyntaxKind.IdentifierName);
                    {
                        M(SyntaxKind.IdentifierToken);
                    }
                }
                N(SyntaxKind.SemicolonToken);
                N(SyntaxKind.PostIncrementExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "i");
                    }
                    N(SyntaxKind.PlusPlusToken);
                }
                N(SyntaxKind.CloseParenToken);
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void ArrayCreation_BadInElementAccess()
        {
            UsingExpression("new[] { in[] }",
                // (1,9): error CS1003: Syntax error, ',' expected
                // new[] { in[] }
                Diagnostic(ErrorCode.ERR_SyntaxError, "in").WithArguments(",").WithLocation(1, 9));

            N(SyntaxKind.ImplicitArrayCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.OpenBracketToken);
                N(SyntaxKind.CloseBracketToken);
                N(SyntaxKind.ArrayInitializerExpression);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void ArrayCreation_BadOutElementAccess()
        {
            UsingExpression("new[] { out[] }",
                    // (1,9): error CS1003: Syntax error, ',' expected
                    // new[] { out[] }
                    Diagnostic(ErrorCode.ERR_SyntaxError, "out").WithArguments(",").WithLocation(1, 9));

            N(SyntaxKind.ImplicitArrayCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.OpenBracketToken);
                N(SyntaxKind.CloseBracketToken);
                N(SyntaxKind.ArrayInitializerExpression);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        [WorkItem(39072, "https://github.com/dotnet/roslyn/issues/39072")]
        public void ArrayCreation_ElementAccess()
        {
            UsingExpression("new[] { obj[] }",
                // (1,13): error CS0443: Syntax error; value expected
                // new[] { obj[] }
                Diagnostic(ErrorCode.ERR_ValueExpected, "]").WithLocation(1, 13));

            N(SyntaxKind.ImplicitArrayCreationExpression);
            {
                N(SyntaxKind.NewKeyword);
                N(SyntaxKind.OpenBracketToken);
                N(SyntaxKind.CloseBracketToken);
                N(SyntaxKind.ArrayInitializerExpression);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.ElementAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "obj");
                        }
                        N(SyntaxKind.BracketedArgumentList);
                        {
                            N(SyntaxKind.OpenBracketToken);
                            M(SyntaxKind.Argument);
                            {
                                M(SyntaxKind.IdentifierName);
                                {
                                    M(SyntaxKind.IdentifierToken);
                                }
                            }
                            N(SyntaxKind.CloseBracketToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact, WorkItem(44789, "https://github.com/dotnet/roslyn/issues/44789")]
        public void MismatchedInterpolatedStringContents_01()
        {
            var text =
@"class A
{
    void M()
    {
        if (b)
        {
            A B = new C($@""{D(.E}"");
            N.O("""", P.Q);
            R.S(T);
            U.V(W.X, Y.Z);
        }
    }

    string M() => """";
}";
            var tree = ParseTree(text, TestOptions.Regular);
            // Note that the parser eventually syncs back up and stops producing diagnostics.
            tree.GetDiagnostics().Verify(
                // (7,31): error CS1001: Identifier expected
                //             A B = new C($@"{D(.E}");
                Diagnostic(ErrorCode.ERR_IdentifierExpected, ".").WithLocation(7, 31),
                // (7,33): error CS1003: Syntax error, ')' expected
                //             A B = new C($@"{D(.E}");
                Diagnostic(ErrorCode.ERR_SyntaxError, "}").WithArguments(")").WithLocation(7, 33),
                // (7,33): error CS1003: Syntax error, ',' expected
                //             A B = new C($@"{D(.E}");
                Diagnostic(ErrorCode.ERR_SyntaxError, "}").WithArguments(",").WithLocation(7, 33),
                // (7,34): error CS1026: ) expected
                //             A B = new C($@"{D(.E}");
                Diagnostic(ErrorCode.ERR_CloseParenExpected, "").WithLocation(7, 34)
                );
        }

        [Fact, WorkItem(44789, "https://github.com/dotnet/roslyn/issues/44789")]
        public void MismatchedInterpolatedStringContents_02()
        {
            var text =
@"class A
{
    void M()
    {
        if (b)
        {
            A B = new C($@""{D(.E}\F\G{H}_{I.J.K(""L"")}.M"");
            N.O("""", P.Q);
            R.S(T);
            U.V(W.X, Y.Z);
        }
    }

    string M() => """";
}";
            var tree = ParseTree(text, TestOptions.Regular);
            // Note that the parser eventually syncs back up and stops producing diagnostics.
            tree.GetDiagnostics().Verify(
                    // (7,31): error CS1001: Identifier expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_IdentifierExpected, ".").WithLocation(7, 31),
                    // (7,33): error CS1003: Syntax error, ')' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "}").WithArguments(")").WithLocation(7, 33),
                    // (7,33): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "}").WithArguments(",").WithLocation(7, 33),
                    // (7,34): error CS1056: Unexpected character '\'
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_UnexpectedCharacter, "").WithArguments("\\").WithLocation(7, 34),
                    // (7,35): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "F").WithArguments(",").WithLocation(7, 35),
                    // (7,36): error CS1056: Unexpected character '\'
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_UnexpectedCharacter, "").WithArguments("\\").WithLocation(7, 36),
                    // (7,37): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "G").WithArguments(",").WithLocation(7, 37),
                    // (7,38): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "{").WithArguments(",").WithLocation(7, 38),
                    // (7,39): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "H").WithArguments(",").WithLocation(7, 39),
                    // (7,40): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "}").WithArguments(",").WithLocation(7, 40),
                    // (7,41): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "_").WithArguments(",").WithLocation(7, 41),
                    // (7,42): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "{").WithArguments(",").WithLocation(7, 42),
                    // (7,43): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "I").WithArguments(",").WithLocation(7, 43),
                    // (7,49): error CS1026: ) expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_CloseParenExpected, "").WithLocation(7, 49),
                    // (7,49): error CS1026: ) expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_CloseParenExpected, "").WithLocation(7, 49),
                    // (7,50): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, "L").WithArguments(",").WithLocation(7, 50),
                    // (7,51): error CS1003: Syntax error, ',' expected
                    //             A B = new C($@"{D(.E}\F\G{H}_{I.J.K("L")}.M");
                    Diagnostic(ErrorCode.ERR_SyntaxError, @""")}.M""").WithArguments(",").WithLocation(7, 51)
                );
        }

        [Fact]
        public void UnsignedRightShift_01()
        {
            foreach (var options in new[] { TestOptions.RegularPreview, TestOptions.Regular10, TestOptions.Regular11 })
            {
                UsingExpression("x >>> y", options);

                N(SyntaxKind.UnsignedRightShiftExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                    N(SyntaxKind.GreaterThanGreaterThanGreaterThanToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
                EOF();
            }
        }

        [Fact]
        public void UnsignedRightShift_02()
        {
            foreach (var options in new[] { TestOptions.RegularPreview, TestOptions.Regular10, TestOptions.Regular11 })
            {
                UsingExpression("x > >> y", options,
                    // (1,5): error CS1525: Invalid expression term '>'
                    // x > >> y
                    Diagnostic(ErrorCode.ERR_InvalidExprTerm, ">").WithArguments(">").WithLocation(1, 5)
                    );

                N(SyntaxKind.GreaterThanExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                    N(SyntaxKind.GreaterThanToken);
                    N(SyntaxKind.RightShiftExpression);
                    {
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                        N(SyntaxKind.GreaterThanGreaterThanToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "y");
                        }
                    }
                }
                EOF();
            }
        }

        [Fact]
        public void UnsignedRightShift_03()
        {
            foreach (var options in new[] { TestOptions.RegularPreview, TestOptions.Regular10, TestOptions.Regular11 })
            {
                UsingExpression("x >> > y", options,
                    // (1,6): error CS1525: Invalid expression term '>'
                    // x >> > y
                    Diagnostic(ErrorCode.ERR_InvalidExprTerm, ">").WithArguments(">").WithLocation(1, 6)
                    );

                N(SyntaxKind.GreaterThanExpression);
                {
                    N(SyntaxKind.RightShiftExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.GreaterThanGreaterThanToken);
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                    }
                    N(SyntaxKind.GreaterThanToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
                EOF();
            }
        }

        [Fact]
        public void UnsignedRightShiftAssignment_01()
        {
            foreach (var options in new[] { TestOptions.RegularPreview, TestOptions.Regular10, TestOptions.Regular11 })
            {
                UsingExpression("x >>>= y", options);

                N(SyntaxKind.UnsignedRightShiftAssignmentExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                    N(SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
                EOF();
            }
        }

        [Fact]
        public void UnsignedRightShiftAssignment_02()
        {
            foreach (var options in new[] { TestOptions.RegularPreview, TestOptions.Regular10, TestOptions.Regular11 })
            {
                UsingExpression("x > >>= y", options,
                    // (1,5): error CS1525: Invalid expression term '>'
                    // x > >>= y
                    Diagnostic(ErrorCode.ERR_InvalidExprTerm, ">").WithArguments(">").WithLocation(1, 5)
                    );

                N(SyntaxKind.RightShiftAssignmentExpression);
                {
                    N(SyntaxKind.GreaterThanExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.GreaterThanToken);
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                    }
                    N(SyntaxKind.GreaterThanGreaterThanEqualsToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
                EOF();
            }
        }

        [Fact]
        public void UnsignedRightShiftAssignment_03()
        {
            foreach (var options in new[] { TestOptions.RegularPreview, TestOptions.Regular10, TestOptions.Regular11 })
            {
                UsingExpression("x >> >= y", options,
                    // (1,6): error CS1525: Invalid expression term '>='
                    // x >> >= y
                    Diagnostic(ErrorCode.ERR_InvalidExprTerm, ">=").WithArguments(">=").WithLocation(1, 6)
                    );

                N(SyntaxKind.GreaterThanOrEqualExpression);
                {
                    N(SyntaxKind.RightShiftExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.GreaterThanGreaterThanToken);
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                    }
                    N(SyntaxKind.GreaterThanEqualsToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
                EOF();
            }
        }

        [Fact]
        public void UnsignedRightShiftAssignment_04()
        {
            foreach (var options in new[] { TestOptions.RegularPreview, TestOptions.Regular10, TestOptions.Regular11 })
            {
                UsingExpression("x >>> = y", options,
                    // (1,7): error CS1525: Invalid expression term '='
                    // x >>> = y
                    Diagnostic(ErrorCode.ERR_InvalidExprTerm, "=").WithArguments("=").WithLocation(1, 7)
                    );

                N(SyntaxKind.SimpleAssignmentExpression);
                {
                    N(SyntaxKind.UnsignedRightShiftExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.GreaterThanGreaterThanGreaterThanToken);
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                    }
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
                EOF();
            }
        }
    }
}
