using System.Net.Http;
using System.Threading.Tasks;

namespace Random
{
    public class RandomOrg : IRandomizer
    {
        private readonly string url = "https://www.random.org";

        public RandomOrg()
        { }

        public async Task<int[]> IntegerAsync(int min, int max, int count)
        {
            string url = this.url + "/integers/?num=" + count + "&min=" + min + "&max=" + max + "&col=1&base=10&format=plain&rnd=new";
            HttpClient client = new HttpClient();

            string result = await client.GetStringAsync(url);
            string[] numbers = result.Split("\n");

            int[] numberList = new int[count];
            for( int i = 0; i< count; i++ )
            {
                numberList[i] = int.Parse(numbers[i]);
            }

            return numberList;
        }

        public int[] Integer(int min, int max, int count)
        {
            Task<int[]> task = IntegerAsync(min, max, count);
            task.Wait();
            return task.Result;
        }
    }
}
