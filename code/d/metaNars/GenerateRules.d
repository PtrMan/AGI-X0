import std.format : format;
import std.stdio : writeln;

void main() {
	const string formatMe = 
"""
UnifiedTerm ruletable%s(UnifiedTerm a, TermConcept aTermConcept, UnifiedTerm b, TermConcept bTermConcept, float k) {
  UnifiedTerm result;
  result.left = %s;
  result.right = %s;
  
  result.flagToLeftInheritance = %s;
  result.flagToRightInheritance = %s;
  
  result.derivationDescriptor = DerivationDescriptor::create(a, aTermConcept, b, bTermConcept);

  result.frequencyCertainty = f%s(a.frequencyCertainty, b.frequencyCertainty, k);
  
  return result;
}

""";
	
	struct RuleDescriptor {
		final this(string ruleName, string leftSource, string rightSource, bool flagToLeftInheritance, bool flagToRightInheritance, string rule) {
			this.ruleName = ruleName;
			this.leftSource = leftSource;
			this.rightSource = rightSource;
			this.flagToLeftInheritance = flagToLeftInheritance;
			this.flagToRightInheritance = flagToRightInheritance;
			this.rule = rule;
		}

		string ruleName;
		string leftSource;
		string rightSource;
		bool flagToLeftInheritance;
		bool flagToRightInheritance;
		string rule;
	};

	
	RuleDescriptor[] ruleDescriptors;
	// rigid flexibility, page 84
	// NAL-2 revision

	ruleDescriptors ~= RuleDescriptor("nal2Revision00", "a.left", "b.right", false, true, "Revision");
	ruleDescriptors ~= RuleDescriptor("nal2Revision10", "a.right", "b.right", true, true, "Revision");
	ruleDescriptors ~= RuleDescriptor("nal2Revision20", "a.left", "b.right", true, true, "Revision");

	ruleDescriptors ~= RuleDescriptor("nal2Revision01", "a.left", "a.right", true, true, "Revision");
	ruleDescriptors ~= RuleDescriptor("nal2Revision11", "a.left", "a.right", false, true, "Revision");
	ruleDescriptors ~= RuleDescriptor("nal2Revision21", "a.left", "a.right", true, true, "Revision");

	ruleDescriptors ~= RuleDescriptor("nal2Revision02", "b.left", "b.right", true, true, "Revision");
	ruleDescriptors ~= RuleDescriptor("nal2Revision12", "b.left", "b.right", true, true, "Revision");
	ruleDescriptors ~= RuleDescriptor("nal2Revision22", "b.left", "b.right", true, true, "Revision");

	// rigid flexibility, page 84
	// NAL-2 similarity, comparision, analogy, deducation
	ruleDescriptors ~= RuleDescriptor("nal2Comparision10", "b.left", "a.left", true, true, "Comparision");
	ruleDescriptors ~= RuleDescriptor("nal2Analogy20", "b.left", "a.right", false, true, "AnalogyTick");
	
	ruleDescriptors ~= RuleDescriptor("nal2Comparision01", "b.right", "a.right", true, true, "Comparision");
	ruleDescriptors ~= RuleDescriptor("nal2Analogy21", "a.right", "b.right", false, true, "AnalogyTick");

	ruleDescriptors ~= RuleDescriptor("nal2Analogy02", "b.left", "a.right", false, true, "Analogy");
	ruleDescriptors ~= RuleDescriptor("nal2Analogy12", "a.left", "b.left", false, true, "Analogy");
	ruleDescriptors ~= RuleDescriptor("nal2Deduction222", "b.left", "a.right", false, true, "Deduction2");



	foreach( iterationRuleDescriptor; ruleDescriptors ) {
		
		string formatedString = format(formatMe, iterationRuleDescriptor.ruleName, iterationRuleDescriptor.leftSource, iterationRuleDescriptor.rightSource, iterationRuleDescriptor.flagToLeftInheritance, iterationRuleDescriptor.flagToRightInheritance, iterationRuleDescriptor.rule);
		writeln(formatedString);
	}
}
