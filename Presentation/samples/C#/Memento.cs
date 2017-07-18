namespace Memento_DesignPattern
{
	using System;

	class Originator 
	{
		private double manufacturer=0;
		private double distributor = 0;
		private double retailer = 0;

		public void MakeSale(double purchasePrice)
		{
			// We assume sales are divided equally amount the three
			manufacturer += purchasePrice * .40;
			distributor += purchasePrice *.3;
			retailer += purchasePrice *.3;
			// Note: to avoid rounding errors for real money handling 
			// apps, we should be using decimal integers
			// (but hey, this is just a demo!)
		}

		public Memento CreateMemento()
		{
			return (new Memento(manufacturer, distributor, retailer));			
		}
		
		public void SetMemento(Memento m)
		{
			manufacturer = m.A;
			distributor = m.B;
			retailer = m.C;
		}		
	}

	class Memento 
	{
		private double iA;
		private double iB;
		private double iC;

		public Memento(double a, double b, double c)
		{
			iA = a;
			iB = b;
			iC = c;
		}

		public double A 
		{
			get 
			{
				return iA;
			}
		}

		public double B 
		{
			get 
			{
				return iB;
			}
		}

		public double C 
		{
			get 
			{
				return iC;
			}
		}
	}

	class caretaker 
	{
		
	}

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{  			
			Originator o = new Originator();
			
			// Assume that during the course of running an application 
			// we we set various data in the originator
			o.MakeSale(45.0);
			o.MakeSale(60.0);

			// Now we wish to record the state of the object
			Memento m = o.CreateMemento();

			// We make further changes to the object
			o.MakeSale(60.0);
			o.MakeSale(10.0);
			o.MakeSale(320.0);

			// Then we decide ot change our minds, and revert to the saved state (and lose the changes since then)
			o.SetMemento(m);

			return 0;
		}
	}
}

