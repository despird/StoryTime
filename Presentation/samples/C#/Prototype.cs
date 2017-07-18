namespace Prototype_DesignPattern
{
	using System;

	// Objects which are to work as prototypes must be based on classes which 
	// are derived from the abstract prototype class
	abstract class AbstractPrototype 
	{
		abstract public AbstractPrototype CloneYourself();
	}

	// This is a sample object
	class MyPrototype : AbstractPrototype 
	{
		override public AbstractPrototype CloneYourself()
		{
			return ((AbstractPrototype)MemberwiseClone());
		}
		// lots of other functions go here!
	}

	// This is the client piece of code which instantiate objects
	// based on a prototype. 
	class Demo 
	{
		private AbstractPrototype internalPrototype;

		public void SetPrototype(AbstractPrototype thePrototype)
		{
			internalPrototype = thePrototype;			
		}

		public void SomeImportantOperation()
		{
			// During Some important operation, imagine we need
			// to instantiate an object - but we do not know which. We use
			// the predefined prototype object, and ask it to clone itself. 

			AbstractPrototype x;
			x = internalPrototype.CloneYourself();
			// now we have two instances of the class which as as a prototype
		}
	}

	/// <summary>
	///    Summary description for Client.
	/// </summary>
	public class Client
	{
		public static int Main(string[] args)
		{            			
			Demo demo = new Demo();
			MyPrototype clientPrototype = new MyPrototype();
			demo.SetPrototype(clientPrototype);
			demo.SomeImportantOperation();

			return 0;
		}
	}
}