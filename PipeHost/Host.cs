using System;
using Winapi.Flags;
using Winapi.Structures;
using static Winapi.Functions;
using SimpleMenu;
using System.Threading;
using System.Text;

namespace PipeHost
{
	class Host
	{
		static Menu mainMenu = new Menu("HOST", 
			new IMenuItem[] { 
				new MenuItem("Create named pipe", CreatePipe),
				new MenuItem("Send message", SendMessage),
				new MenuItem("Disconnect from pipe", DisconnectPipe)
			});

		static bool pipeCreated = false;
		static uint outBufSz = 512, inBufSz = 512;
		static IntPtr pipe;
		static NativeOverlapped overlapped = new NativeOverlapped();
		static IntPtr evt;
		static void CreatePipe()
		{
			evt = CreateEvent(IntPtr.Zero, false, false, null);
			pipe = CreateNamedPipe("\\\\.\\pipe\\mypipe",
				(uint)PipeOpenModeFlags.PIPE_ACCESS_DUPLEX,
				(uint)PipeModeFlags.PIPE_TYPE_MESSAGE | (uint)PipeModeFlags.PIPE_READMODE_MESSAGE | (uint)PipeModeFlags.PIPE_WAIT,
				Constants.PIPE_UNLIMITED_INSTANCES,
				outBufSz,
				inBufSz,
				0,
				null);

			NativeOverlapped syncPipe = new NativeOverlapped() { EventHandle = evt };
			var res = ConnectNamedPipe(pipe, ref syncPipe);
			WaitForSingleObject(evt, Constants.INFINITE);

			if (res)
			{
				Console.WriteLine("Pipe created successfully");
				pipeCreated = true;
			}
			else
				Console.WriteLine($"Error creating pipe! Error code {GetLastError()}");
		}

		static unsafe void SendMessage()
		{
			if (!pipeCreated)
			{
				Console.WriteLine("You need to create pipe at first!");
				return;
			}
			Console.WriteLine("Input message:");
			string str = Console.ReadLine();
			var outputStr = Encoding.UTF8.GetBytes(str);
			Array.Resize(ref outputStr, (int)outBufSz);
			overlapped.EventHandle = evt;
			fixed(NativeOverlapped* o = &overlapped)
			{
				if(WriteFile(pipe, outputStr, outBufSz, out uint written, o) && WaitForSingleObject(evt, 10000) == 0)
					Console.WriteLine("Message written successfully");
				else
					Console.WriteLine($"Error writing message. Error code {GetLastError()}");
			} 
		}

		static void DisconnectPipe()
		{
			if (!pipeCreated)
			{
				Console.WriteLine("You need to create pipe at first!");
				return;
			}
			if(DisconnectNamedPipe(pipe))
				Console.WriteLine("Pipe disconnected!");
			else
				Console.WriteLine($"Error disconnecting pipe! Error code {GetLastError()}");
			pipeCreated = false;
		}
		static void Main(string[] args)
		{
			mainMenu.Select();
		}
	}
}
