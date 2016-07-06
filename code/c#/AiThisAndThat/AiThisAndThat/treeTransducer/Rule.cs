using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// see "Survey: Tree Transducers in Machine Translation"
// http://www.ims.uni-stuttgart.de/institut/mitarbeiter/maletti/pub/mal10f.pdf
// for an overview of tree transducers
namespace treeTransducer {
    class Rule<TreeElementType> {
        public Rule(TreeElementType matching, TreeElementType rewriteTarget) {
            this.matching = matching;
            this.rewriteTarget = rewriteTarget;
        }

        public TreeElementType matching;
        public TreeElementType rewriteTarget;
    }
}
