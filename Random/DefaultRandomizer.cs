using System.Threading.Tasks;

namespace Random
{
    class DefaultRandomizer : IRandomizer
    {
        System.Random random = null;

        public DefaultRandomizer()
        {
            random = new System.Random();
        }

        public int[] Integer(int min, int max, int count)
        {
            Task<int[]> results = IntegerAsync(min, max, count);
            results.Wait();
            return results.Result;
        }

        public async Task<int[]> IntegerAsync(int min, int max, int count)
        {
            int[] results = await Task.Run(() => new int[count]);
            for (int i = 0; i < count; i++)
            {
                results[i] = random.Next(min, max);
            }

            return results;
        }
    }
}
