using System;
using System.Diagnostics;

namespace Konstruktor2.Detail
{
	static class Func2Factory
	{
		public static object instantiate(Type t, ILifetimeScope lifetimeScope)
		{
			Debug.Assert(t.GetGenericTypeDefinition() == typeof(Func<,>));
			var funcArgs = t.GetGenericArguments();
			Debug.Assert(funcArgs.Length == 2);

			var argumentType = funcArgs[0];
			var resultType = funcArgs[1];

			var typeDef = typeof(Func2Factory<,>);
			var factoryType = typeDef.MakeGenericType(argumentType, resultType);

			var factoryInstance = (IFunc2Factory)Activator.CreateInstance(factoryType, lifetimeScope);
			return factoryInstance.resolveFactoryMethod();
		}
	}

	interface IFunc2Factory
	{
		object resolveFactoryMethod();
	}

	sealed class Func2Factory<ArgT, ResultT> : IFunc2Factory
	{
		readonly ILifetimeScope _lifetimeScope;

		public Func2Factory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}

		public object resolveFactoryMethod()
		{
			Func<ArgT, ResultT> method = argument =>
			{
				this.Debug("{0}".fmt(argument));

				var nested = _lifetimeScope.beginNestedScope();
				try
				{
					nested.store(argument);
					return nested.resolveRoot<ResultT>();
				}
				catch (Exception)
				{
					nested.Dispose();
					throw;
				}
			};

			return method;
		}
	}
}
