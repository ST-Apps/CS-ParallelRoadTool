using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace FineRoadAnarchy.Redirection
{
    public class NameOf
    {
        public static String nameof<T>(Expression<Func<T>> name)
        {
            MemberExpression expressionBody = (MemberExpression)name.Body;
            return expressionBody.Member.Name;
        }
    }

    public static class RedirectionUtil
    {

        public static Dictionary<MethodInfo, RedirectCallsState> RedirectAssembly()
        {
            var redirects = new Dictionary<MethodInfo, RedirectCallsState>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                redirects.AddRange(RedirectType(type));
            }
            return redirects;
        }

        public static void RevertRedirects(Dictionary<MethodInfo, RedirectCallsState> redirects)
        {
            if (redirects == null)
            {
                return;
            }
            foreach (var kvp in redirects)
            {
                RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
            }
        }

        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
        {
            if (target == null)
                throw new ArgumentNullException(NameOf.nameof(() => target));
            if (source == null)
            {
                return;
            }
            foreach (var element in source)
                target.Add(element);
        }

        public static Dictionary<MethodInfo, RedirectCallsState> RedirectType(Type type, bool onCreated = false)
        {
            var redirects = new Dictionary<MethodInfo, RedirectCallsState>();

            var customAttributes = type.GetCustomAttributes(typeof(TargetTypeAttribute), false);
            if (customAttributes.Length != 1)
            {
                return null;
            }
            var targetType = ((TargetTypeAttribute)customAttributes[0]).Type;
            RedirectMethods(type, targetType, redirects, onCreated);
            RedirectReverse(type, targetType, redirects, onCreated);
            return redirects;
        }

        private static void RedirectMethods(Type type, Type targetType, Dictionary<MethodInfo, RedirectCallsState> redirects, bool onCreated)
        {
            foreach (
                var method in
                    type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)
                        .Where(method =>
                        {
                            var redirectAttributes = method.GetCustomAttributes(typeof(RedirectMethodAttribute), false);
                            if (redirectAttributes.Length != 1)
                            {
                                return false;
                            }
                            return ((RedirectMethodAttribute)redirectAttributes[0]).OnCreated == onCreated;
                        }))
            {
                DebugUtils.Log("Redirecting " + targetType.Name + "#" + method.Name);
                RedirectMethod(targetType, method, redirects);
            }

            foreach (
                var property in
                    type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)
                        .Where(property =>
                        {
                            var redirectAttributes = property.GetCustomAttributes(typeof(RedirectMethodAttribute), false);
                            if (redirectAttributes.Length != 1)
                            {
                                return false;
                            }
                            return ((RedirectMethodAttribute)redirectAttributes[0]).OnCreated == onCreated;
                        }))
            {
                var getMethod = property.GetGetMethod();
                if (getMethod != null)
                {
                    DebugUtils.Log("Redirecting " + targetType.Name + "#" + getMethod.Name);
                    RedirectMethod(targetType, getMethod, redirects);
                }

                var setMethod = property.GetSetMethod();
                if (setMethod != null)
                {
                    DebugUtils.Log("Redirecting " + targetType.Name + "#" + setMethod.Name);
                    RedirectMethod(targetType, setMethod, redirects);
                }
            }
        }

        private static void RedirectReverse(Type type, Type targetType, Dictionary<MethodInfo, RedirectCallsState> redirects, bool onCreated)
        {
            foreach (
                var method in
                    type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)
                        .Where(method =>
                        {
                            var redirectAttributes = method.GetCustomAttributes(typeof(RedirectReverseAttribute), false);
                            if (redirectAttributes.Length == 1)
                            {
                                return ((RedirectReverseAttribute)redirectAttributes[0]).OnCreated == onCreated;
                            }
                            return false;
                        }))
            {
                DebugUtils.Log("Redirecting reverse " + targetType.Name + "#" + method.Name);
                RedirectMethod(targetType, method, redirects, true);
            }

            foreach (
                var property in
                    type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)
                        .Where(property =>
                        {
                            var redirectAttributes = property.GetCustomAttributes(typeof(RedirectReverseAttribute), false);
                            if (redirectAttributes.Length == 1)
                            {
                                return ((RedirectReverseAttribute)redirectAttributes[0]).OnCreated == onCreated;
                            }
                            return false;
                        }))
            {
                var getMethod = property.GetGetMethod();
                if (getMethod != null)
                {
                    DebugUtils.Log("Redirecting reverse " + targetType.Name + "#" + getMethod.Name);
                    RedirectMethod(targetType, getMethod, redirects, true);
                }

                var setMethod = property.GetSetMethod();
                if (setMethod != null)
                {
                    DebugUtils.Log("Redirecting reverse " + targetType.Name + "#" + setMethod.Name);
                    RedirectMethod(targetType, setMethod, redirects, true);
                }
            }
        }

        private static void RedirectMethod(Type targetType, MethodInfo method, Dictionary<MethodInfo, RedirectCallsState> redirects, bool reverse = false)
        {
            var tuple = RedirectMethod(targetType, method, reverse);
            redirects.Add(tuple.First, tuple.Second);
        }


        private static Tuple<MethodInfo, RedirectCallsState> RedirectMethod(Type targetType, MethodInfo detour, bool reverse)
        {
            var parameters = detour.GetParameters();
            Type[] types;
            if (parameters.Length > 0 && (
                (!targetType.IsValueType && parameters[0].ParameterType == targetType) ||
                (targetType.IsValueType && parameters[0].ParameterType == targetType.MakeByRefType())))
            {
                types = parameters.Skip(1).Select(p => p.ParameterType).ToArray();
            }
            else {
                types = parameters.Select(p => p.ParameterType).ToArray();
            }
            var originalMethod = targetType.GetMethod(detour.Name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, types,
                null);
            var redirectCallsState =
                reverse ? RedirectionHelper.RedirectCalls(detour, originalMethod) : RedirectionHelper.RedirectCalls(originalMethod, detour);
            return Tuple.New(reverse ? detour : originalMethod, redirectCallsState);
        }
    }
}