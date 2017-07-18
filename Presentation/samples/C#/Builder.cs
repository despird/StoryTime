namespace Builder_DesignPattern
{
	using System;

	// These two classes could be part of a framework,
	// which we will call DP
	// ===============================================
	
	class Director 
	{
		public void Construct(AbstractBuilder abstractBuilder)
		{
			abstractBuilder.BuildPartA();
			if (1==1 ) //represents some local decision inside director
			{
				abstractBuilder.BuildPartB();			
			}
			abstractBuilder.BuildPartC();			
		}

	}

	abstract class AbstractBuilder 
	{
		abstract public void BuildPartA();
		abstract public void BuildPartB();
		abstract public void BuildPartC();
	}

	// These two classes could be part of an application 
	// =================================================

	class ConcreteBuilder : AbstractBuilder 
	{
		override public void BuildPartA()
		{
			// Create some object here known to ConcreteBuilder
			Console.WriteLine("ConcreteBuilder.BuildPartA called");
		}
				
		override public void BuildPartB()
		{
			// Create some object here known to ConcreteBuilder
			Console.WriteLine("ConcreteBuilder.BuildPartB called");
		}
		
		override public void BuildPartC()
		{
			// Create some object here known to ConcreteBuilder
			Console.WriteLine("ConcreteBuilder.BuildPartC called");
		}
	}	

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{
			ConcreteBuilder concreteBuilder = new ConcreteBuilder();
			Director director = new Director();

			director.Construct(concreteBuilder);

			return 0;
		}
	}
}
