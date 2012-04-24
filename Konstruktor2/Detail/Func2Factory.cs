using System;
using System.Diagnostics;

namespace Konstruktor.Detail
{
	static class Func2Factory
	{
		public static object instantiate(Type t, IKonstruktorScope konstruktorScope)
		{
			Debug.Assert(t.GetGenericTypeDefinition() == typeof(Func<,>));
			var funcArgs = t.GetGenericArguments();
			Debug.Assert(funcArgs.Length == 2);

			var argumentType = funcArgs[0];
			var resultType = funcArgs[1];

			var typeDef = typeof(Func2Factory<,>);
			var factoryType = typeDef.MakeGenericType(argumentType, resultType);

			var factoryInstance = (IFunc2Factory)Activator.CreateInstance(factoryType, konstruktorScope);
			return factoryInstance.resolveFactoryMethod();
		}
	}

	interface IFunc2Factory
	{
		object resolveFactoryMethod();
	}

	sealed class Func2Factory<ArgT, ResultT> : IFunc2Factory
	{
		readonly IKonstruktorScope _konstruktorScope;

		public Func2Factory(IKonstruktorScope konstruktorScope)
		{
			_konstruktorScope = konstruktorScope;
		}

		public object resolveFactoryMethod()
		{
			Func<ArgT, ResultT> method = argument =>
			{
				this.Debug("{0}".fmt(argument));

				var nested = _konstruktorScope.beginNestedScope();
				// the argument is not owned by the nested scope, but needs to be resolvable.
				nested.store(argument);
				return nested.resolveLocal<ResultT>();
			};

			return method;
		}
	}
}