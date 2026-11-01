namespace BBQKing.EditorTools
{
    /// <summary>
    /// Turns a position into a repeatable, well-scrambled number. Used to scatter details in the
    /// placeholder art and the map so they look random but come out the same on every build.
    /// Simple multiply-and-XOR formulas leave visible stripes; this one mixes the bits thoroughly.
    /// </summary>
    public static class PlaceholderHash
    {
        public static uint Mix(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)x * 0x8da6b343u + (uint)y * 0xd8163841u + (uint)seed * 0xcb1ab31fu;
                // Finishing step from the MurmurHash3 algorithm: spreads every input bit across the result.
                h ^= h >> 16; h *= 0x85ebca6bu;
                h ^= h >> 13; h *= 0xc2b2ae35u;
                h ^= h >> 16;
                return h;
            }
        }
    }
}
