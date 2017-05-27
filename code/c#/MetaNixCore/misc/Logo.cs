namespace MetaNix.misc {
    public sealed class Logo {
        public static string getLogo() {
            // text created using http://patorjk.com/software/taag/#p=display&f=Slant&t=MN

            return
            "              +##+\n" +
            "            ########\n" +
            "           +########+\n" +
            "    __  ___##########\n" +
            "   /  |/  / |#/ /###+\n" +
            "  / /| _ / /  |/ /####\n" +
            " / /  / / /|  /##+\n" +
            "/ _ /  / _ / _ / | _ /\n";
        }

        public static string getDescriptiveInformation() {
            return
            "MetaNix r0\n\n" +
            "Buildtime: < void >\n" +
            "Node     : DEFAULT\n" +
            "Time     : < now >\n" +
            "Status   : ok, up\n";
        }
    }
}
