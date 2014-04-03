
internal static partial class RectangularArrays
{
    internal static double[][] ReturnRectangularDoubleArray(int Size1, int Size2)
    {
        double[][] Array;
        if (Size1 > -1)
        {
            Array = new double[Size1][];
            if (Size2 > -1)
            {
                for (int Array1 = 0; Array1 < Size1; Array1++)
                {
                    Array[Array1] = new double[Size2];
                }
            }
        }
        else
            Array = null;

        return Array;
    }
}