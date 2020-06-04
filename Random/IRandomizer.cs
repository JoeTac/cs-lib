using System.Threading.Tasks;

namespace Random
{
    public interface IRandomizer
    {
        Task<int[]> IntegerAsync(int min, int max, int count);
        int[] Integer(int min, int max, int count);
    }
}
