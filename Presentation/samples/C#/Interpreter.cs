namespace Interpreter_DesignPattern
{
	using System;
	using System.Collections;

	class Context 
	{
		
	}

	abstract class AbstractExpression 
	{
		abstract public void Interpret(Context c);
	}

	// class for terminal symbol
	class TerminalExpression : AbstractExpression
	{
		override public void Interpret(Context c)	
		{
			
		}
	}

	// class for grammar rule (one per rule needed)
	class NonterminalExpression : AbstractExpression
	{
		override public void Interpret(Context c)	
		{
			
		}	
	}
	// to extend grammar, just add other NonterminalExpression classes

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{
			Context c = new Context();
			ArrayList l = new ArrayList(); //really need a tree here!

			// build up context information 
			// . . .

			// Populate abstract syntax tree with data
			l.Add(new TerminalExpression());
			l.Add(new NonterminalExpression());

			// interpret
			foreach (AbstractExpression exp in l)
			{
				exp.Interpret(c);
			}
            		
			return 0;
		}
	}
}

