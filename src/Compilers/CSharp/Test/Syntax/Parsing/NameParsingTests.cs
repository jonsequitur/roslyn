// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;
using Xunit.Abstractions;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class NameParsingTests : ParsingTests
    {
        public NameParsingTests(ITestOutputHelper output) : base(output) { }

        private NameSyntax ParseName(string text)
        {
            return SyntaxFactory.ParseName(text);
        }

        private TypeSyntax ParseTypeName(string text)
        {
            return SyntaxFactory.ParseTypeName(text);
        }

        [Fact]
        public void TestBasicName()
        {
            var text = "goo";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)name).Identifier.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestBasicNameWithTrash()
        {
            var text = "/*comment*/goo/*comment2*/ bar";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)name).Identifier.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(1);
            name.ToFullString().Should().Be(text);
        }

        [Fact]
        public void TestMissingName()
        {
            var text = string.Empty;
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)name).Identifier.IsMissing.Should().BeTrue();
            name.Errors().Length.Should().Be(1);
            name.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            name.ToString().Should().Be(string.Empty);
        }

        [Fact]
        public void TestMissingNameDueToKeyword()
        {
            var text = "class";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            name.IsMissing.Should().BeTrue();
            name.Errors().Length.Should().Be(2);
            name.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedToken);
            name.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            name.ToString().Should().Be(string.Empty);
        }

        [Fact]
        public void TestMissingNameDueToPartialClassStart()
        {
            var text = "partial class";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            name.IsMissing.Should().BeTrue();
            name.Errors().Length.Should().Be(2);
            name.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedToken);
            name.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            name.ToString().Should().Be(string.Empty);
        }

        [Fact]
        public void TestMissingNameDueToPartialMethodStart()
        {
            var text = "partial void Method()";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            name.IsMissing.Should().BeTrue();
            name.Errors().Length.Should().Be(2);
            name.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedToken);
            name.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            name.ToString().Should().Be(string.Empty);
        }

        [Fact]
        public void TestAliasedName()
        {
            var text = "goo::bar";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.AliasQualifiedName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestGlobalAliasedName()
        {
            var text = "global::bar";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.IsMissing.Should().BeFalse();
            name.Kind().Should().Be(SyntaxKind.AliasQualifiedName);
            var an = (AliasQualifiedNameSyntax)name;
            an.Alias.Identifier.Kind().Should().Be(SyntaxKind.GlobalKeyword);
            name.Errors().Length.Should().Be(0);
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestDottedName()
        {
            var text = "goo.bar";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.QualifiedName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestAliasedDottedName()
        {
            var text = "goo::bar.Zed";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.QualifiedName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            name.ToString().Should().Be(text);

            name = ((QualifiedNameSyntax)name).Left;
            name.Kind().Should().Be(SyntaxKind.AliasQualifiedName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestDoubleAliasName()
        {
            // In the original implementation of the parser this error case was parsed as 
            //
            // (goo :: bar ) :: baz
            //
            // However, we have decided that the left hand side of a :: should always be
            // an identifier, not a name, even in error cases. Therefore instead we 
            // parse this as though the error was that the user intended to make the 
            // second :: a dot; we parse this as
            //
            // (goo :: bar ) . baz

            var text = "goo::bar::baz";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.QualifiedName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(1);
            name.ToString().Should().Be(text);

            name = ((QualifiedNameSyntax)name).Left;
            name.Kind().Should().Be(SyntaxKind.AliasQualifiedName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestGenericName()
        {
            var text = "goo<bar>";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.GenericName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            var gname = (GenericNameSyntax)name;
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.IsUnboundGenericName.Should().BeFalse();
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestGenericNameWithTwoArguments()
        {
            var text = "goo<bar,zed>";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.GenericName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            var gname = (GenericNameSyntax)name;
            gname.TypeArgumentList.Arguments.Count.Should().Be(2);
            gname.IsUnboundGenericName.Should().BeFalse();
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestNestedGenericName_01()
        {
            var text = "goo<bar<zed>>";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.GenericName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            var gname = (GenericNameSyntax)name;
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.IsUnboundGenericName.Should().BeFalse();
            gname.TypeArgumentList.Arguments[0].Should().NotBeNull();
            gname.TypeArgumentList.Arguments[0].Kind().Should().Be(SyntaxKind.GenericName);
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestNestedGenericName_02()
        {
            var text = "goo<bar<zed<U>>>";
            var name = ParseName(text);

            UsingNode(text, name);

            N(SyntaxKind.GenericName);
            {
                N(SyntaxKind.IdentifierToken, "goo");
                N(SyntaxKind.TypeArgumentList);
                {
                    N(SyntaxKind.LessThanToken);
                    N(SyntaxKind.GenericName);
                    {
                        N(SyntaxKind.IdentifierToken, "bar");
                        N(SyntaxKind.TypeArgumentList);
                        {
                            N(SyntaxKind.LessThanToken);
                            N(SyntaxKind.GenericName);
                            {
                                N(SyntaxKind.IdentifierToken, "zed");
                                N(SyntaxKind.TypeArgumentList);
                                {
                                    N(SyntaxKind.LessThanToken);
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "U");
                                    }
                                    N(SyntaxKind.GreaterThanToken);
                                }
                            }
                            N(SyntaxKind.GreaterThanToken);
                        }
                    }
                    N(SyntaxKind.GreaterThanToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestOpenNameWithNoCommas()
        {
            var text = "goo<>";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.GenericName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            var gname = (GenericNameSyntax)name;
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.TypeArgumentList.Arguments.SeparatorCount.Should().Be(0);
            gname.IsUnboundGenericName.Should().BeTrue();
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestOpenNameWithAComma()
        {
            var text = "goo<,>";
            var name = ParseName(text);

            name.Should().NotBeNull();
            name.Kind().Should().Be(SyntaxKind.GenericName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            var gname = (GenericNameSyntax)name;
            gname.TypeArgumentList.Arguments.Count.Should().Be(2);
            gname.TypeArgumentList.Arguments.SeparatorCount.Should().Be(1);
            gname.IsUnboundGenericName.Should().BeTrue();
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestBasicTypeName()
        {
            var text = "goo";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.Kind().Should().Be(SyntaxKind.IdentifierName);
            var name = (NameSyntax)tname;
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestDottedTypeName()
        {
            var text = "goo.bar";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.Kind().Should().Be(SyntaxKind.QualifiedName);
            var name = (NameSyntax)tname;
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestGenericTypeName()
        {
            var text = "goo<bar>";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.Kind().Should().Be(SyntaxKind.GenericName);
            var name = (NameSyntax)tname;
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            var gname = (GenericNameSyntax)name;
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.IsUnboundGenericName.Should().BeFalse();
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestNestedGenericTypeName_01()
        {
            var text = "goo<bar<zed>>";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.Kind().Should().Be(SyntaxKind.GenericName);
            var name = (NameSyntax)tname;
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            var gname = (GenericNameSyntax)name;
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.IsUnboundGenericName.Should().BeFalse();
            gname.TypeArgumentList.Arguments[0].Should().NotBeNull();
            gname.TypeArgumentList.Arguments[0].Kind().Should().Be(SyntaxKind.GenericName);
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestNestedGenericTypeName_02()
        {
            var text = "goo<bar<zed<U>>>";
            var tname = ParseTypeName(text);

            UsingNode(text, tname);

            N(SyntaxKind.GenericName);
            {
                N(SyntaxKind.IdentifierToken, "goo");
                N(SyntaxKind.TypeArgumentList);
                {
                    N(SyntaxKind.LessThanToken);
                    N(SyntaxKind.GenericName);
                    {
                        N(SyntaxKind.IdentifierToken, "bar");
                        N(SyntaxKind.TypeArgumentList);
                        {
                            N(SyntaxKind.LessThanToken);
                            N(SyntaxKind.GenericName);
                            {
                                N(SyntaxKind.IdentifierToken, "zed");
                                N(SyntaxKind.TypeArgumentList);
                                {
                                    N(SyntaxKind.LessThanToken);
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "U");
                                    }
                                    N(SyntaxKind.GreaterThanToken);
                                }
                            }
                            N(SyntaxKind.GreaterThanToken);
                        }
                    }
                    N(SyntaxKind.GreaterThanToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestOpenTypeNameWithNoCommas()
        {
            var text = "goo<>";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.Kind().Should().Be(SyntaxKind.GenericName);
            var name = (NameSyntax)tname;
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
            var gname = (GenericNameSyntax)name;
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.TypeArgumentList.Arguments.SeparatorCount.Should().Be(0);
            gname.IsUnboundGenericName.Should().BeTrue();
            name.ToString().Should().Be(text);
        }

        [Fact]
        public void TestKnownTypeNames()
        {
            ParseKnownTypeName(SyntaxKind.BoolKeyword);
            ParseKnownTypeName(SyntaxKind.ByteKeyword);
            ParseKnownTypeName(SyntaxKind.SByteKeyword);
            ParseKnownTypeName(SyntaxKind.ShortKeyword);
            ParseKnownTypeName(SyntaxKind.UShortKeyword);
            ParseKnownTypeName(SyntaxKind.IntKeyword);
            ParseKnownTypeName(SyntaxKind.UIntKeyword);
            ParseKnownTypeName(SyntaxKind.LongKeyword);
            ParseKnownTypeName(SyntaxKind.ULongKeyword);
            ParseKnownTypeName(SyntaxKind.FloatKeyword);
            ParseKnownTypeName(SyntaxKind.DoubleKeyword);
            ParseKnownTypeName(SyntaxKind.DecimalKeyword);
            ParseKnownTypeName(SyntaxKind.StringKeyword);
            ParseKnownTypeName(SyntaxKind.ObjectKeyword);
        }

        private void ParseKnownTypeName(SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.Kind().Should().Be(SyntaxKind.PredefinedType);
            tname.ToString().Should().Be(text);
            var tok = ((PredefinedTypeSyntax)tname).Keyword;
            tok.Kind().Should().Be(kind);
        }

        [Fact]
        public void TestNullableTypeName()
        {
            var text = "goo?";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.Kind().Should().Be(SyntaxKind.NullableType);
            tname.ToString().Should().Be(text);
            var name = (NameSyntax)((NullableTypeSyntax)tname).ElementType;
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestPointerTypeName()
        {
            var text = "goo*";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.Kind().Should().Be(SyntaxKind.PointerType);
            tname.ToString().Should().Be(text);
            var name = (NameSyntax)((PointerTypeSyntax)tname).ElementType;
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestPointerTypeNameWithMultipleAsterisks()
        {
            var text = "goo***";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.ToString().Should().Be(text);
            tname.Kind().Should().Be(SyntaxKind.PointerType);

            // check depth of pointer defers
            int depth = 0;
            while (tname.Kind() == SyntaxKind.PointerType)
            {
                tname = ((PointerTypeSyntax)tname).ElementType;
                depth++;
            }

            depth.Should().Be(3);

            var name = (NameSyntax)tname;
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestArrayTypeName()
        {
            var text = "goo[]";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.ToString().Should().Be(text);
            tname.Kind().Should().Be(SyntaxKind.ArrayType);

            var array = (ArrayTypeSyntax)tname;
            array.RankSpecifiers.Count.Should().Be(1);
            array.RankSpecifiers[0].Sizes.Count.Should().Be(1);
            array.RankSpecifiers[0].Sizes.SeparatorCount.Should().Be(0);
            array.RankSpecifiers[0].Rank.Should().Be(1);

            var name = (NameSyntax)array.ElementType;
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestMultiDimensionalArrayTypeName()
        {
            var text = "goo[,,]";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.ToString().Should().Be(text);
            tname.Kind().Should().Be(SyntaxKind.ArrayType);

            var array = (ArrayTypeSyntax)tname;
            array.RankSpecifiers.Count.Should().Be(1);
            array.RankSpecifiers[0].Sizes.Count.Should().Be(3);
            array.RankSpecifiers[0].Sizes.SeparatorCount.Should().Be(2);
            array.RankSpecifiers[0].Rank.Should().Be(3);

            var name = (NameSyntax)array.ElementType;
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestMultiRankedArrayTypeName()
        {
            var text = "goo[][,][,,]";
            var tname = ParseTypeName(text);

            tname.Should().NotBeNull();
            tname.ToString().Should().Be(text);
            tname.Kind().Should().Be(SyntaxKind.ArrayType);

            var array = (ArrayTypeSyntax)tname;
            array.RankSpecifiers.Count.Should().Be(3);

            array.RankSpecifiers[0].Sizes.Count.Should().Be(1);
            array.RankSpecifiers[0].Sizes.SeparatorCount.Should().Be(0);
            array.RankSpecifiers[0].Rank.Should().Be(1);

            array.RankSpecifiers[1].Sizes.Count.Should().Be(2);
            array.RankSpecifiers[1].Sizes.SeparatorCount.Should().Be(1);
            array.RankSpecifiers[1].Rank.Should().Be(2);

            array.RankSpecifiers[2].Sizes.Count.Should().Be(3);
            array.RankSpecifiers[2].Sizes.SeparatorCount.Should().Be(2);
            array.RankSpecifiers[2].Rank.Should().Be(3);

            var name = (NameSyntax)array.ElementType;
            name.Kind().Should().Be(SyntaxKind.IdentifierName);
            name.IsMissing.Should().BeFalse();
            name.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestVarianceInNameBad()
        {
            var text = "goo<in bar>";
            var tname = ParseName(text);

            tname.Should().NotBeNull();
            tname.ToString().Should().Be(text);
            tname.Kind().Should().Be(SyntaxKind.GenericName);

            var gname = (GenericNameSyntax)tname;
            gname.Identifier.ToString().Should().Be("goo");
            gname.IsUnboundGenericName.Should().BeFalse();
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.TypeArgumentList.Arguments[0].Should().NotBeNull();

            var arg = gname.TypeArgumentList.Arguments[0];
            arg.Kind().Should().Be(SyntaxKind.IdentifierName);
            arg.ContainsDiagnostics.Should().BeTrue();
            arg.Errors().Length.Should().Be(1);
            arg.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IllegalVarianceSyntax);

            tname.ToString().Should().Be(text);
        }

        [Fact]
        public void TestAttributeInNameBad()
        {
            var text = "goo<[My]bar>";
            var tname = ParseName(text);

            tname.Should().NotBeNull();
            tname.ToString().Should().Be(text);
            tname.Kind().Should().Be(SyntaxKind.GenericName);

            var gname = (GenericNameSyntax)tname;
            gname.Identifier.ToString().Should().Be("goo");
            gname.IsUnboundGenericName.Should().BeFalse();
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.TypeArgumentList.Arguments[0].Should().NotBeNull();

            var arg = gname.TypeArgumentList.Arguments[0];
            arg.Kind().Should().Be(SyntaxKind.IdentifierName);
            arg.ContainsDiagnostics.Should().BeTrue();
            arg.Errors().Length.Should().Be(1);
            arg.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);

            tname.ToString().Should().Be(text);
        }

        [WorkItem(7177, "https://github.com/dotnet/roslyn/issues/7177")]
        [Fact]
        public void TestConstantInGenericNameBad()
        {
            var text = "goo<0>";
            var tname = ParseName(text);

            tname.Should().NotBeNull();
            tname.ToString().Should().Be(text);
            tname.Kind().Should().Be(SyntaxKind.GenericName);

            var gname = (GenericNameSyntax)tname;
            gname.Identifier.ToString().Should().Be("goo");
            gname.IsUnboundGenericName.Should().BeFalse();
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.TypeArgumentList.Arguments[0].Should().NotBeNull();

            var arg = gname.TypeArgumentList.Arguments[0];
            arg.Kind().Should().Be(SyntaxKind.IdentifierName);
            arg.ContainsDiagnostics.Should().BeTrue();
            arg.Errors().Length.Should().Be(1);
            arg.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);

            tname.ToString().Should().Be(text);
        }

        [WorkItem(7177, "https://github.com/dotnet/roslyn/issues/7177")]
        [Fact]
        public void TestConstantInGenericNamePartiallyBad()
        {
            var text = "goo<0,bool>";
            var tname = ParseName(text);

            tname.Should().NotBeNull();
            tname.ToString().Should().Be(text);
            tname.Kind().Should().Be(SyntaxKind.GenericName);

            var gname = (GenericNameSyntax)tname;
            gname.Identifier.ToString().Should().Be("goo");
            gname.IsUnboundGenericName.Should().BeFalse();
            gname.TypeArgumentList.Arguments.Count.Should().Be(2);
            gname.TypeArgumentList.Arguments[0].Should().NotBeNull();
            gname.TypeArgumentList.Arguments[1].Should().NotBeNull();

            var arg = gname.TypeArgumentList.Arguments[0];
            arg.Kind().Should().Be(SyntaxKind.IdentifierName);
            arg.ContainsDiagnostics.Should().BeTrue();
            arg.Errors().Length.Should().Be(1);
            arg.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);

            var arg2 = gname.TypeArgumentList.Arguments[1];
            arg2.Kind().Should().Be(SyntaxKind.PredefinedType);
            arg2.ContainsDiagnostics.Should().BeFalse();
            arg2.Errors().Length.Should().Be(0);

            tname.ToString().Should().Be(text);
        }

        [WorkItem(7177, "https://github.com/dotnet/roslyn/issues/7177")]
        [Fact]
        public void TestKeywordInGenericNameBad()
        {
            var text = "goo<static>";
            var tname = ParseName(text);

            tname.Should().NotBeNull();
            tname.ToString().Should().Be(text);
            tname.Kind().Should().Be(SyntaxKind.GenericName);

            var gname = (GenericNameSyntax)tname;
            gname.Identifier.ToString().Should().Be("goo");
            gname.IsUnboundGenericName.Should().BeFalse();
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.TypeArgumentList.Arguments[0].Should().NotBeNull();

            var arg = gname.TypeArgumentList.Arguments[0];
            arg.Kind().Should().Be(SyntaxKind.IdentifierName);
            arg.ContainsDiagnostics.Should().BeTrue();
            arg.Errors().Length.Should().Be(1);
            arg.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);

            tname.ToString().Should().Be(text);
        }

        [Fact]
        public void TestAttributeAndVarianceInNameBad()
        {
            var text = "goo<[My]in bar>";
            var tname = ParseName(text);

            tname.Should().NotBeNull();
            tname.ToString().Should().Be(text);
            tname.Kind().Should().Be(SyntaxKind.GenericName);

            var gname = (GenericNameSyntax)tname;
            gname.Identifier.ToString().Should().Be("goo");
            gname.IsUnboundGenericName.Should().BeFalse();
            gname.TypeArgumentList.Arguments.Count.Should().Be(1);
            gname.TypeArgumentList.Arguments[0].Should().NotBeNull();

            var arg = gname.TypeArgumentList.Arguments[0];
            arg.Kind().Should().Be(SyntaxKind.IdentifierName);
            arg.ContainsDiagnostics.Should().BeTrue();
            arg.Errors().Length.Should().Be(2);
            arg.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            arg.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IllegalVarianceSyntax);

            tname.ToString().Should().Be(text);
        }

        [WorkItem(545778, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545778")]
        [Fact]
        public void TestFormattingCharacter()
        {
            var text = "\u0915\u094d\u200d\u0937";
            var tok = SyntaxFactory.ParseToken(text);

            tok.Should().NotBe(default);
            tok.ToString().Should().Be(text);
            tok.ValueText.Should().NotBe(text);
            tok.ValueText.Should().Be("\u0915\u094d\u0937"); //formatting character \u200d removed

            SyntaxFacts.ContainsDroppedIdentifierCharacters(text).Should().BeTrue();
            SyntaxFacts.ContainsDroppedIdentifierCharacters(tok.ValueText).Should().BeFalse();
        }

        [WorkItem(959148, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/959148")]
        [Fact]
        public void TestSoftHyphen()
        {
            var text = "x\u00ady";
            var tok = SyntaxFactory.ParseToken(text);

            tok.Should().NotBe(default);
            tok.ToString().Should().Be(text);
            tok.ValueText.Should().NotBe(text);
            tok.ValueText.Should().Be("xy"); // formatting character SOFT HYPHEN (U+00AD) removed

            SyntaxFacts.ContainsDroppedIdentifierCharacters(text).Should().BeTrue();
            SyntaxFacts.ContainsDroppedIdentifierCharacters(tok.ValueText).Should().BeFalse();
        }

        [WorkItem(545778, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545778")]
        [Fact]
        public void ContainsDroppedIdentifierCharacters()
        {
            SyntaxFacts.ContainsDroppedIdentifierCharacters(null).Should().BeFalse();
            SyntaxFacts.ContainsDroppedIdentifierCharacters("").Should().BeFalse();
            SyntaxFacts.ContainsDroppedIdentifierCharacters("a").Should().BeFalse();
            SyntaxFacts.ContainsDroppedIdentifierCharacters("a@").Should().BeFalse();

            SyntaxFacts.ContainsDroppedIdentifierCharacters("@").Should().BeTrue();
            SyntaxFacts.ContainsDroppedIdentifierCharacters("@a").Should().BeTrue();
            SyntaxFacts.ContainsDroppedIdentifierCharacters("\u200d").Should().BeTrue();
            SyntaxFacts.ContainsDroppedIdentifierCharacters("a\u200d").Should().BeTrue();
        }
    }
}
