using System;


namespace MetaNix {
    struct VariantRange {
        public Variant min, max; // range

        public bool isInRange(Variant value) {
            Ensure.ensure(value.type == min.type && value.type == max.type);
            Variant.EnumType type = value.type;
            switch (type) {
                case Variant.EnumType.FLOAT:
                return min.valueFloat <= value.valueFloat && value.valueFloat <= max.valueFloat;

                case Variant.EnumType.INT:
                return min.valueInt <= value.valueInt && value.valueInt <= max.valueInt;

                default:
                throw new Exception(); // soft exception which can be catched by our VM
            }
        }
    }
}
