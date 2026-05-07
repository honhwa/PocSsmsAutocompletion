using Microsoft.VisualStudio.Text;
using Moq;
using System;

namespace SsmsAutocompletion.Tests.Helpers {

    internal static class SnapshotHelper {

        /// <summary>
        /// Creates a Moq-backed ITextSnapshot over <paramref name="text"/>.
        /// Supports: Length, indexer, GetText(int,int), GetLineFromPosition.
        /// </summary>
        internal static ITextSnapshot Make(string text) {
            var snap = new Mock<ITextSnapshot>();

            snap.Setup(s => s.Length).Returns(text.Length);
            snap.Setup(s => s[It.IsAny<int>()])
                .Returns<int>(i => text[i]);
            snap.Setup(s => s.GetText(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>((start, len) => text.Substring(start, len));

            snap.Setup(s => s.GetLineFromPosition(It.IsAny<int>()))
                .Returns<int>(pos => BuildLine(snap, text, pos));

            return snap.Object;
        }

        private static ITextSnapshotLine BuildLine(Mock<ITextSnapshot> snap, string text, int pos) {
            int safePos   = Math.Min(pos, text.Length);
            int lineNum   = 0;
            int lineStart = 0;
            for (int i = 0; i < safePos; i++) {
                if (text[i] == '\n') { lineNum++; lineStart = i + 1; }
            }

            var line = new Mock<ITextSnapshotLine>();
            line.Setup(l => l.LineNumber).Returns(lineNum);
            line.Setup(l => l.Start).Returns(new SnapshotPoint(snap.Object, lineStart));
            return line.Object;
        }
    }
}
