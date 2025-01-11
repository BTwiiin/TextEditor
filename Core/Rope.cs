using System;
using System.Text;

namespace TextEditor.Core
{
    /// <summary>
    /// RopeNode represents a node in the Rope's binary tree.
    /// Internal nodes have children and store a weight (length of left subtree).
    /// Leaf nodes store text directly.
    /// </summary>
    public class RopeNode
    {
        public bool IsLeaf { get; private set; }
        public int Weight { get; set; }
        public RopeNode? Left { get; set; }
        public RopeNode? Right { get; set; }
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Construct a leaf node with text.
        /// </summary>
        /// <param name="text">The initial text for this leaf.</param>
        public RopeNode(string text)
        {
            IsLeaf = true;
            Text = text;
            Weight = text.Length;
        }

        /// <summary>
        /// Construct an internal node from left and right subtrees.
        /// </summary>
        /// <param name="left">The left RopeNode.</param>
        /// <param name="right">The right RopeNode.</param>
        public RopeNode(RopeNode left, RopeNode right)
        {
            IsLeaf = false;
            Left = left;
            Right = right;
            // Weight is the length of the entire left subtree
            Weight = left?.GetLength() ?? 0;
        }

        /// <summary>
        /// Returns the total length of text under this node.
        /// </summary>
        public int GetLength()
        {
            if (IsLeaf) return Text.Length;
            int leftLen = Left?.GetLength() ?? 0;
            int rightLen = Right?.GetLength() ?? 0;
            return leftLen + rightLen;
        }
    }

    /// <summary>
    /// Rope provides high-performance text operations via a balanced binary tree.
    /// </summary>
    public class Rope
    {
        private RopeNode root;

        /// <summary>
        /// Default constructor for Rope with empty text.
        /// </summary>
        public Rope()
        {
            root = new RopeNode(string.Empty);
        }

        /// <summary>
        /// Construct a Rope with initial text.
        /// </summary>
        /// <param name="text">The initial text.</param>
        public Rope(string text)
        {
            root = new RopeNode(text);
        }

        /// <summary>
        /// Returns total length of the rope.
        /// </summary>
        public int Length => root.GetLength();

        /// <summary>
        /// Retrieves the full text from the rope.
        /// </summary>
        public string GetText()
        {
            return ToString(root);
        }

        private string ToString(RopeNode? node)
        {
            if (node == null) return string.Empty;
            if (node.IsLeaf) return node.Text;
            return ToString(node.Left) + ToString(node.Right);
        }

        /// <summary>
        /// Inserts text at the given position.
        /// </summary>
        /// <param name="index">Insertion index within the rope.</param>
        /// <param name="text">The text to insert.</param>
        public void Insert(int index, string text)
        {
            if (index < 0 || index > Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            RopeNode newNode = new RopeNode(text);
            root = Insert(root, index, newNode);
        }

        private RopeNode Insert(RopeNode node, int index, RopeNode newNode)
        {
            // If node is null, just return the newNode
            if (node == null) return newNode;

            if (node.IsLeaf)
            {
                // Split the existing text at 'index'
                string existing = node.Text;
                string leftPart = existing.Substring(0, index);
                string rightPart = existing.Substring(index);

                RopeNode leftLeaf = new RopeNode(leftPart);
                RopeNode rightLeaf = new RopeNode(rightPart);

                // Combine leftLeaf and newNode, then combine the result with rightLeaf
                RopeNode merged = new RopeNode(leftLeaf, newNode);
                return new RopeNode(merged, rightLeaf);
            }
            else
            {
                int leftLen = node.Left?.GetLength() ?? 0;
                if (index <= leftLen)
                {
                    node.Left = Insert(node.Left!, index, newNode);
                    node.Weight = node.Left.GetLength();
                }
                else
                {
                    node.Right = Insert(node.Right!, index - leftLen, newNode);
                }
                return node;
            }
        }

        /// <summary>
        /// Deletes 'length' characters starting at 'index'.
        /// </summary>
        public void Delete(int index, int length)
        {
            if (index < 0 || length < 0 || (index + length) > Length)
                throw new ArgumentOutOfRangeException();

            root = Delete(root, index, length)!;
        }

        private RopeNode? Delete(RopeNode? node, int index, int length)
        {
            if (node == null) return null;

            if (node.IsLeaf)
            {
                // Remove substring from the leaf's text
                string s = node.Text;
                string leftPart = s.Substring(0, index);
                string rightPart = s.Substring(index + length);
                return new RopeNode(leftPart + rightPart);
            }
            else
            {
                int leftLen = node.Left?.GetLength() ?? 0;
                if (index + length <= leftLen)
                {
                    // Entire deletion in left subtree
                    node.Left = Delete(node.Left, index, length);
                    node.Weight = node.Left?.GetLength() ?? 0;
                }
                else if (index >= leftLen)
                {
                    // Entire deletion in right subtree
                    node.Right = Delete(node.Right, index - leftLen, length);
                }
                else
                {
                    // Split across both subtrees
                    int leftPortion = leftLen - index;
                    node.Left = Delete(node.Left, index, leftPortion);
                    node.Weight = node.Left?.GetLength() ?? 0;
                    node.Right = Delete(node.Right, 0, length - leftPortion);
                }
                return node;
            }
        }

        /// <summary>
        /// Returns the character at a given index.
        /// </summary>
        public char CharAt(int index)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return CharAt(root, index);
        }

        private char CharAt(RopeNode node, int index)
        {
            if (node.IsLeaf)
            {
                return node.Text[index];
            }
            else
            {
                int leftLen = node.Left?.GetLength() ?? 0;
                if (index < leftLen)
                    return CharAt(node.Left!, index);
                else
                    return CharAt(node.Right!, index - leftLen);
            }
        }

        /// <summary>
        /// Concatenates another rope onto this one.
        /// </summary>
        public void Concatenate(Rope other)
        {
            // Create a new internal node that merges both
            root = new RopeNode(root, other.root);
        }

        /// <summary>
        /// Returns a substring of the rope from startIndex to startIndex + length.
        /// </summary>
        public string Substring(int startIndex, int length)
        {
            if (startIndex < 0 || length < 0 || (startIndex + length) > Length)
                throw new ArgumentOutOfRangeException();

            StringBuilder sb = new StringBuilder();
            Substring(root, startIndex, length, sb);
            return sb.ToString();
        }

        private void Substring(RopeNode? node, int startIndex, int length, StringBuilder sb)
        {
            if (node == null || length <= 0) return;

            if (node.IsLeaf)
            {
                if (startIndex < node.Text.Length)
                {
                    int available = node.Text.Length - startIndex;
                    sb.Append(node.Text.Substring(startIndex, Math.Min(length, available)));
                }
            }
            else
            {
                int leftLen = node.Left?.GetLength() ?? 0;
                if (startIndex < leftLen)
                {
                    int leftAmount = Math.Min(length, leftLen - startIndex);
                    Substring(node.Left, startIndex, leftAmount, sb);
                    int remaining = length - leftAmount;
                    if (remaining > 0)
                    {
                        Substring(node.Right, 0, remaining, sb);
                    }
                }
                else
                {
                    Substring(node.Right, startIndex - leftLen, length, sb);
                }
            }
        }
    }
}