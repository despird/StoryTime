namespace Strategy_DesignPattern
{
	using System;

	
	abstract class Strategy 
	{
		abstract public void DoAlgorithm();
	}

	class FirstStrategy : Strategy 
	{
		override public void DoAlgorithm()
		{
			Console.WriteLine("In first strategy");			
		}
	}

	class SecondStrategy : Strategy 
	{
		override public void DoAlgorithm()
		{
			Console.WriteLine("In second strategy");			
		}
	}

	class Context 
	{
		Strategy s;
		public Context(Strategy strat)
		{
			s = strat;			
		}

		public void DoWork()
		{
			// some of the context's own code goes here
		}

		public void DoStrategyWork()
		{
			// now we can hand off to the strategy to do some 
			// more work
			s.DoAlgorithm();
		}
	}

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{	
			FirstStrategy firstStrategy = new FirstStrategy();
			Context c = new Context(firstStrategy);
			c.DoWork();
			c.DoStrategyWork();

			return 0;
		}
	}
}

