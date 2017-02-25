using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaraFlirt
{
	class Program
	{
		public class Signature
		{
			public string PatternData;
			public int CrcSize;
			public ushort Crc16;
			public int RefOffset;
			public string RefString;

			public override string ToString()
			{
				return PatternData;
			}
		}

		public class Node
		{
			public List<Node> Children = new List<Node>();

			public bool IsPattern;
			public int StartIndex;
			public string Data;

			public Node(int depth, bool isPattern, string data)
			{
				StartIndex = depth;
				IsPattern = isPattern;
				Data = data.Trim(':');
			}

			public override string ToString()
			{
				return string.Format("{0}:{1}:{2}", StartIndex, IsPattern, Data);
			}
		}

		static void MakeSignatures(List<Signature> signatures, Node root, StringBuilder pattern)
		{
			if (!root.IsPattern)
			{
				signatures.Add(new Signature { PatternData = pattern.ToString() });
				return;
			}
			var childPattern = new StringBuilder(pattern.ToString());
			childPattern.Append(root.Data);
			foreach(var child in root.Children)
				MakeSignatures(signatures, child, childPattern);
		}

		static Node Parse(string file)
		{
			var lines = File.ReadAllLines(file);
			var root = new Node(0, true, "");
			var nodeStack = new Stack<Node>();
			nodeStack.Push(root);
			var hadMeta = false;

			foreach (var line in lines)
			{
				if (line.Length == 0)
					continue;

				var startIndex = 0;
				var isPattern = line[line.Length - 1] == ':';
				if (!isPattern)
					hadMeta = true;
				else
				{
					while (line[startIndex] == ' ')
						startIndex++;
				}
				if (isPattern && hadMeta)
				{
					hadMeta = false;
					var n = nodeStack.Pop();
					while (startIndex != n.StartIndex)
						n = nodeStack.Pop();
				}
				var node = new Node(startIndex, isPattern, line.Trim());
				nodeStack.Peek().Children.Add(node);
				if (isPattern)
					nodeStack.Push(node);
			}

			return root;
		}

		static void Main(string[] args)
		{
			var root = Parse("dump.txt");
			var signatures = new List<Signature>();
			MakeSignatures(signatures, root, new StringBuilder());
			foreach(var sig in signatures)
				Console.WriteLine(sig);
		}
	}
}
