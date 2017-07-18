namespace Command_DesignPattern
{
	using System;

	abstract class Command 
	{
		abstract public void Execute();
		protected Receiver r;
		public Receiver R
		{
			set 
			{
				r = value; 
			}
		}
	}

	class ConcreteCommand : Command
	{
		override public void Execute()
		{
			Console.WriteLine("Command executed");
			r.InformAboutCommand();
		}
	}

	class Receiver 
	{
		public void InformAboutCommand()
		{
			Console.WriteLine("Receiver informed about command");
		}
		
	}

	class Invoker 
	{
		private Command command;
		public void StoreCommand(Command c)
		{
			command = c;
		}
		public void ExecuteCommand()
		{
			command.Execute();
		}		
	}

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{		
			// Set up everything
			Command c = new ConcreteCommand();
			Receiver r = new Receiver();
			c.R = r;
			Invoker i = new Invoker();
			i.StoreCommand(c);

			// now let application run

			// the invoker is how the command is exposed for the end-user 
			// (or a client) initiates the command, 
			// (e.g. toolbar button, menu item)

			i.ExecuteCommand();

			return 0;
		}
	}
}

