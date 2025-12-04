// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using System;
using Xunit;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp
{
    public class SyntaxTokenListTests : CSharpTestBase
    {
        [Fact]
        public void TestEquality()
        {
            var node1 = SyntaxFactory.ReturnStatement();

            EqualityTesting.AssertEqual(default(SyntaxTokenList), default(SyntaxTokenList));

            EqualityTesting.AssertEqual(new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 0, 0), new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 0, 0));

            // index is considered
            EqualityTesting.AssertNotEqual(new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 0, 1), new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 0, 0));

            // position not considered:
            EqualityTesting.AssertEqual(new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 1, 0), new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 0, 0));
        }

        [Fact]
        public void TestReverse_Equality()
        {
            var node1 = SyntaxFactory.ReturnStatement();

            EqualityTesting.AssertEqual(default(SyntaxTokenList).Reverse(), default(SyntaxTokenList).Reverse());

            EqualityTesting.AssertEqual(new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 0, 0).Reverse(), new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 0, 0).Reverse());

            // index is considered
            EqualityTesting.AssertNotEqual(new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 0, 1).Reverse(), new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 0, 0).Reverse());

            // position not considered:
            EqualityTesting.AssertEqual(new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 1, 0).Reverse(), new SyntaxTokenList(node1, node1.ReturnKeyword.Node, 0, 0).Reverse());
        }

        [Fact]
        public void TestEnumeratorEquality()
        {
            Assert.Throws<NotSupportedException>(() => default(SyntaxTokenList.Enumerator).GetHashCode());
            Assert.Throws<NotSupportedException>(() => default(SyntaxTokenList.Enumerator).Equals(default(SyntaxTokenList.Enumerator)));
            Assert.Throws<NotSupportedException>(() => default(SyntaxTokenList.Reversed.Enumerator).GetHashCode());
            Assert.Throws<NotSupportedException>(() => default(SyntaxTokenList.Reversed.Enumerator).Equals(default(SyntaxTokenList.Reversed.Enumerator)));
        }

        [Fact]
        public void TestAddInsertRemoveReplace()
        {
            var list = SyntaxFactory.TokenList(SyntaxFactory.ParseToken("A "), SyntaxFactory.ParseToken("B "), SyntaxFactory.ParseToken("C "));

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

            var tokenD = SyntaxFactory.ParseToken("D ");
            var tokenE = SyntaxFactory.ParseToken("E ");

            var newList = list.Add(tokenD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A B C D ");

            newList = list.AddRange(new[] { tokenD, tokenE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A B C D E ");

            newList = list.Insert(0, tokenD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("D A B C ");

            newList = list.Insert(1, tokenD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A D B C ");

            newList = list.Insert(2, tokenD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A B D C ");

            newList = list.Insert(3, tokenD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A B C D ");

            newList = list.InsertRange(0, new[] { tokenD, tokenE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("D E A B C ");

            newList = list.InsertRange(1, new[] { tokenD, tokenE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A D E B C ");

            newList = list.InsertRange(2, new[] { tokenD, tokenE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A B D E C ");

            newList = list.InsertRange(3, new[] { tokenD, tokenE });
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

            newList = list.Replace(elementA, tokenD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("D B C ");

            newList = list.Replace(elementB, tokenD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("A D C ");

            newList = list.Replace(elementC, tokenD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("A B D ");

            newList = list.ReplaceRange(elementA, new[] { tokenD, tokenE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("D E B C ");

            newList = list.ReplaceRange(elementB, new[] { tokenD, tokenE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A D E C ");

            newList = list.ReplaceRange(elementC, new[] { tokenD, tokenE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A B D E ");

            newList = list.ReplaceRange(elementA, new SyntaxToken[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("B C ");

            newList = list.ReplaceRange(elementB, new SyntaxToken[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A C ");

            newList = list.ReplaceRange(elementC, new SyntaxToken[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A B ");

            list.IndexOf(tokenD).Should().Be(-1);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, tokenD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(list.Count + 1, tokenD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(-1, new[] { tokenD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(list.Count + 1, new[] { tokenD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(list.Count));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Add(default(SyntaxToken)));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(0, default(SyntaxToken)));
            Assert.Throws<ArgumentNullException>(() => list.AddRange((IEnumerable<SyntaxToken>)null));
            Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, (IEnumerable<SyntaxToken>)null));
            Assert.Throws<ArgumentNullException>(() => list.ReplaceRange(elementA, (IEnumerable<SyntaxToken>)null));
        }

        [Fact]
        public void TestAddInsertRemoveReplaceOnEmptyList()
        {
            DoTestAddInsertRemoveReplaceOnEmptyList(SyntaxFactory.TokenList());
            DoTestAddInsertRemoveReplaceOnEmptyList(default(SyntaxTokenList));
        }

        private void DoTestAddInsertRemoveReplaceOnEmptyList(SyntaxTokenList list)
        {
            list.Count.Should().Be(0);

            var tokenD = SyntaxFactory.ParseToken("D ");
            var tokenE = SyntaxFactory.ParseToken("E ");

            var newList = list.Add(tokenD);
            newList.Count.Should().Be(1);
            newList.ToFullString().Should().Be("D ");

            newList = list.AddRange(new[] { tokenD, tokenE });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("D E ");

            newList = list.Insert(0, tokenD);
            newList.Count.Should().Be(1);
            newList.ToFullString().Should().Be("D ");

            newList = list.InsertRange(0, new[] { tokenD, tokenE });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("D E ");

            newList = list.Remove(tokenD);
            newList.Count.Should().Be(0);

            list.IndexOf(tokenD).Should().Be(-1);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(1, tokenD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, tokenD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(-1, new[] { tokenD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(list.Count + 1, new[] { tokenD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Replace(tokenD, tokenE));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.ReplaceRange(tokenD, new[] { tokenE }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Add(default(SyntaxToken)));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(0, default(SyntaxToken)));
            Assert.Throws<ArgumentNullException>(() => list.AddRange((IEnumerable<SyntaxToken>)null));
            Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, (IEnumerable<SyntaxToken>)null));
        }

        [Fact]
        public void Extensions()
        {
            var list = SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.SizeOfKeyword),
                SyntaxFactory.Literal("x"),
                SyntaxFactory.Token(SyntaxKind.DotToken));

            list.IndexOf(SyntaxKind.SizeOfKeyword).Should().Be(0);
            list.Any(SyntaxKind.SizeOfKeyword).Should().BeTrue();

            list.IndexOf(SyntaxKind.StringLiteralToken).Should().Be(1);
            list.Any(SyntaxKind.StringLiteralToken).Should().BeTrue();

            list.IndexOf(SyntaxKind.DotToken).Should().Be(2);
            list.Any(SyntaxKind.DotToken).Should().BeTrue();

            list.IndexOf(SyntaxKind.NullKeyword).Should().Be(-1);
            list.Any(SyntaxKind.NullKeyword).Should().BeFalse();
        }
    }
}
