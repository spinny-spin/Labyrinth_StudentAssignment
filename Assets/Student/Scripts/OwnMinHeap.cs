using UnityEngine;
using System.Collections.Generic;

public class OwnMinHeap
{
    private List<HeapEntry> heap = new List<HeapEntry>();
    public int Count => heap.Count;

    public struct HeapEntry
    {
        public Vector2Int node;
        public float priority;

        public HeapEntry(Vector2Int node, float priority)
        { 
            this.node = node;
            this.priority = priority;
        }
    }

    public void Enqueue(Vector2Int node, float priority)
    { 
        heap.Add(new HeapEntry(node, priority));

        int i = heap.Count - 1;

        // Swim: Flyttar det nya elementet uppåt i heapen så länge
        // dess prioritet är lägre än förälderns.
        // På så sätt behåller vi min-heap-egenskapen.
        // Tidskomplexitet: O(log N).
        while (i > 0)
        {
            int parent = (i - 1) / 2; //Dessa uträkningar gör så att vi inte faktiskt behöver skapa en träd struktur, utan bara imitera en

            if (heap[parent].priority <= heap[i].priority) break;

            HeapEntry temp = heap[parent];
            heap[parent] = heap[i];
            heap[i] = temp;

            i = parent;
        }
    }

    public HeapEntry Dequeue()
    {
        HeapEntry smallest = heap[0];

        int last = heap.Count - 1;
        heap[0] = heap[last];
        heap.RemoveAt(last);

        int i = 0;

        // Hämtar elementet med lägst prioritet (roten).

        // Ersätter roten med det sista elementet i listan
        // och tar sedan bort sista platsen.

        // Sink: Jämför elementet med sina barn och
        // byter med barnet som har lägst prioritet
        // tills min-heap-egenskapen är återställd.

        // Tidskomplexitet: O(log N).

        while (true)
        { 
            int left = 2 * i + 1; //Dessa uträkningar gör så att vi inte faktiskt behöver skapa en träd struktur, utan bara imitera en
            int right = 2 * i + 2; //Dessa uträkningar gör så att vi inte faktiskt behöver skapa en träd struktur, utan bara imitera en

            if (left >= heap.Count) break;

            int smaller = left;

            if (right < heap.Count && heap[right].priority < heap[left].priority)
                smaller = right;

            if (heap[i].priority <= heap[smaller].priority) break;

            HeapEntry temp = heap[i];
            heap[i] = heap[smaller];
            heap[smaller] = temp;

            i = smaller;
        }

        return smallest;
    }
}
