namespace Adapter_DesignPattern
{
	using System;

	class FrameworkXTarget 
	{
		virtual public void SomeRequest(int x)
		{
			// normal implementation of SomeRequest goes here					
		}
	}

	class FrameworkYAdaptee
	{
		public void QuiteADifferentRequest(string str) 
		{
			Console.WriteLine("QuiteADifferentRequest = {0}", str);
		}		
	}

	class OurAdapter : FrameworkXTarget
	{
		private FrameworkYAdaptee adaptee = new FrameworkYAdaptee();
		override public void SomeRequest(int a)
		{
			string b;
			b = a.ToString();
			adaptee.QuiteADifferentRequest(b);
		}		
	}

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		void GenericClientCode(FrameworkXTarget x)
		{
			// We assume this function contains client-side code that only 
			// knows about FrameworkXTarget.
			x.SomeRequest(4);
			// other calls to FrameworkX go here
			// ...
		}
		
		public static int Main(string[] args)
		{
			Client c = new Client();
			FrameworkXTarget x = new OurAdapter();
			c.GenericClientCode(x);	
			return 0;
		}
	}
}
