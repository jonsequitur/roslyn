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

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class SyntaxTriviaListTests : CSharpTestBase
    {
        [Fact]
        public void Equality()
        {
            var node1 = SyntaxFactory.Token(SyntaxKind.AbstractKeyword);
            var node2 = SyntaxFactory.Token(SyntaxKind.VirtualKeyword);

            EqualityTesting.AssertEqual(default(SyntaxTriviaList), default(SyntaxTriviaList));
            EqualityTesting.AssertEqual(new SyntaxTriviaList(node1, node1.Node, 0, 0), new SyntaxTriviaList(node1, node1.Node, 0, 0));
            EqualityTesting.AssertNotEqual(new SyntaxTriviaList(node1, node1.Node, 0, 1), new SyntaxTriviaList(node1, node1.Node, 0, 0));
            EqualityTesting.AssertNotEqual(new SyntaxTriviaList(node1, node2.Node, 0, 0), new SyntaxTriviaList(node1, node1.Node, 0, 0));
            EqualityTesting.AssertNotEqual(new SyntaxTriviaList(node2, node1.Node, 0, 0), new SyntaxTriviaList(node1, node1.Node, 0, 0));

            // position not considered:
            EqualityTesting.AssertEqual(new SyntaxTriviaList(node1, node1.Node, 1, 0), new SyntaxTriviaList(node1, node1.Node, 0, 0));
        }

        [Fact]
        public void Reverse_Equality()
        {
            var node1 = SyntaxFactory.Token(SyntaxKind.AbstractKeyword);
            var node2 = SyntaxFactory.Token(SyntaxKind.VirtualKeyword);

            EqualityTesting.AssertEqual(default(SyntaxTriviaList.Reversed), default(SyntaxTriviaList.Reversed));
            EqualityTesting.AssertEqual(new SyntaxTriviaList(node1, node1.Node, 0, 0).Reverse(), new SyntaxTriviaList(node1, node1.Node, 0, 0).Reverse());
            EqualityTesting.AssertNotEqual(new SyntaxTriviaList(node1, node1.Node, 0, 1).Reverse(), new SyntaxTriviaList(node1, node1.Node, 0, 0).Reverse());
            EqualityTesting.AssertNotEqual(new SyntaxTriviaList(node1, node2.Node, 0, 0).Reverse(), new SyntaxTriviaList(node1, node1.Node, 0, 0).Reverse());
            EqualityTesting.AssertNotEqual(new SyntaxTriviaList(node2, node1.Node, 0, 0).Reverse(), new SyntaxTriviaList(node1, node1.Node, 0, 0).Reverse());

            // position not considered:
            EqualityTesting.AssertEqual(new SyntaxTriviaList(node1, node1.Node, 1, 0).Reverse(), new SyntaxTriviaList(node1, node1.Node, 0, 0).Reverse());
        }

        [Fact]
        public void TestAddInsertRemoveReplace()
        {
            var list = SyntaxFactory.ParseLeadingTrivia("/*A*//*B*//*C*/");

            list.Count.Should().Be(3);
            list[0].ToString().Should().Be("/*A*/");
            list[1].ToString().Should().Be("/*B*/");
            list[2].ToString().Should().Be("/*C*/");
            list.ToFullString().Should().Be("/*A*//*B*//*C*/");

            var elementA = list[0];
            var elementB = list[1];
            var elementC = list[2];

            list.IndexOf(elementA).Should().Be(0);
            list.IndexOf(elementB).Should().Be(1);
            list.IndexOf(elementC).Should().Be(2);

            var triviaD = SyntaxFactory.ParseLeadingTrivia("/*D*/")[0];
            var triviaE = SyntaxFactory.ParseLeadingTrivia("/*E*/")[0];

            var newList = list.Add(triviaD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("/*A*//*B*//*C*//*D*/");

            newList = list.AddRange(new[] { triviaD, triviaE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("/*A*//*B*//*C*//*D*//*E*/");

            newList = list.Insert(0, triviaD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("/*D*//*A*//*B*//*C*/");

            newList = list.Insert(1, triviaD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("/*A*//*D*//*B*//*C*/");

            newList = list.Insert(2, triviaD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("/*A*//*B*//*D*//*C*/");

            newList = list.Insert(3, triviaD);
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("/*A*//*B*//*C*//*D*/");

            newList = list.InsertRange(0, new[] { triviaD, triviaE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("/*D*//*E*//*A*//*B*//*C*/");

            newList = list.InsertRange(1, new[] { triviaD, triviaE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("/*A*//*D*//*E*//*B*//*C*/");

            newList = list.InsertRange(2, new[] { triviaD, triviaE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("/*A*//*B*//*D*//*E*//*C*/");

            newList = list.InsertRange(3, new[] { triviaD, triviaE });
            newList.Count.Should().Be(5);
            newList.ToFullString().Should().Be("/*A*//*B*//*C*//*D*//*E*/");

            newList = list.RemoveAt(0);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("/*B*//*C*/");

            newList = list.RemoveAt(list.Count - 1);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("/*A*//*B*/");

            newList = list.Remove(elementA);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("/*B*//*C*/");

            newList = list.Remove(elementB);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("/*A*//*C*/");

            newList = list.Remove(elementC);
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("/*A*//*B*/");

            newList = list.Replace(elementA, triviaD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("/*D*//*B*//*C*/");

            newList = list.Replace(elementB, triviaD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("/*A*//*D*//*C*/");

            newList = list.Replace(elementC, triviaD);
            newList.Count.Should().Be(3);
            newList.ToFullString().Should().Be("/*A*//*B*//*D*/");

            newList = list.ReplaceRange(elementA, new[] { triviaD, triviaE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("/*D*//*E*//*B*//*C*/");

            newList = list.ReplaceRange(elementB, new[] { triviaD, triviaE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("/*A*//*D*//*E*//*C*/");

            newList = list.ReplaceRange(elementC, new[] { triviaD, triviaE });
            newList.Count.Should().Be(4);
            newList.ToFullString().Should().Be("/*A*//*B*//*D*//*E*/");

            newList = list.ReplaceRange(elementA, new SyntaxTrivia[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("/*B*//*C*/");

            newList = list.ReplaceRange(elementB, new SyntaxTrivia[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("/*A*//*C*/");

            newList = list.ReplaceRange(elementC, new SyntaxTrivia[] { });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("/*A*//*B*/");

            list.IndexOf(triviaD).Should().Be(-1);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, triviaD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(list.Count + 1, triviaD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(-1, new[] { triviaD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(list.Count + 1, new[] { triviaD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(list.Count));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Add(default(SyntaxTrivia)));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(0, default(SyntaxTrivia)));
            Assert.Throws<ArgumentNullException>(() => list.AddRange((IEnumerable<SyntaxTrivia>)null));
            Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, (IEnumerable<SyntaxTrivia>)null));
            Assert.Throws<ArgumentNullException>(() => list.ReplaceRange(elementA, (IEnumerable<SyntaxTrivia>)null));
        }

        [Fact]
        public void TestAddInsertRemoveReplaceOnEmptyList()
        {
            DoTestAddInsertRemoveReplaceOnEmptyList(SyntaxFactory.ParseLeadingTrivia("/*A*/").RemoveAt(0));
            DoTestAddInsertRemoveReplaceOnEmptyList(default(SyntaxTriviaList));
        }

        private void DoTestAddInsertRemoveReplaceOnEmptyList(SyntaxTriviaList list)
        {
            list.Count.Should().Be(0);

            var triviaD = SyntaxFactory.ParseLeadingTrivia("/*D*/")[0];
            var triviaE = SyntaxFactory.ParseLeadingTrivia("/*E*/")[0];

            var newList = list.Add(triviaD);
            newList.Count.Should().Be(1);
            newList.ToFullString().Should().Be("/*D*/");

            newList = list.AddRange(new[] { triviaD, triviaE });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("/*D*//*E*/");

            newList = list.Insert(0, triviaD);
            newList.Count.Should().Be(1);
            newList.ToFullString().Should().Be("/*D*/");

            newList = list.InsertRange(0, new[] { triviaD, triviaE });
            newList.Count.Should().Be(2);
            newList.ToFullString().Should().Be("/*D*//*E*/");

            newList = list.Remove(triviaD);
            newList.Count.Should().Be(0);

            list.IndexOf(triviaD).Should().Be(-1);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(1, triviaD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, triviaD));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(1, new[] { triviaD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(-1, new[] { triviaD }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Replace(triviaD, triviaE));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.ReplaceRange(triviaD, new[] { triviaE }));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Add(default(SyntaxTrivia)));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(0, default(SyntaxTrivia)));
            Assert.Throws<ArgumentNullException>(() => list.AddRange((IEnumerable<SyntaxTrivia>)null));
            Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, (IEnumerable<SyntaxTrivia>)null));
        }

        [Fact]
        public void Extensions()
        {
            var list = SyntaxFactory.ParseLeadingTrivia("/*A*//*B*//*C*/");

            list.IndexOf(SyntaxKind.MultiLineCommentTrivia).Should().Be(0);
            list.Any(SyntaxKind.MultiLineCommentTrivia).Should().BeTrue();

            list.IndexOf(SyntaxKind.SingleLineCommentTrivia).Should().Be(-1);
            list.Any(SyntaxKind.SingleLineCommentTrivia).Should().BeFalse();
        }
    }
}
