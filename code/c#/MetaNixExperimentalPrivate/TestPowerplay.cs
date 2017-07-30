using MetaNix.schmidhuber.powerplay;

namespace MetaNixExperimentalPrivate {
    public static class TestPowerplay {
        public static void test() {
            PowerplayMakeParameters powerplayMakeParameters = new PowerplayMakeParameters();
            powerplayMakeParameters.context = new Environment2dPowerplayContext();

            Powerplay powerplay = Powerplay.make(powerplayMakeParameters);
            powerplay.run();
        }
    }
}
