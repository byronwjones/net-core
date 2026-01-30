using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace BWJ.Core
{
    /// <summary>
    /// A suite of methods intended for method argument validation
    /// </summary>
    public static class MethodGuard
    {
        /// <summary>
        /// Asserts that all the members of the given object are not null
        /// </summary>
        /// <param name="args">An anonymous type object containing nullable arguments provided to the method invoked</param>
        public static void NoNull(object args)
        {
            NoNull(nameof(args), args);
            var type = args.GetType();
            if (type.IsNotAnonymous())
            {
                throw new ArgumentException("Argument provided must be an anonymous type", nameof(args));
            }

            var props = type.GetProperties();
            foreach(var p in props)
            {
                NoNull(p.Name, p.GetValue(args));
            }
        }

        /// <summary>
        /// Asserts that an argument provided to the method invoked is not null
        /// </summary>
        /// <param name="parameterName">Name of the method parameter</param>
        /// <param name="arg">The argument provided to the given parameter</param>
        /// <exception cref="ArgumentException">If parameter name is null or empty string</exception>
        /// <exception cref="ArgumentNullException">If argument is null</exception>
        public static void NoNull(string parameterName, object? arg)
        {
            if(string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException(nameof(parameterName));
            }

            if(arg is null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void NoEmptyString(object args)
        {
            NoNull(nameof(args), args);
            var type = args.GetType();
            if (type.IsNotAnonymous())
            {
                throw new ArgumentException("Argument provided must be an anonymous type", nameof(args));
            }

            var props = type.GetProperties();
            foreach (var p in props)
            {
                var value = p.GetValue(args);
                if(value is not string)
                {
                    throw new InvalidOperationException($"Method argument '{p.Name}' is not a string, and cannot be validated with this guard");
                }
                NoEmptyString(p.Name, value as string);
            }
        }

        public static void NoEmptyString(string parameterName, string? parameter)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException(nameof(parameterName));
            }

            NoNull(parameterName, parameter);

            if(string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentException("Argument must not be empty or only consist of white space.", parameterName);
            }
        }

        public static void NoEmptyCollection(object args)
        {
            NoNull(nameof(args), args);
            var type = args.GetType();
            if (type.IsNotAnonymous())
            {
                throw new ArgumentException("Argument provided must be an anonymous type", nameof(args));
            }

            var props = type.GetProperties();
            foreach (var p in props)
            {
                var value = p.GetValue(args);
                if (value is not IEnumerable)
                {
                    throw new InvalidOperationException($"Method argument '{p.Name}' is not enumerable, and cannot be validated with this guard");
                }
                NoEmptyCollection(p.Name, value as IEnumerable);
            }
        }

        public static void NoEmptyCollection(string parameterName, IEnumerable? parameter)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException(nameof(parameterName));
            }

            NoNull(parameterName, parameter);

            if (parameter!.GetEnumerator().MoveNext() == false)
            {
                throw new ArgumentException("Argument must not be a null or empty collection.", parameterName);
            }
        }

        public static void NoDuplicateValues<T>(object args)
        {
            NoNull(nameof(args), args);

            var props = args.GetType().GetProperties();
            foreach (var p in props)
            {
                var value = p.GetValue(args);
                if(value is null) { continue; }

                if (value is not IEnumerable<T>)
                {
                    throw new InvalidOperationException($"Method argument '{p.Name}' is not an enumerable of type {typeof(T).Name}, and cannot be validated with this guard");
                }
                NoDuplicateValues(p.Name, value as IEnumerable<T>);
            }
        }

        public static void NoDuplicateValues<T>(string parameterName, IEnumerable<T>? parameter)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException(nameof(parameterName));
            }

            if(parameter is null)
            {
                return; // null is okay, the only no-no is a collection with duplicates
            }

            if (parameter.Count() != parameter.Distinct().Count())
            {
                throw new ArgumentException("All values in this collection must be distinct.", parameterName);
            }
        }

        public static void Acceptable<T>(object args,
            Func<T?, bool> predicate,
            string errorMessage) => ValidateWithRule(args, predicate, errorMessage, true);
        public static void Acceptable<T>(string parameterName,
            T parameter,
            Func<T?, bool> predicate,
            string errorMessage) => ValidateWithRule(parameterName, parameter, predicate, errorMessage, true);

        public static void Forbidden<T>(object args,
            Func<T?, bool> predicate,
            string errorMessage) => ValidateWithRule(args, predicate, errorMessage, false);
        public static void Forbidden<T>(string parameterName,
            T parameter,
            Func<T?, bool> predicate,
            string errorMessage) => ValidateWithRule(parameterName, parameter, predicate, errorMessage, false);

        private static void ValidateWithRule<T>(object args,
            Func<T?, bool> predicate,
            string errorMessage,
            bool validPredicateReturnValue)
        {
            NoNull(nameof(args), args);

            var props = args.GetType().GetProperties();
            foreach (var p in props)
            {
                var value = p.GetValue(args);
                if (value is not T)
                {
                    throw new InvalidOperationException($"Method argument '{p.Name}' is not of type {typeof(T).FullName}, and cannot be validated with this guard");
                }
                ValidateWithRule(p.Name,(T)value, predicate, errorMessage, validPredicateReturnValue);
            }
        }

        private static void ValidateWithRule<T>(string parameterName,
            T parameter,
            Func<T?, bool> predicate,
            string errorMessage,
            bool validPredicateReturnValue)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException(nameof(parameterName));
            }
            NoNull(nameof(predicate), predicate);
            NoEmptyString(nameof(errorMessage), errorMessage);

            if (predicate(parameter) != validPredicateReturnValue)
            {
                throw new ArgumentException($"Invalid argument: {errorMessage}", parameterName);
            }
        }
    }
}
