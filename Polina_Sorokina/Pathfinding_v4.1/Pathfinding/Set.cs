using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinding
{
    public class Set
    {
        int[] Values;
        public Set()
        {
            Values = new int[0];
        }

        // Добавление элемента в множество
        public void Add(int value)
        {
            for (int j = 0; j < Values.Length; j++)
                if (value == Values[j])
                    return;
            int[] temp = (int[])Values.Clone();
            Values = new int[Values.Length + 1];
            int i = 0;
            while ((i < temp.Length) && (value > temp[i]))
            {
                Values[i] = temp[i];
                i++;
            }
            Values[i++] = value;
            for (int j = i; (j < Values.Length) && (j - 1 >= 0) && (j - 1 < temp.Length); j++)
                Values[j] = temp[j - 1];
        }

        // Удаление элемента из множества
        public void Delete(int value)
        {
            int i = 0;
            for (i = 0; i < Values.Length; i++)
                if (value == Values[i])
                    break;
            if (i == Values.Length)
                return;
            int[] temp = (int[])Values.Clone();
            Values = new int[Values.Length - 1];
            for (int j = 0; j < i; j++)
                Values[j] = temp[j];
            for (int j = i; (j < Values.Length) && (j + 1 < temp.Length); j++)
                Values[j] = temp[j + 1];
        }

        // Определяет, содержатся ли все элементы входного массива в множестве
        public bool Contains(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (!this.Contains(array[i]))
                    return (false);
            }
            return (true);
        }

        // Определяет, содержатся ли все элементы из указанного отрезка
        public bool Contains(int begin, int end)
        {
            System.Collections.ArrayList tArray = new System.Collections.ArrayList();
            for (int i = begin; i <= end; i++)
                tArray.Add(i);
            int[] array = new int[tArray.Count];
            for (int i = 0; i < array.Length; i++)
                array[i] = int.Parse(tArray[i].ToString());
            return (this.Contains(array));
        }

        // Определяет, содержится ли элемент в множестве
        public bool Contains(int value)
        {
            for (int i = 0; i < Values.Length; i++)
                if (value == Values[i])
                    return (true);
            return (false);
        }
    }
}
