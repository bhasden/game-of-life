using Xunit;

namespace GameOfLife.Tests;

internal static class MatrixAssert
{
    public static void Equal(bool[][] expected, bool[][] actual)
    {
        Assert.Equal(expected.Length, actual.Length);

        for (var row = 0; row < expected.Length; row++)
        {
            Assert.Equal(expected[row].Length, actual[row].Length);

            for (var column = 0; column < expected[row].Length; column++)
            {
                Assert.Equal(expected[row][column], actual[row][column]);
            }
        }
    }
}
