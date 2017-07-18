namespace Proxy_DesignPattern
{
	using System;
	using System.Threading;

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	abstract class CommonSubject 
	{
		abstract public void Request();		
	}

	class ActualSubject : CommonSubject
	{
		public ActualSubject()
		{
			// Assume constructor here does some operation that takes quite a
			// while - hence the need for a proxy - to delay incurring this 
			// delay until (and if) the actual subject is needed
			Console.WriteLine("Starting to construct ActualSubject");		
			Thread.Sleep(1000); // represents lots of processing! 
			Console.WriteLine("Finished constructing ActualSubject");
		}
			
		override public void Request()
		{
			Console.WriteLine("Executing request in ActualSubject");
		}
	}

	class Proxy : CommonSubject
	{
		ActualSubject actualSubject;

		override public void Request()
		{
			if (actualSubject == null)
				actualSubject = new ActualSubject();
			actualSubject.Request();
		}	
		
	}
	
	public class Client
	{
		public static int Main(string[] args)
		{
			Proxy p = new Proxy();

			// Perform actions here
			// . . . 

			if (1==1)		// at some later point, based on a condition, 
				p.Request();// we determine if we need to use subject
					            
			return 0;
		}
	}
}

