using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Snork.EventBus.Meta;

namespace Snork.EventBus
{
    internal class SubscriberMethodFinder
    {
        static SubscriberMethodFinder()
        {
            MethodCache =
                new Dictionary<Type, List<SubscriberMethod>>();
        }
        private const int PoolSize = 4;

        private static readonly Dictionary<Type, List<SubscriberMethod>> MethodCache;
      
        private static readonly FindState?[] FindStatePool = new FindState?[PoolSize];
        private readonly bool _ignoreGeneratedIndex;
        private readonly bool _strictMethodVerification;

        private readonly List<ISubscriberInfoIndex>? _subscriberInfoIndexes;

        public SubscriberMethodFinder(List<ISubscriberInfoIndex> subscriberInfoIndexes, bool strictMethodVerification,
            bool ignoreGeneratedIndex)
        {
            _subscriberInfoIndexes = subscriberInfoIndexes;
            _strictMethodVerification = strictMethodVerification;
            _ignoreGeneratedIndex = ignoreGeneratedIndex;
        }


        public List<SubscriberMethod> FindSubscriberMethods(Type subscriberType)
        {
            lock (MethodCache)
            {
                var subscriberMethods = MethodCache.ContainsKey(subscriberType) ? MethodCache[subscriberType] : null;
                if (subscriberMethods != null) return subscriberMethods;

                if (_ignoreGeneratedIndex)
                    subscriberMethods = FindUsingReflection(subscriberType);
                else
                    subscriberMethods = FindUsingInfo(subscriberType);

                if (!subscriberMethods.Any())
                    throw new EventBusException(
                        $"Type {subscriberType.FullName} and its base classes have no public methods with attribute {typeof(SubscribeAttribute).FullName}");

                MethodCache[subscriberType] = subscriberMethods;
                return subscriberMethods;
            }
        }

        private List<SubscriberMethod> FindUsingInfo(Type subscriberType)
        {
            var findState = PrepareFindState();
            findState.InitForSubscriber(subscriberType);
            while (findState.Type != null)
            {
                findState.SubscriberInfo = GetSubscriberInfo(findState);
                if (findState.SubscriberInfo != null)
                {
                    var subscriberMethods = findState.SubscriberInfo.GetSubscriberMethods(findState.Iteration);
                    foreach (var subscriberMethod in subscriberMethods)
                        if (findState.CheckAdd(subscriberMethod.Method, subscriberMethod.EventType))
                            findState.SubscriberMethods.Add(subscriberMethod);
                }
                else
                {
                    FindUsingReflectionInSingleType(findState);
                }

                findState.MoveToSuperclass();
                findState.Iteration++;
            }

            //remove duplicate signatures
            var tmp =  GetMethodsAndRelease(findState);
            var uniques = tmp.OrderBy(i => i.Iteration).GroupBy(i => i.Method.ToString()).Select(i => i.FirstOrDefault()).ToList();
            return uniques;
        }

        private List<SubscriberMethod> GetMethodsAndRelease(FindState findState)
        {
            //.Tolist extension is important, we're making a copy of the list
            var subscriberMethods = findState.SubscriberMethods.ToList();
            findState.Recycle();
            lock (FindStatePool)
            {
                for (var i = 0; i < PoolSize; i++)
                    if (FindStatePool[i] == null)
                    {
                        FindStatePool[i] = findState;
                        break;
                    }
            }

            return subscriberMethods;
        }

        private FindState PrepareFindState()
        {
            lock (FindStatePool)
            {
                for (var i = 0; i < PoolSize; i++)
                {
                    var state = FindStatePool[i];
                    if (state != null)
                    {
                        FindStatePool[i] = null;
                        return state;
                    }
                }
            }

            return new FindState();
        }

        private ISubscriberInfo? GetSubscriberInfo(FindState findState)
        {
            if (findState.SubscriberInfo?.GetSuperSubscriberInfo() != null)
            {
                var superclassInfo = findState.SubscriberInfo.GetSuperSubscriberInfo();
                if (findState.Type == superclassInfo.SubscriberType) return superclassInfo;
            }

            if (_subscriberInfoIndexes != null)
                foreach (var index in _subscriberInfoIndexes)
                {
                    var info = index.GetSubscriberInfo(findState.Type);
                    if (info != null) return info;
                }

            return null;
        }

        private List<SubscriberMethod> FindUsingReflection(Type subscriberType)
        {
            var findState = PrepareFindState();
            findState.InitForSubscriber(subscriberType);
            while (findState.Type != null)
            {
                FindUsingReflectionInSingleType(findState);
                findState.MoveToSuperclass();
            }

            return GetMethodsAndRelease(findState);
        }

        private void FindUsingReflectionInSingleType(FindState findState)
        {
            MethodInfo[] methods;
            try
            {
                // This is faster than getMethods, especially when subscribers are fat classes like Activities
                methods = findState.Type.GetMethods();
            }
            catch (Exception th)
            {
                // Workaround for java.lang.NoClassDefFoundError, see https://github.com/greenrobot/EventBus/issues/149
                try
                {
                    methods = findState.Type.GetMethods();
                }
                catch (Exception error)
                {
                    // super class of NoClassDefFoundError to be a bit more broad...
                    var msg = $"Could not inspect methods of {findState.Type.FullName}";
                    if (_ignoreGeneratedIndex)
                        msg += ". Please consider using EventBus annotation processor to avoid reflection.";
                    else
                        msg += ". Please make this class visible to EventBus annotation processor to avoid reflection.";

                    throw new EventBusException(msg, error);
                }

                findState.SkipSuperClasses = true;
            }

            foreach (var item in methods.Select(i => new
                     {
                         Method = i,
                         ParameterInfos = i.GetParameters(),
                         SubscribeAttributes = i.GetCustomAttributes(typeof(SubscribeAttribute))
                             .OfType<SubscribeAttribute>()
                             .ToList()
                     }).Where(i => i.SubscribeAttributes.Count == 1))
            {
                var method = item.Method;
                var subscribeAttributes = item.SubscribeAttributes;

                if (method.IsPublic && !method.IsAbstract && !method.IsStatic)
                {
                    var parameterInfos = item.ParameterInfos;
                    if (parameterInfos.Length == 1)
                    {
                        var subscribeAttribute = subscribeAttributes.FirstOrDefault();
                        if (subscribeAttribute != null)
                        {
                            var messageType = parameterInfos[0].ParameterType;
                            if (findState.CheckAdd(method, messageType))
                            {
                                var threadMode = subscribeAttribute.ThreadMode;
                                findState.SubscriberMethods.Add(new SubscriberMethod(method, messageType, threadMode,
                                    subscribeAttribute.Priority, subscribeAttribute.Sticky, findState.Iteration));
                            }
                        }
                    }
                    else if (_strictMethodVerification)
                    {
                        throw new EventBusException(
                            $"@Subscribe method {method.DeclaringType.FullName}.{method.Name} must have exactly 1 parameter but has {parameterInfos.Length}");
                    }
                }
                else if (_strictMethodVerification)
                {
                    throw new EventBusException(
                        $"{method.DeclaringType.FullName}.{method.Name} is a illegal @Subscribe method: must be public, non-static, and non-abstract");
                }
            }
        }

        public static void ClearCaches()
        {
            lock (MethodCache)
            {
                MethodCache.Clear();
            }
        }

        internal class FindState
        {
            private readonly Dictionary<Type, object> _anyMethodByEventType = new Dictionary<Type, object>();
            private readonly StringBuilder _methodKeyBuilder = new StringBuilder(128);
            private readonly Dictionary<string, Type> _subscriberTypeByMethodKey = new Dictionary<string, Type>();

            public int Iteration { get; set; }
            public bool SkipSuperClasses { get; set; }
            public Type? Type { get; private set; }
            public List<SubscriberMethod> SubscriberMethods { get; } = new List<SubscriberMethod>();
            public ISubscriberInfo? SubscriberInfo { get; set; }

            public void InitForSubscriber(Type subscriberType)
            {
                subscriberType = Type = subscriberType;
                SkipSuperClasses = false;
                SubscriberInfo = null;
                Iteration = 0;
            }

            public void Recycle()
            {
                SubscriberMethods.Clear();
                _anyMethodByEventType.Clear();
                _subscriberTypeByMethodKey.Clear();
                _methodKeyBuilder.Length = 0;
                Type = null;
                SkipSuperClasses = false;
                Iteration = 0;
                SubscriberInfo = null;
            }

            public bool CheckAdd(MethodInfo method, Type messageType)
            {
                // 2 level check: 1st level with message type only (fast), 2nd level with complete signature when required.
                // Usually a subscriber doesn't have methods listening to the same message type.
                var existing = _anyMethodByEventType.Put(messageType, method);
                if (existing == null) return true;

                if (existing is MethodInfo info)
                {
                    if (!CheckAddWithMethodSignature(info, messageType))
                        // Paranoia check
                        throw new InvalidOperationException();

                    // Put any non-MethodInfo object to "consume" the existing MethodInfo
                    _anyMethodByEventType[messageType] = this;
                }

                return CheckAddWithMethodSignature(method, messageType);
            }

            private bool CheckAddWithMethodSignature(MethodInfo method, Type messageType)
            {
                _methodKeyBuilder.Length = 0;
                _methodKeyBuilder.Append(method.Name);
                _methodKeyBuilder.Append('>').Append(messageType.Name);

                var methodKey = _methodKeyBuilder.ToString();
                var methodType = method.DeclaringType;
                var methodTypeOld = _subscriberTypeByMethodKey.Put(methodKey, methodType);
                if (methodTypeOld == null || methodTypeOld.IsAssignableFromExt(methodType))
                    // Only Add if not already found in a sub class
                    return true;

                // Revert the put, old class is further down the class hierarchy
                _subscriberTypeByMethodKey[methodKey] = methodTypeOld;
                return false;
            }

            public void MoveToSuperclass()
            {
                if (SkipSuperClasses)
                {
                    Type = null;
                }
                else
                {
                    //if (Type == typeof(object))
                    //{
                    //    Type = null;
                    //    return;
                    //}

                    if (Type != null)
                    {
                        Type = Type.BaseType;
                        //if (Type != null)
                        //    if (Type == typeof(object))
                        //        Type = null;
                    }
                }
            }
        }
    }
}