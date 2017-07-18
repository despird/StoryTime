namespace Mediator_DesignPattern
{
	using System;

	class Mediator 
	{
		private DataProviderColleague dataProvider;
		private DataConsumerColleague dataConsumer;
		public void IntroduceColleagues(DataProviderColleague c1, DataConsumerColleague c2)
		{
			dataProvider = c1;
			dataConsumer = c2;			
		}
		
		public void DataChanged()
		{
			int i = dataProvider.MyData;
			dataConsumer.NewValue(i);
		}
	}

	class DataConsumerColleague 
	{
		public void NewValue(int i)
		{
			Console.WriteLine("New value {0}", i);
		}
	}

	class DataProviderColleague
	{
		private Mediator mediator;
		private int iMyData=0;
		public int MyData 
		{
			get 
			{
				return iMyData;
			}
			set 
			{
				iMyData = value;
			}
		}
		public DataProviderColleague(Mediator m)
		{
			mediator = m;
		}

		public void ChangeData()
		{
			iMyData = 403;

			// Inform mediator that I have changed the data
			if (mediator != null)
				mediator.DataChanged();	
		}		
	}

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{            
			Mediator m = new Mediator();
			DataProviderColleague c1 = new DataProviderColleague(m);
			DataConsumerColleague c2 = new DataConsumerColleague();
			m.IntroduceColleagues(c1,c2);

			c1.ChangeData();

			return 0;
		}
	}
}

