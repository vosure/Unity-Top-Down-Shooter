using System.Collections;

public class Utility
{
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random randomizer = new System.Random(seed);

        for (int i = 0; i < array.Length; i++)
        {
            int random = randomizer.Next(i, array.Length);
            T temp = array[random];
            array[random] = array[i];
            array[i] = temp;
        }

        return array;
    }

}
