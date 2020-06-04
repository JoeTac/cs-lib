using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Random
{
    public class Dice
    {
        public readonly static string D4 = "d4";
        public readonly static string D6 = "d6";
        public readonly static string D8 = "d8";
        public readonly static string D10 = "d10";
        public readonly static string D12 = "d12";
        public readonly static string D20 = "d20";
        public readonly static string D100 = "d100";

        private IRandomizer randomizer = null;
        private Dictionary<int, Queue<int>> cached;
        private int cacheSize = 20;
        
        public void setCacheSize(int cacheSize)
        {
            if ( cacheSize>0 )
            {
                this.cacheSize = cacheSize;
            }
        }


        public Dice(IRandomizer randomizer = null)
        {
            if (randomizer == null)
            {
                randomizer = new DefaultRandomizer();
            }
            this.randomizer = randomizer;

            cached = new Dictionary<int, Queue<int>>();
        }

        public async Task LoadCacheAsync(int max)
        {
            int[] values = await randomizer.IntegerAsync(1, max, cacheSize);
            foreach ( int value in values )
            {
                cached[max].Enqueue(value);
            }
        }
        public void LoadCache(int max)
        {
            LoadCacheAsync(max).Wait();
        }

        private async Task<int[]> TakeAsync(int max, int count)
        {
            int[] results = new int[count];

            if ( !cached.ContainsKey(max) )
            {
                cached[max] = new Queue<int>();
            }
            if ( cached[max].Count <= max )
            {
                await LoadCacheAsync(max);
            }

            for(int i=0; i<count; i++)
            {
                results[i] = cached[max].Dequeue();
            }
            return results;
        }

        public async Task<int> RollAsync(string diceSyntax)
        {
            ParseResult result = parseDice(diceSyntax);

            int[] rolls = await TakeAsync(result.DieType, result.DieCount);

            int total = 0;
            foreach (int roll in rolls)
            {
                total += roll;
            }

            return total + result.Modifier;
        }
        public int Roll(string diceSyntax)
        {
            Task<int> task = RollAsync(diceSyntax);
            task.Wait();
            return task.Result;
        }

        private ParseResult parseDice(string diceSyntax)
        {
            Regex results = new Regex(@"(?<count>[1-9][0-9]*)?[dD](?<die>[1-9][0-9]{0,2})(?<modifier>[-+][1-9][0-9]*)?");
            MatchCollection matches = results.Matches(diceSyntax.ToLower());

            int count = 1;
            int die = 0;
            int modifier = 0;
            if (matches.Count > 0)
            {
                foreach (var group in matches[0].Groups)
                {
                    if (group.GetType() == typeof(Group))
                    {
                        Group match = (Group)group;
                        switch (match.Name)
                        {
                            case "count":
                                count = int.Parse(match.Value);
                                break;
                            case "die":
                                die = int.Parse(match.Value);
                                break;
                            case "modifier":
                                modifier = int.Parse(match.Value);
                                break;
                        }
                    }
                }
            }

            return new ParseResult()
            {
                DieCount = count,
                DieType = die,
                Modifier = modifier
            };
        }

        private class DefaultRandomizer : IRandomizer
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
                for(int i=0; i<count; i++)
                {
                    results[i] = random.Next(min, max);
                }

                return results;
            }
        }

        private class ParseResult
        {
            public int DieCount;
            public int DieType;
            public int Modifier;
        }
    }
}
