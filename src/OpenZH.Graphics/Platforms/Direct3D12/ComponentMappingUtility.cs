namespace OpenZH.Graphics.Platforms.Direct3D12
{
    internal static class ComponentMappingUtility
    {
        public const int ComponentMappingMask = 0x7;

        public const int ComponentMappingShift = 3;

        public const int ComponentMappingAlwaysSetBitAvoidingZeromemMistakes = (1 << (ComponentMappingShift * 4));

        public static int ComponentMapping(int src0, int src1, int src2, int src3) => 
            ((src0) & ComponentMappingMask)
            | (((src1) & ComponentMappingMask) << ComponentMappingShift)
            | (((src2) & ComponentMappingMask) << (ComponentMappingShift * 2))
            | (((src3) & ComponentMappingMask) << (ComponentMappingShift * 3))
            | ComponentMappingAlwaysSetBitAvoidingZeromemMistakes;

        public static int DefaultComponentMapping()
        {
            return ComponentMapping(0, 1, 2, 3);
        }

        public static int ComponentMapping(int ComponentToExtract, int Mapping)
        {
            return Mapping >> (ComponentMappingShift * ComponentToExtract) & ComponentMappingMask;
        }
    }
}
