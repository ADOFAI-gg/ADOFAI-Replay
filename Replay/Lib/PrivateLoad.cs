using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Replay.Lib
{
    public class PrivateLoad<T>
    {
        public FieldInfo field;
        public MethodInfo method;
        public object instance;

        public PrivateLoad(string name, object _instance)
        {
            instance = _instance;
            field = typeof(T).GetField(name, AccessTools.all);
            method = typeof(T).GetMethod(name, AccessTools.all);
        }

        public object Get()
        {
            return field.GetValue(instance);
        }

        public void Set(object value)
        {
            field.SetValue(instance,value);
        }

        public object Call(object[] args)
        {
            return method.Invoke(instance, args);
        }

    }
}
