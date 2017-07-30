using AiThisAndThat.patternMatching;
using System;
using System.Diagnostics;

namespace AiThisAndThat.framework.pattern.withDecoration {
    // used to manipulate (add, remove, move, ...) a pattern
    static class PatternManipulation {
        // appends target to source
        public static void append(Pattern<Decoration> target, Pattern<Decoration> appended) {
            Debug.Assert(target.isBranch, "Must be branch!");

            var newReferenced = new  Pattern<AiThisAndThat.patternMatching.Decoration>[target.referenced.Length+1];
            Array.Copy(target.referenced, newReferenced, target.referenced.Length);
            newReferenced[newReferenced.Length-1] = appended;
            target.referenced = newReferenced;

            appended.parent = target;
        }
    }
}
