
namespace TextEditor.Core;

public class Deque
{
    private class Node
    {
        public Action Data { get; }
        public Node? Prev { get; set; }
        public Node? Next { get; set; }

        public Node(Action data)
        {
            Data = data;
        }
    }

    private Node? head;
    private Node? tail;
    private int count;
    private int maxCapacity;

    public Deque(int capacity = 20)
    {
        maxCapacity = capacity;
        head = null;
        tail = null;
        count = 0;
    }

    public void Clear()
    {
        head = null;
        tail = null;
        count = 0;
    }

    /// <summary>
    /// Pushes an action at the head (stack push).
    /// Removes the tail if capacity is exceeded.
    /// </summary>
    public void Push(Action action)
    {
        Node newNode = new Node(action);
        if (head == null)
        {
            head = newNode;
            tail = newNode;
        }
        else
        {
            newNode.Next = head;
            head.Prev = newNode;
            head = newNode;
        }
        count++;

        // If capacity exceeded, remove from tail
        if (count > maxCapacity)
        {
            PopQueue(); // remove from tail
        }
    }

    /// <summary>
    /// Pop from the head (stack pop).
    /// </summary>
    public Action Pop()
    {
        if (IsEmpty())
            throw new InvalidOperationException("List is empty.");

        Node node = head!;
        Action result = node.Data;

        head = head!.Next;
        if (head == null)
        {
            tail = null;
        }
        else
        {
            head.Prev = null;
        }
        count--;
        return result;
    }

    /// <summary>
    /// Pop from the tail (queue pop).
    /// </summary>
    public Action PopQueue()
    {
        if (IsEmpty())
            throw new InvalidOperationException("List is empty.");

        Node node = tail!;
        Action result = node.Data;

        tail = tail!.Prev;
        if (tail == null)
        {
            head = null;
        }
        else
        {
            tail.Next = null;
        }
        count--;
        return result;
    }

    /// <summary>
    /// Checks if the structure is empty.
    /// </summary>
    public bool IsEmpty()
    {
        return count == 0;
    }
}
