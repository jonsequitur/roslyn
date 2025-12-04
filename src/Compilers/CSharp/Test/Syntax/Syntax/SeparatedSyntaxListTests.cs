// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class SeparatedSyntaxListTests : CSharpTestBase
    {
        [Fact]
        public void Equality()
        {
            var node1 = SyntaxFactory.Parameter(SyntaxFactory.Identifier("a"));
            var node2 = SyntaxFactory.Parameter(SyntaxFactory.Identifier("b"));

            EqualityTesting.AssertEqual(default(SeparatedSyntaxList<CSharpSyntaxNode>), default(SeparatedSyntaxList<CSharpSyntaxNode>));

            EqualityTesting.AssertEqual(
                new SeparatedSyntaxList<CSharpSyntaxNode>(new SyntaxNodeOrTokenList(node1, 0)),
                new SeparatedSyntaxList<CSharpSyntaxNode>(new SyntaxNodeOrTokenList(node1, 0)));

            EqualityTesting.AssertEqual(
                new SeparatedSyntaxList<CSharpSyntaxNode>(new SyntaxNodeOrTokenList(node1, 0)),
                new SeparatedSyntaxList<CSharpSyntaxNode>(new SyntaxNodeOrTokenList(node1, 1)));

            EqualityTesting.AssertNotEqual(
                new SeparatedSyntaxList<CSharpSyntaxNode>(new SyntaxNodeOrTokenList(node1, 0)),
                new SeparatedSyntaxList<CSharpSyntaxNode>(new SyntaxNodeOrTokenList(node2, 0)));
        }

        [Fact]
        public void EnumeratorEquality()
        {
            Assert.Throws<NotSupportedException>(() => default(SeparatedSyntaxList<CSharpSyntaxNode>.Enumerator).GetHashCode());
            Assert.Throws<NotSupportedException>(() => default(SeparatedSyntaxList<CSharpSyntaxNode>.Enumerator).Equals(default(SeparatedSyntaxList<CSharpSyntaxNode>.Enumerator)));
        }

        [WorkItem(308077, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/308077")]
        [Fact]
        public void TestSeparatedListInsert()
        {
            var list = SyntaxFactory.SeparatedList<ExpressionSyntax>();
            var addList = list.Insert(0, SyntaxFactory.ParseExpression("x"));
            addList.ToFullString().Should().Be("x");

            var insertBefore = addList.Insert(0, SyntaxFactory.ParseExpression("y"));
            insertBefore.ToFullString().Should().Be("y,x");

            var insertAfter = addList.Insert(1, SyntaxFactory.ParseExpression("y"));
            insertAfter.ToFullString().Should().Be("x,y");

            var insertBetween = insertAfter.InsertRange(1, new[] { SyntaxFactory.ParseExpression("a"), SyntaxFactory.ParseExpression("b"), SyntaxFactory.ParseExpression("c") });
            insertBetween.ToFullString().Should().Be("x,a,b,c,y");

            // inserting after a single line comment keeps separator with previous item
            var argsWithComment = SyntaxFactory.ParseArgumentList(@"(a, // a is good
b // b is better
)").Arguments;
            var insertAfterComment = argsWithComment.Insert(1, SyntaxFactory.Argument(SyntaxFactory.ParseExpression("c")));
            insertAfterComment.ToFullString().Should().Be(@"a, // a is good
c,b // b is better
");

            // inserting after a end of line trivia keeps separator with previous item
            var argsWithEOL = SyntaxFactory.ParseArgumentList(@"(a,
b)").Arguments;
            var insertAfterEOL = argsWithEOL.Insert(1, SyntaxFactory.Argument(SyntaxFactory.ParseExpression("c")));
            insertAfterEOL.ToFullString().Should().Be(@"a,
c,b");

            // inserting after any other trivia keeps separator with following item
            var argsWithMultiLineComment = SyntaxFactory.ParseArgumentList("(a, /* b is best */ b)").Arguments;
            var insertBeforeMultiLineComment = argsWithMultiLineComment.Insert(1, SyntaxFactory.Argument(SyntaxFactory.ParseExpression("c")));
            insertBeforeMultiLineComment.ToFullString().Should().Be("a,c, /* b is best */ b");
        }

        [Fact]
        public void TestAddInsertRemove()
        {
            var list = SyntaxFactory.SeparatedList<SyntaxNode>(
                new[] {
                    SyntaxFactory.ParseExpression("A"),
                    SyntaxFactory.ParseExpression("B"),
                    SyntaxFactory.ParseExpression("C") });

            list.Count.Should().Be(3);
            list[0].ToString().Should().Be("A");
            list[1].ToString().Should().Be("B");
            list[2].ToString().Should().Be("C");
            list.ToFullString().Should().Be("A,B,C");

            var elementA = list[0];
            var elementB = list[1];
            var elementC = list[2];

            list.IndexOf(elementA).Should().Be(0);
            list.IndexOf(elementB).Should().Be(1);
            list.IndexOf(elementC).Should().Be(2);

            SyntaxNode nodeD = SyntaxFactory.ParseExpression("D");
            SyntaxNode nodeE = SyntaxFactory.ParseExpression("E");

            var newList = list.Add(nodeD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A,B,C,D");

            newList = list.AddRange(new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A,B,C,D,E");

            newList = list.Insert(0, nodeD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("D,A,B,C");

            newList = list.InsertRange(0, new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("D,E,A,B,C");

            newList = list.Insert(1, nodeD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A,D,B,C");

            newList = list.Insert(2, nodeD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A,B,D,C");

            newList = list.Insert(3, nodeD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A,B,C,D");

            newList = list.InsertRange(0, new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("D,E,A,B,C");

            newList = list.InsertRange(1, new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A,D,E,B,C");

            newList = list.InsertRange(2, new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A,B,D,E,C");

            newList = list.InsertRange(3, new[] { nodeD, nodeE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A,B,C,D,E");

            newList = list.RemoveAt(0);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("B,C");

            newList = list.RemoveAt(list.Count - 1);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A,B");

            newList = list.Remove(elementA);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("B,C");

            newList = list.Remove(elementB);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A,C");

            newList = list.Remove(elementC);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A,B");

            newList = list.Replace(elementA, nodeD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("D,B,C");

            newList = list.Replace(elementB, nodeD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("A,D,C");

            newList = list.Replace(elementC, nodeD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("A,B,D");

            newList = list.ReplaceRange(elementA, new[] { nodeD, nodeE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("D,E,B,C");

            newList = list.ReplaceRange(elementB, new[] { nodeD, nodeE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A,D,E,C");

            newList = list.ReplaceRange(elementC, new[] { nodeD, nodeE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A,B,D,E");

            newList = list.ReplaceRange(elementA, new SyntaxNode[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("B,C");

            newList = list.ReplaceRange(elementB, new SyntaxNode[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A,C");

            newList = list.ReplaceRange(elementC, new SyntaxNode[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A,B");

            list.IndexOf(nodeD).Should().Be(-1);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, nodeD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(list.Count + 1, nodeD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(-1, new[] { nodeD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(list.Count + 1, new[] { nodeD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(list.Count + 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Replace(nodeD, nodeE));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.ReplaceRange(nodeD, new[] { nodeE }));
            Assert.Throws<ArgumentNullException>(() => list.AddRange((IEnumerable<SyntaxNode>)null));
            Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, (IEnumerable<SyntaxNode>)null));
            Assert.Throws<ArgumentNullException>(() => list.ReplaceRange(elementA, (IEnumerable<SyntaxNode>)null));
        }

        [Fact]
        public void TestAddInsertRemoveOnEmptyList()
        {
            DoTestAddInsertRemoveOnEmptyList(SyntaxFactory.SeparatedList<SyntaxNode>());
            DoTestAddInsertRemoveOnEmptyList(default(SeparatedSyntaxList<SyntaxNode>));
        }

        private void DoTestAddInsertRemoveOnEmptyList(SeparatedSyntaxList<SyntaxNode> list)
        {
            list.Count.Should().Be(0);

            SyntaxNode nodeD = SyntaxFactory.ParseExpression("D");
            SyntaxNode nodeE = SyntaxFactory.ParseExpression("E");

            var newList = list.Add(nodeD);
            newList.Count.Should().Be(1);
            newList.ToFullString().Should().Be("D");

            newList = list.AddRange(new[] { nodeD, nodeE });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("D,E");

            newList = list.Insert(0, nodeD);
            newList.Count.Should().Be(1);
            newList.ToFullString().Should().Be("D");

            newList = list.InsertRange(0, new[] { nodeD, nodeE });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("D,E");

            newList = list.Remove(nodeD);
            newList.Count.Should().Be(0);

            list.IndexOf(nodeD).Should().Be(-1);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(1, nodeD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, nodeD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(1, new[] { nodeD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(-1, new[] { nodeD }));
            Assert.Throws<ArgumentNullException>(() => list.Add(null));
            Assert.Throws<ArgumentNullException>(() => list.AddRange((IEnumerable<SyntaxNode>)null));
            Assert.Throws<ArgumentNullException>(() => list.Insert(0, null));
            Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, (IEnumerable<SyntaxNode>)null));
        }

        [Fact]
        public void Extensions()
        {
            var list = SyntaxFactory.SeparatedList<SyntaxNode>(
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
        [WorkItem(2630, "https://github.com/dotnet/roslyn/issues/2630")]
        public void ReplaceSeparator()
        {
            var list = SyntaxFactory.SeparatedList<SyntaxNode>(
                new[] {
                    SyntaxFactory.IdentifierName("A"),
                    SyntaxFactory.IdentifierName("B"),
                    SyntaxFactory.IdentifierName("C"),
                });

            var newComma = SyntaxFactory.Token(
                SyntaxFactory.TriviaList(SyntaxFactory.Space),
                SyntaxKind.CommaToken,
                SyntaxFactory.TriviaList());
            var newList = list.ReplaceSeparator(
                list.GetSeparator(1),
                newComma);
            newList.Count.Should().Be(3);
            newList.SeparatorCount.Should().Be(2);
            newList.GetSeparator(1).GetLeadingTrivia().Count.Should().Be(1);
        }
    }
}
