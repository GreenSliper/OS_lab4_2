using SimpleMenu;
using System;
using System.Diagnostics;

namespace OS_lab4_2
{
	class Starter
	{
		static IMenu mainMenu = new Menu("Lab 4, Part 2: pipes", 
			new IMenuItem[]{
				new MenuItem("Start host", StartHost),
				new MenuItem("Start client", StartClient),
			});

		static void StartHost() => StartProcess(@"..\..\..\..\PipeHost\bin\Debug\net5.0\PipeHost.exe");
		static void StartClient() => StartProcess(@"..\..\..\..\PipeClient\bin\Debug\net5.0\PipeClient.exe");

		static void StartProcess(string path)
		{
			using (var pr = new Process())
			{
				pr.StartInfo.FileName = path;
				pr.StartInfo.UseShellExecute = true;
				pr.Start();
			}
		}

		static void Main(string[] args)
		{
			mainMenu.Select();
		}
	}
}
