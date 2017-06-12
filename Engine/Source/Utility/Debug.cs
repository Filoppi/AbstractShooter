using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace UnrealMono
{
	public static class ExtendedException
	{
		public static void ThrowMessage(string message, bool addDebugString = false)
		{
#if DEBUG
			Debug.WriteLine("\n" + message + "\n");

			MessageBox.Show(message, "Error");

			if (addDebugString)
			{
				UnrealMonoGame.AddDebugString(message, Color.Red, false, 7F);
			}
#endif
		}

		public static void ThrowMessage(Exception ex, string message, bool addDebugString = false)
		{
#if DEBUG
			message += " with error: " + ex.Message;

			Debug.WriteLine("\n" + message + "\nDetails:\n" + ex.ToString() + "\n");

			MessageBox.Show(message + "\n\nDetails:\n" + ex.ToString(), "Exception");

			if (addDebugString)
			{
				UnrealMonoGame.AddDebugString(message, Color.Red, false, 7F);
			}
#endif
		}

		public static void ThrowException(Exception ex, string message)
		{
#if DEBUG
			message = "\n" + message + " with error: " + ex.Message + "\nDetails:\n" + ex.ToString();

			Debug.WriteLine(message + "\n");

			throw new System.ArgumentException(message, ex);
#endif
		}
	}
}