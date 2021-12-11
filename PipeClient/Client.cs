using Winapi.Flags;
using Winapi.Structures;
using static Winapi.Functions;
using SimpleMenu;
using System;
using System.Threading;
using System.Text;

namespace PipeClient
{
	class Client
	{
		static Menu mainMenu = new Menu("CLIENT",
			new IMenuItem[] {
				new MenuItem("Connect pipe", ConnectPipe),
				new MenuItem("Receive message", GetMessage),
				new MenuItem("Disconnect from pipe", DisconnectPipe)
			});

		static NativeOverlapped overlapped;
		static IntPtr evt;
		static IntPtr pipe;
		static uint bufSz = 512;
		static unsafe IOCompletionCallback completionCallback = new IOCompletionCallback(Callback);
		static bool connected = false;

		static unsafe void Callback(uint errCode, uint bytes, NativeOverlapped* ov)
		{
			Console.WriteLine("Data received!");
		}
		static void ConnectPipe()
		{
			evt = CreateEvent(IntPtr.Zero, false, false, null);
			pipe = CreateFile(@"\\.\pipe\mypipe",
				(uint)DesiredAccess.GENERIC_READ | (uint)DesiredAccess.GENERIC_WRITE,
				(uint)ShareMode.None,
				null,
				(uint)CreationDisposition.OPEN_EXISTING,
				(uint)PipeOpenModeFlags.FILE_FLAG_OVERLAPPED,
				IntPtr.Zero);
			connected = true;
		}
		static void GetMessage()
		{
			if(!connected)
			{
				Console.WriteLine("You need to connect pipe at first!");
				return;
			}
			overlapped.EventHandle = evt;
			byte[] buf = new byte[bufSz];
			if (ReadFileEx(pipe, buf, bufSz, ref overlapped, completionCallback))
			{
				string str = Encoding.UTF8.GetString(buf);
				Console.WriteLine(str);
			}
			else
				Console.WriteLine($"Error occured while reading!. Code: {GetLastError()}");
		}
		static void DisconnectPipe()
		{
			if (!connected)
			{
				Console.WriteLine("You need to connect pipe at first!");
				return;
			}
			if (CloseHandle(pipe))
				Console.WriteLine("Disconnection successful~");
			else
				Console.WriteLine($"Error occured. Code: {GetLastError()}");
			connected = false;
		}

		static void Main(string[] args)
		{
			mainMenu.Select();
		}
	}
}
