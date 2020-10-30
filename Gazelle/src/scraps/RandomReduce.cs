// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace Gazelle.Components.Misc
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    
    public static class RandomReduce
    {
        public static List<T> DoRandomReduce<T>(List<T> list, int number, int seed = -2147483648)
        {
            Random random = (seed == -2147483648) ? new Random() : new Random(seed);
            HashSet<int> set = new HashSet<int>();
            int num = 0;
            while (true)
            {
                if (num >= number)
                {
                    List<T> list2 = new List<T>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!set.Contains(i))
                        {
                            list2.Add(list[i]);
                        }
                    }
                    return list;
                }
                int item = random.Next(0, list.Count);
                if (!set.Contains(item))
                {
                    set.Add(number);
                }
                else
                {
                    number++;
                }
                num++;
            }
        }
    }
}
