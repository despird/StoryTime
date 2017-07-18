namespace FactoryMethod_DesignPattern
{
	using System;

	// These two classes could be part of a framework,
	// which we will call DP
	// ===============================================
	
	class DPDocument 
	{
	

	}

	abstract class DPApplication 
	{
		protected DPDocument doc;
		
		abstract public void CreateDocument();

		public void ConstructObjects()
		{
			
			// Create objects as needed
			// . . .

			// including document
			CreateDocument();
		}		
		abstract public void Dump();
	}

	// These two classes could be part of an application 
	// =================================================

	class MyApplication : DPApplication 
	{
		override public void CreateDocument()
		{
			doc = new MyDocument();			
		}			

		override public void Dump()
		{
			Console.WriteLine("MyApplication exists");
		}
	}	

	class MyDocument : DPDocument 
	{

	}

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{
			MyApplication myApplication = new MyApplication();

			myApplication.ConstructObjects();

			myApplication.Dump();
			
			return 0;
		}
	}
}
