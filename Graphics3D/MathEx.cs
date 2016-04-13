namespace Graphics3D
{
    public static class MathEx
    {
        public static float Constrain(this float value, float low = 0, float high = 1)
        {
            if (value < low) return low;
            if (value > high) return high;
            return value;
        }

        public static float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * gradient.Constrain();
        }

        public static float Map(this float value, float min1, float max1, float min2, float max2)
        {
            return min2 + (
                    (value - min1) * (
                        (max2 - min2) / (max1 - min1)));
        }        
    }
}
