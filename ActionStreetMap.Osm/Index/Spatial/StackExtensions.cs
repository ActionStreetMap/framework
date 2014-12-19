using System.Collections.Generic;

namespace ActionStreetMap.Osm.Index.Spatial
{
	internal static class StackExtensions
	{
		public static T TryPop<T>(this Stack<T> stack)
		{
			return stack.Count == 0 ? default(T) : stack.Pop();
		}

		public static T TryPeek<T>(this Stack<T> stack)
		{
			return stack.Count == 0 ? default(T) : stack.Peek();
		}
	}
}