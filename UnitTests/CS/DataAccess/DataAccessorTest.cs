using System;
using System.Collections;
using System.Data;

using NUnit.Framework;

#if FW2
using System.Collections.Generic;
using PersonDataSet = DataAccessTest.PersonDataSet2;
#endif

using BLToolkit.EditableObjects;
using BLToolkit.Data;
using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using BLToolkit.Validation;
using BLToolkit.Reflection;
using BLToolkit.TypeBuilder;

namespace DataAccessTest
{
	[TestFixture]
	public class DataAccessorTest
	{
		public enum Gender
		{
			[MapValue("F")] Female,
			[MapValue("M")] Male,
			[MapValue("U")] Unknown,
			[MapValue("O")] Other
		}

		[TableName("Person")]
		public abstract class Person : EditableObject
		{
			[PrimaryKey, NonUpdatable]
			[MapField("PersonID")]         public abstract int    ID         { get; set; }
			[MaxLength(50), Required]      public abstract string LastName   { get; set; }
			[MaxLength(50), Required]      public abstract string FirstName  { get; set; }
			[MaxLength(50), NullValue("")] public abstract string MiddleName { get; set; }
			[Required]                     public abstract Gender Gender     { get; set; }

			public abstract ArrayList Territories { get; set; }
		}

		public abstract class PersonDataAccessor : DataAccessor
		{
			public abstract int    Person_SelectAll();
			public abstract void   Person_SelectAll(DbManager db);
			public abstract Person SelectByName(string firstName, string lastName);

			[SprocName("Person_SelectByName"), DiscoverParameters]
			public abstract Person AnySprocName(string firstName, string lastName);

			[ActionName("SelectByName")]
			public abstract Person AnyActionName(string firstName, string lastName);

			[ActionName("SelectByName")]
			public abstract Person AnyParamName(
				[ParamName("FirstName")] string name1,
				[ParamName("@LastName")] string name2);

			[ActionName("SelectAll"), ObjectType(typeof(Person))]
			public abstract ArrayList SelectAllList();

#if FW2
			[ActionName("SelectAll")]
			public abstract List<Person> SelectAllListT();
			[SprocName("Person_SelectAll")]
			public abstract PersonDataSet.PersonDataTable SelectAllTypedDataTable();
#endif

			[SprocName("Person_SelectAll")] public abstract DataSet       SelectAllDataSet();
			[SprocName("Person_SelectAll")] public abstract PersonDataSet SelectAllTypedDataSet();
			[SprocName("Person_SelectAll")] public abstract DataTable     SelectAllDataTable();


			public Person AnyActionName1(string firstName, string lastName)
			{
				DataAccessorTest.Person person1 = null;
				using (DbManager manager1 = this.GetDbManager())
				{
					Type type1 = typeof(DataAccessorTest.Person);
					object[] objArray1 = new object[] { manager1.Parameter("@firstName", firstName), manager1.Parameter("@lastName", lastName) } ;
					person1 = (DataAccessorTest.Person) manager1.SetSpCommand(this.GetSpName(type1, "SelectByName"), objArray1).ExecuteObject(type1);
				}
				return person1;
			}
		}

		public abstract class PersonDataAccessor2 : DataAccessor
		{
			[SprocName("Person_SelectAll")] public abstract ArrayList SelectAllList();
		}

		public abstract class PersonDataAccessor1 : PersonDataAccessor
		{
			public DataSet SelectByName()
			{
				using (DbManager db = GetDbManager())
				{
					DataSet ds = new DataSet();

					db.SetSpCommand("Person_SelectAll");

					if (ds.Tables.Count > 0)
						db.ExecuteDataSet(ds, ds.Tables[0].TableName);
					else
						db.ExecuteDataSet(ds);

					return ds;
				}
			}
		}

		public class PersonList : ArrayList
		{
			public new Person this[int idx]
			{
				get { return (Person)base[idx]; }
				set { base[idx] = value;        }
			}
		}

		private PersonDataAccessor _da;

		public DataAccessorTest()
		{
			TypeFactory.SaveTypes = true;

			_da = (PersonDataAccessor)DataAccessor.CreateInstance(typeof(PersonDataAccessor));
		}

		[Test]
		public void Sql_Select()
		{
			Person e = (Person)_da.SelectByKeySql(typeof(Person), 1);
		}

		[Test]
		public void Sql_SelectAll()
		{
			ArrayList list = _da.SelectAllSql(typeof(Person));

			Console.WriteLine(list.Count);
		}

		[Test]
		public void Sql_Insert()
		{
			ArrayList list = _da.SelectAllSql(typeof(Person));
			Hashtable tbl  = new Hashtable();

			foreach (Person e in list)
				tbl[e.ID] = e;

			Person em = (Person)Map.CreateInstance(typeof(Person));

			em.FirstName = "1";
			em.LastName  = "2";

			_da.InsertSql(em);

			list = _da.SelectAllSql(typeof(Person));

			foreach (Person e in list)
				if (tbl.ContainsKey(e.ID) == false)
					_da.DeleteSql(e);
		}

		[Test]
		public void Sql_Update()
		{
			Person e = (Person)_da.SelectByKeySql(typeof(Person), 1);

			int n = _da.UpdateSql(e);

			Assert.AreEqual(1, n);
		}

		[Test]
		public void Sql_DeleteByKey()
		{
			ArrayList list = _da.SelectAllSql(typeof(Person));
			Hashtable tbl = new Hashtable();

			foreach (Person e in list)
				tbl[e.ID] = e;

			Person em = (Person)Map.CreateInstance(typeof(Person));

			em.FirstName = "1";
			em.LastName  = "2";

			_da.InsertSql(em);

			list = _da.SelectAllSql(typeof(Person));

			foreach (Person e in list)
				if (tbl.ContainsKey(e.ID) == false)
					_da.DeleteByKeySql(typeof(Person), e.ID);
		}

		[Test]
		public void Sproc_SelectAll()
		{
			ArrayList list = _da.SelectAll(typeof(Person));
			Console.WriteLine(list.Count);
		}

		[Test]
		public void Gen_Person_SelectAll()
		{
			int n = _da.Person_SelectAll();
			Console.WriteLine(n);
		}

		[Test]
		public void Gen_Person_SelectAll_DbManager()
		{
			using (DbManager db = _da.GetDbManager())
				_da.Person_SelectAll(db);
		}

		[Test]
		public void Gen_SelectByName()
		{
			Person e = _da.SelectByName("John", "Pupkin");
			Assert.AreEqual(1, e.ID);
		}

		[Test]
		public void Gen_SprocName()
		{
			Person e = _da.AnySprocName("John", "Pupkin");
			Assert.AreEqual(1, e.ID);
		}

		[Test]
		public void Gen_ActionName()
		{
			Person e = _da.AnyActionName("John", "Pupkin");
			Assert.AreEqual(1, e.ID);
		}

		[Test]
		public void Gen_ParamName()
		{
			Person e = _da.AnyParamName("John", "Pupkin");
			Assert.AreEqual(1, e.ID);
		}

		[Test]
		public void Gen_SelectAllDataSet()
		{
			DataSet ds = _da.SelectAllDataSet();
			Assert.AreNotEqual(0, ds.Tables[0].Rows.Count);
		}

		[Test]
		public void Gen_SelectAllTypedDataSet()
		{
			PersonDataSet ds = _da.SelectAllTypedDataSet();
			Assert.AreNotEqual(0, ds.Person.Rows.Count);
		}

		[Test]
		public void Gen_SelectAllDataTable()
		{
			DataTable dt = _da.SelectAllDataTable();
			Assert.AreNotEqual(0, dt.Rows.Count);
		}

		[Test, ExpectedException(typeof(TypeBuilderException))]
		public void Gen_SelectAllListexception()
		{
			TypeAccessor.CreateInstance(typeof(PersonDataAccessor2));
		}

		[Test]
		public void Gen_SelectAllList()
		{
			ArrayList list = _da.SelectAllList();
			Assert.AreNotEqual(0, list.Count);
		}

#if FW2
		[Test]
		public void Gen_SelectAllTypedDataTable()
		{
			PersonDataSet.PersonDataTable dt = _da.SelectAllTypedDataTable();
			Assert.AreNotEqual(0, dt.Rows.Count);
		}

		[Test]
		public void Gen_SelectAllListT()
		{
			List<Person> list = _da.SelectAllListT();
			Assert.AreNotEqual(0, list.Count);
		}
#endif
	}
}