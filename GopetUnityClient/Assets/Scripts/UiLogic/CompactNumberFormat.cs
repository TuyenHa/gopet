namespace Gopet.UiLogic
{
    public static class CompactNumberFormat
    {
        public static string Format(long value)
        {
            if (value < 0) return $"-{Format(-value)}";
            if (value < 1000) return value.ToString();
            if (value < 10_000) return $"{value / 1000}.{value % 1000 / 100}k";
            if (value < 1_000_000) return $"{value / 1000f:0.#}k";
            if (value < 10_000_000) return $"{value / 1_000_000f:0.##}M";
            return $"{value / 1_000_000f:0.#}M";
        }
    }
}
