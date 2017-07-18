namespace Facade_DesignPattern
{
	using System;

	class SubSystem_class1 
	{
		public void OperationX() 
		{
			Console.WriteLine("SubSystem_class1.OperationX called");
		}
	}

	class SubSystem_class2
	{
		public void OperationY()
		{
			Console.WriteLine("SubSystem_class2.OperationY called");
		}
	}

	class SubSystem_class3 
	{
		public void OperationZ()
		{			
			Console.WriteLine("SubSystem_class3.OperationZ called");
		}	
	}

	class Facade 
	{
		private SubSystem_class1 c1 = new SubSystem_class1();
		private SubSystem_class2 c2 = new SubSystem_class2();
		private SubSystem_class3 c3 = new SubSystem_class3();

		public void OperationWrapper()
		{
			Console.WriteLine("The Facade OperationWrapper carries out complex decision-making");
			Console.WriteLine("which in turn results in calls to the subsystem classes");
			c1.OperationX();
			if (1==1 /*some really complex decision*/)
			{
				c2.OperationY();
			}
			// lots of complex code here . . .
			c3.OperationZ();
		}
		
	}

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{
			Facade facade = new Facade();
			Console.WriteLine("Client calls the Facade OperationWrapper");
			facade.OperationWrapper();      
			return 0;
		}
	}
}
