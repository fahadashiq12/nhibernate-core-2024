﻿using System;
using System.Linq;
using NHibernate.DomainModel.Northwind.Entities;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.Linq
{
	public class ExpressionSessionLeakTest : LinqTestCase
	{
		[Test]
		[Ignore("Not fixed yet. NH-3579")]
		public void SessionGetsCollected()
		{
			var reference = DoLinqInSeparateSession();

			GC.Collect();

			Assert.Null(reference.Target);
		}


		private WeakReference DoLinqInSeparateSession()
		{
			var leakTest = session.SessionFactory.OpenSession();

			// It appears linq expressions will (or might) contain a reference to the
			// IQueryable. At time of writing, linq expressions are helt within NhLinqExpression,
			// which in turn will be held in the query plan cache. Since the IQueryable will
			// be an NhQueryable, which holds a reference to the SessionImpl instance,
			// we will be leaking session instances.

			var query = leakTest.Query<Customer>().FirstOrDefault(t => t.CustomerId != "");
			leakTest.Dispose();

			return new WeakReference(leakTest, false);
		}
	}
}
