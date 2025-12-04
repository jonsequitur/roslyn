// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Test.Utilities;
using Xunit;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class SyntaxListTests : CSharpTestBase
    {
        [Fact]
        public void Equality()
        {
            var node1 = SyntaxFactory.ReturnStatement();
            var node2 = SyntaxFactory.ReturnStatement();

            EqualityTesting.AssertEqual(default(SyntaxList<CSharpSyntaxNode>), default(SyntaxList<CSharpSyntaxNode>));
            EqualityTesting.AssertEqual(new SyntaxList<CSharpSyntaxNode>(node1), new SyntaxList<CSharpSyntaxNode>(node1));

            EqualityTesting.AssertNotEqual(new SyntaxList<CSharpSyntaxNode>(node1), new SyntaxList<CSharpSyntaxNode>(node2));
        }

        [Fact]
        public void EnumeratorEquality()
        {
            FluentActions.Invoking(() => default(SyntaxList<CSharpSyntaxNode>.Enumerator).GetHashCode()).Should().Throw<NotSupportedException>();
            FluentActions.Invoking(() => default(SyntaxList<CSharpSyntaxNode>.Enumerator).Equals(default(SyntaxList<CSharpSyntaxNode>.Enumerator))).Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void TestAddInsertRemoveReplace()
        {
            var list = SyntaxFactory.List<SyntaxNode>(
                new[] {
                    SyntaxFactory.ParseExpression("A "),
                    SyntaxFactory.ParseExpression("B "),
                    SyntaxFactory.ParseExpression("C ") });

            list.Count.Should().Be(3);
            list[0].ToString().Should().Be("A");
            list[1].ToString().Should().Be("B");
            list[2].ToString().Should().Be("C");
            list.ToFullString().Should().Be("A B C ");

            var elementA = list[0];
            var elementB = list[1];
            var elementC = list[2];

            list.IndexOf(elementA).Should().Be(0);
            list.IndexOf(elementB).Should().Be(1);
            list.IndexOf(elementC).Should().Be(2);

            SyntaxNode nodeD = SyntaxFactory.ParseExpression("D ");
            SyntaxNode nodeE = SyntaxFactory.ParseExpression("E ");

            var newList = list.Add(nodeD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A B C D ");

            newList = list.AddRange(new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A B C D E ");

            newList = list.Insert(0, nodeD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("D A B C ");

            newList = list.Insert(1, nodeD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A D B C ");

            newList = list.Insert(2, nodeD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A B D C ");

            newList = list.Insert(3, nodeD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A B C D ");

            newList = list.InsertRange(0, new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("D E A B C ");

            newList = list.InsertRange(1, new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A D E B C ");

            newList = list.InsertRange(2, new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A B D E C ");

            newList = list.InsertRange(3, new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A B C D E ");

            newList = list.RemoveAt(0);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("B C ");

            newList = list.RemoveAt(list.Count - 1);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A B ");

            newList = list.Remove(elementA);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("B C ");

            newList = list.Remove(elementB);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A C ");

            newList = list.Remove(elementC);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A B ");

            newList = list.Replace(elementA, nodeD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("D B C ");

            newList = list.Replace(elementB, nodeD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("A D C ");

            newList = list.Replace(elementC, nodeD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("A B D ");

            newList = list.ReplaceRange(elementA, new[] { nodeD, nodeE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("D E B C ");

            newList = list.ReplaceRange(elementB, new[] { nodeD, nodeE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A D E C ");

            newList = list.ReplaceRange(elementC, new[] { nodeD, nodeE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A B D E ");

            newList = list.ReplaceRange(elementA, new SyntaxNode[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("B C ");

            newList = list.ReplaceRange(elementB, new SyntaxNode[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A C ");

            newList = list.ReplaceRange(elementC, new SyntaxNode[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A B ");

            list.IndexOf(nodeD).Should().Be(-1);
            FluentActions.Invoking(() => list.Insert(-1, nodeD)).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.Insert(list.Count + 1, nodeD)).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.InsertRange(-1, new[] { nodeD })).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.InsertRange(list.Count + 1, new[] { nodeD })).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.RemoveAt(-1)).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.RemoveAt(list.Count)).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.Replace(nodeD, nodeE)).Should().Throw<ArgumentException>();
            FluentActions.Invoking(() => list.ReplaceRange(nodeD, new[] { nodeE })).Should().Throw<ArgumentException>();
            FluentActions.Invoking(() => list.AddRange((IEnumerable<SyntaxNode>)null)).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => list.InsertRange(0, (IEnumerable<SyntaxNode>)null)).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => list.ReplaceRange(elementA, (IEnumerable<SyntaxNode>)null)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TestAddInsertRemoveReplaceOnEmptyList()
        {
            DoTestAddInsertRemoveReplaceOnEmptyList(SyntaxFactory.List<SyntaxNode>());
            DoTestAddInsertRemoveReplaceOnEmptyList(default(SyntaxList<SyntaxNode>));
        }

        private void DoTestAddInsertRemoveReplaceOnEmptyList(SyntaxList<SyntaxNode> list)
        {
            list.Count.Should().Be(0);

            SyntaxNode nodeD = SyntaxFactory.ParseExpression("D ");
            SyntaxNode nodeE = SyntaxFactory.ParseExpression("E ");

            var newList = list.Add(nodeD);
            newList.Count.Should().Be(1);
            newList.ToFullString().Should().Be("D ");

            newList = list.AddRange(new[] { nodeD, nodeE });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("D E ");

            newList = list.Insert(0, nodeD);
            newList.Count.Should().Be(1);
            newList.ToFullString().Should().Be("D ");

            newList = list.InsertRange(0, new[] { nodeD, nodeE });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("D E ");

            newList = list.Remove(nodeD);
            newList.Count.Should().Be(0);

            list.IndexOf(nodeD).Should().Be(-1);
            FluentActions.Invoking(() => list.RemoveAt(0)).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.Insert(1, nodeD)).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.Insert(-1, nodeD)).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.InsertRange(1, new[] { nodeD })).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.InsertRange(-1, new[] { nodeD })).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => list.Replace(nodeD, nodeE)).Should().Throw<ArgumentException>();
            FluentActions.Invoking(() => list.ReplaceRange(nodeD, new[] { nodeE })).Should().Throw<ArgumentException>();
            FluentActions.Invoking(() => list.Add(null)).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => list.AddRange((IEnumerable<SyntaxNode>)null)).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => list.Insert(0, null)).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => list.InsertRange(0, (IEnumerable<SyntaxNode>)null)).Should().Throw<ArgumentNullException>();
        }

        [Fact, WorkItem(127, "https://github.com/dotnet/roslyn/issues/127")]
        public void AddEmptySyntaxList()
        {
            var attributes = new AttributeListSyntax[0];
            var newMethodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "M");
            newMethodDeclaration.AddAttributeLists(attributes);
        }

        [Fact]
        public void AddNamespaceAttributeListsAndModifiers()
        {
            var declaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("M"));

            declaration.AttributeLists.Count == 0.Should().BeTrue();
            declaration.Modifiers.Count == 0.Should().BeTrue();

            declaration = declaration.AddAttributeLists(new[]
            {
                SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("Attr")))),
            });

            declaration.AttributeLists.Count == 1.Should().BeTrue();
            declaration.Modifiers.Count == 0.Should().BeTrue();

            declaration = declaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            declaration.AttributeLists.Count == 1.Should().BeTrue();
            declaration.Modifiers.Count == 1.Should().BeTrue();
        }

        [Fact]
        public void Extensions()
        {
            var list = SyntaxFactory.List<SyntaxNode>(
                new[] {
                    SyntaxFactory.ParseExpression("A+B"),
                    SyntaxFactory.IdentifierName("B"),
                    SyntaxFactory.ParseExpression("1") });

            list.IndexOf(SyntaxKind.AddExpression).Should().Be(0);
            list.Any(SyntaxKind.AddExpression).Should().BeTrue();

            list.IndexOf(SyntaxKind.IdentifierName).Should().Be(1);
            list.Any(SyntaxKind.IdentifierName).Should().BeTrue();

            list.IndexOf(SyntaxKind.NumericLiteralExpression).Should().Be(2);
            list.Any(SyntaxKind.NumericLiteralExpression).Should().BeTrue();

            list.IndexOf(SyntaxKind.WhereClause).Should().Be(-1);
            list.Any(SyntaxKind.WhereClause).Should().BeFalse();
        }

        [Fact]
        public void WithLotsOfChildrenTest()
        {
            var alphabet = "abcdefghijklmnopqrstuvwxyz";
            var commaSeparatedList = string.Join(",", (IEnumerable<char>)alphabet);
            var parsedArgumentList = SyntaxFactory.ParseArgumentList(commaSeparatedList);
            parsedArgumentList.Arguments.Count.Should().Be(alphabet.Length);

            for (int position = 0; position < parsedArgumentList.FullWidth; position++)
            {
                var item = ChildSyntaxList.ChildThatContainsPosition(parsedArgumentList, position);
                item.Position.Should().Be(position);
                item.Width.Should().Be(1);
                if (position % 2 == 0)
                {
                    // Even. We should get a node
                    item.IsNode.Should().BeTrue();
                    item.IsKind(SyntaxKind.Argument).Should().BeTrue();
                    string expectedArgName = ((char)('a' + (position / 2))).ToString();
                    ((ArgumentSyntax)item).Expression.ToString().Should().Be(expectedArgName);
                }
                else
                {
                    // Odd. We should get a comma
                    item.IsToken.Should().BeTrue();
                    item.IsKind(SyntaxKind.CommaToken).Should().BeTrue();
                    int expectedTokenIndex = position + 1; // + 1 because there is a (missing) OpenParen at slot 0
                    item.AsToken().Index.Should().Be(expectedTokenIndex);
                }
            }
        }
    }
}
