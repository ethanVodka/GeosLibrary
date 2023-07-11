using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeosLibrary.Utils
{
    public class UArrayManager
    {
        public static T[] AddFirstElementToLast<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return array;
            }

            T firstElement = array[0];

            // Create a new array with one additional element
            T[] newArray = new T[array.Length + 1];

            // Copy elements from the original array to the new array
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            // Add the first element to the end
            newArray[newArray.Length - 1] = firstElement;

            return newArray;
        }

        public static T[] CreateReversedArray<T>(T[] original)
        {
            if (original == null)
            {
                return null;
            }

            int length = original.Length;
            T[] reversed = new T[length];

            for (int i = 0; i < length; i++)
            {
                reversed[i] = original[length - i - 1];
            }

            return reversed;
        }
    }


}
