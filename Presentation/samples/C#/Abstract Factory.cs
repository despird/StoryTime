namespace AbstractFactory_DesignPattern
{
	using System;

	// These classes could be part of a framework,
	// which we will call DP
	// ===========================================
	
	abstract class DPDocument 
	{
		abstract public void Dump();		
	}

	abstract class DPWorkspace
	{
		abstract public void Dump();
	}
	
	abstract class DPView 
	{
		abstract public void Dump();
	}	
	
	abstract class DPFactory 
	{
		abstract public DPDocument CreateDocument();
		abstract public DPView CreateView();
		abstract public DPWorkspace CreateWorkspace();
	}

	abstract class DPApplication 
	{
		protected DPDocument doc;
		protected DPWorkspace workspace;
		protected DPView view;
		
		public void ConstructObjects(DPFactory factory)
		{
			// Create objects as needed
			doc = factory.CreateDocument();
			workspace = factory.CreateWorkspace();
			view = factory.CreateView();
		}		
		
		abstract public void Dump();

		public void DumpState()
		{
			if (doc != null) doc.Dump();
			if (workspace != null) workspace.Dump();
			if (view != null) view.Dump();
		}
	}

	// These classes could be part of an application 
	class MyApplication : DPApplication 
	{
		MyFactory myFactory = new MyFactory();

		override public void Dump()
		{
			Console.WriteLine("MyApplication exists");
		}

		public void CreateFamily()
		{
			MyFactory myFactory = new MyFactory();
			ConstructObjects(myFactory);			
		}
	}	

	class MyDocument : DPDocument 
	{
		public MyDocument()
		{
			Console.WriteLine("in MyDocument constructor");			
		}
		
		override public void Dump()
		{
			Console.WriteLine("MyDocument exists");
		}
	}

	class MyWorkspace : DPWorkspace 
	{
		override public void Dump()
		{
			Console.WriteLine("MyWorkspace exists");
		}
	}

	class MyView : DPView 
	{
		override public void Dump()
		{
			Console.WriteLine("MyView exists");
		}
	}

	class MyFactory : DPFactory 
	{
		override public DPDocument CreateDocument()
		{
			return new MyDocument();
		}
		override public DPWorkspace CreateWorkspace()
		{
			return new MyWorkspace();
		}		
		override public DPView CreateView()
		{
			return new MyView();
		}
	}

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{
			MyApplication myApplication = new MyApplication();

			myApplication.CreateFamily();

			myApplication.DumpState();
			
			return 0;
		}
	}
}
