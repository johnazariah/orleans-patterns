namespace Orleans.Patterns.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static partial class Extensions
    {
        private static class CastMethodCache<T>
        {
            private static readonly Dictionary<Type, MethodInfo> _forwardCache = new Dictionary<Type, MethodInfo>();
            private static readonly Dictionary<Type, MethodInfo> _reverseCache = new Dictionary<Type, MethodInfo>();

            public static MethodInfo GetCastMethodFromType(Type sourceType)
            {
                if (!_forwardCache.ContainsKey(sourceType))
                {
                    _forwardCache[sourceType] =
                           GetMethod(typeof(T), "op_Implicit")
                        ?? GetMethod(typeof(T), "op_Explicit");
                }
                if (_forwardCache[sourceType] != null) return _forwardCache[sourceType];

                if (!_reverseCache.ContainsKey(sourceType))
                {
                    _reverseCache[sourceType] =
                           GetMethod(sourceType, "op_Implicit")
                        ?? GetMethod(sourceType, "op_Explicit");
                }
                if (_reverseCache[sourceType] != null) return _reverseCache[sourceType];

                throw new InvalidCastException($"Cannot convert between {typeof(T).FullName} and {sourceType}");

                MethodInfo GetMethod(Type onType, string methodName) =>
                    Array.Find(
                        onType.GetMethods(BindingFlags.Static | BindingFlags.Public),
                        _mi => (
                            _mi.Name == methodName)
                            && (_mi.GetParameters().Single().ParameterType == sourceType)
                            && (_mi.ReturnType == typeof(T)));
            }
        }

        public static T DynamicCast<T>(this object sourceObject)
        {
            var sourceObjectType = sourceObject.GetType();

            if (typeof(T).IsAssignableFrom(sourceObjectType))
                return (T)(sourceObject);

            var mi = CastMethodCache<T>.GetCastMethodFromType(sourceObjectType);
            return (T)mi?.Invoke(null, new object[] { sourceObject });
        }
    }
}