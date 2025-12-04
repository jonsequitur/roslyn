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
    public class SyntaxNodeOrTokenListTests : CSharpTestBase
    {
        [Fact]
        public void Equality()
        {
            var node1 = SyntaxFactory.Parameter(SyntaxFactory.Identifier("a"));
            var node2 = SyntaxFactory.Parameter(SyntaxFactory.Identifier("b"));

            EqualityTesting.AssertEqual(default(SeparatedSyntaxList<CSharpSyntaxNode>), default(SeparatedSyntaxList<CSharpSyntaxNode>));
            EqualityTesting.AssertEqual(new SyntaxNodeOrTokenList(node1, 0), new SyntaxNodeOrTokenList(node1, 0));
            EqualityTesting.AssertEqual(new SyntaxNodeOrTokenList(node1, 0), new SyntaxNodeOrTokenList(node1, 1));
            EqualityTesting.AssertNotEqual(new SyntaxNodeOrTokenList(node1, 0), new SyntaxNodeOrTokenList(node2, 0));
        }

        [Fact]
        public void EnumeratorEquality()
        {
            Assert.Throws<NotSupportedException>(() => default(SyntaxNodeOrTokenList.Enumerator).GetHashCode());
            Assert.Throws<NotSupportedException>(() => default(SyntaxNodeOrTokenList.Enumerator).Equals(default(SyntaxNodeOrTokenList.Enumerator)));
        }

        [Fact]
        public void TestAddInsertRemove()
        {
            var list = SyntaxFactory.NodeOrTokenList(SyntaxFactory.ParseToken("A "), SyntaxFactory.ParseToken("B "), SyntaxFactory.ParseToken("C "));

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

            SyntaxNodeOrToken tokenD = SyntaxFactory.ParseToken("D ");
            SyntaxNodeOrToken nameE = SyntaxFactory.ParseExpression("E ");

            var newList = list.Add(tokenD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A B C D ");

            newList = list.AddRange(new[] { tokenD, nameE });
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

            newList = list.InsertRange(0, new[] { tokenD, nameE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("D E A B C ");

            newList = list.InsertRange(1, new[] { tokenD, nameE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A D E B C ");

            newList = list.InsertRange(2, new[] { tokenD, nameE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("A B D E C ");

            newList = list.InsertRange(3, new[] { tokenD, nameE });
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

            newList = list.ReplaceRange(elementA, new[] { tokenD, nameE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("D E B C ");

            newList = list.ReplaceRange(elementB, new[] { tokenD, nameE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A D E C ");

            newList = list.ReplaceRange(elementC, new[] { tokenD, nameE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("A B D E ");

            newList = list.ReplaceRange(elementA, new SyntaxNodeOrToken[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("B C ");

            newList = list.ReplaceRange(elementB, new SyntaxNodeOrToken[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A C ");

            newList = list.ReplaceRange(elementC, new SyntaxNodeOrToken[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("A B ");

            list.IndexOf(tokenD).Should().Be(-1);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, tokenD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(list.Count + 1, tokenD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(-1, new[] { tokenD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(list.Count + 1, new[] { tokenD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(list.Count));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Replace(tokenD, nameE));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.ReplaceRange(tokenD, new[] { nameE }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Add(default(SyntaxNodeOrToken)));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(0, default(SyntaxNodeOrToken)));
            Assert.Throws<ArgumentNullException>(() => list.AddRange((IEnumerable<SyntaxNodeOrToken>)null));
            Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, (IEnumerable<SyntaxNodeOrToken>)null));
            Assert.Throws<ArgumentNullException>(() => list.ReplaceRange(elementA, (IEnumerable<SyntaxNodeOrToken>)null));
        }

        [Fact]
        public void TestAddInsertRemoveReplaceOnEmptyList()
        {
            DoTestAddInsertRemoveReplaceOnEmptyList(SyntaxFactory.NodeOrTokenList());
            DoTestAddInsertRemoveReplaceOnEmptyList(default(SyntaxNodeOrTokenList));
        }

        private void DoTestAddInsertRemoveReplaceOnEmptyList(SyntaxNodeOrTokenList list)
        {
            list.Count.Should().Be(0);

            SyntaxNodeOrToken tokenD = SyntaxFactory.ParseToken("D ");
            SyntaxNodeOrToken nodeE = SyntaxFactory.ParseExpression("E ");

            var newList = list.Add(tokenD);
            newList.Count.Should().Be(1);
            newList.ToFullString().Should().Be("D ");

            newList = list.AddRange(new[] { tokenD, nodeE });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("D E ");

            newList = list.Insert(0, tokenD);
            newList.Count.Should().Be(1);
            newList.ToFullString().Should().Be("D ");

            newList = list.InsertRange(0, new[] { tokenD, nodeE });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("D E ");

            newList = list.Remove(tokenD);
            newList.Count.Should().Be(0);

            list.IndexOf(tokenD).Should().Be(-1);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(1, tokenD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, tokenD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(1, new[] { tokenD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(-1, new[] { tokenD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Add(default(SyntaxNodeOrToken)));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(0, default(SyntaxNodeOrToken)));
            Assert.Throws<ArgumentNullException>(() => list.AddRange((IEnumerable<SyntaxNodeOrToken>)null));
            Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, (IEnumerable<SyntaxNodeOrToken>)null));
        }
    }
}
