namespace Permutations {

    using System.Collections;

    public static class Permutation
    {
        private static void Swap(ref int a, ref int b)
        {
            if (a == b) return;

            var temp = a;
            a = b;
            b = temp;
        }

        public static ArrayList get_permutations(int[] list)
        {
            ArrayList array_list=new ArrayList();
            int x = list.Length - 1;
            GetPer(list, 0, x,array_list);
            return array_list;
        }

        private static void GetPer(int[] array, int k, int m,ArrayList array_list)
        {
            if (k == m)
            {
                array_list.Add((int[])array.Clone());
            }
            else
                for (int i = k; i <= m; i++)
                {
                        Swap(ref array[k], ref array[i]);
                        GetPer(array, k + 1, m, array_list);
                        Swap(ref array[k], ref array[i]);
                }
        }
    }
}