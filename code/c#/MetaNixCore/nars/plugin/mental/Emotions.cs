using System;
using MetaNix.nars.config;
using MetaNix.nars.control;
using System.Threading;

namespace MetaNix.nars.plugin.mental {
    // see https://github.com/opennars/opennars/blob/master/nars_core/nars/plugin/mental/Emotions.java
    /** emotional value; self-felt internal mental states; variables used to record emotional values */
    public class Emotions /*: Plugin*/ {
        public Emotions() {}

        public Emotions(float happy, float busy) {
            set(happy, busy);
        }

        public void set(float happy, float busy) {
            this.happy = happy;
            this.busy = busy;
        }
        
        public double lasthappy = -1;
        
        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/plugin/mental/Emotions.java#L112
        public void manageBusy(DerivationContext ctx) {
            lock(busyMutex) { // locking for concurrent requests
                if (!enabled) {
                    return;
                }
                if (lastbusy != -1) {
                    float frequency = -1;
                    if (busy > Parameters.BUSY_EVENT_HIGHER_THRESHOLD && lastbusy <= Parameters.BUSY_EVENT_HIGHER_THRESHOLD) {
                        frequency = 1.0f;
                    }
                    if (busy < Parameters.BUSY_EVENT_LOWER_THRESHOLD && lastbusy >= Parameters.BUSY_EVENT_LOWER_THRESHOLD) {
                        frequency = 0.0f;
                    }
                    if (frequency != -1) { // ok lets add an event now
                        // TODO< translate to C# >
                        /*commented because we have to translate this and emotions are not that important in the beginning

                        Term predicate = SetInt.make(new Term("busy"));
                        Term subject = new Term("SELF");
                        Inheritance inh = Inheritance.make(subject, predicate);
                        TruthValue truth = new TruthValue(1.0f, Parameters.DEFAULT_JUDGMENT_CONFIDENCE);
                        Sentence s = new Sentence(inh, Symbols.JUDGMENT_MARK, truth, new Stamp(ctx.memory));
                        s.stamp.setOccurrenceTime(nal.memory.time());
                        Task t = new Task(s, new BudgetValue(Parameters.DEFAULT_JUDGMENT_PRIORITY, Parameters.DEFAULT_JUDGMENT_DURABILITY, BudgetFunctions.truthToQuality(truth)));
                        ctx.addTask(t, "emotion");
                        */
                    }
                }
                lastbusy = busy;
            }
        }

        public void adjustHappy(float newValue, float weight, DerivationContext nal) {
            if (!enabled) {
                return;
            }
            //        float oldV = happyValue;
            happy += newValue * weight;
            happy /= 1.0f + weight;

            if (lasthappy != -1) {
                float frequency = -1;
                if (happy > Parameters.HAPPY_EVENT_HIGHER_THRESHOLD && lasthappy <= Parameters.HAPPY_EVENT_HIGHER_THRESHOLD) {
                    frequency = 1.0f;
                }
                if (happy < Parameters.HAPPY_EVENT_LOWER_THRESHOLD && lasthappy >= Parameters.HAPPY_EVENT_LOWER_THRESHOLD) {
                    frequency = 0.0f;
                }
            }
            lasthappy = happy;
        }

        public double lastbusy = -1;

        // commented because not used and has to be translated to C#
        /*
        public void manageBusy(DerivationContext nal) {
            if (!enabled) {
                return;
            }
            if (lastbusy != -1) {
                float frequency = -1;
                if (busy > Parameters.BUSY_EVENT_HIGHER_THRESHOLD && lastbusy <= Parameters.BUSY_EVENT_HIGHER_THRESHOLD) {
                    frequency = 1.0f;
                }
                if (busy < Parameters.BUSY_EVENT_LOWER_THRESHOLD && lastbusy >= Parameters.BUSY_EVENT_LOWER_THRESHOLD) {
                    frequency = 0.0f;
                }
                if (frequency != -1) { //ok lets add an event now
                    Term predicate = SetInt.make(new Term("busy"));
                    Term subject = new Term("SELF");
                    Inheritance inh = Inheritance.make(subject, predicate);
                    TruthValue truth = new TruthValue(1.0f, Parameters.DEFAULT_JUDGMENT_CONFIDENCE);
                    Sentence s = new Sentence(inh, Symbols.JUDGMENT_MARK, truth, new Stamp(nal.memory));
                    s.stamp.setOccurrenceTime(nal.memory.time());
                    Task t = new Task(s, new BudgetValue(Parameters.DEFAULT_JUDGMENT_PRIORITY, Parameters.DEFAULT_JUDGMENT_DURABILITY, BudgetFunctions.truthToQuality(truth)));
                    nal.addTask(t, "emotion");
                }
            }
            lastbusy = busy;
        }
        */

        public void adjustBusy(float newValue, float weight) {
            //        float oldV = busyValue;
            if (!enabled) {
                return;
            }
            busy += newValue * weight;
            busy /= (1.0f + weight);
            //        if (Math.abs(oldV - busyValue) > 0.1) {
            //            Record.append("BUSY: " + (int) (oldV*10.0) + " to " + (int) (busyValue*10.0) + "\n");
        }


        Mutex busyMutex = new Mutex();



        bool enabled = true;
        
        // uncommented because plugin system was not implemented
        /*
        @Override
        public boolean setEnabled(NAR n, boolean enabled) {
            this.enabled = enabled;
            return enabled;
        }
         */

        float happy; /** average desire-value */
        float busy; /** average priority */

    }
}
