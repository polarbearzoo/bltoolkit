using System;

using NUnit.Framework;

using BLToolkit.Aspects;
using BLToolkit.Reflection;
using BLToolkit.TypeBuilder;

namespace Aspects
{
	[TestFixture]
	public class AsyncAspectTest
	{
		public AsyncAspectTest()
		{
			TypeFactory.SaveTypes = true;
		}

		private const int ExecutionTime = 200;

		public abstract class TestObject
		{
			public int Test(int intVal, string strVal)
			{
				System.Threading.Thread.Sleep(ExecutionTime);
				return intVal;
			}

			[Async] public abstract IAsyncResult BeginTest(int intVal, string strVal);
			[Async] public abstract IAsyncResult BeginTest(int intVal, string strVal, AsyncCallback callback);
			[Async] public abstract IAsyncResult BeginTest(int intVal, string strVal, AsyncCallback callback, object state);
			[Async] public abstract int EndTest(IAsyncResult asyncResult);

			[Async("Test")]
			public abstract IAsyncResult AnyName(int intVal, string strVal, AsyncCallback callback, object state);
			[Async("Test", typeof(int), typeof(string))]
			public abstract int AnyName(IAsyncResult asyncResult);
		}

		[Test]
		public void AsyncTest()
		{
			TestObject o = (TestObject)TypeAccessor.CreateInstance(typeof(TestObject));

			DateTime begin = DateTime.Now;
			Assert.AreEqual(1, o.Test(1, null));
			Assert.IsTrue((DateTime.Now - begin).TotalMilliseconds >= ExecutionTime);

			begin = DateTime.Now;
			IAsyncResult ar = o.BeginTest(2, "12");
			Assert.IsTrue((DateTime.Now - begin).TotalMilliseconds < ExecutionTime);

			Assert.AreEqual(2, o.EndTest(ar));
			Assert.IsTrue((DateTime.Now - begin).TotalMilliseconds >= ExecutionTime);
		}

		private static void CallBack(IAsyncResult ar)
		{
			TestObject o = (TestObject) ar.AsyncState;
			Console.WriteLine("Callback");
			o.EndTest(ar);
		}

		[Test]
		public void CallbackTest()
		{
			TestObject o = (TestObject)TypeAccessor.CreateInstance(typeof(TestObject));

			o.BeginTest(2, null, new AsyncCallback(CallBack), o);
		}

		[Test]
		public void NoStateTest()
		{
			TestObject o = (TestObject)TypeAccessor.CreateInstance(typeof(TestObject));

			Assert.AreEqual(1, o.Test(1, null));

			IAsyncResult ar = o.BeginTest(2, null, null);
			Assert.AreEqual(2, o.EndTest(ar));
		}

		[Test]
		public void NoCallbackTest()
		{
			TestObject o = (TestObject)TypeAccessor.CreateInstance(typeof(TestObject));

			Assert.AreEqual(1, o.Test(1, null));

			IAsyncResult ar = o.BeginTest(2, "1234");
			Assert.AreEqual(2, o.EndTest(ar));
		}

		[Test]
		public void AnyNameTest()
		{
			TestObject o = (TestObject)TypeAccessor.CreateInstance(typeof(TestObject));

			IAsyncResult ar = o.AnyName(2, null, null, null);
			Assert.AreEqual(2, o.AnyName(ar));
		}
	}
}
