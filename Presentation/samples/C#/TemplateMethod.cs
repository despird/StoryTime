namespace TemplateMethod_DesignPattern
{
	using System;

	class Algorithm 
	{
		public void DoAlgorithm() 
		{
			Console.WriteLine("In DoAlgorithm");
			
			// do some part of the algorithm here
			
			// step1 goes here
			Console.WriteLine("In Algorithm - DoAlgoStep1");			
			// . . . 

			// step 2 goes here
			Console.WriteLine("In Algorithm - DoAlgoStep2");			
			// . . . 

			// Now call configurable/replacable part
			DoAlgoStep3();

			// step 4 goes here
			Console.WriteLine("In Algorithm - DoAlgoStep4");			
			// . . . 

			// Now call next configurable part
			DoAlgoStep5();
		}

		virtual public void DoAlgoStep3()
		{
			Console.WriteLine("In Algorithm - DoAlgoStep3");		
		}

		virtual public void DoAlgoStep5()
		{
			Console.WriteLine("In Algorithm - DoAlgoStep5");			
		}
	}

	class CustomAlgorithm : Algorithm
	{
		public override void DoAlgoStep3()
		{
			Console.WriteLine("In CustomAlgorithm - DoAlgoStep3");
		}

		public override void DoAlgoStep5()
		{
			Console.WriteLine("In CustomAlgorithm - DoAlgoStep5");
		}
	}

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{
			CustomAlgorithm c = new CustomAlgorithm();

			c.DoAlgorithm();

			return 0;
		}
	}
}
